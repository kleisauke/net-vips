using System.IO;

namespace NetVips.Samples.Samples
{
    public class GenerateFunctions : ISample
    {
        public string Name => "Generate all functions";
        public string Category => "Internal";

        public string Execute(string[] args)
        {
            File.WriteAllText("functions.txt", Operation.GenerateAllFunctions());

            return "See functions.txt";
        }
    }
}