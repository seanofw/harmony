//-------------------------------------------------
//     This is a generated file. Do not edit!
//-------------------------------------------------

// Harmony/Collections/ReadOnlyDictionary.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dictionary;

		public ReadOnlyDictionary()
		{
			_dictionary = new Dictionary<TKey, TValue>();
		}

		public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		#region IDictionary<TKey,TValue> Members

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			throw ReadOnlyException();
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get { return _dictionary.Keys; }
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			throw ReadOnlyException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public ICollection<TValue> Values
		{
			get { return _dictionary.Values; }
		}

		public TValue this[TKey key]
		{
			get
			{
				return _dictionary[key];
			}
		}

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get
			{
				return this[key];
			}
			set
			{
				throw ReadOnlyException();
			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			throw ReadOnlyException();
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			throw ReadOnlyException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _dictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _dictionary.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			throw ReadOnlyException();
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		private static Exception ReadOnlyException()
		{
			return new NotSupportedException("This dictionary is read-only");
		}
	}
}

// Harmony/Libraries/ExportDictionaries.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System.Collections.Generic;

	internal class ExportDictionaries
	{
		public readonly Dictionary<string, HarmonyExport> ExportsByName;
		public readonly Dictionary<ushort, HarmonyExport> ExportsByOrdinal;

		public ExportDictionaries(Dictionary<string, HarmonyExport> exportsByName,
			Dictionary<ushort, HarmonyExport> exportsByOrdinal)
		{
			ExportsByName = exportsByName;
			ExportsByOrdinal = exportsByOrdinal;
		}
	}
}

// Harmony/Libraries/HarmonyExport.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;
	using System.Runtime.InteropServices;

	public class HarmonyExport
	{
		public readonly string Name;
		public readonly int Ordinal;
		public readonly IntPtr Address;

		public HarmonyExport(string name, int ordinal, IntPtr address)
		{
			Name = name;
			Ordinal = ordinal;
			Address = address;
		}

		private object _lastDelegate;

		public T AsDelegate<T>()
			where T : class // , delegate
		{
			Type delegateType = typeof(T);

			object lastDelegate = _lastDelegate;
			if (lastDelegate != null && lastDelegate.GetType() == delegateType)
				return (T)lastDelegate;

			if (!typeof(Delegate).IsAssignableFrom(delegateType))
				throw new ArgumentException("The generic type T must be a delegate Kind.", "T");

			Delegate d = Marshal.GetDelegateForFunctionPointer(Address, typeof(T));
			_lastDelegate = d;
			return (T)(object)d;
		}

		public override string ToString()
		{
			return string.Format("{0} (ordinal {1}): 0x{2:X8}", Name, Ordinal, (long)Address);
		}
	}
}

// Harmony/Libraries/HarmonyImport.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;

	public class HarmonyImport
	{
		public readonly int? Ordinal;
		public readonly string Name;
		public readonly IntPtr ProcAddress;

		public HarmonyImport(int? ordinal, string name, IntPtr procAddress)
		{
			Ordinal = ordinal;
			Name = name;
			ProcAddress = procAddress;
		}

		public override string ToString()
		{
			return string.Format("{0}: 0x{1:X8}", Name ?? Ordinal.GetValueOrDefault().ToString(), (ulong)ProcAddress);
		}
	}
}

// Harmony/Libraries/HarmonyImportLibrary.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class HarmonyImportLibrary
	{
		public readonly HarmonyImportLibraryKind Kind;
		public readonly string Name;
		public readonly IList<HarmonyImport> Imports;

		public HarmonyImportLibrary(HarmonyImportLibraryKind kind, string name, IEnumerable<HarmonyImport> imports)
		{
			Kind = kind;
			Name = name;
			Imports = Array.AsReadOnly(imports.ToArray());
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} imports", Name, Imports.Count);
		}
	}
}

// Harmony/Libraries/HarmonyImportLibraryKind.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	public enum HarmonyImportLibraryKind
	{
		ExternalDll,
		HarmonyLibrary,
	}
}

// Harmony/Libraries/HarmonyLibrary.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
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

// Harmony/Libraries/HarmonyLibraryKind.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	public enum HarmonyLibraryKind
	{
		Unknown = 0,
		Dll = 1,
		Exe = 2,
	}
}

// Harmony/Libraries/HarmonyLibrarySection.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;

	public sealed class HarmonyLibrarySection
	{
		public readonly string Name;
		public readonly UIntPtr BaseAddress;
		public readonly uint Size;

		public HarmonyLibrarySection(string name, UIntPtr baseAddress, uint size)
		{
			Name = name;
			BaseAddress = baseAddress;
			Size = size;
		}

		public override string ToString()
		{
			return string.Format("\"{0}\": at 0x{1:X8}, size 0x{2:X4}", Name, (ulong)BaseAddress, Size);
		}
	}
}

// Harmony/Libraries/HarmonyLoadFlags.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;

	[Flags]
	public enum HarmonyLoadFlags
	{
		/// <summary>
		/// If this flag is set, any imports this library is dependent on will not be resolved.
		/// </summary>
		NoImports = (1 << 0),

		/// <summary>
		/// If this flag is set, then when imports are resolved, only the provided HarmonyLibraries
		/// will be used; any external DLLs will be ignored/skipped.
		/// </summary>
		PrivateImports = (1 << 1),

		/// <summary>
		/// If this flag is set, then the library's DLL_PROCESS_ATTACH handlers will not be called
		/// when the library is loaded.
		/// </summary>
		NoAttach = (1 << 2),

		/// <summary>
		/// If this flag is set, then the library's DLL_PROCESS_DETACH handlers will not be called
		/// when the library is Disposed/garbage-collected.
		/// </summary>
		NoDetach = (1 << 3),
	}
}

