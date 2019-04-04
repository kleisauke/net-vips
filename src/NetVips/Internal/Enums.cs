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

        [Flags]
        internal enum VipsArgumentFlags
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

        [Flags]
        internal enum VipsOperationFlags
        {
            VIPS_OPERATION_NONE = 0,
            VIPS_OPERATION_SEQUENTIAL = 1,
            VIPS_OPERATION_SEQUENTIAL_UNBUFFERED = 2,
            VIPS_OPERATION_NOCACHE = 4,
            VIPS_OPERATION_DEPRECATED = 8
        }

        internal static class VipsEvaluation
        {
            /// <summary>Evaluation is starting.</summary>
            /// <remarks>
            /// The preeval signal is emitted once before computation of <see cref="Image"/>
            /// starts. It's a good place to set up evaluation feedback.
            /// </remarks>
            public const string PreEval = "preeval";

            /// <summary>Evaluation progress.</summary>
            /// <remarks>
            /// The eval signal is emitted once per work unit (typically a 128 x
            /// 128 area of pixels) during image computation.
            ///
            /// You can use this signal to update user-interfaces with progress
            /// feedback. Beware of updating too frequently: you will usually
            /// need some throttling mechanism.
            /// </remarks>
            public const string Eval = "eval";

            /// <summary>Evaluation is ending.</summary>
            /// <remarks>
            /// The posteval signal is emitted once at the end of the computation
            /// of <see cref="Image"/>. It's a good place to shut down evaluation feedback.
            /// </remarks>
            public const string PostEval = "posteval";
        }
    }
}