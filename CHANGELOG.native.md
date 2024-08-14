# Changelog
All notable changes to the pre-compiled binaries of libvips will be documented in this file. The version number of these NuGet packages is in sync with libvips' version number.
The changes of libvips are documented [here](https://github.com/libvips/libvips/blob/master/ChangeLog).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.15.3] - 2024-08-14
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.15.3

## [8.15.2] - 2024-03-24
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.15.2

## [8.15.1] - 2024-01-14
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.15.1

## [8.15.0] - 2023-11-12
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2023/10/10/What's-new-in-8.15.html

### Changed
- Bump the minimum required glibc version to 2.26 ([lovell/sharp-libvips#197](https://github.com/lovell/sharp-libvips/pull/197)).
- Bump the minimum required musl version to 1.2.2 ([lovell/sharp-libvips#197](https://github.com/lovell/sharp-libvips/pull/197)).
- Switch from liborc to highway ([lovell/sharp-libvips#198](https://github.com/lovell/sharp-libvips/pull/198)).

## [8.14.5] - 2023-09-23
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.14.5

## [8.14.4] - 2023-08-16
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.14.4

## [8.14.3] - 2023-07-21
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.14.3

### Changed
- Restore support for tile-based output ([libvips/libvips#3354](https://github.com/libvips/libvips/issues/3354)).

## [8.14.2] - 2023-03-24
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2022/12/22/What's-new-in-8.14.html

### Changed
- Bump the minimum required macOS version to 10.13 (was 10.9) ([lovell/sharp-libvips#164](https://github.com/lovell/sharp-libvips/pull/164)).
- Support for tile-based output temporarily removed due to licensing issue ([libvips/libvips#3354](https://github.com/libvips/libvips/issues/3354)).

## [8.13.2] - 2022-10-01
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.13.2

## [8.13.1] - 2022-09-05
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.13.1

### Changed
- Align Linux x64 binaries with the v2 standard microarchitecture ([lovell/sharp-libvips#155](https://github.com/lovell/sharp-libvips/pull/155)).

## [8.13.0] - 2022-07-25
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2022/05/28/What's-new-in-8.13.html

### Added
- Enable Arm Neon support in libwebp, if available ([lovell/sharp-libvips#135](https://github.com/lovell/sharp-libvips/pull/135)).
- Enable WebP support in libtiff ([lovell/sharp-libvips#135](https://github.com/lovell/sharp-libvips/pull/135)).

### Changed
- Build Linux x64 glibc binaires with GCC 11.
- Build Linux musl binaires on Alpine 3.12.
- Switch libvips build to Meson ([lovell/sharp-libvips#144](https://github.com/lovell/sharp-libvips/pull/144)).

## [8.12.2] - 2022-01-27
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.12.2

### Changed
- Build macOS binaries with Clang 13.

## [8.12.1] - 2021-12-02
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2021/11/14/What's-new-in-8.12.html

### Added
- Include cgif as dependency.

### Changed
- Build Linux ARM64v8 binaries on CircleCI ([lovell/sharp-libvips#114](https://github.com/lovell/sharp-libvips/issues/114)).
- Move macOS ARM64 NuGet package to `netstandard2.1` group ([#151](https://github.com/kleisauke/net-vips/issues/151)).

## [8.11.4] - 2021-09-25
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.11.4

### Fixed
- Copy binaries to the output directory only when needed ([#140](https://github.com/kleisauke/net-vips/issues/140)).

## [8.11.3.1] - 2021-09-08
### Fixed
- `System.DllNotFoundException` on .NET Framework ([#136](https://github.com/kleisauke/net-vips/issues/136)).

## [8.11.3] - 2021-08-17
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.11.3

### Changed
- Build Linux ARM64v8 and ARMv7 binaries with GCC 11.2.

## [8.11.0] - 2021-06-23
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2021/06/04/What's-new-in-8.11.html

### Changed
- Build Linux ARM64v8 and ARMv7 binaries with GCC 11.1.

## [8.10.6] - 2021-03-30
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.10.6

### Added
- Provide prebuilt binaries for macOS ARM64 ([lovell/sharp-libvips#74](https://github.com/lovell/sharp-libvips/pull/74)).
- Provide prebuilt binaries for Linux musl ARM64 ([lovell/sharp-libvips#90](https://github.com/lovell/sharp-libvips/pull/90)).
- Include libimagequant as dependency ([lovell/sharp-libvips#91](https://github.com/lovell/sharp-libvips/pull/91)).

### Changed
- Switch from zlib to zlib-ng ([lovell/sharp-libvips#25](https://github.com/lovell/sharp-libvips/issues/25)).
- Allow linker to remove unused sections ([lovell/sharp-libvips#88](https://github.com/lovell/sharp-libvips/pull/88)).
- Switch from libjpeg-turbo to MozJPEG ([lovell/sharp-libvips#89](https://github.com/lovell/sharp-libvips/pull/89)).
- Build a more minimal libxml2 ([lovell/sharp-libvips#92](https://github.com/lovell/sharp-libvips/pull/92)).
- Build aom without HBR/WebM support ([lovell/sharp-libvips#94](https://github.com/lovell/sharp-libvips/pull/94)).
- Windows binaries are being built with Clang 11.1 (was GCC 10.2).

## [8.10.5.1] - 2020-12-27
### Fixed
- AVIF decode/encode on Windows with CPUs lacking support for the AVX2 instruction set ([#104](https://github.com/kleisauke/net-vips/issues/104)).
- Compatibility with older Linux ARM64v8 and ARMv7 distributions ([kleisauke/libvips-packaging#3](https://github.com/kleisauke/libvips-packaging/issues/3)).

## [8.10.5] - 2020-12-24
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.10.5

### Added
- Provide prebuilt binaries for Windows ARM64 ([libvips/build-win64-mxe#12](https://github.com/libvips/build-win64-mxe/issues/12)).
- Add support for AVIF ([lovell/sharp#2289](https://github.com/lovell/sharp/issues/2289)).

## [8.10.1] - 2020-09-12
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.10.1

## [8.10.0] - 2020-08-26
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2020/06/18/What's-new-in-8.10.html

### Fixed
- Compatibility with Unity on Linux ([#90](https://github.com/kleisauke/net-vips/issues/90)).

### Added
- Provide prebuilt binaries for Linux ARM64v8 and ARMv7 ([kleisauke/libvips-packaging#3](https://github.com/kleisauke/libvips-packaging/issues/3)).
- Include libspng as dependency.

## [8.9.2-build3] - 2020-06-21
### Note
Requires NetVips v1.2.3.

### Changed
- A single shared libvips binary is build for Linux and macOS ([#83](https://github.com/kleisauke/net-vips/issues/83)).

## [8.9.2-build2] - 2020-06-16
### Changed
- Binaries are copied to the output directory for Mono runtime ([#80](https://github.com/kleisauke/net-vips/issues/80)).
- A statically linked libvips binary is build for macOS ([kleisauke/libvips-packaging#1](https://github.com/kleisauke/libvips-packaging/issues/1)).

## [8.9.2] - 2020-04-28
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.9.2

### Fixed
- Compatibility with Windows Nano Server ([#61](https://github.com/kleisauke/net-vips/issues/61)).
- A GLib regression when running inside the Visual Studio debugger ([libvips/build-win64-mxe#9](https://github.com/libvips/build-win64-mxe/issues/9)).

## [8.9.1] - 2020-01-30
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.9.1

## [8.9.0] - 2020-01-30
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2019/12/11/What's-new-in-8.9.html

### Changed
- A statically linked libvips binary is build for Linux ([#51](https://github.com/kleisauke/net-vips/issues/51)).
- The NuGet packages now includes:
  - the latest third-party notices ([`THIRD-PARTY-NOTICES.md`](https://github.com/kleisauke/libvips-packaging/blob/master/THIRD-PARTY-NOTICES.md));
  - a JSON file containing the version numbers of libvips and its dependencies (`versions.json`).

## [8.8.4] - 2019-12-30
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.8.4

## [8.8.3] - 2019-10-14
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.8.3

## [8.8.2] - 2019-09-05
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.8.2

## [8.8.1] - 2019-07-29
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://github.com/libvips/libvips/releases/tag/v8.8.1

## [8.8.0] - 2019-06-22
### Note
If you would like to see what's changed, please visit the release notes of libvips:  
https://www.libvips.org/2019/04/22/What's-new-in-8.8.html

## [8.7.4] - 2019-04-10
### Added
- A [NetVips.Native](https://www.nuget.org/packages/NetVips.Native/) package which depends on the other [NetVips.Native.*](https://www.nuget.org/packages?q=id%3ANetVips.Native) packages.

### Changed
- The OS X binaries are now distributed with [`jpeg-turbo`](https://github.com/libjpeg-turbo/libjpeg-turbo) instead than [`jpeg`](https://www.ijg.org/) ([lovell/package-libvips-darwin#3](https://github.com/lovell/package-libvips-darwin/pull/3)).

## [8.7.4-beta1] - 2019-02-25
### Added
- Pre-compiled libvips binaries for a few distros ([#21](https://github.com/kleisauke/net-vips/issues/21)):
  - [NetVips.Native.linux-x64](https://www.nuget.org/packages/NetVips.Native.linux-x64) - Linux x64 glibc (Ubuntu, Debian, etc).
  - [NetVips.Native.linux-musl-x64](https://www.nuget.org/packages/NetVips.Native.linux-musl-x64) - Linux x64 musl (Alpine, Gentoo Linux, etc).
  - [NetVips.Native.osx-x64](https://www.nuget.org/packages/NetVips.Native.osx-x64) - macOS x64.
  - [NetVips.Native.win-x64](https://www.nuget.org/packages/NetVips.Native.win-x64) - Windows 64-bit.
  - [NetVips.Native.win-x86](https://www.nuget.org/packages/NetVips.Native.win-x86) - Windows 32-bit.

### Changed
- A statically linked libvips binary is build for Windows. This reduces the number of DLLs from 37 to 3 ([libvips/build-win64#21](https://github.com/libvips/build-win64/issues/21#issuecomment-458112440)).

[8.15.3]: https://github.com/kleisauke/libvips-packaging/compare/v8.15.2...v8.15.3
[8.15.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.15.1...v8.15.2
[8.15.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.15.0...v8.15.1
[8.15.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.14.5...v8.15.0
[8.14.5]: https://github.com/kleisauke/libvips-packaging/compare/v8.14.4...v8.14.5
[8.14.4]: https://github.com/kleisauke/libvips-packaging/compare/v8.14.3...v8.14.4
[8.14.3]: https://github.com/kleisauke/libvips-packaging/compare/v8.14.2...v8.14.3
[8.14.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.13.2...v8.14.2
[8.13.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.13.1...v8.13.2
[8.13.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.13.0...v8.13.1
[8.13.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.12.2...v8.13.0
[8.12.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.12.1...v8.12.2
[8.12.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.11.4...v8.12.1
[8.11.4]: https://github.com/kleisauke/libvips-packaging/compare/v8.11.3-build2...v8.11.4
[8.11.3.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.11.3...v8.11.3-build2
[8.11.3]: https://github.com/kleisauke/libvips-packaging/compare/v8.11.0...v8.11.3
[8.11.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.10.6...v8.11.0
[8.10.6]: https://github.com/kleisauke/libvips-packaging/compare/v8.10.5-build2...v8.10.6
[8.10.5.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.10.5...v8.10.5-build2
[8.10.5]: https://github.com/kleisauke/libvips-packaging/compare/v8.10.1...v8.10.5
[8.10.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.10.0-build2...v8.10.1
[8.10.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.9.2-build3...v8.10.0-build2
[8.9.2-build3]: https://github.com/kleisauke/libvips-packaging/compare/v8.9.2-build2...v8.9.2-build3
[8.9.2-build2]: https://github.com/kleisauke/libvips-packaging/compare/v8.9.2...v8.9.2-build2
[8.9.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.9.1...v8.9.2
[8.9.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.9.0...v8.9.1
[8.9.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.8.4...v8.9.0
[8.8.4]: https://github.com/kleisauke/libvips-packaging/compare/v8.8.3...v8.8.4
[8.8.3]: https://github.com/kleisauke/libvips-packaging/compare/v8.8.2...v8.8.3
[8.8.2]: https://github.com/kleisauke/libvips-packaging/compare/v8.8.1...v8.8.2
[8.8.1]: https://github.com/kleisauke/libvips-packaging/compare/v8.8.0...v8.8.1
[8.8.0]: https://github.com/kleisauke/libvips-packaging/compare/v8.7.4...v8.8.0
[8.7.4]: https://github.com/kleisauke/libvips-packaging/compare/v8.7.4-beta1...v8.7.4
[8.7.4-beta1]: https://github.com/kleisauke/libvips-packaging/releases/tag/v8.7.4-beta1
