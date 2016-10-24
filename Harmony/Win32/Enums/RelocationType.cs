﻿//----------------------------------------------------------------------------
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
