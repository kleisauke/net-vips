using System.IO;

namespace NetVips.Samples
{
    public class GenerateImageClass : ISample
    {
        public string Name => "Generate image class";
        public string Category => "Internal";

        public string Execute(string[] args)
        {
            File.WriteAllText("Image.Generated.cs", Operation.GenerateImageClass());

            return "See Image.Generated.cs";
        }
    }
}