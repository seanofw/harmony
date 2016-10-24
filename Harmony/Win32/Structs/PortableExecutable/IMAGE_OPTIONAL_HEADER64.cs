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