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