// Harmony/Libraries/Image.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony
{
	using System;

	using Win32.Structs.PortableExecutable;

	namespace Libraries
	{
		internal unsafe class Image
		{
			public byte* BasePtr;
			public uint Size;

			public Image(byte* basePtr, uint size)
			{
				BasePtr = basePtr;
				Size = size;
			}

			public IMAGE_NT_HEADERS32* Headers
			{
				get { return (IMAGE_NT_HEADERS32*)(BasePtr + ((IMAGE_DOS_HEADER*)BasePtr)->e_lfanew); }
			}

			public IMAGE_SECTION_HEADER* FirstSection
			{
				get
				{
					const int OptionalHeaderOffset = 24; // In IMAGE_NT_HEADERS, this is the OptionalHeader's FieldOffset.
					return (IMAGE_SECTION_HEADER*)((byte*)Headers + OptionalHeaderOffset + Headers->FileHeader.SizeOfOptionalHeader);
				}
			}

			public override string ToString()
			{
				return string.Format("base 0x{0:X8}, size 0x{1:X8}", (long)(IntPtr)BasePtr, Size);
			}
		}
	}
}

// Harmony/Libraries/LibraryLoader.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
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

						bool isDll = (cookedImage.Headers->FileHeader.Characteristics & ImageFileHeaderCharacteristics.IsDLL) != 0;

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
							imageSize: cookedImage.Headers->OptionalHeader.SizeOfImage,
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

				uint cookedSize = RoundAddressUp(image.Headers->OptionalHeader.SizeOfImage, _pageSize);
				if (cookedSize != RoundAddressUp(lastSectionEnd, _pageSize))
					throw new LoadFailedException("This DLL is damaged or invalid; its end does not match the system page size.");

				byte* cookedBase = (byte*)Kernel32.VirtualAlloc((UIntPtr)0, (UIntPtr)cookedSize,
					AllocationType.RESERVE | AllocationType.COMMIT, MemoryProtection.EXECUTE_READWRITE);

				return new Image(cookedBase, cookedSize);
			}

			private static uint FindEndOfLastSection(Image image)
			{
				IMAGE_SECTION_HEADER* section = image.FirstSection;
				uint optionalSectionSize = image.Headers->OptionalHeader.SectionAlignment;
				uint lastSectionEnd = 0;

				for (int i = 0; i < image.Headers->FileHeader.NumberOfSections; i++, section++)
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

				IMAGE_DATA_DIRECTORY directory = image.Headers->OptionalHeader.ExportTable;
				if (directory.VirtualAddress == 0)
					return new ExportDictionaries(exportsByName, exportsByOrdinal);

				IMAGE_EXPORT_DIRECTORY* exportDirectory = (IMAGE_EXPORT_DIRECTORY*)(image.BasePtr + directory.VirtualAddress);
				UIntPtr nameRef = (UIntPtr)(image.BasePtr + exportDirectory->AddressOfNames);
				UIntPtr ordinalRef = (UIntPtr)(image.BasePtr + exportDirectory->AddressOfNameOrdinals);

				for (int i = 0; i < exportDirectory->NumberOfNames; i++, nameRef += sizeof(UIntPtr), ordinalRef += sizeof(UInt16))
				{
					byte* namePtr = image.BasePtr + (long)*(UIntPtr*)nameRef;
					string name = StringOperations.NulTerminatedBytesToString(namePtr, image.BasePtr, image.Size);
					ushort ordinal = *(UInt16*)ordinalRef;

					IntPtr exportAddress = (IntPtr)(image.BasePtr + (long)*(IntPtr*)(image.BasePtr + exportDirectory->AddressOfFunctions + ordinal * 4));

					HarmonyExport export = new HarmonyExport(name, ordinal, exportAddress);
					exportsByName[name] = export;
					exportsByOrdinal[ordinal] = export;
				}

				return new ExportDictionaries(exportsByName, exportsByOrdinal);
			}

			#endregion

			#region Startup/shutdown function execution

			public void ExecuteDllMain(Image image, DllCallType callType)
			{
				uint addressOfEntryPoint = image.Headers->OptionalHeader.AddressOfEntryPoint;
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
				uint addressOfEntryPoint = image.Headers->OptionalHeader.AddressOfEntryPoint;
				if (addressOfEntryPoint >= image.Size - sizeof(IntPtr))
					throw new LoadFailedException("Cannot invoke executable's Main() function; its address is invalid.");

				IntPtr callback = (IntPtr)(image.BasePtr + addressOfEntryPoint);
				ExeEntryProc exeEntryProc = (ExeEntryProc)Marshal.GetDelegateForFunctionPointer(callback, typeof(ExeEntryProc));

				exeEntryProc();
			}

			public void ExecuteTlsFunctions(Image image, DllCallType callType)
			{
				IMAGE_DATA_DIRECTORY directory = image.Headers->OptionalHeader.TLSTable;
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

			private static uint GetEffectiveSectionSize(IMAGE_NT_HEADERS32* ntHeaders32, IMAGE_SECTION_HEADER* section)
			{
				if (section->SizeOfRawData != 0)
					return section->SizeOfRawData;

				if ((section->Characteristics & DataSectionFlags.ContentInitializedData) != 0)
					return ntHeaders32->OptionalHeader.SizeOfInitializedData;

				if ((section->Characteristics & DataSectionFlags.ContentUninitializedData) != 0)
					return ntHeaders32->OptionalHeader.SizeOfUninitializedData;

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

				for (int i = 0; i < image.Headers->FileHeader.NumberOfSections; i++, section++)
				{
					UIntPtr sectionAddress = (UIntPtr)image.BasePtr + (int)section->VirtualAddress;
					UIntPtr alignedAddress = RoundAddressDown(sectionAddress, (int)_pageSize);
					uint sectionSize = GetEffectiveSectionSize(image.Headers, section);

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

				IMAGE_NT_HEADERS32* headers = image.Headers;
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
				Kernel32.MoveMemory((IntPtr)destImage.BasePtr, (IntPtr)srcImage.BasePtr, (IntPtr)srcImage.Headers->OptionalHeader.SizeOfHeaders);

				// Copy all the sections, verbatim.
				IEnumerable<HarmonyLibrarySection> sections = CopySections(srcImage, destImage);

				// Apply relocations to make the data correct in its new home.
				PerformBaseRelocation(destImage, (long)destImage.BasePtr - destImage.Headers->OptionalHeader.ImageBase);

				return sections;
			}

			private IEnumerable<HarmonyLibrarySection> CopySections(Image srcImage, Image destImage)
			{
				List<HarmonyLibrarySection> resultSections = new List<HarmonyLibrarySection>();

				IMAGE_SECTION_HEADER* section = destImage.FirstSection;
				for (int i = 0; i < destImage.Headers->FileHeader.NumberOfSections; i++, section++)
				{
					byte* dest = destImage.BasePtr + section->VirtualAddress;
					uint sectionSize;

					if (section->SizeOfRawData == 0)
					{
						sectionSize = destImage.Headers->OptionalHeader.SectionAlignment;
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
				IMAGE_DATA_DIRECTORY directory = image.Headers->OptionalHeader.BaseRelocationTable;
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
				IMAGE_DATA_DIRECTORY directory = image.Headers->OptionalHeader.ImportTable;
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

				bool is64BitMode = false;
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

// Harmony/Libraries/LoadFailedException.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;

	public class LoadFailedException : Exception
	{
		internal LoadFailedException(string message)
			: base(message)
		{
		}

		internal LoadFailedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}

// Harmony/Libraries/StreamCollector.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Libraries
{
	using System;
	using System.IO;

	internal class StreamCollector
	{
		private const int DefaultSize = 4096;

		public byte[] Bytes = new byte[DefaultSize];

		public int Length { get; private set; }

		public void Read(Stream stream)
		{
			for (;;)
			{
				int numDesired = Bytes.Length - Length;
				if (numDesired <= 0)
				{
					byte[] newBytes = new byte[Bytes.Length * 2];
					Buffer.BlockCopy(Bytes, 0, newBytes, 0, Length);
					Bytes = newBytes;
					numDesired = Bytes.Length - Length;
				}

				int numRead = stream.Read(Bytes, Length, numDesired);
				if (numRead <= 0) break;

				Length += numRead;
			}
		}
	}
}

// Harmony/Raw/StringOperations.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Raw
{
	internal static unsafe class StringOperations
	{
		public static string NulTerminatedBytesToString(byte* stringPointer, byte* basePtr, uint baseEnd)
		{
			uint stringLength = StrLen(stringPointer, basePtr, baseEnd);
			char[] stringChars = new char[stringLength];

			fixed (char* destBase = stringChars)
			{
				char* dest = destBase;
				byte* src = stringPointer;
				uint unrollCount = stringLength >> 3;

				while (unrollCount-- != 0)
				{
					dest[0] = (char)src[0];
					dest[1] = (char)src[1];
					dest[2] = (char)src[2];
					dest[3] = (char)src[3];
					dest[4] = (char)src[4];
					dest[5] = (char)src[5];
					dest[6] = (char)src[6];
					dest[7] = (char)src[7];
					dest += 8;
					src += 8;
				}

				// Unroll the rest of the short copies.
				switch (stringLength & 7)
				{
					case 7: *dest++ = (char)*src++; goto case 6;
					case 6: *dest++ = (char)*src++; goto case 5;
					case 5: *dest++ = (char)*src++; goto case 4;
					case 4: *dest++ = (char)*src++; goto case 3;
					case 3: *dest++ = (char)*src++; goto case 2;
					case 2: *dest++ = (char)*src++; goto case 1;
					case 1: *dest = (char)*src; goto case 0;
					case 0:
						break;
				}
			}

			return new string(stringChars);
		}

		private static uint StrLen(byte* strPtr, byte* basePtr, uint baseEnd)
		{
			byte* end = basePtr + baseEnd;
			byte* ptr = strPtr;

			while (ptr < end && *ptr != '\0') ptr++;

			return (uint)(ptr - strPtr);
		}
	}
}

// Harmony/Reflection/HarmonyClass.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony
{
	using System;

	using Libraries;

	namespace Reflection
	{
		public class HarmonyClass
		{
			public static T Create<T>(HarmonyLibrary library)
				where T : class, IDisposable
			{
				if (!typeof(T).IsInterface)
					throw new ArgumentException("The generic T used for a Harmony class must be an interface.");

				return null;
			}
		}
	}
}

// Harmony/Win32/Delegates/DllEntryProc.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;

	namespace Delegates
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate uint DllEntryProc(IntPtr hinstDll, DllCallType fdwReason, IntPtr reserved);
	}
}

// Harmony/Win32/Delegates/ExeEntryProc.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System.Runtime.InteropServices;

	namespace Delegates
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate int ExeEntryProc();
	}
}

// Harmony/Win32/Enums/AllocationType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	using System;

	[Flags]
	internal enum AllocationType : uint
	{
		COMMIT = 0x1000,
		RESERVE = 0x2000,
		RESET = 0x80000,
		LARGE_PAGES = 0x20000000,
		PHYSICAL = 0x400000,
		TOP_DOWN = 0x100000,
		WRITE_WATCH = 0x200000
	}
}

