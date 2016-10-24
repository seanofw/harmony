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
