using CppSharp;
using System;

namespace NetVips.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var vipsInfo = new VipsInfo
            {
                VipsPath = @"C:\vips-dev-w64-all-8.7.0"
            };

            var netVips = new NetVips(vipsInfo);
            ConsoleDriver.Run(netVips);
            netVips.Clean();

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}