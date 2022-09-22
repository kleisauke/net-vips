namespace NetVips
{
    using System;
    using Internal;
    using SMath = System.Math;

    /// <summary>
    /// This class represents a libvips image which can be modified. See
    /// <see cref="Image.Mutate"/>.
    /// </summary>
    public sealed partial class MutableImage : Image
    {
        /// <summary>
        /// The <see cref="Image"/> this <see cref="MutableImage"/> is modifying.
        /// Only use this once you have finished all modifications.
        /// </summary>
        internal Image Image { get; private set; }

        /// <summary>
        /// Make a <see cref="MutableImage"/> from a regular copied <see cref="Image"/>.
        /// </summary>
        /// <remarks>
        /// This is for internal use only. See <see cref="Image.Mutate"/> for the
        /// user-facing interface.
        /// </remarks>
        internal MutableImage(Image copiedImage) : base(copiedImage.ObjectRef())
        {
            Image = copiedImage;
        }

        #region set/remove metadata

        /// <summary>
        /// Set the type and value of an item of metadata.
        /// </summary>
        /// <remarks>
        /// Sets the type and value of an item of metadata. Any old item of the
        /// same name is removed. See <see cref="GValue"/> for types.
        /// </remarks>
        /// <param name="gtype">The GType of the metadata item to create.</param>
        /// <param name="name">The name of the piece of metadata to create.</param>
        /// <param name="value">The value to set as a C# value. It is
        /// converted to the GType, if possible.</param>
        public new void Set(IntPtr gtype, string name, object value)
        {
            using var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            VipsImage.Set(this, name, in gv.Struct);
        }

        /// <summary>
        /// Set the value of an item of metadata.
        /// </summary>
        /// <remarks>
        /// Sets the value of an item of metadata. The metadata item must already
        /// exist.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to set the value of.</param>
        /// <param name="value">The value to set as a C# value. It is
        /// converted to the type of the metadata item, if possible.</param>
        /// <exception cref="T:System.ArgumentException">If metadata item <paramref name="name"/> does not exist.</exception>
        public void Set(string name, object value)
        {
            var gtype = GetTypeOf(name);
            if (gtype == IntPtr.Zero)
            {
                throw new ArgumentException(
                    $"metadata item {name} does not exist - use the Set(IntPtr, string, object) overload to create and set");
            }

            Set(gtype, name, value);
        }

        /// <summary>
        /// Remove an item of metadata.
        /// </summary>
        /// <remarks>
        /// The named metadata item is removed.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to remove.</param>
        /// <returns><see langword="true"/> if the metadata is successfully removed;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Remove(string name)
        {
            return VipsImage.Remove(this, name);
        }

        #endregion

        #region overrides

        /// <summary>
        /// Overload `[]`.
        /// </summary>
        /// <remarks>
        /// Use `[]` to set band elements on an image. For example:
        /// <code language="lang-csharp">
        /// using var test = image.Mutate(x => x[1] = green);
        /// </code>
        /// Will change band 1 (the middle band).
        /// </remarks>
        /// <param name="i">The band element to change.</param>
        public new Image this[int i]
        {
            set
            {
                // number of bands to the left and right of value
                var nLeft = SMath.Min(Bands, SMath.Max(0, i));
                var nRight = SMath.Min(Bands, SMath.Max(0, Bands - 1 - i));
                var offset = Bands - nRight;
                using var left = nLeft > 0 ? Image.ExtractBand(0, n: nLeft) : null;
                using var right = nRight > 0 ? Image.ExtractBand(offset, n: nRight) : null;
                if (left == null)
                {
                    using (Image)
                    {
                        Image = value.Bandjoin(right);
                    }
                }
                else if (right == null)
                {
                    using (Image)
                    {
                        Image = left.Bandjoin(value);
                    }
                }
                else
                {
                    using (Image)
                    {
                        Image = left.Bandjoin(value, right);
                    }
                }
            }
        }

        /// <inheritdoc cref="Image"/>
        public override Image Mutate(Action<MutableImage> action)
        {
            action.Invoke(this);
            return Image;
        }

        /// <inheritdoc cref="Image"/>
        public override string ToString()
        {
            return $"<NetVips.MutableImage {Width}x{Height} {Format}, {Bands} bands, {Interpretation}>";
        }

        #endregion
    }
}