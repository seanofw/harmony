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