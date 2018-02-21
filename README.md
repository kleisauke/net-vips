# Mono/.NET bindings for libvips

A quick & dirty pass at generating a [libvips](https://github.com/jcupitt/libvips) P/Invoke layer in C# using [CppSharp](https://github.com/mono/CppSharp).

## Instructions
1. Clone this repo.
2. Ensure that the path in NetVips.CLI/Program.cs points to the path where libvips was downloaded.
3. Build and run NetVips.CLI -- libvips.cs will be generated. Take this file and use it in the application that you'd like to use libvips in.
