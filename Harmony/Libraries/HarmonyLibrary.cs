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
	using Collections;
	using Win32;
	using Win32.Enums;

	namespace Libraries
	{
		using System;
		using System.Collections.Generic;
		using System.IO;
		using System.Linq;

		/// <summary>
		/// A HarmonyLibrary is like a Windows HMODULE (i.e., a loaded DLL in memory), but unlike an HMODULE,
		/// it can be constructed from any data source, including a byte array or a .NET Stream.
		/// </summary>
		public sealed class HarmonyLibrary : IDisposable
		{
			#region Fields

			/// <summary>
			/// The name of this library, primarily for use when resolving imports in other HarmonyLibraries.
			/// </summary>
			public readonly string Name;

			/// <summary>
			/// What kind of library this is, an EXE or a DLL.
			/// </summary>
			public readonly HarmonyLibraryKind Kind;

			/// <summary>
			/// The load flags that were used when creating this library.
			/// </summary>
			public readonly HarmonyLoadFlags LoadFlags;

			/// <summary>
			/// The loaded base address of this library in memory.
			/// </summary>
			public readonly UIntPtr BaseAddress;

			/// <summary>
			/// The actual size of this library, in bytes.
			/// </summary>
			public readonly uint ImageSize;

			/// <summary>
			/// The size of this library after it has been loaded into memory, in bytes.
			/// </summary>
			public readonly uint AlignedImageSize;

			/// <summary>
			/// The size of a memory page.
			/// </summary>
			public readonly uint PageSize;

			/// <summary>
			/// The PE sections of this library, at their loaded memory addresses.  This is a read-only array.
			/// </summary>
			public readonly IList<HarmonyLibrarySection> Sections;

			/// <summary>
			/// The list of import libraries required by this library, at their loaded memory addresses.  This is a read-only array.
			/// </summary>
			public readonly IList<HarmonyImportLibrary> ImportLibraries;

			/// <summary>
			/// The exported functions and data provided by this library, by name.  This is a read-only dictionary.
			/// </summary>
			public readonly IDictionary<string, HarmonyExport> ExportsByName;

			/// <summary>
			/// The exported functions and data provided by this library, by ordinal.  This is a read-only dictionary.
			/// </summary>
			public readonly IDictionary<ushort, HarmonyExport> ExportsByOrdinal;

			#endregion

			#region Construction

			/// <summary>
			/// The constructor is internal, because the real work is done inside the LibraryLoader class.  This
			/// merely fills in the instance data after the LibraryLoader is done.
			/// </summary>
			internal HarmonyLibrary(string name, HarmonyLibraryKind kind, HarmonyLoadFlags loadFlags,
				UIntPtr baseAddress, uint imageSize, uint alignedImageSize, uint pageSize,
				IEnumerable<HarmonyLibrarySection> sections, IEnumerable<HarmonyImportLibrary> importLibraries,
				IDictionary<string, HarmonyExport> exportsByName, IDictionary<ushort, HarmonyExport> exportsByOrdinal)
			{
				Name = name;
				Kind = kind;
				LoadFlags = loadFlags;

				BaseAddress = baseAddress;
				ImageSize = imageSize;
				AlignedImageSize = alignedImageSize;
				PageSize = pageSize;

				Sections = Array.AsReadOnly(sections.ToArray());
				ImportLibraries = Array.AsReadOnly(importLibraries.ToArray());
				ExportsByName = new ReadOnlyDictionary<string, HarmonyExport>(exportsByName);
				ExportsByOrdinal = new ReadOnlyDictionary<ushort, HarmonyExport>(exportsByOrdinal);
			}

			/// <summary>
			/// Create a HarmonyLibrary from the given byte array, which should contain the bytes of
			/// an unmanaged EXE or an unmanaged DLL.
			/// </summary>
			/// <param name="rawDll">The bytes of the EXE or DLL to load.</param>
			/// <param name="name">(optional) The name of the library we're loading (to be used when resolving other libraries).</param>
			/// <param name="loadFlags">(optional) Flags to control how this loading is performed.</param>
			/// <param name="otherLibraries">(optional) Any other HarmonyLibraries to use when resolving this library's imports.</param>
			/// <returns>A HarmonyLibrary instance.</returns>
			/// <throws cref="LoadFailedException">Thrown when the library cannot be loaded (if, for example, it is corrupt
			/// data, or if one of its imports cannot be resolved).  The exception message will provide details to explain
			/// why the library-load failed.</throws>
			public static HarmonyLibrary CreateFromBytes(byte[] rawDll, string name = null,
				HarmonyLoadFlags loadFlags = 0, IEnumerable<HarmonyLibrary> otherLibraries = null)
			{
				return CreateFromBytes(rawDll, 0, (uint)rawDll.Length, name, loadFlags, otherLibraries);
			}

			/// <summary>
			/// Create a HarmonyLibrary from a subset of the given byte array, which should contain the bytes of
			/// an unmanaged EXE or an unmanaged DLL.
			/// </summary>
			/// <param name="rawDll">An array of bytes that contains the EXE or DLL to load.</param>
			/// <param name="offset">Where within the raw byte array the actual EXE or DLL starts.</param>
			/// <param name="length">The length of the EXE or DLL bytes starting at the given offset in the array.</param>
			/// <param name="name">(optional) The name of the library we're loading (to be used when resolving other libraries).</param>
			/// <param name="loadFlags">(optional) Flags to control how this loading is performed.</param>
			/// <param name="otherLibraries">(optional) Any other HarmonyLibraries to use when resolving this library's imports.</param>
			/// <returns>A HarmonyLibrary instance.</returns>
			/// <throws cref="LoadFailedException">Thrown when the library cannot be loaded (if, for example, it is corrupt
			/// data, or if one of its imports cannot be resolved).  The exception message will provide details to explain
			/// why the library-load failed.</throws>
			public static HarmonyLibrary CreateFromBytes(byte[] rawDll, uint offset, uint length, string name = null,
				HarmonyLoadFlags loadFlags = 0, IEnumerable<HarmonyLibrary> otherLibraries = null)
			{
				LibraryLoader libraryLoader = new LibraryLoader(loadFlags, otherLibraries);
				HarmonyLibrary library = libraryLoader.CreateLibrary(rawDll, offset, length, name);
				return library;
			}

			/// <summary>
			/// Create a HarmonyLibrary from the given stream, which should contain the bytes of
			/// an unmanaged EXE or an unmanaged DLL.
			/// </summary>
			/// <param name="rawDll">The stream that contains the EXE or DLL to load.</param>
			/// <param name="name">(optional) The name of the library we're loading (to be used when resolving other libraries).</param>
			/// <param name="loadFlags">(optional) Flags to control how this loading is performed.</param>
			/// <param name="otherLibraries">(optional) Any other HarmonyLibraries to use when resolving this library's imports.</param>
			/// <returns>A HarmonyLibrary instance.</returns>
			/// <throws cref="LoadFailedException">Thrown when the library cannot be loaded (if, for example, it is corrupt
			/// data, or if one of its imports cannot be resolved).  The exception message will provide details to explain
			/// why the library-load failed.</throws>
			public static HarmonyLibrary CreateFromStream(Stream rawDll, string name = null,
				HarmonyLoadFlags loadFlags = 0, IEnumerable<HarmonyLibrary> otherLibraries = null)
			{
				StreamCollector collector = new StreamCollector();
				collector.Read(rawDll);
				return CreateFromBytes(collector.Bytes, 0, (uint)collector.Length, name, loadFlags, otherLibraries);
			}

			#endregion

			#region Destruction and Disposal

			private bool _disposed;

			/// <summary>
			/// Destroy a HarmonyLibrary instance, freeing its memory.
			/// 
			/// WARNING:  After a HarmonyLibrary is destroyed, any delegates still pointing at it will fail
			/// silently if they are called!  (If you're lucky, you may get an exception; if you are unlucky,
			/// you will get data corruption.)  Do not free a HarmonyLibrary unless you are sure that its
			/// functions can no longer be called.
			/// </summary>
			~HarmonyLibrary()
			{
				Dispose(false);
			}

			/// <summary>
			/// Dispose of a HarmonyLibrary instance, freeing its memory.
			/// 
			/// WARNING:  After a HarmonyLibrary is destroyed, any delegates still pointing at it will fail
			/// silently if they are called!  (If you're lucky, you may get an exception; if you are unlucky,
			/// you will get data corruption.)  Do not free a HarmonyLibrary unless you are sure that its
			/// functions can no longer be called.
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Dispose of a HarmonyLibrary instance, freeing its memory.
			/// 
			/// WARNING:  After a HarmonyLibrary is destroyed, any delegates still pointing at it will fail
			/// silently if they are called!  (If you're lucky, you may get an exception; if you are unlucky,
			/// you will get data corruption.)  Do not free a HarmonyLibrary unless you are sure that its
			/// functions can no longer be called.
			/// </summary>
			/// <param name="disposing">True if this is from an explicit call to Dispose(); false if this
			/// is a request made by the garbage collector.</param>
			private void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					// Free anything else that implements IDisposable here.
				}

				if ((LoadFlags & HarmonyLoadFlags.NoDetach) == 0)
				{
					Detach();
				}

				// Free things that are unmanaged here.
				Kernel32.VirtualFree(BaseAddress, (UIntPtr)AlignedImageSize, FreeType.RELEASE);

				_disposed = true;
			}

			/// <summary>
			/// Invoke this library's PROCESS_DETACH handlers, if it has any.
			/// </summary>
			private unsafe void Detach()
			{
				LibraryLoader libraryLoader = new LibraryLoader(0, null);
				Image image = new Image((byte*)BaseAddress, AlignedImageSize);

				libraryLoader.ExecuteTlsFunctions(image, DllCallType.PROCESS_DETACH);
				if (Kind == HarmonyLibraryKind.Dll)
				{
					libraryLoader.ExecuteDllMain(image, DllCallType.PROCESS_DETACH);
				}
			}

			#endregion

			#region Function lookup

			/// <summary>
			/// Get the address of the given function in the library, by name.
			/// </summary>
			/// <param name="name">The name of the function to locate.</param>
			/// <returns>The address of that function, or IntPtr.Zero if no such function exists in this library's exports.</returns>
			public IntPtr GetProcAddress(string name)
			{
				HarmonyExport export;
				return ExportsByName.TryGetValue(name, out export) ? export.Address : IntPtr.Zero;
			}

			/// <summary>
			/// Get the address of the given function in the library, by ordinal.
			/// </summary>
			/// <param name="ordinal">The ordinal of the function to locate.</param>
			/// <returns>The address of that function, or IntPtr.Zero if no such function exists in this library's exports.</returns>
			public IntPtr GetProcAddressByOrdinal(ushort ordinal)
			{
				HarmonyExport export;
				return ExportsByOrdinal.TryGetValue(ordinal, out export) ? export.Address : IntPtr.Zero;
			}

			/// <summary>
			/// Get a delegate for a named function in the library.
			/// </summary>
			/// <param name="T">The delegate type for the function.</param>
			/// <param name="name">The name of the function to locate.  If this is null, the name of the
			/// delegate will be used instead of this string.</param>
			/// <returns>A delegate for that function, or null if no such function exists in this library's exports.</returns>
			/// <throws cref="ArgumentException">Thrown if the type T is not a delegate type.</throws>
			public T GetFunction<T>(string name = null)
				where T : class // , delegate
			{
				HarmonyExport export;
				return ExportsByName.TryGetValue(name ?? typeof(T).Name, out export) ? export.AsDelegate<T>() : null;
			}

			#endregion

			#region Stringification (for debugging)

			/// <summary>
			/// Convert this object to a string, for debugging purposes.
			/// </summary>
			public override string ToString()
			{
				return string.Format("{0}: at 0x{1:X8}, size 0x{2:X4}, {3} exports (by name; {4} by ordinal)",
					!string.IsNullOrEmpty(Name) ? Name : "<unnamed library>",
					(long)BaseAddress, AlignedImageSize, ExportsByName.Count, ExportsByOrdinal.Count);
			}

			#endregion
		}
	}
}