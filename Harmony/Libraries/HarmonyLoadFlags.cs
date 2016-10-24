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

namespace Harmony.Libraries
{
	using System;

	[Flags]
	public enum HarmonyLoadFlags
	{
		/// <summary>
		/// If this flag is set, any imports this library is dependent on will not be resolved.
		/// </summary>
		NoImports = (1 << 0),

		/// <summary>
		/// If this flag is set, then when imports are resolved, only the provided HarmonyLibraries
		/// will be used; any external DLLs will be ignored/skipped.
		/// </summary>
		PrivateImports = (1 << 1),

		/// <summary>
		/// If this flag is set, then the library's DLL_PROCESS_ATTACH handlers will not be called
		/// when the library is loaded.
		/// </summary>
		NoAttach = (1 << 2),

		/// <summary>
		/// If this flag is set, then the library's DLL_PROCESS_DETACH handlers will not be called
		/// when the library is Disposed/garbage-collected.
		/// </summary>
		NoDetach = (1 << 3),
	}
}
