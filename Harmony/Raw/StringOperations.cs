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

namespace Harmony.Raw
{
	internal static unsafe class StringOperations
	{
		public static string NulTerminatedBytesToString(byte* stringPointer, byte* basePtr, uint baseEnd)
		{
			uint stringLength = StrLen(stringPointer, basePtr, baseEnd);
			char[] stringChars = new char[stringLength];

			fixed (char* destBase = stringChars)
			{
				char* dest = destBase;
				byte* src = stringPointer;
				uint unrollCount = stringLength >> 3;

				while (unrollCount-- != 0)
				{
					dest[0] = (char)src[0];
					dest[1] = (char)src[1];
					dest[2] = (char)src[2];
					dest[3] = (char)src[3];
					dest[4] = (char)src[4];
					dest[5] = (char)src[5];
					dest[6] = (char)src[6];
					dest[7] = (char)src[7];
					dest += 8;
					src += 8;
				}

				// Unroll the rest of the short copies.
				switch (stringLength & 7)
				{
					case 7: *dest++ = (char)*src++; goto case 6;
					case 6: *dest++ = (char)*src++; goto case 5;
					case 5: *dest++ = (char)*src++; goto case 4;
					case 4: *dest++ = (char)*src++; goto case 3;
					case 3: *dest++ = (char)*src++; goto case 2;
					case 2: *dest++ = (char)*src++; goto case 1;
					case 1: *dest = (char)*src; goto case 0;
					case 0:
						break;
				}
			}

			return new string(stringChars);
		}

		private static uint StrLen(byte* strPtr, byte* basePtr, uint baseEnd)
		{
			byte* end = basePtr + baseEnd;
			byte* ptr = strPtr;

			while (ptr < end && *ptr != '\0') ptr++;

			return (uint)(ptr - strPtr);
		}
	}
}
