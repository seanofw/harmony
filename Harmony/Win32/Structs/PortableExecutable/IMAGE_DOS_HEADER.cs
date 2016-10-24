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