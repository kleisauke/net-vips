namespace NetVips.Samples
{
    using System;

    public class Progress : ISample
    {
        public string Name => "Progress reporting";
        public string Category => "Other";

        public const int TileSize = 50;
        public const string Filename = "images/sample2.v";

        public void Execute(string[] args)
        {
            // Build test image
            using var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            using var test = im.Replicate(TileSize, TileSize);

            // Enable progress reporting
            test.SetProgress(true);

            // Connect signals
            test.SignalConnect(Enums.Signals.PreEval, PreEvalHandler);
            test.SignalConnect(Enums.Signals.Eval, EvalHandler);
            test.SignalConnect(Enums.Signals.PostEval, PostEvalHandler);

            var avg = test.Avg();
        }

        private void ProgressPrint(Enums.Signals signal, VipsProgress progress)
        {
            Console.WriteLine($"{signal}:");
            Console.WriteLine($"   Run = : {progress.Run}");
            Console.WriteLine($"   Eta = : {progress.Eta}");
            Console.WriteLine($"   TPels = : {progress.TPels}");
            Console.WriteLine($"   NPels = : {progress.NPels}");
            Console.WriteLine($"   Percent = : {progress.Percent}");
        }

        private void PreEvalHandler(Image image, VipsProgress progress)
        {
            ProgressPrint(Enums.Signals.PreEval, progress);
        }

        private void EvalHandler(Image image, VipsProgress progress)
        {
            ProgressPrint(Enums.Signals.Eval, progress);
        }

        private void PostEvalHandler(Image image, VipsProgress progress)
        {
            ProgressPrint(Enums.Signals.PostEval, progress);
        }
    }
}