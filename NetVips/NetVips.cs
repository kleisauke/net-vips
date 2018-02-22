using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using System;
using System.IO;
using System.Text;
using CppSharp.Parser;
using NetVips.Passes;

namespace NetVips
{
    public class NetVips : ILibrary
    {
        private readonly VipsInfo vipsInfo;

        public NetVips(VipsInfo vipsInfo)
        {
            this.vipsInfo = vipsInfo ?? throw new ArgumentNullException(nameof(vipsInfo));
            Console.WriteLine($"Using {Path.GetFullPath(vipsInfo.OutputPath)} as output directory.");
        }

        /// <summary>
        /// Sets the driver options. First method called.
        /// </summary>
        /// <param name="driver"></param>
        public void Setup(Driver driver)
        {
            ParserOptions parserOptions = driver.ParserOptions;
            parserOptions.AddIncludeDirs(Path.Combine(vipsInfo.VipsPath, "include"));
            parserOptions.AddIncludeDirs(Path.Combine(vipsInfo.VipsPath, "include", "glib-2.0"));
            parserOptions.AddIncludeDirs(Path.Combine(vipsInfo.VipsPath, "lib", "glib-2.0", "include"));

            DriverOptions options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            options.CompileCode = true;
            options.StripLibPrefix = false;
            options.GenerateSingleCSharpFile = true;
            options.MarshalCharAsManagedChar = true;
            options.GenerateFinalizers = true;
            options.OutputDir = vipsInfo.OutputPath;
            options.GenerateDefaultValuesForArguments = true;
            // options.GenerateDebugOutput = true;
            // options.CheckSymbols = true;

            var vipsModule = driver.Options.AddModule("NetVips");
            vipsModule.SymbolsLibraryName = "libvips-42.dll";
            vipsModule.SharedLibraryName = "libvips-42.dll";
            vipsModule.OutputNamespace = "NetVips";
            vipsModule.LibraryName = "libvips";
            vipsModule.Headers.Add("vips/vips.h");
        }

        /// <summary>
        /// Setup passes. Second method called.
        /// </summary>
        /// <param name="driver"></param>
        public void SetupPasses(Driver driver)
        {
            driver.AddTranslationUnitPass(new IgnoreUnneededVipsDecls());
            driver.AddTranslationUnitPass(new ClearComments());
        }

        /// <summary>
        /// Do transformations that should happen before any passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Preprocess(Driver driver, ASTContext ctx)
        {
            // Ignore stuff that doesn't compile.
            // TODO: Some of these should have been reported upstream.

            // Error CS0426 The type name '_' does not exist in the type 'GValue'
            ctx.IgnoreClassField("_GValue", "data");

            // Error CS0266 Cannot implicitly convert type 'string*' to 'sbyte**'
            ctx.IgnoreClassField("_VipsFormatClass", "suffs");
            ctx.IgnoreClassField("_VipsForeignClass", "suffs");

            // Error CS0266 Cannot implicitly convert type 'NetVips.VipsBandFormat*' to 'System.IntPtr'
            // Error CS0266 Cannot implicitly convert type 'System.IntPtr' to 'NetVips.VipsBandFormat*'
            ctx.IgnoreClassField("_VipsForeignSaveClass", "format_table");
        }

        /// <summary>
        /// Do transformations that should happen after all passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Postprocess(Driver driver, ASTContext ctx)
        {
        }

        public void Clean()
        {
            // Removing unneeded file
            string stdFile = Path.Combine(vipsInfo.OutputPath, "Std.cs");
            if (File.Exists(stdFile))
            {
                File.Delete(stdFile);
                Console.WriteLine($"Removing unneeded file {stdFile}");
            }

            // Fix DLL references
            string f = Path.Combine(Path.GetFullPath(vipsInfo.OutputPath), "libvips.cs");
            string s = File.ReadAllText(f);
            StringBuilder sb = new StringBuilder(s);
            if (s.Contains("DllImport(\"libvips\""))
            {
                sb.Replace("DllImport(\"libvips\"", "DllImport(\"libvips-42.dll\"");
            }

            File.WriteAllText(f, sb.ToString());
        }
    }
}