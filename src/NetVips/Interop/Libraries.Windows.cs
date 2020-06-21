namespace NetVips.Interop
{
    internal static partial class Libraries
    {
        // We cannot define all these variables as `libvips-42.dll` without
        // breaking compatibility with the shared Windows build. Therefore,
        // we always ship at least 3 DLLs.
        internal const string GLib = "libglib-2.0-0.dll",
                              GObject = "libgobject-2.0-0.dll",
                              Vips = "libvips-42.dll";
    }
}