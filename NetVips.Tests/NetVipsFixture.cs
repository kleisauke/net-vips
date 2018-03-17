using System;

namespace NetVips.Tests
{
    public class NetVipsFixture : IDisposable
    {
        public NetVipsFixture()
        {
            Base.VipsInit();
        }

        public void Dispose()
        {
        }
    }
}