// Harmony/Win32/Enums/DataSectionFlags.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	using System;

	[Flags]
	internal enum DataSectionFlags : uint
	{
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeReg = 0x00000000,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeDsect = 0x00000001,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeNoLoad = 0x00000002,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeGroup = 0x00000004,
		/// <summary>
		/// The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES. This is valid only for object files.
		/// </summary>
		TypeNoPadded = 0x00000008,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeCopy = 0x00000010,
		/// <summary>
		/// The section contains executable code.
		/// </summary>
		ContentCode = 0x00000020,
		/// <summary>
		/// The section contains initialized data.
		/// </summary>
		ContentInitializedData = 0x00000040,
		/// <summary>
		/// The section contains uninitialized data.
		/// </summary>
		ContentUninitializedData = 0x00000080,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		LinkOther = 0x00000100,
		/// <summary>
		/// The section contains comments or other information. The .drectve section has this type. This is valid for object files only.
		/// </summary>
		LinkInfo = 0x00000200,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		TypeOver = 0x00000400,
		/// <summary>
		/// The section will not become part of the image. This is valid only for object files.
		/// </summary>
		LinkRemove = 0x00000800,
		/// <summary>
		/// The section contains COMDAT data. For more information, see section 5.5.6, COMDAT Sections (Object Only). This is valid only for object files.
		/// </summary>
		LinkComDat = 0x00001000,
		/// <summary>
		/// Reset speculative exceptions handling bits in the TLB entries for this section.
		/// </summary>
		NoDeferSpecExceptions = 0x00004000,
		/// <summary>
		/// The section contains data referenced through the global pointer (GP).
		/// </summary>
		RelativeGP = 0x00008000,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		MemPurgeable = 0x00020000,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		Memory16Bit = 0x00020000,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		MemoryLocked = 0x00040000,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		MemoryPreload = 0x00080000,
		/// <summary>
		/// Align data on a 1-byte boundary. Valid only for object files.
		/// </summary>
		Align1Bytes = 0x00100000,
		/// <summary>
		/// Align data on a 2-byte boundary. Valid only for object files.
		/// </summary>
		Align2Bytes = 0x00200000,
		/// <summary>
		/// Align data on a 4-byte boundary. Valid only for object files.
		/// </summary>
		Align4Bytes = 0x00300000,
		/// <summary>
		/// Align data on an 8-byte boundary. Valid only for object files.
		/// </summary>
		Align8Bytes = 0x00400000,
		/// <summary>
		/// Align data on a 16-byte boundary. Valid only for object files.
		/// </summary>
		Align16Bytes = 0x00500000,
		/// <summary>
		/// Align data on a 32-byte boundary. Valid only for object files.
		/// </summary>
		Align32Bytes = 0x00600000,
		/// <summary>
		/// Align data on a 64-byte boundary. Valid only for object files.
		/// </summary>
		Align64Bytes = 0x00700000,
		/// <summary>
		/// Align data on a 128-byte boundary. Valid only for object files.
		/// </summary>
		Align128Bytes = 0x00800000,
		/// <summary>
		/// Align data on a 256-byte boundary. Valid only for object files.
		/// </summary>
		Align256Bytes = 0x00900000,
		/// <summary>
		/// Align data on a 512-byte boundary. Valid only for object files.
		/// </summary>
		Align512Bytes = 0x00A00000,
		/// <summary>
		/// Align data on a 1024-byte boundary. Valid only for object files.
		/// </summary>
		Align1024Bytes = 0x00B00000,
		/// <summary>
		/// Align data on a 2048-byte boundary. Valid only for object files.
		/// </summary>
		Align2048Bytes = 0x00C00000,
		/// <summary>
		/// Align data on a 4096-byte boundary. Valid only for object files.
		/// </summary>
		Align4096Bytes = 0x00D00000,
		/// <summary>
		/// Align data on an 8192-byte boundary. Valid only for object files.
		/// </summary>
		Align8192Bytes = 0x00E00000,
		/// <summary>
		/// The section contains extended relocations.
		/// </summary>
		LinkExtendedRelocationOverflow = 0x01000000,
		/// <summary>
		/// The section can be discarded as needed.
		/// </summary>
		MemoryDiscardable = 0x02000000,
		/// <summary>
		/// The section cannot be cached.
		/// </summary>
		MemoryNotCached = 0x04000000,
		/// <summary>
		/// The section is not pageable.
		/// </summary>
		MemoryNotPaged = 0x08000000,
		/// <summary>
		/// The section can be shared in memory.
		/// </summary>
		MemoryShared = 0x10000000,
		/// <summary>
		/// The section can be executed as code.
		/// </summary>
		MemoryExecute = 0x20000000,
		/// <summary>
		/// The section can be read.
		/// </summary>
		MemoryRead = 0x40000000,
		/// <summary>
		/// The section can be written to.
		/// </summary>
		MemoryWrite = 0x80000000
	}
}

