using CppSharp;
using System;
using NetVips.Generator;

namespace NetVips.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var vipsInfo = new VipsInfo
            {
                VipsPath = @"C:\vips-dev-w64-all-8.7.0",
                OutputPath = "../../../NetVips/AutoGen"
            };

            var netVips = new Generator.NetVips(vipsInfo);
            ConsoleDriver.Run(netVips);
            netVips.FixDllReferences();

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}