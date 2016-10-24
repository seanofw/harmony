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
	using System.IO;

	internal class StreamCollector
	{
		private const int DefaultSize = 4096;

		public byte[] Bytes = new byte[DefaultSize];

		public int Length { get; private set; }

		public void Read(Stream stream)
		{
			for (;;)
			{
				int numDesired = Bytes.Length - Length;
				if (numDesired <= 0)
				{
					byte[] newBytes = new byte[Bytes.Length * 2];
					Buffer.BlockCopy(Bytes, 0, newBytes, 0, Length);
					Bytes = newBytes;
					numDesired = Bytes.Length - Length;
				}

				int numRead = stream.Read(Bytes, Length, numDesired);
				if (numRead <= 0) break;

				Length += numRead;
			}
		}
	}
}
