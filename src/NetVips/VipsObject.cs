using System;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips;

/// <summary>
/// Manage a <see cref="Internal.VipsObject"/>.
/// </summary>
public class VipsObject : GObject
{
    /// <summary>
    /// Attach a post-close delegate. This is called on finalization.
    /// </summary>
    /// <remarks>
    /// Useful for e.g. deleting the file associated with a temp image.
    /// </remarks>
    public event Action OnPostClose
    {
        add => SignalConnect("postclose", value);
        remove => SignalHandlersDisconnectByFunc(value);
    }

    /// <inheritdoc cref="GObject"/>
    internal VipsObject(nint pointer) : base(pointer)
    {
    }

    /// <summary>
    /// Print a table of all active libvips objects. Handy for debugging.
    /// </summary>
    internal static void PrintAll()
    {
        GC.Collect();
        Internal.VipsObject.PrintAll();
    }

    /// <summary>
    /// slow! eeeeew.
    /// </summary>
    /// <param name="name">Arg to fetch.</param>
    /// <returns>The pspec for this arg.</returns>
    private GParamSpec.Struct? GetPspec(string name)
    {
        var argument = Internal.VipsObject.GetArgument(this, name, out var pspec, out _, out _);

        return argument != 0
            ? default(GParamSpec.Struct?)
            : Marshal.PtrToStructure<GParamSpec.Struct>(pspec);
    }

    /// <summary>
    /// Get a GObject property.
    /// </summary>
    /// <remarks>
    /// The value of the property is converted to a C# value.
    /// </remarks>
    /// <param name="name">Arg to fetch.</param>
    /// <returns>The GObject property.</returns>
    internal object Get(string name)
    {
        var pspec = GetPspec(name);
        if (!pspec.HasValue)
        {
            throw new VipsException("Property not found.");
        }

        var gtype = pspec.Value.ValueType;
        using var gv = new GValue();
        gv.SetType(gtype);

        // this will add a reference for GObject properties, that ref will be
        // unreferenced when the GValue is finalized
        Internal.GObject.GetProperty(this, name, ref gv.Struct);

        return gv.Get();
    }

    /// <summary>
    /// Set a GObject property. The value is converted to the property type, if possible.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value.</param>
    /// <param name="gtype">The GType of the property.</param>
    internal void Set(nint gtype, string name, object value)
    {
        using var gv = new GValue();
        gv.SetType(gtype);
        gv.Set(value);

        Internal.GObject.SetProperty(this, name, in gv.Struct);
    }

    /// <summary>
    /// Set a series of properties using a string.
    /// </summary>
    /// <remarks>
    /// For example:
    /// "fred=12, tile"
    /// "[fred=12]"
    /// </remarks>
    /// <param name="stringOptions">Arguments as a string.</param>
    /// <returns><see langword="true"/> on success; otherwise, <see langword="false"/>.</returns>
    internal bool SetString(string stringOptions)
    {
        var result = Internal.VipsObject.SetFromString(this, stringOptions);
        return result == 0;
    }

    /// <summary>
    /// Get the GType of a GObject property.
    /// </summary>
    /// <param name="name">The name of the GType to get the type of.</param>
    /// <returns>A new instance of <see langword="nint"/> initialized to the GType or
    /// <see cref="IntPtr.Zero"/> if the property does not exist.</returns>
    public nint GetTypeOf(string name)
    {
        var pspec = GetPspec(name);
        if (!pspec.HasValue)
        {
            // need to clear any error, this is horrible
            Vips.ErrorClear();
            return IntPtr.Zero;
        }

        return pspec.Value.ValueType;
    }

    /// <summary>
    /// Get the blurb for a GObject property.
    /// </summary>
    /// <param name="name">Arg to fetch.</param>
    /// <returns>The blurb.</returns>
    public string GetBlurb(string name)
    {
        var pspec = GetPspec(name);
        if (!pspec.HasValue)
        {
            return null;
        }

        var pspecValue = pspec.Value;
        return Marshal.PtrToStringAnsi(GParamSpec.GetBlurb(in pspecValue));
    }

    /// <summary>
    /// Get the description of a GObject.
    /// </summary>
    /// <returns>The description of a GObject.</returns>
    public string GetDescription()
    {
        return Marshal.PtrToStringAnsi(Internal.VipsObject.GetDescription(this));
    }
}