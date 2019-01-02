using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Specifies the prototype of log handler functions.
    /// </summary>
    /// <param name="logDomain">the log domain of the message</param>
    /// <param name="logLevel">the log level of the message (including the fatal and recursion flags)</param>
    /// <param name="message">the message to process</param>
    public delegate void LogFunc(string logDomain, Enums.LogLevelFlags logLevel, string message);

    /// <summary>
    /// Specifies the type of the print handler functions.
    /// </summary>
    /// <param name="message">the message to output</param>
    public delegate void PrintFunc(string message);

    /// <summary>
    /// Wrapper for message logging functions
    /// </summary>
    public static class Log
    {
        private static GLib.LogFuncNative _nativeHandler;

        private static void NativeCallback(IntPtr logDomainNative, Enums.LogLevelFlags flags, IntPtr messageNative,
            IntPtr userData)
        {
            if (userData == IntPtr.Zero)
                return;
            var logDomain = logDomainNative.ToUtf8String();
            var message = messageNative.ToUtf8String();
            var gch = (GCHandle)userData;
            if (gch.Target is LogFunc func)
                func(logDomain, flags, message);
        }

        private class PrintHelper
        {
            private GLib.PrintFuncNative native;
            private PrintFunc managed;

            public PrintHelper(GLib.PrintFuncNative native)
            {
                this.native = native;
            }

            public PrintHelper(PrintFunc managed)
            {
                this.managed = managed;
                GCHandle.Alloc(this);
            }

            private void Callback(IntPtr nmessage)
            {
                var message = nmessage.ToUtf8String();
                managed(message);
            }

            private void Invoke(string message)
            {
                var nmessage = message.ToUtf8Ptr();
                native(nmessage);
                GLib.GFree(nmessage);
            }

            public GLib.PrintFuncNative Handler => Callback;

            public PrintFunc Invoker => Invoke;
        }

        private static ConcurrentDictionary<uint, GCHandle> _handlers = new ConcurrentDictionary<uint, GCHandle>();

        /// <summary>
        /// Logs an error or debugging message.
        /// </summary>
        /// <param name="logDomain">the log domain, or <see langword="null" /> for the default "" application domain</param>
        /// <param name="flags">the log level</param>
        /// <param name="format">the message format</param>
        /// <param name="args">the parameters to insert into the format string</param>
        public static void WriteLog(string logDomain, Enums.LogLevelFlags flags, string format, params object[] args)
        {
            var nmessage = string.Format(format, args).ToUtf8Ptr();
            GLib.GLogv(logDomain, flags, nmessage);
            GLib.GFree(nmessage);
        }

        /// <summary>
        /// Sets the log handler for a domain and a set of log levels.
        /// </summary>
        /// <param name="logDomain">the log domain, or <see langword="null" /> for the default "" application domain</param>
        /// <param name="flags">the log levels to apply the log handler for</param>
        /// <param name="logFunc">the log handler function</param>
        /// <returns>the id of the handler</returns>
        public static uint SetLogHandler(string logDomain, Enums.LogLevelFlags flags, LogFunc logFunc)
        {
            if (_nativeHandler == null)
                _nativeHandler = NativeCallback;

            var gch = GCHandle.Alloc(logFunc);
            var result = GLib.GLogSetHandler(logDomain, flags, _nativeHandler, (IntPtr)gch);
            _handlers.AddOrUpdate(result, gch, (k, v) => gch);
            return result;
        }

        /// <summary>
        /// Removes the log handler.
        /// </summary>
        /// <param name="logDomain">the log domain</param>
        /// <param name="handlerId">the id of the handler, which was returned in <see cref="SetLogHandler"/></param>
        public static void RemoveLogHandler(string logDomain, uint handlerId)
        {
            if (_handlers != null &&
                _handlers.ContainsKey(handlerId) &&
                _handlers.TryRemove(handlerId, out var handler))
            {
                handler.Free();
            }

            GLib.GLogRemoveHandler(logDomain, handlerId);
        }

        /// <summary>
        /// Sets the print handler.
        /// </summary>
        /// <param name="handler">the new print handler</param>
        /// <returns>the old print handler</returns>
        public static PrintFunc SetPrintHandler(PrintFunc handler)
        {
            var helper = new PrintHelper(handler);
            var prev = GLib.GSetPrintHandler(helper.Handler);
            helper = new PrintHelper(prev);
            return helper.Invoker;
        }

        /// <summary>
        /// Sets the handler for printing error messages.
        /// </summary>
        /// <param name="handler">the new error message handler</param>
        /// <returns>the old error message handler</returns>
        public static PrintFunc SetPrintErrorHandler(PrintFunc handler)
        {
            var helper = new PrintHelper(handler);
            var prev = GLib.GSetPrinterrHandler(helper.Handler);
            helper = new PrintHelper(prev);
            return helper.Invoker;
        }

        /// <summary>
        /// The default log handler set up by GLib; <see cref="SetDefaultHandler"/>
        /// allows to install an alternate default log handler.
        /// </summary>
        /// <param name="logDomain">the log domain, or <see langword="null" /> for the default "" application domain</param>
        /// <param name="logLevel">the level of the message</param>
        /// <param name="message">the message</param>
        public static void DefaultHandler(string logDomain, Enums.LogLevelFlags logLevel, string message)
        {
            var nmess = message.ToUtf8Ptr();
            GLib.GLogDefaultHandler(logDomain, logLevel, nmess, IntPtr.Zero);
            GLib.GFree(nmess);
        }

        /// <summary>
        /// Sets the message levels which are always fatal, in any log domain.
        /// When a message with any of these levels is logged the program terminates.
        /// </summary>
        /// <param name="fatalMask">the mask containing bits set for each level of error which is to be fatal</param>
        /// <returns>the old fatal mask</returns>
        public static Enums.LogLevelFlags SetAlwaysFatal(Enums.LogLevelFlags fatalMask)
        {
            return GLib.GLogSetAlwaysFatal(fatalMask);
        }

        /// <summary>
        /// Sets the log levels which are fatal in the given domain.
        /// </summary>
        /// <param name="logDomain">the log domain</param>
        /// <param name="fatalMask">the new fatal mask</param>
        /// <returns>the old fatal mask for the log domain</returns>
        public static Enums.LogLevelFlags SetAlwaysFatal(string logDomain, Enums.LogLevelFlags fatalMask)
        {
            return GLib.GLogSetFatalMask(logDomain, fatalMask);
        }

        private class Invoker
        {
            GLib.LogFuncNative native;

            public Invoker(GLib.LogFuncNative native)
            {
                this.native = native;
            }

            private void Invoke(string logDomain, Enums.LogLevelFlags flags, string message)
            {
                var ndom = logDomain.ToUtf8Ptr();
                var nmess = message.ToUtf8Ptr();
                native(ndom, flags, nmess, IntPtr.Zero);
                GLib.GFree(ndom);
                GLib.GFree(nmess);
            }

            public LogFunc Handler => Invoke;
        }

        /// <summary>
        /// Installs a default log handler which is used if no log handler
        /// has been set for the particular log domain and log level combination. 
        /// </summary>
        /// <param name="logFunc">the log handler function</param>
        /// <returns>the previous default log handler</returns>
        public static LogFunc SetDefaultHandler(LogFunc logFunc)
        {
            if (_nativeHandler == null)
                _nativeHandler = NativeCallback;

            var prev = GLib.GLogSetDefaultHandler(_nativeHandler, (IntPtr)GCHandle.Alloc(logFunc));
            if (prev == null)
                return null;
            var invoker = new Invoker(prev);
            return invoker.Handler;
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
        /// <param name="domain">the log domain of the message</param>
        /// <param name="level">the log level of the message (including the fatal and recursion flags)</param>
        /// <param name="message">the message to process</param>
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
        /// <param name="domain">the log domain of the message</param>
        /// <param name="level">the log level of the message (including the fatal and recursion flags)</param>
        /// <param name="message">the message to process</param>
        public static void PrintTraceLogFunction(string domain, Enums.LogLevelFlags level, string message)
        {
            PrintLogFunction(domain, level, message);
            Console.WriteLine("Trace follows:\n{0}", new StackTrace());
        }
    }
}