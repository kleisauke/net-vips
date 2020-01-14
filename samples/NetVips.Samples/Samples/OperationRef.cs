namespace NetVips.Samples
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// See: https://github.com/kleisauke/net-vips/issues/53
    /// </summary>
    public class OperationRef : ISample
    {
        public string Name => "Operation reference test";
        public string Category => "Internal";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            NetVips.CacheSetMax(0);

            var image = Image.NewFromFile(Filename);

            for (var i = 0; i < 100; i++)
            {
                using (var crop = image.Crop(0, 0, 256, 256))
                {
                    var _ = crop.Avg();
                }

                // RefCount should not increase (i.e. operation should be freed)
                Debug.Assert(image.RefCount == 1);
                Console.WriteLine("reference count: " + image.RefCount);
            }

            return "All done!";
        }
    }
}