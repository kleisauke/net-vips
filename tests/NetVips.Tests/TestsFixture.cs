namespace NetVips.Tests
{
    using System;
    using Xunit.Abstractions;

    public class TestsFixture : IDisposable
    {
        private uint _handlerId;

        public void SetUpLogging(ITestOutputHelper output)
        {
            _handlerId = Log.SetLogHandler("VIPS", Enums.LogLevelFlags.Error, (domain, level, message) =>
            {
                output.WriteLine("Domain: '{0}' Level: {1}", domain, level);
                output.WriteLine("Message: {0}", message);
            });
        }

        public void Dispose()
        {
            if (_handlerId > 0)
            {
                Log.RemoveLogHandler("VIPS", _handlerId);
                _handlerId = 0;
            }
        }
    }
}