// Harmony/Win32/Enums/DllCallType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum DllCallType : uint
	{
		PROCESS_DETACH = 0,
		PROCESS_ATTACH = 1,
		THREAD_ATTACH = 2,
		THREAD_DETACH = 3,
	}
}

// Harmony/Win32/Enums/DllCharacteristicsType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	using System;

	[Flags]
	internal enum DllCharacteristicsType : ushort
	{
		RES_0 = 0x0001,
		RES_1 = 0x0002,
		RES_2 = 0x0004,
		RES_3 = 0x0008,
		IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
		IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
		IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
		IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
		IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
		IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
		RES_4 = 0x1000,
		IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
		IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
	}
}

// Harmony/Win32/Enums/FreeType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum FreeType : uint
	{
		DECOMMIT = 0x4000,
		RELEASE = 0x8000,
	}
}

// Harmony/Win32/Enums/ImageFileHeaderCharacteristics.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	using System;

	[Flags]
	internal enum ImageFileHeaderCharacteristics : ushort
	{
		RelocationInformationStrippedFromFile = 0x1,
		Executable = 0x2,
		LineNumbersStripped = 0x4,
		SymbolTableStripped = 0x8,
		AggresiveTrimWorkingSet = 0x10,
		LargeAddressAware = 0x20,
		Supports16Bit = 0x40,
		ReservedBytesWo = 0x80,
		Supports32Bit = 0x100,
		DebugInfoStripped = 0x200,
		RunFromSwapIfInRemovableMedia = 0x400,
		RunFromSwapIfInNetworkMedia = 0x800,
		IsSytemFile = 0x1000,
		IsDLL = 0x2000,
		IsOnlyForSingleCoreProcessor = 0x4000,
		BytesOfWordReserved = 0x8000,
	}
}

