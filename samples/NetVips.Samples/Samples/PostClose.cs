namespace NetVips.Samples
{
    using System;

    public class PostClose : ISample
    {
        public string Name => "Post close";
        public string Category => "Other";

        public const string Filename = "images/sample2.v";

        public void OnPostClose()
        {
            Console.WriteLine("Post close!");
        }

        public void Execute(string[] args)
        {
            // Avoid reusing the image after subsequent use
            Cache.Max = 0;

            Action action = OnPostClose;

            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            im.OnPostClose += action;

            // This will call OnPostClose
            im.Dispose();

            im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            im.OnPostClose += action;
            im.OnPostClose -= action;

            // This will not call OnPostClose
            im.Dispose();
        }
    }
}