using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Harmony.Libraries;

namespace Crc32Example
{
	class Program
	{
		// This is the shape of the CRC-32 function in the C library.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint Crc32([MarshalAs(UnmanagedType.LPArray)]byte[] sourceBytes, int offset, int length, uint crc);

		static void Main(string[] args)
		{
			// This assembly has a copy of Crc32.dll stored as an embedded resource, so retrieve that resource.
			Stream crc32Dll = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crc32Example.Crc32.dll");

			// Now make a HarmonyLibrary from that resource stream.
			using (HarmonyLibrary harmonyLibrary = HarmonyLibrary.CreateFromStream(crc32Dll))
			{
				// Get the Crc32 function from the HarmonyLibrary.
				Crc32 method = harmonyLibrary.GetFunction<Crc32>();

				// We're going to perform a CRC32 on this string.
				string sourceString = "This is a test.";	// Should have a CRC-32 of 0xC6C3C95D.
				Console.WriteLine(sourceString);

				// Do the CRC-32, using the compiled C library.
				uint crc32 = method(Encoding.ASCII.GetBytes(sourceString), 0, sourceString.Length, 0);
				Console.WriteLine("CRC-32: 0x{0:X8}", crc32);
			}
		}
	}
}
