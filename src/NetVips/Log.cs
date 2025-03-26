using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips;

/// <summary>
/// Wrapper for message logging functions.
/// </summary>
public static class Log
{
    private static GLib.LogFuncNative _nativeHandler;

    /// <summary>
    /// Specifies the prototype of log handler functions.
    /// </summary>
    /// <param name="logDomain">The log domain of the message.</param>
    /// <param name="logLevel">The log level of the message (including the fatal and recursion flags).</param>
    /// <param name="message">The message to process.</param>
    public delegate void LogDelegate(string logDomain, Enums.LogLevelFlags logLevel, string message);

    private static void NativeCallback(string logDomain, Enums.LogLevelFlags flags, nint messagePtr,
        nint userData)
    {
        if (userData == IntPtr.Zero)
        {
            return;
        }

        var message = messagePtr.ToUtf8String();
        var gch = (GCHandle)userData;
        if (gch.Target is LogDelegate func)
        {
            func(logDomain, flags, message);
        }
    }

    private static readonly ConcurrentDictionary<uint, GCHandle> Handlers = new();

    /// <summary>
    /// Sets the log handler for a domain and a set of log levels.
    /// </summary>
    /// <param name="logDomain">The log domain, or <see langword="null"/> for the default "" application domain.</param>
    /// <param name="flags">The log levels to apply the log handler for.</param>
    /// <param name="logFunc">The log handler function.</param>
    /// <returns>The id of the handler.</returns>
    public static uint SetLogHandler(string logDomain, Enums.LogLevelFlags flags, LogDelegate logFunc)
    {
        _nativeHandler ??= NativeCallback;

        var gch = GCHandle.Alloc(logFunc);
        var result = GLib.GLogSetHandler(logDomain, flags, _nativeHandler, (nint)gch);
        Handlers.AddOrUpdate(result, gch, (_, _) => gch);
        return result;
    }

    /// <summary>
    /// Removes the log handler.
    /// </summary>
    /// <param name="logDomain">The log domain.</param>
    /// <param name="handlerId">The id of the handler, which was returned in <see cref="SetLogHandler"/>.</param>
    public static void RemoveLogHandler(string logDomain, uint handlerId)
    {
        if (Handlers != null &&
            Handlers.ContainsKey(handlerId) &&
            Handlers.TryRemove(handlerId, out var handler))
        {
            handler.Free();
        }

        GLib.GLogRemoveHandler(logDomain, handlerId);
    }

    /// <summary>
    /// Sets the message levels which are always fatal, in any log domain.
    /// When a message with any of these levels is logged the program terminates.
    /// </summary>
    /// <param name="fatalMask">The mask containing bits set for each level of error which is to be fatal.</param>
    /// <returns>The old fatal mask.</returns>
    public static Enums.LogLevelFlags SetAlwaysFatal(Enums.LogLevelFlags fatalMask)
    {
        return GLib.GLogSetAlwaysFatal(fatalMask);
    }

    /// <summary>
    /// Sets the log levels which are fatal in the given domain.
    /// </summary>
    /// <param name="logDomain">The log domain.</param>
    /// <param name="fatalMask">The new fatal mask.</param>
    /// <returns>The old fatal mask for the log domain.</returns>
    public static Enums.LogLevelFlags SetAlwaysFatal(string logDomain, Enums.LogLevelFlags fatalMask)
    {
        return GLib.GLogSetFatalMask(logDomain, fatalMask);
    }

    /// <summary>
    /// Common logging method.
    /// </summary>
    /// <remarks>
    /// Sample usage:
    /// <code language="lang-csharp">
    /// // Print the messages for the NULL domain
    /// var logFunc = new LogFunc(Log.PrintLogFunction);
    /// Log.SetLogHandler(null, Enums.LogLevelFlags.All, logFunc);
    /// </code>
    /// </remarks>
    /// <param name="domain">The log domain of the message.</param>
    /// <param name="level">The log level of the message (including the fatal and recursion flags).</param>
    /// <param name="message">The message to process.</param>
    public static void PrintLogFunction(string domain, Enums.LogLevelFlags level, string message)
    {
        Console.WriteLine("Domain: '{0}' Level: {1}", domain, level);
        Console.WriteLine("Message: {0}", message);
    }

    /// <summary>
    /// Common logging method.
    /// </summary>
    /// <remarks>
    /// Sample usage:
    /// <code language="lang-csharp">
    /// // Print messages and stack trace for vips critical messages
    /// var logFunc = new LogFunc(Log.PrintTraceLogFunction);
    /// Log.SetLogHandler("VIPS", Enums.LogLevelFlags.Critical, logFunc);
    /// </code>
    /// </remarks>
    /// <param name="domain">The log domain of the message.</param>
    /// <param name="level">The log level of the message (including the fatal and recursion flags).</param>
    /// <param name="message">The message to process.</param>
    public static void PrintTraceLogFunction(string domain, Enums.LogLevelFlags level, string message)
    {
        PrintLogFunction(domain, level, message);
        Console.WriteLine("Trace follows:\n{0}", new StackTrace());
    }
}