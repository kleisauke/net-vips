using System;

namespace NetVips.Internal
{
    public class Enums
    {
        public enum GParamFlags
        {
            G_PARAM_READABLE = 1,
            G_PARAM_WRITABLE = 2,
            G_PARAM_READWRITE = 3,
            G_PARAM_CONSTRUCT = 4,
            G_PARAM_CONSTRUCT_ONLY = 8,
            G_PARAM_LAX_VALIDATION = 16,
            G_PARAM_STATIC_NAME = 32,
            G_PARAM_PRIVATE = 32,
            G_PARAM_STATIC_NICK = 64,
            G_PARAM_STATIC_BLURB = 128,
            G_PARAM_EXPLICIT_NOTIFY = 1073741824,
            G_PARAM_DEPRECATED = -2147483648
        }

        [Flags]
        public enum VipsArgumentFlags
        {
            VIPS_ARGUMENT_NONE = 0,
            VIPS_ARGUMENT_REQUIRED = 1,
            VIPS_ARGUMENT_CONSTRUCT = 2,
            VIPS_ARGUMENT_SET_ONCE = 4,
            VIPS_ARGUMENT_SET_ALWAYS = 8,
            VIPS_ARGUMENT_INPUT = 16,
            VIPS_ARGUMENT_OUTPUT = 32,
            VIPS_ARGUMENT_DEPRECATED = 64,
            VIPS_ARGUMENT_MODIFY = 128
        }

        public enum VipsBandFormat
        {
            VIPS_FORMAT_NOTSET = -1,
            VIPS_FORMAT_UCHAR = 0,
            VIPS_FORMAT_CHAR = 1,
            VIPS_FORMAT_USHORT = 2,
            VIPS_FORMAT_SHORT = 3,
            VIPS_FORMAT_UINT = 4,
            VIPS_FORMAT_INT = 5,
            VIPS_FORMAT_FLOAT = 6,
            VIPS_FORMAT_COMPLEX = 7,
            VIPS_FORMAT_DOUBLE = 8,
            VIPS_FORMAT_DPCOMPLEX = 9,
            VIPS_FORMAT_LAST = 10
        }

        public enum VipsCoding
        {
            VIPS_CODING_ERROR = -1,
            VIPS_CODING_NONE = 0,
            VIPS_CODING_LABQ = 2,
            VIPS_CODING_RAD = 6,
            VIPS_CODING_LAST = 7
        }

        public enum VipsInterpretation
        {
            VIPS_INTERPRETATION_ERROR = -1,
            VIPS_INTERPRETATION_MULTIBAND = 0,
            VIPS_INTERPRETATION_B_W = 1,
            VIPS_INTERPRETATION_HISTOGRAM = 10,
            VIPS_INTERPRETATION_XYZ = 12,
            VIPS_INTERPRETATION_LAB = 13,
            VIPS_INTERPRETATION_CMYK = 15,
            VIPS_INTERPRETATION_LABQ = 16,
            VIPS_INTERPRETATION_RGB = 17,
            VIPS_INTERPRETATION_CMC = 18,
            VIPS_INTERPRETATION_LCH = 19,
            VIPS_INTERPRETATION_LABS = 21,
            VIPS_INTERPRETATION_sRGB = 22,
            VIPS_INTERPRETATION_YXY = 23,
            VIPS_INTERPRETATION_FOURIER = 24,
            VIPS_INTERPRETATION_RGB16 = 25,
            VIPS_INTERPRETATION_GREY16 = 26,
            VIPS_INTERPRETATION_MATRIX = 27,
            VIPS_INTERPRETATION_scRGB = 28,
            VIPS_INTERPRETATION_HSV = 29,
            VIPS_INTERPRETATION_LAST = 30
        }

        public enum VipsImageType
        {
            VIPS_IMAGE_ERROR = -1,
            VIPS_IMAGE_NONE = 0,
            VIPS_IMAGE_SETBUF = 1,
            VIPS_IMAGE_SETBUF_FOREIGN = 2,
            VIPS_IMAGE_OPENIN = 3,
            VIPS_IMAGE_MMAPIN = 4,
            VIPS_IMAGE_MMAPINRW = 5,
            VIPS_IMAGE_OPENOUT = 6,
            VIPS_IMAGE_PARTIAL = 7
        }

        public enum VipsDemandStyle
        {
            VIPS_DEMAND_STYLE_ERROR = -1,
            VIPS_DEMAND_STYLE_SMALLTILE = 0,
            VIPS_DEMAND_STYLE_FATSTRIP = 1,
            VIPS_DEMAND_STYLE_THINSTRIP = 2,
            VIPS_DEMAND_STYLE_ANY = 3
        }

        [Flags]
        public enum VipsOperationFlags
        {
            VIPS_OPERATION_NONE = 0,
            VIPS_OPERATION_SEQUENTIAL = 1,
            VIPS_OPERATION_SEQUENTIAL_UNBUFFERED = 2,
            VIPS_OPERATION_NOCACHE = 4,
            VIPS_OPERATION_DEPRECATED = 8
        }
    }
}
