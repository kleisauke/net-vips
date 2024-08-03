# Changelog
All notable changes to NetVips will be documented in this file. See [here](CHANGELOG.native.md) for the changes to the [pre-compiled binaries of libvips](https://www.nuget.org/packages/NetVips.Native/).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.4.1] - 2024-03-24
### Fixed
- Avoid key collision during shared `VOption` merge ([#228](https://github.com/kleisauke/net-vips/issues/228)).

## [2.4.0] - 2023-11-12
### Changed
- Update methods/enums for libvips 8.15.

### Fixed
- Ensure correct calling convention for unmanaged-to-managed callbacks.

## [2.3.1] - 2023-06-29
### Fixed
- Ensure `Image.FindLoad()` works on UTF-8 strings ([#210](https://github.com/kleisauke/net-vips/issues/210)).

## [2.3.0] - 2023-03-24
### Added
- Add IntPtr-based overload for `Image.NewFromMemory()` ([#177](https://github.com/kleisauke/net-vips/issues/177)).
- Add `image.Invalidate()` to drop caches on an image, and any downstream images.
- Add `NetVips.Shutdown()` to finalize the internal leak checker and profiler.

### Changed
- Update methods/enums for libvips 8.14.
- The [NetVips.Extensions](https://www.nuget.org/packages/NetVips.Extensions/) package is now only supported on Windows when targeting .NET 6.0 or higher. See https://aka.ms/systemdrawingnonwindows for more information.

### Fixed
- Ensure compatibility with FreeBSD and variants.
- Ensure code is AOT-friendly ([#196](https://github.com/kleisauke/net-vips/issues/196)).

## [2.2.0] - 2022-07-25
### Added
- Add `NetVips.BlockUntrusted()` and `Operation.Block()` for blocking operations at runtime.
- Add `image.SignalHandlersDisconnectByFunc()` and `image.SignalHandlersDisconnectByData()` for disconnecting signal handlers that match.
- Implement `image.OnPostClose` remove event accessor.

### Changed
- Update methods/enums for libvips 8.13.
- Remove internal `VipsSaveable` enum.
- Avoid throwing general exceptions.
- Ensure debug and unit tests functions are internal.

### Fixed
- Use the correct type for signal handler IDs.

## [2.1.0] - 2021-12-02
### Added
- Add `image.SignalHandlerDisconnect()` for disconnecting a signal handler.

### Changed
- Update methods/enums for libvips 8.12.
- Drop internal `ModuleInit.Fody` dependency in favor of the `[ModuleInitializer]` attribute.
- `image.WriteToBuffer()` tries to use the new target API first.
- Bump minimum required .NET Framework version to v4.5.2.
- The [NetVips.Extensions](https://www.nuget.org/packages/NetVips.Extensions/) package is now attributed as a Windows-specific library when targeting .NET 6.0 or higher. See https://aka.ms/systemdrawingnonwindows for more information.

### Fixed
- Ensure recorded delegates are not released too early ([#141](https://github.com/kleisauke/net-vips/issues/141)).

## [2.0.1] - 2021-06-23
### Changed
- Update methods/enums for libvips 8.11.
- Avoid using `Span<T>` throughout the codebase ([#131](https://github.com/kleisauke/net-vips/issues/131)).

### Fixed
- Ensure strings are null-terminated ([#131](https://github.com/kleisauke/net-vips/issues/131)).

## [2.0.0] - 2021-03-30
### Added
- Expose "speed" parameter in heifsave to control the CPU effort spent on improving compression (applies to AV1 compression only, see [libvips/libvips#1819](https://github.com/libvips/libvips/pull/1819)).
- Add missing `ForeignPngFilter` enum ([#106](https://github.com/kleisauke/net-vips/pull/106)).
- Add missing `image.AddAlpha()` operation ([#116](https://github.com/kleisauke/net-vips/issues/116)).
- Add `image.Mutate()` function for creating an `MutableImage` ([#119](https://github.com/kleisauke/net-vips/issues/119)).

### Changed
- Move cache/statistics helpers to dedicated classes ([#98](https://github.com/kleisauke/net-vips/issues/98)).
- Use enumerations where possible ([#112](https://github.com/kleisauke/net-vips/issues/112)).
- Methods which modify the image, such as setting or removing metadata requires an `MutableImage` (see `image.Mutate()`).

### Fixed
- Ensure images are disposed early throughout the codebase ([#114](https://github.com/kleisauke/net-vips/issues/114)).

## [1.2.4] - 2020-07-13
### Changed
- Update methods/enums for libvips 8.10.

## [1.2.3] - 2020-06-21
### Added
- Add support for a single shared libvips binary on Linux and macOS ([#83](https://github.com/kleisauke/net-vips/issues/83)).

## [1.2.2] - 2020-06-16
### Changed
- Free the associated streams within `Image.*loadStream()` and `image.*saveStream()` earlier ([#78](https://github.com/kleisauke/net-vips/issues/78)).
- Speed-up UTF8 string marshalling by using `System.Buffers.ArrayPool`.

### Fixed
- Fix the buffer-based fallback mechanism for `Image.NewFromStream()` and `Image.NewFromSource()` on Windows 32-bit.

## [1.2.1] - 2020-03-16
### Changed
- Update enums.

### Fixed
- Fix a bug that freed the stream within `Image.NewFromStream()` too early ([#58](https://github.com/kleisauke/net-vips/issues/58)).

## [1.2.0] - 2020-01-30
### Added
- Add support for true streaming ([#33](https://github.com/kleisauke/net-vips/issues/33)).
  - See the [blogpost](https://www.libvips.org/2019/11/29/True-streaming-for-libvips.html) and the [tutorial](https://kleisauke.github.io/net-vips/introduction.html#custom-sources-and-targets) for more information.
- A new [NetVips.Extensions](https://www.nuget.org/packages/NetVips.Extensions/) package containing useful helpers ([#41](https://github.com/kleisauke/net-vips/issues/41)).
- Add `Image.Switch()`, `image.Case()` and `image.NewFromMemoryCopy()` operations.
- Add support for the short-circuit operators (`&&` and `||`).
- Add `Enums.Signals` and `Image.EvalDelegate` that can be used with `image.SignalConnect()`.
- Add `image.RefCount()` to get the reference count of an image.
- Add `NetVips.GetOperations()` to get a list of operations available within the libvips library.
- Add `VipsProgress` struct to the public API.
- Add `image.WriteToMemory(out ulong)` to write the image to an unformatted C-style array.
  - This is a low-level operation, make sure you free the returned memory with `NetVips.Free()`.

### Changed
- The `image.SetType()` function has been renamed to `image.Set()`.
- The `Image.Sum()` function uses the params keyword.
- Speed-up `Operation.Call()`.
- Free the `VipsOperation` pointers within `Operation.Call()` earlier ([#53](https://github.com/kleisauke/net-vips/issues/53)).
- Unset the `GValue` within `VipsObject.Set()` and `VipsObject.Get()` earlier.
- The internal function `Operation.GenerateImageClass()` has moved to the [samples directory](https://github.com/kleisauke/net-vips/blob/master/samples/NetVips.Samples/Samples/GenerateImageClass.cs).

## [1.1.0] - 2019-07-29
### Added
- A new [NetVips.Native](https://www.nuget.org/packages/NetVips.Native/) package containing the pre-compiled libvips binaries for Linux, macOS and Windows ([#21](https://github.com/kleisauke/net-vips/issues/21)).
  - Changes to the [NetVips.Native.*](https://www.nuget.org/packages?q=id%3ANetVips.Native) packages will be documented [here](CHANGELOG.native.md).
- Add `NetVips.GetSuffixes()` to get a list of all the filename suffixes supported by libvips ([libvips/ruby-vips#186](https://github.com/libvips/ruby-vips/issues/186)).
- Add support for progress feedback (`image.SetProgress()`) and signal handling (`image.SignalConnect()`) ([#31](https://github.com/kleisauke/net-vips/issues/31)).
- Add `image.SetKill()` and `image.IsKilled()` ([#31](https://github.com/kleisauke/net-vips/issues/31), [libvips/libvips@91d0e7e](https://github.com/libvips/libvips/commit/91d0e7e3d06fe6293f8e7513f30fd21585ea4305)).
- Add `NetVips.ProfileSet()`, `NetVips.VectorSet()`, `NetVips.ConcurrencySet()` and `NetVips.ConcurrencyGet()` utilities.
- Add support for loading and saving from and to streams (`Image.NewFromStream()` and `image.WriteToStream()`) ([#33](https://github.com/kleisauke/net-vips/issues/33)).
- Add `Region` class to read pixels from images without storing the entire image in memory.
- Add `image[x, y]` overload as a synonym for `image.Getpoint(x, y)`.
- Add missing arithmetic operators (`1 - image`, `1 / image`, etc.).
- Add support for identifying image formats (`Image.FindLoad()`, `Image.FindLoadBuffer()` and `Image.FindLoadStream()`) ([#37](https://github.com/kleisauke/net-vips/issues/37)).
- Add `image.PageHeight` property for retrieving the page height for multi-page images.

### Changed
- Improve memory management ([#26](https://github.com/kleisauke/net-vips/issues/26)).
- The bundled libvips Windows binaries were moved to the [NetVips.Native](https://www.nuget.org/packages/NetVips.Native/) package.
- Update and improve the [NetVips.Benchmarks](https://github.com/kleisauke/net-vips/tree/master/tests/NetVips.Benchmarks) ([#34](https://github.com/kleisauke/net-vips/issues/34)).
- The overloadable operators `==` and `!=` have been changed to `Equal` and `NotEqual` to avoid conflicts with `null` checks.
- Some methods are overloaded instead of defining the parameters as `object` type.
- The base class was renamed from `Base` to `NetVips` to comply with the C# code conventions.
- The `Operation.VipsCacheSet*()` utilities has been moved to `NetVips.CacheSet*()`.
- Speed-up `Operation.Call()` by avoiding unnecessary loops.
- Remove usage of LINQ in several critical paths.
- The composite x and y positions were changed into an array ([#39](https://github.com/kleisauke/net-vips/issues/39)).

### Removed
- The `UseGlobalLibvips` property since the bundled libvips binaries were moved to the [NetVips.Native](https://www.nuget.org/packages/NetVips.Native/) package.

## [1.0.7] - 2019-01-18
### Changed
- Update bundled libvips x86/x64 binary to 8.7.4.
- Speed-up `Base.Version()` by caching the libvips version as soon as the assembly is loaded.

## [1.0.6] - 2019-01-10
### Added
- The `LibvipsOutputBase` property to specify the subdirectory (within your project's output directory) where the libvips binaries are copied to ([#20](https://github.com/kleisauke/net-vips/issues/20)).

### Changed
- Update bundled libvips x86/x64 binary to 8.7.3.
- No exceptions will be thrown by the `ModuleInitializer` (used to initialize libvips once the assembly is loaded) ([#15](https://github.com/kleisauke/net-vips/issues/15), [#20](https://github.com/kleisauke/net-vips/issues/20)).

### Removed
- The redundant `LibvipsDLLPath` property.

## [1.0.5] - 2018-09-25
### Added
- Bundle pre-compiled libvips binary and its dependencies for 32-bit Windows.

### Changed
- Update bundled libvips binary to 8.7.0.

### Fixed
- Fix five small memleaks ([libvips/lua-vips#24](https://github.com/libvips/lua-vips/issues/24)).

## [1.0.4] - 2018-06-28
### Added
- Add `image.Contains()` helper (to check if the image contains an property of metadata).
- Support 32-bit architecture ([#7](https://github.com/kleisauke/net-vips/issues/7)).

### Changed
- Update bundled libvips binary to 8.6.4.

### Fixed
- Fix a bug that freed a string pointer too early ([#9](https://github.com/kleisauke/net-vips/issues/9)).

## [1.0.3] - 2018-06-06
### Added
- Bundle pre-compiled libvips binary and its dependencies for 64-bit Windows ([#3](https://github.com/kleisauke/net-vips/issues/3)).

### Changed
- Target .NET Standard 2.0 instead of .NET Core 2.0 ([#4](https://github.com/kleisauke/net-vips/issues/4)).
- Lower the minimum required .NET Framework version to 4.5 ([#4](https://github.com/kleisauke/net-vips/issues/4)).

## [1.0.2] - 2018-04-23
### Added
- Add missing libvips 8.7 methods.
- Add logging handler to log warnings and debug messages from libvips.

### Fixed
- Fix a bug that tried to reference an empty pointer.
- Fix a bug that causes libvips arguments to be set incorrectly.
- Fix up memory errors and leaks.
- Prevent the GC from unsetting the gvalue and disposing a delegate prematurely.

## [1.0.1] - 2018-04-10
### Fixed
- Fix reference count bug.

## [1.0.0] - 2018-04-08
### Added
- First release!

[2.4.1]: https://github.com/kleisauke/net-vips/compare/v2.4.0...v2.4.1
[2.4.0]: https://github.com/kleisauke/net-vips/compare/v2.3.1...v2.4.0
[2.3.1]: https://github.com/kleisauke/net-vips/compare/v2.3.0...v2.3.1
[2.3.0]: https://github.com/kleisauke/net-vips/compare/v2.2.0...v2.3.0
[2.2.0]: https://github.com/kleisauke/net-vips/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/kleisauke/net-vips/compare/v2.0.1...v2.1.0
[2.0.1]: https://github.com/kleisauke/net-vips/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/kleisauke/net-vips/compare/v1.2.4...v2.0.0
[1.2.4]: https://github.com/kleisauke/net-vips/compare/v1.2.3...v1.2.4
[1.2.3]: https://github.com/kleisauke/net-vips/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/kleisauke/net-vips/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/kleisauke/net-vips/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/kleisauke/net-vips/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/kleisauke/net-vips/compare/v1.0.7...v1.1.0
[1.0.7]: https://github.com/kleisauke/net-vips/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/kleisauke/net-vips/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/kleisauke/net-vips/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/kleisauke/net-vips/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/kleisauke/net-vips/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/kleisauke/net-vips/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/kleisauke/net-vips/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/kleisauke/net-vips/releases/tag/v1.0.0
