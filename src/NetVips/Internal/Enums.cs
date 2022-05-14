namespace NetVips.Internal
{
    using System;

    internal static class Enums
    {
        [Flags]
        internal enum GParamFlags
        {
            G_PARAM_READABLE = 1 << 0,
            G_PARAM_WRITABLE = 1 << 1,
            G_PARAM_READWRITE = G_PARAM_READABLE | G_PARAM_WRITABLE,
            G_PARAM_CONSTRUCT = 1 << 2,
            G_PARAM_CONSTRUCT_ONLY = 1 << 3,
            G_PARAM_LAX_VALIDATION = 1 << 4,
            G_PARAM_STATIC_NAME = 1 << 5,
            G_PARAM_PRIVATE = G_PARAM_STATIC_NAME,
            G_PARAM_STATIC_NICK = 1 << 6,
            G_PARAM_STATIC_BLURB = 1 << 7,
            G_PARAM_EXPLICIT_NOTIFY = 1 << 30,
            G_PARAM_DEPRECATED = 1 << 31
        }

        [Flags]
        internal enum GConnectFlags
        {
            G_CONNECT_AFTER = 1 << 0,
            G_CONNECT_SWAPPED = 1 << 1
        }

        [Flags]
        internal enum GSignalMatchType
        {
            G_SIGNAL_MATCH_ID = 1 << 0,
            G_SIGNAL_MATCH_DETAIL = 1 << 1,
            G_SIGNAL_MATCH_CLOSURE = 1 << 2,
            G_SIGNAL_MATCH_FUNC = 1 << 3,
            G_SIGNAL_MATCH_DATA = 1 << 4,
            G_SIGNAL_MATCH_UNBLOCKED = 1 << 5
        }
    }
}