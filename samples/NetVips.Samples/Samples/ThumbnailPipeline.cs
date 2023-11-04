namespace NetVips.Samples
{
    using System;
    using System.IO;

    public class ThumbnailPipeline : ISample
    {
        public string Name => "Thumbnail (configurable pipeline)";
        public string Category => "Resample";

        // Source: https://storage.googleapis.com/downloads.webmproject.org/webp/images/dancing-banana.gif
        public const string Filename = "images/dancing-banana.gif";

        // Maximum value for a coordinate.
        private const int VipsMaxCoord = 10000000;

        // = 71 megapixels
        private const int MaxImageSize = 71000000;

        // Halt processing and raise an error when loading invalid images.
        // Set this flag to Enums.FailOn.None if you'd rather apply a "best effort" to decode
        // images, even if the data is corrupt or invalid.
        // See: CVE-2019-6976
        // https://blog.silentsignal.eu/2019/04/18/drop-by-drop-bleeding-through-libvips/
        private const Enums.FailOn FailOn = Enums.FailOn.Error;

        // The name libvips uses to attach an ICC profile.
        private const string VipsMetaIccName = "icc-profile-data";

        /// <summary>
        /// Does this loader support multiple pages?
        /// </summary>
        /// <param name="loader">The name of the load operation.</param>
        /// <returns>A bool indicating if this loader support multiple pages.</returns>
        public bool LoaderSupportPage(string loader)
        {
            return loader.StartsWith("VipsForeignLoadPdf") ||
                   loader.StartsWith("VipsForeignLoadNsgif") ||
                   loader.StartsWith("VipsForeignLoadTiff") ||
                   loader.StartsWith("VipsForeignLoadWebp") ||
                   loader.StartsWith("VipsForeignLoadHeif") ||
                   loader.StartsWith("VipsForeignLoadMagick");
        }

#pragma warning disable CS0162 // Unreachable code detected
        public void Execute(string[] args)
        {
            // If you set a number to zero (0), it will resize on the other specified axis.
            var width = 200;
            var height = 0;

            // Enums.Size.Both - for both up and down.
            // Enums.Size.Up - only upsize.
            // Enums.Size.Down - only downsize.
            // Enums.Size.Force - force size, that is, break aspect ratio.
            const Enums.Size size = Enums.Size.Both;

            // Just for example.
            var buffer = File.ReadAllBytes(Filename);

            // Find the name of the load operation vips will use to load a buffer
            // so that we can work out what options to pass to NewFromBuffer().
            var loader = Image.FindLoadBuffer(buffer);

            if (loader == null)
            {
                // No known loader is found, stop further processing.
                throw new Exception("Invalid or unsupported image format. Is it a valid image?");
            }

            var loadOptions = new VOption
            {
                {"access", Enums.Access.Sequential},
                {"fail_on", FailOn}
            };
            var stringOptions = "";

            if (LoaderSupportPage(loader))
            {
                // -1 means "until the end of the document", handy for animated images.
                loadOptions.Add("n", -1);
                stringOptions = "[n=-1]";
            }

            int inputWidth;
            int inputHeight;
            int pageHeight;
            bool hasIccProfile;

            Image image = null;
            try
            {
                image = (Image)Operation.Call(loader, loadOptions, buffer);

                // Or:
                // image = Image.NewFromBuffer(buffer, kwargs: loadOptions);
                // (but the loader is already found, so the above will be a little faster).

                inputWidth = image.Width;
                inputHeight = image.Height;

                // Use 64-bit unsigned type, to handle PNG decompression bombs.
                if ((ulong)(inputWidth * inputHeight) > MaxImageSize)
                {
                    throw new Exception(
                        "Image is too large for processing. Width x height should be less than 71 megapixels.");
                }

                pageHeight = image.PageHeight;
                hasIccProfile = image.Contains(VipsMetaIccName);
            }
            catch (VipsException e)
            {
                throw new Exception("Image has a corrupt header.", e);
            }
            finally
            {
                // We're done with the image; dispose early
                image?.Dispose();
            }


            string importProfile = null;
            string exportProfile = null;
            Enums.Intent? intent = null;

            // If there's some kind of import profile, we can transform to the
            // output.
            if (hasIccProfile)
            {
                // Fallback to sRGB.
                importProfile = "srgb";

                // Convert to sRGB using embedded or import profile.
                exportProfile = "srgb";

                // Use "perceptual" intent to better match *magick.
                intent = Enums.Intent.Perceptual;
            }

            // Scaling calculations
            var thumbnailWidth = width;
            var thumbnailHeight = height;

            if (width > 0 && height > 0) // Fixed width and height
            {
                var xFactor = (double)inputWidth / width;
                var yFactor = (double)pageHeight / height;

                if (xFactor > yFactor) // Or: if (xFactor < yFactor)
                {
                    thumbnailHeight = (int)Math.Round(pageHeight / xFactor);
                }
                else
                {
                    thumbnailWidth = (int)Math.Round(inputWidth / yFactor);
                }
            }
            else if (width > 0) // Fixed width
            {
                if (size == Enums.Size.Force)
                {
                    thumbnailHeight = pageHeight;
                    height = pageHeight;
                }
                else
                {
                    // Auto height
                    var yFactor = (double)inputWidth / width;
                    height = (int)Math.Round(pageHeight / yFactor);

                    // Height is missing, replace with a huuuge value to prevent
                    // reduction or enlargement in that axis
                    thumbnailHeight = VipsMaxCoord;
                }
            }
            else if (height > 0) // Fixed height
            {
                if (size == Enums.Size.Force)
                {
                    thumbnailWidth = inputWidth;
                    width = inputWidth;
                }
                else
                {
                    // Auto width
                    var xFactor = (double)pageHeight / height;
                    width = (int)Math.Round(inputWidth / xFactor);

                    // Width is missing, replace with a huuuge value to prevent
                    // reduction or enlargement in that axis
                    thumbnailWidth = VipsMaxCoord;
                }
            }
            else // Identity transform
            {
                thumbnailWidth = inputWidth;
                width = inputWidth;

                thumbnailHeight = pageHeight;
                height = pageHeight;
            }

            // Note: don't use "image.ThumbnailImage". Otherwise, none of the very fast
            // shrink-on-load tricks are possible. This can make thumbnailing of large
            // images extremely slow.
            using var thumb = Image.ThumbnailBuffer(buffer, thumbnailWidth, stringOptions, thumbnailHeight, size,
                importProfile: importProfile, exportProfile: exportProfile, intent: intent);

            thumb.WriteToFile("thumbnail.webp", new VOption
            {
                {"keep", Enums.ForeignKeep.None}
            });

            // Or:
            /*buffer = thumb.WriteToBuffer(".webp", new VOption
            {
                {"keep", Enums.ForeignKeep.None}
            });*/

            Console.WriteLine("See thumbnail.webp");
        }
    }
#pragma warning restore CS0162 // Unreachable code detected
}