using System;

namespace NetVips.Tests
{
    public class TestsFixture : IDisposable
    {
        private readonly uint _handlerId;

        public TestsFixture()
        {
            var logFunc = new LogFunc(Log.PrintTraceLogFunction);
            _handlerId = Log.SetLogHandler("VIPS", Enums.LogLevelFlags.Critical, logFunc);
        }

        public void Dispose()
        {
            Log.RemoveLogHandler("VIPS", _handlerId);
        }
    }
}