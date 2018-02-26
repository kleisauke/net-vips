using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Parser;
using NetVips.Generator.Passes;

namespace NetVips.Generator
{
    public class NetVips : ILibrary
    {
        private readonly string[] _headersToKeep =
        {
            "vips.h",
            "gmem.h", // for g_free, g_malloc
            "gmessages.h", // for g_log_remove_handler
            "gobject.h",
            "gtype.h",
            "gvalue.h",
            "gparam.h",
            "genums.h",
            "gvaluetypes.h",
            "basic.h",
            "util.h",
            "object.h",
            "type.h",
            "image.h",
            "error.h",
            "interpolate.h",
            "header.h",
            "operation.h",
            "foreign.h",
            "enumtypes.h"
        };

        private readonly List<string> _generatedFiles = new List<string>();
        private readonly VipsInfo _vipsInfo;

        public NetVips(VipsInfo vipsInfo)
        {
            this._vipsInfo = vipsInfo ?? throw new ArgumentNullException(nameof(vipsInfo));
        }

        /// <summary>
        /// Sets the driver options. First method called.
        /// </summary>
        /// <param name="driver"></param>
        public void Setup(Driver driver)
        {
            ParserOptions parserOptions = driver.ParserOptions;
            parserOptions.AddIncludeDirs(Path.Combine(_vipsInfo.VipsPath, "include"));
            parserOptions.AddIncludeDirs(Path.Combine(_vipsInfo.VipsPath, "include", "glib-2.0"));
            parserOptions.AddIncludeDirs(Path.Combine(_vipsInfo.VipsPath, "lib", "glib-2.0", "include"));
            parserOptions.LanguageVersion = LanguageVersion.C99_GNU;

            DriverOptions options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            options.CompileCode = true;
            options.StripLibPrefix = false;
            options.GenerateSingleCSharpFile = false; 
            options.MarshalCharAsManagedChar = true;
            // options.Encoding = Encoding.Unicode;
            options.GenerateFinalizers = true;
            options.OutputDir = _vipsInfo.OutputPath;
            options.GenerateDefaultValuesForArguments = true;
            options.GenerateName = file =>
            {
                _generatedFiles.Add(file.FileNameWithoutExtension);
                return file.FileNameWithoutExtension;
            };
            // options.GenerateDebugOutput = true;
            // options.CheckSymbols = true;

            var vipsModule = driver.Options.AddModule("NetVips");
            vipsModule.SymbolsLibraryName = "libvips";
            vipsModule.SharedLibraryName = "libvips";
            vipsModule.OutputNamespace = "NetVips.AutoGen";
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
            driver.AddTranslationUnitPass(new FixTypes());
        }

        /// <summary>
        /// Do transformations that should happen before any passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Preprocess(Driver driver, ASTContext ctx)
        {
            // Only keep relevant headers
            var units = ctx.TranslationUnits.FindAll(m => m.IsValid && !_headersToKeep.Any(m.FileName.Equals));
            foreach (var unit in units)
            {
                unit.ExplicitlyIgnore();
            }

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

        /// <summary>
        /// Fix DLL references
        /// </summary>
        public void FixDllReferences()
        {
            // TODO: Linux / MacOS support
            foreach (var fileName in _generatedFiles)
            {
                string replaceWith = "libvips-42.dll";
                if (fileName.Equals("gobject") || 
                    fileName.Equals("gtype") ||
                    fileName.Equals("gvalue") ||
                    fileName.Equals("gparam") ||
                    fileName.Equals("genums") || 
                    fileName.Equals("gvaluetypes"))
                {
                    replaceWith = "libgobject-2.0-0.dll";
                }
                else if (fileName.StartsWith("g"))
                {
                    replaceWith = "libglib-2.0-0.dll";
                }

                string f = Path.Combine(Path.GetFullPath(_vipsInfo.OutputPath), $"{fileName}.cs");
                string s = File.ReadAllText(f);
                StringBuilder sb = new StringBuilder(s);
                if (s.Contains("DllImport(\"libvips\""))
                {
                    sb.Replace("DllImport(\"libvips\"", $"DllImport(\"{replaceWith}\"");
                }

                File.WriteAllText(f, sb.ToString());
            }
        }
    }
}