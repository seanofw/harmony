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

namespace Harmony.Win32
{
	using System;
	using System.Runtime.InteropServices;

	using Enums;

	namespace Delegates
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate uint DllEntryProc(IntPtr hinstDll, DllCallType fdwReason, IntPtr reserved);
	}
}