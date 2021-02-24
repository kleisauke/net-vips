namespace NetVips.Samples
{
    using System;

    /// <summary>
    /// From: https://github.com/libvips/lua-vips/blob/master/example/hello-world.lua
    /// </summary>
    public class HelloWorld : ISample
    {
        public string Name => "Hello world";
        public string Category => "Create";

        public void Execute(string[] args)
        {
            using var image = Image.Text("Hello <i>World!</i>", dpi: 300);
            image.WriteToFile("hello-world.png");

            Console.WriteLine("See hello-world.png");
        }
    }
}