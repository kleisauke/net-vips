namespace NetVips.Samples.Samples
{
    public class HelloWorld : ISample
    {
        public string Name => "Hello world";
        public string Category => "Create";

        public string Execute(string[] args)
        {
            var image = Image.Text("Hello <i>World!</i>", dpi: 300);
            image.WriteToFile("hello-world.png");

            return "See hello-world.png";
        }
    }
}
