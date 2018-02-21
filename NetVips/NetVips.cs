using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
            DriverOptions options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            options.CompileCode = true;
            options.StripLibPrefix = false;
            options.GenerateSingleCSharpFile = true;
            options.MarshalCharAsManagedChar = false;
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
            vipsModule.IncludeDirs.Add(Path.Combine(vipsInfo.VipsPath, "include"));
            vipsModule.IncludeDirs.Add(Path.Combine(vipsInfo.VipsPath, "include/glib-2.0"));
            vipsModule.IncludeDirs.Add(Path.Combine(vipsInfo.VipsPath, "lib/glib-2.0/include"));
            vipsModule.Headers.Add("vips/vips.h");
            vipsModule.LibraryDirs.Add(Path.Combine(vipsInfo.VipsPath, "lib"));
            vipsModule.Libraries.Add("libvips.lib");
            vipsModule.Defines.Add("GType=uint64_t");
        }

        /// <summary>
        /// Setup passes. Second method called.
        /// </summary>
        /// <param name="driver"></param>
        public void SetupPasses(Driver driver)
        {
            driver.AddTranslationUnitPass(new IgnoreNonVipsDeclsPass());
            driver.AddTranslationUnitPass(new FixParameterUsageFromName());
            driver.AddTranslationUnitPass(new AddOptionsParamForVariadicFuncs());
        }

        /// <summary>
        /// Do transformations that should happen before any passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Preprocess(Driver driver, ASTContext ctx)
        {
            // Ignore stuff that doesn't compile. TODO: Needs better fixing.

            // Error CS1002 ; expected
            ctx.FindFunction("vips__image_copy_fields_array").First().ExplicitlyIgnore();

            // Error CS0266 Cannot implicitly convert type 'string*' to 'sbyte**'
            foreach (var item in ctx.FindCompleteClass("_VipsFormatClass").Fields)
            {
                if (item.QualifiedLogicalName.Equals("_VipsFormatClass::suffs"))
                {
                    item.ExplicitlyIgnore();
                }
            }

            // Error CS0266 Cannot implicitly convert type 'string*' to 'sbyte**'
            foreach (var item in ctx.FindCompleteClass("_VipsForeignClass").Fields)
            {
                if (item.QualifiedLogicalName.Equals("_VipsForeignClass::suffs"))
                {
                    item.ExplicitlyIgnore();
                }
            }

            // Error CS0266 Cannot implicitly convert type 'NetVips.VipsBandFormat*' to 'System.IntPtr'
            // Error CS0266 Cannot implicitly convert type 'System.IntPtr' to 'NetVips.VipsBandFormat*'
            foreach (var item in ctx.FindCompleteClass("_VipsForeignSaveClass").Fields)
            {
                if (item.QualifiedLogicalName.Equals("_VipsForeignSaveClass::format_table"))
                {
                    item.ExplicitlyIgnore();
                }
            }
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