using System;
using System.Diagnostics;

namespace NetVips.Samples;

public class VipsArenaTest : ISample
{
    public string Name => "VipsArena reference test";
    public string Category => "Internal";

    public const string Filename = "images/lichtenstein.jpg";

    public void Execute(string[] args)
    {
        Cache.Max = 0;

        Image image;

        using (var arena = new VipsArena())
        {
            image = Image.NewFromFile(Filename);
        } // calls image.Dispose();

        Console.WriteLine($"reference count: {image.RefCount}");

        // RefCount should be 0 (i.e. image should be freed)
        Debug.Assert(image.RefCount == 0u);

        using (var arena = new VipsArena())
        {
            image = arena.Keep(Image.NewFromFile(Filename));
        }

        Console.WriteLine($"reference count: {image.RefCount}");
        Debug.Assert(image.RefCount == 1u);

        image.Dispose();
    }
}