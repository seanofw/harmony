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