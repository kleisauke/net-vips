namespace NetVips.Interop
{
    internal static partial class Libraries
    {
        // We can safely define all these variables as `libvips.42.dylib` since
        // DLLImport uses dlsym() on macOS. This function also searches for named
        // symbols in the dependencies of the shared library. Therefore, we can
        // provide libvips as a single shared library with all dependencies
        // statically linked without breaking compatibility with shared builds
        // (i.e. what is usually installed via package managers).
        internal const string GLib = "libvips.42.dylib",
                              GObject = "libvips.42.dylib",
                              Vips = "libvips.42.dylib";
    }
}