// Harmony/Win32/Enums/MachineType.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum MachineType : ushort
	{
		Native = 0,
		I386 = 0x014c,
		Itanium = 0x0200,
		x64 = 0x8664
	}
}

// Harmony/Win32/Enums/MagicType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum MagicType : ushort
	{
		IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
		IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
	}
}

// Harmony/Win32/Enums/MemoryProtection.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	using System;

	[Flags]
	internal enum MemoryProtection : uint
	{
		EXECUTE = 0x10,
		EXECUTE_READ = 0x20,
		EXECUTE_READWRITE = 0x40,
		EXECUTE_WRITECOPY = 0x80,
		NOACCESS = 0x01,
		READONLY = 0x02,
		READWRITE = 0x04,
		WRITECOPY = 0x08,
		GUARD_Modifierflag = 0x100,
		NOCACHE_Modifierflag = 0x200,
		WRITECOMBINE_Modifierflag = 0x400
	}
}

// Harmony/Win32/Enums/ProcessorArchitecture.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum ProcessorArchitecture : ushort
	{
		INTEL = 0,
		IA64 = 6,
		AMD64 = 9,
		UNKNOWN = 0xFFFF,
	}
}

// Harmony/Win32/Enums/Protection.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum Protection
	{
		PAGE_NOACCESS = 0x01,
		PAGE_READONLY = 0x02,
		PAGE_READWRITE = 0x04,
		PAGE_WRITECOPY = 0x08,
		PAGE_EXECUTE = 0x10,
		PAGE_EXECUTE_READ = 0x20,
		PAGE_EXECUTE_READWRITE = 0x40,
		PAGE_EXECUTE_WRITECOPY = 0x80,
		PAGE_GUARD = 0x100,
		PAGE_NOCACHE = 0x200,
		PAGE_WRITECOMBINE = 0x400
	}
}

// Harmony/Win32/Enums/RelocationType.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum RelocationType : byte
	{
		Absolute = 0,
		High = 1,
		Low = 2,
		HighLow = 3,
		HighAdj = 4,
		MIPS_JmpAddr = 5,
		Section = 6,
		Rel32 = 7,
		MIPS_JmpAddr16 = 9,
		IA64_Imm64 = 9,
		Dir64 = 10,
		High3Adj = 11,
	}
}

// Harmony/Win32/Enums/SubSystemType.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Enums
{
	internal enum SubSystemType : ushort
	{
		IMAGE_SUBSYSTEM_UNKNOWN = 0,
		IMAGE_SUBSYSTEM_NATIVE = 1,
		IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
		IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
		IMAGE_SUBSYSTEM_POSIX_CUI = 7,
		IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
		IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
		IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
		IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
		IMAGE_SUBSYSTEM_EFI_ROM = 13,
		IMAGE_SUBSYSTEM_XBOX = 14
	}
}

