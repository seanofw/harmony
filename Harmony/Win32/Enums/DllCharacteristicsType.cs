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