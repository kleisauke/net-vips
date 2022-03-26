namespace NetVips.Samples
{
    using System;
    using System.IO;
    using System.Text;

    public class IdentifyExtension : ISample
    {
        public string Name => "Identify image extension";
        public string Category => "Utils";

        /// <summary>
        /// Get image extension from a buffer.
        /// </summary>
        /// <param name="buffer">Buffer to check.</param>
        /// <returns>The image extension, or <see langword="null"/>.</returns>
        public string GetExtension(byte[] buffer)
        {
            var loader = Image.FindLoadBuffer(buffer);

            if (loader == null)
            {
                Console.WriteLine("Couldn't identify image extension");
                return null;
            }

            const int startIndex = 15; // VipsForeignLoad
            var suffixLength =
                loader.EndsWith("Buffer") || loader.EndsWith("Source") ? 6 : 4 /* loader.EndsWith("File") */;

            return loader.Substring(startIndex,
                    loader.Length - startIndex - suffixLength)
                .ToLower();
        }

        /// <summary>
        /// Get image extension of a non-truncated buffer.
        /// </summary>
        /// <param name="buffer">Buffer to check.</param>
        /// <returns>The image extension, or <see langword="null"/>.</returns>
        public string GetExtensionNonTruncated(byte[] buffer)
        {
            try
            {
                // The failOn option makes NetVips throw an exception on a file format error
                using var image = Image.NewFromBuffer(buffer, failOn: Enums.FailOn.Error, access: Enums.Access.Sequential);

                // Calculate the average pixel value. That way you are guaranteed to read every pixel
                // and the operation is cheap.
                var avg = image.Avg();

                // Unfortunately, vips-loader is the operation nickname, rather
                // than the canonical name returned by vips_foreign_find_load().
                var vipsLoader = (string)image.Get("vips-loader");
                var suffixLength = vipsLoader.EndsWith("load_buffer") || vipsLoader.EndsWith("load_source") ? 11 : 4;

                return vipsLoader[..^suffixLength];
            }
            catch (VipsException e)
            {
                Console.WriteLine($"Couldn't identify image extension: {e.Message}");
                return null;
            }
        }

        public void Execute(string[] args)
        {
            Console.WriteLine("FindLoadBuffer function (non-truncated buffer)");
            Console.WriteLine(GetExtension(File.ReadAllBytes("images/lichtenstein.jpg")));

            Console.WriteLine("vips-loader function (non-truncated buffer)");
            Console.WriteLine(GetExtensionNonTruncated(File.ReadAllBytes("images/lichtenstein.jpg")));

            Console.WriteLine("FindLoad function (truncated buffer)");
            Console.WriteLine(GetExtension(Encoding.UTF8.GetBytes("GIF89a")));

            Console.WriteLine("vips-loader function (truncated buffer)");
            Console.WriteLine(GetExtensionNonTruncated(Encoding.UTF8.GetBytes("GIF89a")));
        }
    }
}