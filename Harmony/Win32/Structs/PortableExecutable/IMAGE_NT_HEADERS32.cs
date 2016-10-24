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