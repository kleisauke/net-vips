namespace NetVips.Samples
{
    using System;

    public class MutableImage : ISample
    {
        public string Name => "Mutable image";
        public string Category => "Create";

        public void Execute(string[] args)
        {
            using var im = Image.Black(500, 500);
            using var mutated = im.Mutate(x =>
            {
                for (var i = 0; i <= 100; i++)
                {
                    var j = i / 100.0;
                    x.DrawLine(new[] { 255.0 }, (int)(x.Width * j), 0, 0, (int)(x.Height * (1 - j)));
                }
            });
            mutated.WriteToFile("mutated.jpg");

            Console.WriteLine("See mutated.jpg");
        }
    }
}