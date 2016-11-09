# Harmony .NET

**A dynamic loader and linker library for integrating .NET and C**

**Copyright &copy; 2016 Sean Werkema**

-----------------------------------------------------------------------------

## Introduction

Harmony is a .NET library that allows you to easily load and unload unmanaged DLLs
within your managed .NET code.  It's like P/Invoke, but it's much more flexible:  Your
unmanaged DLLs can be stored as embedded resources, or stored as arbitrary byte arrays,
and loaded and unloaded on the fly, as needed.

Why would you want to do this?  Well, .NET is powerful, but it will never be as fast as
compiled C code.  But packaging an unmanaged C DLL with your managed C#/VB/F# DLL is often
tricky:  So the idea is to be able to store the unmanaged DLL _inside_ the managed DLL, so
that from the perspective of .NET callers, there is only managed code that appears to be
"magically fast," just like the built-in external .NET methods are.

Harmony supports both 32-bit and 64-bit .NET environments on Windows.

## Current Status

Harmony is currently in **Beta** status.  It works, but you should probably
not depend on it for production use (yet).

### Known Limitations

  - Harmony does not provide access to resources inside DLLs.
  - Harmony (likely) does not run under Mono.
  - Harmony does not support Windows side-by-side (SxS) linking.
  - Harmony does not support loading managed DLLs --- only unmanaged DLLs.
  - Harmony does not support mixed environments:  64-bit code must load 64-bit DLLs,
    and 32-bit code must load 32-bit DLLs.
  - Harmony cannot autodetect calling conventions:  You need to explicitly specify the
    calling convention, just like with P/Invoke.
  - Marshalling isn't as clean as it should be.

## Using Harmony

Harmony is simple and easy to get running.  Include the Harmony assembly (soon to be
a proper NuGet package), and then write just a couple of lines of code to transform any
byte array containing a DLL into a live, callable library:

```
byte[] bytes = ...get your DLL into a byte array... ;

using (HarmonyLibrary harmonyLibrary = HarmonyLibrary.CreateFromBytes(bytes))
{
    ...
}
```

There is a similar `CreateFromStream` method (in addition to `CreateFromBytes`), which is
useful if your library is stored as an embedded resource.

Notice the `using` block there:  `HarmonyLibrary` implements `IDisposable`, so when you're
done with your dynamic library, you can simply call `Dispose()` to unload it!  (That's right
--- using Harmony, you can actually unload a DLL in .NET!)

When your library is loaded, you have a few options at your disposal.  The easiest option
is to have a delegate declared that matches the name and shape of your C function, and then
to call `GetFunction<>()` to transform it into a real .NET callable delegate:

```
delegate int Add(int x, int y);

Add add = harmonyLibrary.GetFunction<Add>();
int sum = add(123, 456);
```

(You could also call `harmonyLibrary.GetProcAddress()`, which works like `GetProcAddress()`
does in Windows, but that leaves you with an `IntPtr` you then have to do something with.
The easiest way is to use the delegate form, above.)

### Marshalling Data

