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