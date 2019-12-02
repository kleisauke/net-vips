namespace NetVips.Internal
{
    using System;

    internal static class Enums
    {
        [Flags]
        internal enum GParamFlags
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

        internal enum GConnectFlags
        {
            G_CONNECT_AFTER = 1,
            G_CONNECT_SWAPPED = 2
        }

        internal enum VipsBandFormat
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
    }
}