Harmony uses the standard [P/Invoke](https://msdn.microsoft.com/en-us/library/aa288468(v=vs.71).aspx)
conventions for marshalling data.  This means that simple numeric types (byte, short, int, long, float,
double, bool) all move back and forth pretty easily, while more complex types (arrays, structs, strings)
require a bit of effort.

An example for marshalling simple types is shown above.

If you wanted to marshal a byte array, you might use this:

```
delegate void VectorAdd(
    [MarshalAs(UnmanagedType.LPArray)]float[] x,
    [MarshalAs(UnmanagedType.LPArray)]float[] y,
    [MarshalAs(UnmanagedType.LPArray)]float[] sum,
    int length);

VectorAdd add = harmonyLibrary.GetFunction<VectorAdd>();

float[] x = ... ;
float[] y = ... ;
float[] sum = new float[128];
add(x, y, sum, 128);
```

Passing strings to C requires similar declarations:

```
delegate bool RegexMatch([MarshalAs(UnmanagedType.LPWStr)]string input,
    [MarshalAs(UnmanagedType.LPWStr)]string pattern);

RegexMatch regexMatch = harmonyLibrary.GetFunction<RegexMatch>();

if (regexMatch("Hello, World", "[a-z]")) { ... }
```

Passing strings back from C to C# requires a bit more effort.  The best solution is
to use the [pre-allocated StringBuilder technique](http://stackoverflow.com/questions/10856127/passing-string-from-native-c-dll-to-c-sharp-app):

```
delegate int Concat([MarshalAs(UnmanagedType.LPWStr)]string a,
    [MarshalAs(UnmanagedType.LPWStr)]string b,
    StringBuilder result,
    int resultMax);
    
StringBuilder stringBuilder = new StringBuilder(4096);
int length = Concat("Hello,", " World.", stringBuilder, stringBuilder.Length);
string result = stringBuilder.ToString(0, length);
```

### Calling Conventions

It's easy to get bitten by calling conventions in C.  There is only one calling convention
in C#, which is "whatever the JITter does."  C programs, on the other hand, have many calling
conventions to pick from: `cdecl`, `stdcall`, `fastcall`, `thiscall`, `vectorcall`, and more.

Unfortunately, there is no way for an external program to know which calling convention a
C library was built with.  Most C programs are compiled with the `cdecl` calling convention,
while Windows usually uses the `stdcall` calling convention for its APIs.

.NET (and, by extension, Harmony) defaults to assuming you're using the `stdcall` calling
convention.  If `stdcall` is incorrect, you have two choices:

  - **(A) You can change your C# code.**  Add a `[UnmanagedFunctionPointer(CallingConvention.Cdecl)]`
    attribute on your delegate to describe which calling convention you're using, like this:

```
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate uint Crc32([MarshalAs(UnmanagedType.LPArray)]byte[] sourceBytes,
        int offset, int length, uint crc);
```

  - **(B) You can change your C code.**  Add `__stdcall` or `__cdecl` (or whichever keyword
    you want) on your functions to change their calling convention, like this:

```
    __declspec(dllexport) __stdcall Crc32(const void *sourceBytes,
        int offset, int length, unsigned int crc) { ... }
```

Either way around, it is critical that you match calling conventions; a mismatched calling
convention will usually result in a crash the moment you try to call the function.

### HarmonyPacked.cs

In addition to the regular Harmony assembly/NuGet package, you could also just drop the file
`HarmonyPacked.cs` into your code.  `HarmonyPacked.cs` is automatically generated as part of
the standard Harmony build for people that want all of Harmony as a single C# source file.

#### Pros:

  - It's only one file total!
  - Your code has no external dependencies when you use it.
  - Installation is dirt-simple.
  - Your use of Harmony --- and your use of external C code --- is completely invisible to
    your code's callers:  Your code is just "magically fast C#."
  
#### Cons:

  - You don't get automatic updates the way you would with NuGet.
  - Debugging Harmony issues can be harder.
  - Only works with C# projects.
  - Visual Studio doesn't like opening source files with thousands of lines in them.
  - You'll have to mark your assembly as "unsafe" or it won't compile.

Either way, whether you choose to use the Harmony NuGet package (or DLL), or you choose
to use `HarmonyPacked.cs`, the exposed Harmony classes and methods are exactly the same.
  
## Documentation

Harmony does not expose a lot of methods to call; its interface is intentionally simple
and narrow.  The core methods on `HarmonyLibrary` are as follows:

### Construction & Destruction
  - `static CreateFromBytes(bytes)` - Construct a new `HarmonyLibrary` from the given array of bytes, which contain a DLL or executable.
  - `static CreateFromBytes(bytes, offset, length)` - Construct a new `HarmonyLibrary` from a subset of the given array of bytes, which contains a DLL or executable.
  - `static CreateFromStream(stream)` - Construct a new `HarmonyLibrary` from the given `Stream`, which contains a DLL or executable.
  - `Dispose()` - Release a `HarmonyLibrary` from memory, and all its resources.  Yes, you can actually unload a DLL with this.
  - `~HarmonyLibrary()` - If you forget to `Dispose()`, the destructor will automatically free the library's resources when the last reference goes away.  (But note that a delegate or `IntPtr` to the library does _not_ count as a reference!)

### Library Methods  
  - `GetProcAddress()` - Just like `GetProcAddress()` for a standard Windows DLL, this returns an `IntPtr` for the given named function.
  - `GetProcAddressOrdinal()` - Just like `GetProcAddress()`, but by ordinal instead of by name.
  - `GetFunction<T>()` - This is the main interface for retrieving functions from the loaded library.  It can optionally take a function name that's different from the delegate type name.

## Credits

Harmony was written by Sean Werkema in C# because it sounded like a really good idea.

Some parts of Harmony are loosely based on Joachim Bauch's excellent
[MemoryModule](https://github.com/fancycode/MemoryModule) library, which
provides a similar kind of concept to C programs.

## License

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