// Harmony/Win32/Kernel32.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;
	using Structs;

	internal static class Kernel32
	{
		[DllImport("kernel32.dll")]
		public static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern UIntPtr VirtualAlloc(UIntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualFree(UIntPtr lpAddress, UIntPtr dwSize, FreeType dwFreeType);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualProtect(UIntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

		[DllImport("kernel32.dll")]
		public static extern UIntPtr VirtualQuery(UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, UIntPtr dwLength);

		public static MEMORY_BASIC_INFORMATION VirtualQuery(UIntPtr lpAddress)
		{
			MEMORY_BASIC_INFORMATION memoryBasicInformation;
			if (VirtualQuery(lpAddress, out memoryBasicInformation, (UIntPtr)0x1000) == UIntPtr.Zero)
				return new MEMORY_BASIC_INFORMATION();
			return memoryBasicInformation;
		}

		[DllImport("kernel32.dll")]
		public static extern bool IsBadReadPtr(UIntPtr lpAddress, uint ucb);

		[DllImport("kernel32.dll")]
		public static extern bool IsBadWritePtr(UIntPtr lpAddress, uint ucb);

		[DllImport("kernel32.dll")]
		public static extern bool IsBadCodePtr(UIntPtr lpAddress);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetProcAddress")]
		public static extern IntPtr GetProcAddressOrdinal(IntPtr hModule, IntPtr procName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

		[DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
		public static extern void ZeroMemory(IntPtr dest, IntPtr size);

		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
		public static extern void MoveMemory(IntPtr dest, IntPtr src, IntPtr size);
	}
}

// Harmony/Win32/Structs/MEMORY_BASIC_INFORMATION.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	internal struct MEMORY_BASIC_INFORMATION
	{
		public UIntPtr BaseAddress;
		public UIntPtr AllocationBase;
		public uint AllocationProtect;
		public UIntPtr RegionSize;
		public uint State;
		public uint Protect;
		public uint Type;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_BASE_RELOCATION.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;
	
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct IMAGE_BASE_RELOCATION
	{
		public UInt32 VirtualAddress;
		public UInt32 BlockSizeInclusive;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_DATA_DIRECTORY.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	internal struct IMAGE_DATA_DIRECTORY
	{
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_DOS_HEADER.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;
	
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal unsafe struct IMAGE_DOS_HEADER
	{
		public fixed byte e_magic_byte[2];       // Magic number
		public UInt16 e_cblp;    // Bytes on last page of file
		public UInt16 e_cp;      // Pages in file
		public UInt16 e_crlc;    // Relocations
		public UInt16 e_cparhdr;     // Size of header in paragraphs
		public UInt16 e_minalloc;    // Minimum extra paragraphs needed
		public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
		public UInt16 e_ss;      // Initial (relative) SS value
		public UInt16 e_sp;      // Initial SP value
		public UInt16 e_csum;    // Checksum
		public UInt16 e_ip;      // Initial IP value
		public UInt16 e_cs;      // Initial (relative) CS value
		public UInt16 e_lfarlc;      // File address of relocation table
		public UInt16 e_ovno;    // Overlay number
		public fixed UInt16 e_res1[4];    // Reserved words
		public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
		public UInt16 e_oeminfo;     // OEM information; e_oemid specific
		public fixed UInt16 e_res2[10];    // Reserved words
		public Int32 e_lfanew;      // File address of new exe header
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_EXPORT_DIRECTORY.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;
	
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct IMAGE_EXPORT_DIRECTORY
	{
		public UInt32 Characteristics;
		public UInt32 TimeDateStamp;
		public UInt16 MajorVersion;
		public UInt16 MinorVersion;
		public UInt32 Name;
		public UInt32 Base;
		public UInt32 NumberOfFunctions;
		public UInt32 NumberOfNames;
		public UInt32 AddressOfFunctions;
		public UInt32 AddressOfNames;
		public UInt32 AddressOfNameOrdinals;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_FILE_HEADER.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;

	namespace Structs.PortableExecutable
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct IMAGE_FILE_HEADER
		{
			public MachineType Machine;
			public UInt16 NumberOfSections;
			public UInt32 TimeDateStamp;
			public UInt32 PointerToSymbolTable;
			public UInt32 NumberOfSymbols;
			public UInt16 SizeOfOptionalHeader;
			public ImageFileHeaderCharacteristics Characteristics;
		}
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_IMPORT_BY_NAME.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct IMAGE_IMPORT_BY_NAME
	{
		[FieldOffset(0)]
		public UInt16 Hint;

		[FieldOffset(2)]
		public fixed byte Name[1];
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_IMPORT_DESCRIPTOR.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	internal struct IMAGE_IMPORT_DESCRIPTOR
	{
		[FieldOffset(0)]
		public UInt32 Characteristics;

		[FieldOffset(0)]
		public UInt32 OriginalFirstThunk;

		[FieldOffset(4)]
		public UInt32 DateTimeStamp;

		[FieldOffset(8)]
		public UInt32 ForwarderChain;

		[FieldOffset(12)]
		public UInt32 Name;

		[FieldOffset(16)]
		public UInt32 FirstThunk;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_NT_HEADERS32.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct IMAGE_NT_HEADERS32
	{
		[FieldOffset(0)]
		public fixed byte Signature[4];

		[FieldOffset(4)]
		public IMAGE_FILE_HEADER FileHeader;

		[FieldOffset(24)]
		public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_NT_HEADERS64.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct IMAGE_NT_HEADERS64
	{
		[FieldOffset(0)]
		public fixed byte Signature[4];

		[FieldOffset(4)]
		public IMAGE_FILE_HEADER FileHeader;

		[FieldOffset(24)]
		public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_OPTIONAL_HEADER32.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System.Runtime.InteropServices;

	using Enums;

	namespace Structs.PortableExecutable
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct IMAGE_OPTIONAL_HEADER32
		{
			[FieldOffset(0)]
			public MagicType Magic;

			[FieldOffset(2)]
			public byte MajorLinkerVersion;

			[FieldOffset(3)]
			public byte MinorLinkerVersion;

			[FieldOffset(4)]
			public uint SizeOfCode;

			[FieldOffset(8)]
			public uint SizeOfInitializedData;

			[FieldOffset(12)]
			public uint SizeOfUninitializedData;

			[FieldOffset(16)]
			public uint AddressOfEntryPoint;

			[FieldOffset(20)]
			public uint BaseOfCode;

			// PE32 contains this additional field
			[FieldOffset(24)]
			public uint BaseOfData;

			[FieldOffset(28)]
			public uint ImageBase;

			[FieldOffset(32)]
			public uint SectionAlignment;

			[FieldOffset(36)]
			public uint FileAlignment;

			[FieldOffset(40)]
			public ushort MajorOperatingSystemVersion;

			[FieldOffset(42)]
			public ushort MinorOperatingSystemVersion;

			[FieldOffset(44)]
			public ushort MajorImageVersion;

			[FieldOffset(46)]
			public ushort MinorImageVersion;

			[FieldOffset(48)]
			public ushort MajorSubsystemVersion;

			[FieldOffset(50)]
			public ushort MinorSubsystemVersion;

			[FieldOffset(52)]
			public uint Win32VersionValue;

			[FieldOffset(56)]
			public uint SizeOfImage;

			[FieldOffset(60)]
			public uint SizeOfHeaders;

			[FieldOffset(64)]
			public uint CheckSum;

			[FieldOffset(68)]
			public SubSystemType Subsystem;

			[FieldOffset(70)]
			public DllCharacteristicsType DllCharacteristics;

			[FieldOffset(72)]
			public uint SizeOfStackReserve;

			[FieldOffset(76)]
			public uint SizeOfStackCommit;

			[FieldOffset(80)]
			public uint SizeOfHeapReserve;

			[FieldOffset(84)]
			public uint SizeOfHeapCommit;

			[FieldOffset(88)]
			public uint LoaderFlags;

			[FieldOffset(92)]
			public uint NumberOfRvaAndSizes;

			[FieldOffset(96)]
			public IMAGE_DATA_DIRECTORY ExportTable;

			[FieldOffset(104)]
			public IMAGE_DATA_DIRECTORY ImportTable;

			[FieldOffset(112)]
			public IMAGE_DATA_DIRECTORY ResourceTable;

			[FieldOffset(120)]
			public IMAGE_DATA_DIRECTORY ExceptionTable;

			[FieldOffset(128)]
			public IMAGE_DATA_DIRECTORY CertificateTable;

			[FieldOffset(136)]
			public IMAGE_DATA_DIRECTORY BaseRelocationTable;

			[FieldOffset(144)]
			public IMAGE_DATA_DIRECTORY Debug;

			[FieldOffset(152)]
			public IMAGE_DATA_DIRECTORY Architecture;

			[FieldOffset(160)]
			public IMAGE_DATA_DIRECTORY GlobalPtr;

			[FieldOffset(168)]
			public IMAGE_DATA_DIRECTORY TLSTable;

			[FieldOffset(176)]
			public IMAGE_DATA_DIRECTORY LoadConfigTable;

			[FieldOffset(184)]
			public IMAGE_DATA_DIRECTORY BoundImport;

			[FieldOffset(192)]
			public IMAGE_DATA_DIRECTORY IAT;

			[FieldOffset(200)]
			public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

			[FieldOffset(208)]
			public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

			[FieldOffset(216)]
			public IMAGE_DATA_DIRECTORY Reserved;
		}
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_OPTIONAL_HEADER64.cs

//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System.Runtime.InteropServices;

	using Enums;

	namespace Structs.PortableExecutable
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct IMAGE_OPTIONAL_HEADER64
		{
			[FieldOffset(0)]
			public MagicType Magic;

			[FieldOffset(2)]
			public byte MajorLinkerVersion;

			[FieldOffset(3)]
			public byte MinorLinkerVersion;

			[FieldOffset(4)]
			public uint SizeOfCode;

			[FieldOffset(8)]
			public uint SizeOfInitializedData;

			[FieldOffset(12)]
			public uint SizeOfUninitializedData;

			[FieldOffset(16)]
			public uint AddressOfEntryPoint;

			[FieldOffset(20)]
			public uint BaseOfCode;

			[FieldOffset(24)]
			public ulong ImageBase;

			[FieldOffset(32)]
			public uint SectionAlignment;

			[FieldOffset(36)]
			public uint FileAlignment;

			[FieldOffset(40)]
			public ushort MajorOperatingSystemVersion;

			[FieldOffset(42)]
			public ushort MinorOperatingSystemVersion;

			[FieldOffset(44)]
			public ushort MajorImageVersion;

			[FieldOffset(46)]
			public ushort MinorImageVersion;

			[FieldOffset(48)]
			public ushort MajorSubsystemVersion;

			[FieldOffset(50)]
			public ushort MinorSubsystemVersion;

			[FieldOffset(52)]
			public uint Win32VersionValue;

			[FieldOffset(56)]
			public uint SizeOfImage;

			[FieldOffset(60)]
			public uint SizeOfHeaders;

			[FieldOffset(64)]
			public uint CheckSum;

			[FieldOffset(68)]
			public SubSystemType Subsystem;

			[FieldOffset(70)]
			public DllCharacteristicsType DllCharacteristics;

			[FieldOffset(72)]
			public ulong SizeOfStackReserve;

			[FieldOffset(80)]
			public ulong SizeOfStackCommit;

			[FieldOffset(88)]
			public ulong SizeOfHeapReserve;

			[FieldOffset(96)]
			public ulong SizeOfHeapCommit;

			[FieldOffset(104)]
			public uint LoaderFlags;

			[FieldOffset(108)]
			public uint NumberOfRvaAndSizes;

			[FieldOffset(112)]
			public IMAGE_DATA_DIRECTORY ExportTable;

			[FieldOffset(120)]
			public IMAGE_DATA_DIRECTORY ImportTable;

			[FieldOffset(128)]
			public IMAGE_DATA_DIRECTORY ResourceTable;

			[FieldOffset(136)]
			public IMAGE_DATA_DIRECTORY ExceptionTable;

			[FieldOffset(144)]
			public IMAGE_DATA_DIRECTORY CertificateTable;

			[FieldOffset(152)]
			public IMAGE_DATA_DIRECTORY BaseRelocationTable;

			[FieldOffset(160)]
			public IMAGE_DATA_DIRECTORY Debug;

			[FieldOffset(168)]
			public IMAGE_DATA_DIRECTORY Architecture;

			[FieldOffset(176)]
			public IMAGE_DATA_DIRECTORY GlobalPtr;

			[FieldOffset(184)]
			public IMAGE_DATA_DIRECTORY TLSTable;

			[FieldOffset(192)]
			public IMAGE_DATA_DIRECTORY LoadConfigTable;

			[FieldOffset(200)]
			public IMAGE_DATA_DIRECTORY BoundImport;

			[FieldOffset(208)]
			public IMAGE_DATA_DIRECTORY IAT;

			[FieldOffset(216)]
			public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

			[FieldOffset(224)]
			public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

			[FieldOffset(232)]
			public IMAGE_DATA_DIRECTORY Reserved;
		}
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_SECTION_HEADER.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;

	namespace Structs.PortableExecutable
	{
		[StructLayout(LayoutKind.Explicit)]
		internal unsafe struct IMAGE_SECTION_HEADER
		{
			[FieldOffset(0)]
			public fixed byte Name[8];

			[FieldOffset(8)]
			public UInt32 VirtualSize;

			[FieldOffset(12)]
			public UInt32 VirtualAddress;

			[FieldOffset(16)]
			public UInt32 SizeOfRawData;

			[FieldOffset(20)]
			public UInt32 PointerToRawData;

			[FieldOffset(24)]
			public UInt32 PointerToRelocations;

			[FieldOffset(28)]
			public UInt32 PointerToLinenumbers;

			[FieldOffset(32)]
			public UInt16 NumberOfRelocations;

			[FieldOffset(34)]
			public UInt16 NumberOfLinenumbers;

			[FieldOffset(36)]
			public DataSectionFlags Characteristics;

			public string Section
			{
				get
				{
					fixed (byte* name = Name)
					{
						char[] buffer = new char[8];

						buffer[0] = (char)name[0];
						buffer[1] = (char)name[1];
						buffer[2] = (char)name[2];
						buffer[3] = (char)name[3];
						buffer[4] = (char)name[4];
						buffer[5] = (char)name[5];
						buffer[6] = (char)name[6];
						buffer[7] = (char)name[7];

						int length = 8;
						while (length > 0 && buffer[length - 1] == '\0')
							length--;

						return length > 0 ? new string(buffer, 0, length) : string.Empty;
					}
				}
			}
		}
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_TLS_DIRECTORY32.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct IMAGE_TLS_DIRECTORY32
	{
		public UInt32 StartAddressOfRawData;
		public UInt32 EndAddressOfRawData;
		public UInt32 AddressOfIndex;
		public UInt32 AddressOfCallBacks;
		public UInt32 SizeOfZeroFill;
		public UInt32 Characteristics;
	}
}

// Harmony/Win32/Structs/PortableExecutable/IMAGE_TLS_DIRECTORY64.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32.Structs.PortableExecutable
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct IMAGE_TLS_DIRECTORY64
	{
		public UInt64 StartAddressOfRawData;
		public UInt64 EndAddressOfRawData;
		public UInt64 AddressOfIndex;
		public UInt64 AddressOfCallBacks;
		public UInt32 SizeOfZeroFill;
		public UInt32 Characteristics;
	}
}

// Harmony/Win32/Structs/SYSTEM_INFO.cs

﻿//----------------------------------------------------------------------------
// Harmony .NET
// A dynamic loader and linker library for integrating .NET and C
// Copyright (C) 2016 Sean Werkema
//
// This software is licensed under the terms of the open-source
// Apache License 2.0, a copy of which can be found in the file
// "LICENSE.txt" included with this software.
//----------------------------------------------------------------------------

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;

	namespace Structs
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct SYSTEM_INFO
		{
			public ProcessorArchitecture wProcessorArchitecture;
			public ushort wReserved;
			public uint dwPageSize;
			public IntPtr lpMinimumApplicationAddress;
			public IntPtr lpMaximumApplicationAddress;
			public UIntPtr dwActiveProcessorMask;
			public uint dwNumberOfProcessors;
			public uint dwProcessorType;
			public uint dwAllocationGranularity;
			public ushort wProcessorLevel;
			public ushort wProcessorRevision;
		}
	}
}
