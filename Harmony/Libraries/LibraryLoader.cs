//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------------

namespace Harmony
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;

	using Raw;
	using Win32;
	using Win32.Delegates;
	using Win32.Enums;
	using Win32.Structs;
	using Win32.Structs.PortableExecutable;

	namespace Libraries
	{
		internal unsafe class LibraryLoader
		{
			#region Fields

			private readonly HarmonyLoadFlags _loadFlags;
			private readonly IEnumerable<HarmonyLibrary> _otherLibraries;
			private readonly uint _pageSize;

			#endregion

			#region Constructors

			public LibraryLoader(HarmonyLoadFlags loadFlags, IEnumerable<HarmonyLibrary> otherLibraries)
			{
				otherLibraries = otherLibraries ?? Enumerable.Empty<HarmonyLibrary>();

				_loadFlags = loadFlags;
				_otherLibraries = otherLibraries;
				_pageSize = GetPageSize();
			}

			#endregion

			#region Public interface

			public HarmonyLibrary CreateLibrary(byte[] rawDll, uint offset, uint length, string moduleName)
			{
				moduleName = (moduleName ?? string.Empty).Trim();

				fixed (byte* rawDllFixedPointer = rawDll)
				{
					Image rawImage = new Image(rawDllFixedPointer + offset, length);
					Image cookedImage = null;

					try
					{
						ValidateRawDll(rawImage);

						cookedImage = AllocateVirtualImage(rawImage);

						IEnumerable<HarmonyLibrarySection> sections = CopyImageToVirtualAddress(rawImage, cookedImage);

						IEnumerable<HarmonyImportLibrary> importLibraries = ResolveImports(cookedImage);

						ExportDictionaries exports = GetExportDictionaries(cookedImage);
					
						ApplyProperMemoryProtection(cookedImage);

						bool isDll = (cookedImage.FileHeader->Characteristics & ImageFileHeaderCharacteristics.IsDLL) != 0;

						if ((_loadFlags & HarmonyLoadFlags.NoAttach) == 0)
						{
							ExecuteTlsFunctions(cookedImage, DllCallType.PROCESS_ATTACH);
							if (isDll)
							{
								ExecuteDllMain(cookedImage, DllCallType.PROCESS_ATTACH);
							}
							else
							{
								ExecuteExeMain(cookedImage);
							}
						}

						return new HarmonyLibrary(
							name: moduleName,
							kind: isDll ? HarmonyLibraryKind.Dll : HarmonyLibraryKind.Exe,
							loadFlags: _loadFlags,
							baseAddress: (UIntPtr)cookedImage.BasePtr,
							imageSize: Environment.Is64BitProcess
								? cookedImage.Headers64->OptionalHeader.SizeOfImage
								: cookedImage.Headers32->OptionalHeader.SizeOfImage,
							alignedImageSize: cookedImage.Size,
							pageSize: _pageSize,
							sections: sections,
							importLibraries: importLibraries,
							exportsByName: exports.ExportsByName,
							exportsByOrdinal: exports.ExportsByOrdinal
						);
					}
					catch (Exception)
					{
						if (cookedImage != null)
						{
							// If we fail at any point during the load, release any memory we may have allocated.
							Kernel32.VirtualFree((UIntPtr)cookedImage.BasePtr, (UIntPtr)cookedImage.Size, FreeType.RELEASE);
						}
						throw;
					}
				}
			}

			#endregion

			#region Memory-allocation logic

			private Image AllocateVirtualImage(Image image)
			{
				uint lastSectionEnd = FindEndOfLastSection(image);

				uint cookedSize = RoundAddressUp(Environment.Is64BitProcess
					? image.OptionalHeader64->SizeOfImage : image.OptionalHeader32->SizeOfImage, _pageSize);
				if (cookedSize != RoundAddressUp(lastSectionEnd, _pageSize))
					throw new LoadFailedException("This DLL is damaged or invalid; its end does not match the system page size.");

				byte* cookedBase = (byte*)Kernel32.VirtualAlloc((UIntPtr)0, (UIntPtr)cookedSize,
					AllocationType.RESERVE | AllocationType.COMMIT, MemoryProtection.EXECUTE_READWRITE);

				return new Image(cookedBase, cookedSize);
			}

			private static uint FindEndOfLastSection(Image image)
			{
				IMAGE_SECTION_HEADER* section = image.FirstSection;
				uint optionalSectionSize = Environment.Is64BitProcess
					? image.OptionalHeader64->SectionAlignment : image.OptionalHeader32->SectionAlignment;
				uint lastSectionEnd = 0;
				int numberOfSections = image.FileHeader->NumberOfSections;

				for (int i = 0; i < numberOfSections; i++, section++)
				{
					uint endOfSection = section->SizeOfRawData == 0
						? section->VirtualAddress + optionalSectionSize
						: section->VirtualAddress + section->SizeOfRawData;

					if (endOfSection > lastSectionEnd)
					{
						lastSectionEnd = endOfSection;
					}
				}

				return lastSectionEnd;
			}

			#endregion

			#region Exports

			private ExportDictionaries GetExportDictionaries(Image image)
			{
				Dictionary<string, HarmonyExport> exportsByName = new Dictionary<string, HarmonyExport>();
				Dictionary<ushort, HarmonyExport> exportsByOrdinal = new Dictionary<ushort, HarmonyExport>();

				IMAGE_DATA_DIRECTORY directory = Environment.Is64BitProcess
					? image.OptionalHeader64->ExportTable : image.OptionalHeader32->ExportTable;
				if (directory.VirtualAddress == 0)
					return new ExportDictionaries(exportsByName, exportsByOrdinal);

				IMAGE_EXPORT_DIRECTORY* exportDirectory = (IMAGE_EXPORT_DIRECTORY*)(image.BasePtr + directory.VirtualAddress);
				UIntPtr nameRef = (UIntPtr)(image.BasePtr + exportDirectory->AddressOfNames);
				UIntPtr ordinalRef = (UIntPtr)(image.BasePtr + exportDirectory->AddressOfNameOrdinals);
				ushort ordinalBase = (ushort)exportDirectory->Base;

				for (int i = 0; i < exportDirectory->NumberOfNames; i++, nameRef += sizeof(UInt32), ordinalRef += sizeof(UInt16))
				{
					byte* namePtr = image.BasePtr + (int)*(UInt32*)nameRef;
					string name = StringOperations.NulTerminatedBytesToString(namePtr, image.BasePtr, image.Size);
					ushort ordinal = *(UInt16*)ordinalRef;

					IntPtr exportAddress = (IntPtr)(image.BasePtr + (int)*(UInt32*)(image.BasePtr + exportDirectory->AddressOfFunctions + ordinal * sizeof(UInt32)));

					ushort displayOrdinal = (ushort)(ordinal + ordinalBase);
					HarmonyExport export = new HarmonyExport(name, displayOrdinal, exportAddress);
					exportsByName[name] = export;
					exportsByOrdinal[displayOrdinal] = export;
				}

				return new ExportDictionaries(exportsByName, exportsByOrdinal);
			}

			#endregion

			#region Startup/shutdown function execution

			public void ExecuteDllMain(Image image, DllCallType callType)
			{
				uint addressOfEntryPoint = Environment.Is64BitProcess
					? image.OptionalHeader64->AddressOfEntryPoint : image.OptionalHeader32->AddressOfEntryPoint;
				if (addressOfEntryPoint >= image.Size - sizeof(IntPtr))
					throw new LoadFailedException("Cannot invoke this library's DllMain() function; its address is invalid.");

				IntPtr callback = (IntPtr)(image.BasePtr + addressOfEntryPoint);
				DllEntryProc dllEntryProc = (DllEntryProc)Marshal.GetDelegateForFunctionPointer(callback, typeof(DllEntryProc));

				uint succeeded = dllEntryProc((IntPtr)image.BasePtr, callType, IntPtr.Zero);

				if (succeeded == 0)
					throw new LoadFailedException("This library's DllMain() function returned false in response to " + callType + ".");
			}

			public void ExecuteExeMain(Image image)
			{
				uint addressOfEntryPoint = Environment.Is64BitProcess
					? image.OptionalHeader64->AddressOfEntryPoint : image.OptionalHeader32->AddressOfEntryPoint;
				if (addressOfEntryPoint >= image.Size - sizeof(IntPtr))
					throw new LoadFailedException("Cannot invoke executable's Main() function; its address is invalid.");

				IntPtr callback = (IntPtr)(image.BasePtr + addressOfEntryPoint);
				ExeEntryProc exeEntryProc = (ExeEntryProc)Marshal.GetDelegateForFunctionPointer(callback, typeof(ExeEntryProc));

				exeEntryProc();
			}

			public void ExecuteTlsFunctions(Image image, DllCallType callType)
			{
				IMAGE_DATA_DIRECTORY directory = Environment.Is64BitProcess
					? image.OptionalHeader64->TLSTable : image.OptionalHeader32->TLSTable;
				if (directory.VirtualAddress == 0)
					return;

				if (directory.VirtualAddress > image.Size - sizeof(IntPtr))
					throw new LoadFailedException("This library's TLS table is damaged or invalid.");

				IMAGE_TLS_DIRECTORY32* tlsDirectory32 = (IMAGE_TLS_DIRECTORY32*)(image.BasePtr + directory.VirtualAddress);
				IntPtr callbacks = (IntPtr)tlsDirectory32->AddressOfCallBacks;
				IntPtr endPtr = (IntPtr)(image.BasePtr + image.Size - sizeof(IntPtr));
				if (callbacks != IntPtr.Zero)
				{
					IntPtr callback;
					while ((callback = *(IntPtr*)callbacks) != IntPtr.Zero)
					{
						if ((long)callback > (long)endPtr)
							throw new LoadFailedException("One of this library's TLS functions is damaged or invalid.");

						DllEntryProc dllEntryProc = (DllEntryProc)Marshal.GetDelegateForFunctionPointer(callback, typeof(DllEntryProc));
						uint succeeded = dllEntryProc((IntPtr)image.BasePtr, DllCallType.PROCESS_ATTACH, IntPtr.Zero);
						if (succeeded == 0)
							throw new LoadFailedException("One of this library's TLS functions returned false in response to " + callType + ".");
					}
				}
			}

			#endregion

			#region Memory protection logic

			private static uint GetEffectiveSectionSize32(IMAGE_NT_HEADERS32* ntHeaders32, IMAGE_SECTION_HEADER* section)
			{
				if (section->SizeOfRawData != 0)
					return section->SizeOfRawData;

				if ((section->Characteristics & DataSectionFlags.ContentInitializedData) != 0)
					return ntHeaders32->OptionalHeader.SizeOfInitializedData;

				if ((section->Characteristics & DataSectionFlags.ContentUninitializedData) != 0)
					return ntHeaders32->OptionalHeader.SizeOfUninitializedData;

				return 0;
			}

			private static uint GetEffectiveSectionSize64(IMAGE_NT_HEADERS64* ntHeaders64, IMAGE_SECTION_HEADER* section)
			{
				if (section->SizeOfRawData != 0)
					return section->SizeOfRawData;

				if ((section->Characteristics & DataSectionFlags.ContentInitializedData) != 0)
					return ntHeaders64->OptionalHeader.SizeOfInitializedData;

				if ((section->Characteristics & DataSectionFlags.ContentUninitializedData) != 0)
					return ntHeaders64->OptionalHeader.SizeOfUninitializedData;

				return 0;
			}

			private static Protection ChooseMemoryProtection(DataSectionFlags sectionFlags)
			{
				switch (sectionFlags & (DataSectionFlags.MemoryExecute | DataSectionFlags.MemoryRead | DataSectionFlags.MemoryWrite))
				{
					default:
						return Protection.PAGE_NOACCESS;
					case DataSectionFlags.MemoryRead:
						return Protection.PAGE_READONLY;
					case DataSectionFlags.MemoryWrite:
						return Protection.PAGE_WRITECOPY;
					case DataSectionFlags.MemoryRead | DataSectionFlags.MemoryWrite:
						return Protection.PAGE_READWRITE;

					case DataSectionFlags.MemoryExecute:
						return Protection.PAGE_EXECUTE;
					case DataSectionFlags.MemoryExecute | DataSectionFlags.MemoryRead:
						return Protection.PAGE_EXECUTE_READ;
					case DataSectionFlags.MemoryExecute | DataSectionFlags.MemoryWrite:
						return Protection.PAGE_EXECUTE_WRITECOPY;
					case DataSectionFlags.MemoryExecute | DataSectionFlags.MemoryRead | DataSectionFlags.MemoryWrite:
						return Protection.PAGE_EXECUTE_READWRITE;
				}
			}

			private void ApplyProperMemoryProtection(Image image)
			{
				IMAGE_SECTION_HEADER* section = image.FirstSection;
				int numberOfSections = image.FileHeader->NumberOfSections;

				for (int i = 0; i < numberOfSections; i++, section++)
				{
					UIntPtr sectionAddress = (UIntPtr)image.BasePtr + (int)section->VirtualAddress;
					UIntPtr alignedAddress = RoundAddressDown(sectionAddress, (int)_pageSize);
					uint sectionSize = Environment.Is64BitProcess
						? GetEffectiveSectionSize64(image.Headers64, section)
						: GetEffectiveSectionSize32(image.Headers32, section);

					if (sectionSize == 0) continue;

					UIntPtr endAddress = RoundAddressUp(sectionAddress + (int)sectionSize, _pageSize);
					UIntPtr sizeRounded = (UIntPtr)((long)endAddress - (long)alignedAddress);

					if ((section->Characteristics & DataSectionFlags.MemoryDiscardable) != 0)
					{
						// Decommit this section, since we don't need it anymore; the OS can have
						// the page(s) back, but not the address space.
						Kernel32.VirtualFree(alignedAddress, sizeRounded, FreeType.DECOMMIT);
						continue;
					}

					Protection oldProtection;
					Protection protection = ChooseMemoryProtection(section->Characteristics);
					if (!Kernel32.VirtualProtect(alignedAddress, (uint)sizeRounded, protection, out oldProtection))
					{
						throw new LoadFailedException(string.Format("Cannot assign proper memory protection to pages for section \"{0}\".", section->Section));
					}
				}
			}

			#endregion

			#region Helper Methods

			private static uint RoundAddressUp(uint value, uint alignment)
			{
				return (value + alignment - 1) & ~(alignment - 1);
			}

			private static UIntPtr RoundAddressUp(UIntPtr value, uint alignment)
			{
				return (UIntPtr)(((long)value + alignment - 1) & ~((long)alignment - 1));
			}

			private static UIntPtr RoundAddressDown(UIntPtr value, int alignment)
			{
				return (UIntPtr)((long)value & ~((long)alignment - 1));
			}

			private static uint GetPageSize()
			{
				SYSTEM_INFO systemInfo = new SYSTEM_INFO();
				Kernel32.GetNativeSystemInfo(ref systemInfo);
				return systemInfo.dwPageSize;
			}

			#endregion

			#region DLL validation

			private void ValidateRawDll(Image image)
			{
				IMAGE_DOS_HEADER* dosHeader = (IMAGE_DOS_HEADER*)image.BasePtr;

				if (image.Size < sizeof(IMAGE_DOS_HEADER))
					throw new LoadFailedException("Bad or unknown DLL format (missing DOS header).");

				if (dosHeader->e_magic_byte[0] != 'M' || dosHeader->e_magic_byte[1] != 'Z')
					throw new LoadFailedException("Bad or unknown DLL format (missing DOS header 'MZ' signature).");

				if (dosHeader->e_lfanew + sizeof(IMAGE_NT_HEADERS32) > image.Size)
					throw new LoadFailedException("Bad or unknown DLL format (missing NT header data).");

				IMAGE_NT_HEADERS32* headers = image.Headers32;
				if (headers->Signature[0] != 'P' || headers->Signature[1] != 'E'
					|| headers->Signature[2] != '\0' || headers->Signature[3] != '\0')
					throw new LoadFailedException("Bad or unknown DLL format (missing NT header 'PE' signature).");

				switch (headers->FileHeader.Machine)
				{
					case MachineType.I386:
						if (Environment.Is64BitProcess)
							throw new LoadFailedException("The DLL is compiled for 32-bit x86, but the current process is not a 32-bit process.");
						break;

					case MachineType.x64:
						if (!Environment.Is64BitProcess)
							throw new LoadFailedException("The DLL is compiled for 64-bit x64, but the current process is not a 64-bit process.");
						break;

					default:
						throw new LoadFailedException("The DLL is compiled for an unsupported processor architecture.");
				}

				if ((headers->OptionalHeader.SectionAlignment & 1) != 0)
					throw new LoadFailedException("This DLL has an unsupported section alignment of " + headers->OptionalHeader.SectionAlignment);

				if (headers->OptionalHeader.SizeOfHeaders > image.Size)
					throw new LoadFailedException("Bad or unknown DLL format (damaged NT header data).");
			}

			#endregion

			#region Image Copying/Relocation

			private IEnumerable<HarmonyLibrarySection> CopyImageToVirtualAddress(Image srcImage, Image destImage)
			{
				// Copy the headers first.
				Kernel32.MoveMemory((IntPtr)destImage.BasePtr, (IntPtr)srcImage.BasePtr,
					Environment.Is64BitProcess
						? (IntPtr)srcImage.OptionalHeader64->SizeOfHeaders
						: (IntPtr)srcImage.OptionalHeader32->SizeOfHeaders);

				// Copy all the sections, verbatim.
				IEnumerable<HarmonyLibrarySection> sections = CopySections(srcImage, destImage);

				// Apply relocations to make the data correct in its new home.
				PerformBaseRelocation(destImage,
					Environment.Is64BitProcess
						? (long)destImage.BasePtr - (long)destImage.OptionalHeader64->ImageBase
						: (long)destImage.BasePtr - (int)destImage.OptionalHeader32->ImageBase);

				return sections;
			}

			private IEnumerable<HarmonyLibrarySection> CopySections(Image srcImage, Image destImage)
			{
				List<HarmonyLibrarySection> resultSections = new List<HarmonyLibrarySection>();

				IMAGE_SECTION_HEADER* section = destImage.FirstSection;
				for (int i = 0; i < destImage.FileHeader->NumberOfSections; i++, section++)
				{
					byte* dest = destImage.BasePtr + section->VirtualAddress;
					uint sectionSize;

					if (section->SizeOfRawData == 0)
					{
						sectionSize = Environment.Is64BitProcess
							? destImage.OptionalHeader64->SectionAlignment
							: destImage.OptionalHeader32->SectionAlignment;
						if (sectionSize > 0)
						{
							if (dest + sectionSize > destImage.BasePtr + destImage.Size)
								throw new LoadFailedException(string.Format("This DLL is damaged; section \"{0}\" has an illegal virtual address or size.", section->Section));

							Kernel32.ZeroMemory((IntPtr)dest, (IntPtr)sectionSize);
						}
					}
					else
					{
						sectionSize = section->SizeOfRawData;
						if (dest + sectionSize > destImage.BasePtr + destImage.Size)
							throw new LoadFailedException(string.Format("This DLL is damaged; section \"{0}\" has an illegal virtual address or size.", section->Section));

						byte* src = srcImage.BasePtr + section->PointerToRawData;
						if (src + sectionSize > srcImage.BasePtr + srcImage.Size)
							throw new LoadFailedException(string.Format("This DLL is damaged; section \"{0}\" has an illegal file offset or size.", section->Section));

						Kernel32.MoveMemory((IntPtr)dest, (IntPtr)src, (IntPtr)sectionSize);
					}

					resultSections.Add(new HarmonyLibrarySection(section->Section, (UIntPtr)dest, sectionSize));
				}

				return resultSections;
			}

			private void PerformBaseRelocation(Image image, long delta)
			{
				IMAGE_DATA_DIRECTORY directory = Environment.Is64BitProcess
					? image.OptionalHeader64->BaseRelocationTable
					: image.OptionalHeader32->BaseRelocationTable;
				if (directory.Size == 0)
					return;

				IMAGE_BASE_RELOCATION* relocation = (IMAGE_BASE_RELOCATION*)(image.BasePtr + directory.VirtualAddress);
				while (relocation->VirtualAddress > 0)
				{
					ApplyRelocationBlock(image, delta, relocation);
					relocation = (IMAGE_BASE_RELOCATION*)((byte*)relocation + relocation->BlockSizeInclusive);
				}
			}

			private void ApplyRelocationBlock(Image image, long delta, IMAGE_BASE_RELOCATION* relocation)
			{
				byte* dest = image.BasePtr + relocation->VirtualAddress;
				ushort* relocationInfo = (ushort*) ((byte*) relocation + sizeof(IMAGE_BASE_RELOCATION));

				for (int i = 0; i < (relocation->BlockSizeInclusive - sizeof(IMAGE_BASE_RELOCATION))/2; i++, relocationInfo++)
				{
					RelocationType type = (RelocationType) (*relocationInfo >> 12);
					int offset = *relocationInfo & 0xFFF;

					switch (type)
					{
						case RelocationType.Absolute:
							// Skip relocation;
							break;

						case RelocationType.HighLow:
							// Change complete 32-bit address.
							int* patchAddressHighLow = (int*) (dest + offset);
							if ((byte*)patchAddressHighLow < image.BasePtr || (byte*)patchAddressHighLow > image.BasePtr + image.Size)
								throw new LoadFailedException("This DLL is damaged; relocation table references an illegal 32-bit offset.");
							*patchAddressHighLow += (int) delta;
							break;

						case RelocationType.Dir64:
							// Change complete 64-bit address.
							long* patchAddress64 = (long*) (dest + offset);
							if ((byte*)patchAddress64 < image.BasePtr || (byte*)patchAddress64 > image.BasePtr + image.Size)
								throw new LoadFailedException("This DLL is damaged; relocation table references an illegal 64-bit offset.");
							*patchAddress64 += delta;
							break;

						default:
							throw new LoadFailedException(string.Format("This DLL requires unsupported address-relocation type {0} ({1})", type, (int) type));
					}
				}
			}

			#endregion

			#region Imports and Thunks

			private IEnumerable<HarmonyImportLibrary> ResolveImports(Image image)
			{
				IMAGE_DATA_DIRECTORY directory = Environment.Is64BitProcess
					? image.OptionalHeader64->ImportTable
					: image.OptionalHeader32->ImportTable;
				if (directory.Size == 0 || (_loadFlags & HarmonyLoadFlags.NoImports) != 0)
					return Enumerable.Empty<HarmonyImportLibrary>();

				Dictionary<string, HarmonyLibrary> otherLibraryLookup =
					_otherLibraries.ToDictionary(l => (Path.GetFileName(l.Name) ?? l.Name).ToLowerInvariant());

				List<HarmonyImportLibrary> imports = new List<HarmonyImportLibrary>();

				IMAGE_IMPORT_DESCRIPTOR* importDescriptor = (IMAGE_IMPORT_DESCRIPTOR*)(image.BasePtr + directory.VirtualAddress);
				for (; !Kernel32.IsBadReadPtr((UIntPtr)importDescriptor, (uint)sizeof(IMAGE_IMPORT_DESCRIPTOR))
					&& importDescriptor->Name != 0; importDescriptor++)
				{
					HarmonyImportLibrary import = LoadImport(image.BasePtr, image.Size, importDescriptor, otherLibraryLookup, _loadFlags);

					imports.Add(import);
				}

				return imports;
			}

			private static HarmonyImportLibrary LoadImport(byte* basePtr, uint size, IMAGE_IMPORT_DESCRIPTOR* importDescriptor,
				Dictionary<string, HarmonyLibrary> otherLibraryLookup, HarmonyLoadFlags loadFlags)
			{
				string moduleName = StringOperations.NulTerminatedBytesToString(basePtr + importDescriptor->Name, basePtr, size);

				HarmonyLibrary otherLibrary;
				if (otherLibraryLookup.TryGetValue(moduleName.ToLowerInvariant(), out otherLibrary))
					return LoadHarmonyImport(basePtr, size, importDescriptor, otherLibrary);

				if ((loadFlags & HarmonyLoadFlags.PrivateImports) == 0)
					return LoadExternalDllImport(basePtr, size, importDescriptor, moduleName);

				return null;
			}

			private static HarmonyImportLibrary LoadHarmonyImport(byte* basePtr, uint size, IMAGE_IMPORT_DESCRIPTOR* importDescriptor, HarmonyLibrary library)
			{
				HarmonyImportLibrary importLibrary = ApplyImportLibrary(
					basePtr, size, importDescriptor, library.Name, HarmonyImportLibraryKind.HarmonyLibrary,
					library.GetProcAddress, library.GetProcAddressByOrdinal
				);

				return importLibrary;
			}

			private static HarmonyImportLibrary LoadExternalDllImport(byte* basePtr, uint size, IMAGE_IMPORT_DESCRIPTOR* importDescriptor, string moduleName)
			{
				IntPtr moduleHandle = Kernel32.LoadLibrary(moduleName);
				if (moduleHandle == IntPtr.Zero)
				{
					int lastError = Marshal.GetLastWin32Error();
					Exception innerEx = new Win32Exception(lastError);
					innerEx.Data.Add("LastWin32Error", lastError);
					throw new LoadFailedException(string.Format("Unable to load dependent library \"{0}\".", moduleName), innerEx);
				}

				HarmonyImportLibrary importLibrary = ApplyImportLibrary(
					basePtr, size, importDescriptor, moduleName, HarmonyImportLibraryKind.ExternalDll,
					name => Kernel32.GetProcAddress(moduleHandle, name),
					ordinal => Kernel32.GetProcAddressOrdinal(moduleHandle, (IntPtr)ordinal)
				);

				return importLibrary;
			}

			private static HarmonyImportLibrary ApplyImportLibrary(byte* basePtr, uint size, IMAGE_IMPORT_DESCRIPTOR* importDescriptor,
				string moduleName, HarmonyImportLibraryKind importLibraryKind,
				Func<string, IntPtr> getImportByName, Func<ushort, IntPtr> getImportByOrdinal)
			{
				const uint ImageOrdinalFlag32 = 0x80000000U;
				const ulong ImageOrdinalFlag64 = 0x8000000000000000UL;

				List<HarmonyImport> importThunks = new List<HarmonyImport>();

				UIntPtr thunkRef;
				UIntPtr funcRef;
				if (importDescriptor->OriginalFirstThunk != 0)
				{
					thunkRef = (UIntPtr)(basePtr + importDescriptor->OriginalFirstThunk);
					funcRef = (UIntPtr)(basePtr + importDescriptor->FirstThunk);
				}
				else
				{
					// No hint table.
					thunkRef = (UIntPtr)(basePtr + importDescriptor->FirstThunk);
					funcRef = (UIntPtr)(basePtr + importDescriptor->FirstThunk);
				}

				bool is64BitMode = Environment.Is64BitProcess;
				uint thunk;
				for (; (thunk = *(uint*)thunkRef) != 0; thunkRef += sizeof(UIntPtr), funcRef += sizeof(UIntPtr))
				{
					IntPtr procAddress;
					if (!is64BitMode && (thunk & ImageOrdinalFlag32) != 0
						|| is64BitMode && (thunk & ImageOrdinalFlag64) != 0)
					{
						procAddress = getImportByOrdinal((ushort)(thunk & 0xFFFF));

						if (procAddress == IntPtr.Zero)
							throw new LoadFailedException(string.Format("Unable to dependent library \"{0}\" is missing required import ordinal {1}.", moduleName, thunk & 0xFFFF));

						*(IntPtr*)funcRef = procAddress;

						importThunks.Add(new HarmonyImport((int)(thunk & 0xFFFF), null, procAddress));
					}
					else
					{
						IMAGE_IMPORT_BY_NAME* thunkData = (IMAGE_IMPORT_BY_NAME*)(basePtr + thunk);
						string thunkName = StringOperations.NulTerminatedBytesToString((byte*)thunkData + 2, basePtr, size);
						procAddress = getImportByName(thunkName);

						if (procAddress == IntPtr.Zero)
							throw new LoadFailedException(string.Format("Unable to dependent library \"{0}\" is missing required import \"{1}\".", moduleName, thunkName));

						*(IntPtr*)funcRef = procAddress;

						importThunks.Add(new HarmonyImport(null, thunkName, procAddress));
					}
				}

				return new HarmonyImportLibrary(importLibraryKind, moduleName, importThunks);
			}

			#endregion
		}
	}
}