namespace NetVips.Interop
{
    internal static class Libraries
    {
        /// <remarks>
        /// These library names are remapped in a cross-platform manner,
        /// <see cref="ModuleInitializer.Initialize"/>.
        /// </remarks>
        internal const string GLib = "libglib-2.0-0.dll",
                              GObject = "libgobject-2.0-0.dll",
                              Vips = "libvips-42.dll";
    }
}