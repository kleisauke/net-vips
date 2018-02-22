using System.Linq;
using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips.Passes
{
    public class IgnoreUnneededVipsDecls : TranslationUnitPass
    {
        private readonly string[] _functionsToKeep =
        {
            "vips_init",
            "g_log_set_handler",
            "g_log_remove_handler",
            "g_malloc",
            "g_free",
            "vips_leak_set",
            "vips_path_filename7",
            "vips_path_mode7",
            "vips_type_find",
            "vips_nickname_find",
            "g_type_name",
            "g_type_from_name",
            "vips_type_map",
            "vips_version",
            "vips_error_buffer",
            "vips_error_clear",
            "g_object_ref",
            "g_object_unref",
            "g_object_set_property",
            "g_object_get_property",
            "g_value_init",
            "g_value_unset",
            "g_type_fundamental",
            "vips_enum_from_nick",
            "g_value_set_boolean",
            "g_value_set_int",
            "g_value_set_double",
            "g_value_set_enum",
            "g_value_set_flags",
            "g_value_set_string",
            "g_value_set_object",
            "vips_value_set_array_double",
            "vips_value_set_array_int",
            "vips_value_set_array_image",
            "vips_value_set_blob",
            "g_value_get_boolean",
            "g_value_get_int",
            "g_value_get_double",
            "g_value_get_enum",
            "g_value_get_flags",
            "g_value_get_string",
            "vips_value_get_ref_string",
            "g_value_get_object",
            "vips_value_get_array_double",
            "vips_value_get_array_int",
            "vips_value_get_array_image",
            "vips_value_get_blob",
            "vips_interpretation_get_type",
            "vips_operation_flags_get_type",
            "vips_band_format_get_type",
            "vips_blend_mode_get_type", // Since libvips 8.6
            "vips_foreign_find_load",
            "vips_foreign_find_load_buffer",
            "vips_foreign_find_save",
            "vips_foreign_find_save_buffer",
            "vips_image_new_matrix_from_array",
            "vips_image_new_from_memory",
            "vips_image_copy_memory",
            "vips_image_get_typeof",
            "vips_image_get",
            "vips_image_set",
            "vips_image_remove",
            "vips_image_get_fields",
            "vips_filename_get_filename",
            "vips_filename_get_options",
            "vips_image_new_temp_file",
            "vips_image_write",
            "vips_image_write_to_memory",
            "vips_interpolate_new",
            "vips_object_get_argument",
            "vips_object_print_all",
            "vips_object_set_from_string",
            "vips_object_get_description",
            "g_param_spec_get_blurb",
            "vips_operation_new",
            "vips_argument_map",
            "vips_cache_operation_build",
            "vips_object_unref_outputs",
            "vips_operation_get_flags",
            "vips_cache_set_max",
            "vips_cache_set_max_mem",
            "vips_cache_set_max_files",
            "vips_cache_set_trace",
        };

        private readonly string[] _classesToKeep =
        {
            "_VipsImage",
            "_GValue",
            "_GTypeClass",
            "_GObject",
            "_GParamSpec",
            "_VipsInterpolate",
            "_VipsObject",
            "_VipsObjectClass",
            "_VipsArgument",
            "_VipsArgumentInstance",
            "_VipsArgumentClass",
            "_VipsOperation",
        };

        private readonly string[] _enumsToKeep =
        {
            "_VipsArgumentFlags"
        };

        public override bool VisitFunctionDecl(Function function)
        {
            if (!base.VisitFunctionDecl(function))
            {
                return false;
            }

            if (!_functionsToKeep.Any(function.Name.Equals))
            {
                function.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!base.VisitClassDecl(@class))
            {
                return false;
            }

            if (!_classesToKeep.Any(@class.Name.Equals))
            {
                @class.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        public override bool VisitEnumDecl(Enumeration @enum)
        {
            if (!base.VisitEnumDecl(@enum))
            {
                return false;
            }

            if (!_enumsToKeep.Any(@enum.Name.Equals))
            {
                @enum.ExplicitlyIgnore();
                return false;
            }

            return true;
        }
    }
}