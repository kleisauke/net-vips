# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.3] - 2018-06-06
### Added
- Bundle pre-compiled libvips binary and its dependencies for 64-bit Windows.

### Changed
- Target .NET Standard 2.0 instead of .NET Core 2.0.
- Lower the minimum required .NET Framework version to 4.5.

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

[1.0.3]: https://github.com/kleisauke/net-vips/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/kleisauke/net-vips/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/kleisauke/net-vips/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/kleisauke/net-vips/releases/tag/v1.0.0
