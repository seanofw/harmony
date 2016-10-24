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
	using System.Runtime.InteropServices;

	public class HarmonyExport
	{
		public readonly string Name;
		public readonly int Ordinal;
		public readonly IntPtr Address;

		public HarmonyExport(string name, int ordinal, IntPtr address)
		{
			Name = name;
			Ordinal = ordinal;
			Address = address;
		}

		private object _lastDelegate;

		public T AsDelegate<T>()
			where T : class // , delegate
		{
			Type delegateType = typeof(T);

			object lastDelegate = _lastDelegate;
			if (lastDelegate != null && lastDelegate.GetType() == delegateType)
				return (T)lastDelegate;

			if (!typeof(Delegate).IsAssignableFrom(delegateType))
				throw new ArgumentException("The generic type T must be a delegate Kind.", "T");

			Delegate d = Marshal.GetDelegateForFunctionPointer(Address, typeof(T));
			_lastDelegate = d;
			return (T)(object)d;
		}

		public override string ToString()
		{
			return string.Format("{0} (ordinal {1}): 0x{2:X8}", Name, Ordinal, (long)Address);
		}
	}
}
