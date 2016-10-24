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
	internal enum ImageFileHeaderCharacteristics : ushort
	{
		RelocationInformationStrippedFromFile = 0x1,
		Executable = 0x2,
		LineNumbersStripped = 0x4,
		SymbolTableStripped = 0x8,
		AggresiveTrimWorkingSet = 0x10,
		LargeAddressAware = 0x20,
		Supports16Bit = 0x40,
		ReservedBytesWo = 0x80,
		Supports32Bit = 0x100,
		DebugInfoStripped = 0x200,
		RunFromSwapIfInRemovableMedia = 0x400,
		RunFromSwapIfInNetworkMedia = 0x800,
		IsSytemFile = 0x1000,
		IsDLL = 0x2000,
		IsOnlyForSingleCoreProcessor = 0x4000,
		BytesOfWordReserved = 0x8000,
	}
}