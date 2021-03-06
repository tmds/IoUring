using System;
using System.Threading;
using Tmds.Linux;
using Xunit;
using static Tmds.Linux.LibC;

namespace IoUring.Tests
{
    public class TimeoutTest
    {
        [Fact]
        public unsafe void TimeoutFiresAfterTimeSpecified()
        {
            var r = new Ring(8);

            timespec ts;
            ts.tv_sec = 1;
            ts.tv_nsec = 0;

            Assert.True(r.TryPrepareTimeout(&ts));

            Assert.Equal(1u, r.Submit());
            Assert.Equal(1u, r.Flush(1u));

            Completion c = default;
            Assert.False(r.TryRead(ref c));

            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.True(r.TryRead(ref c));
            Assert.Equal(-ETIME, c.result);
        }

        [Fact]
        public unsafe void TimeoutFiresAfterNCompletions()
        {
            var r = new Ring(8);

            timespec ts;
            ts.tv_sec = 10;
            ts.tv_nsec = 0;

            Assert.True(r.TryPrepareTimeout(&ts));

            Assert.Equal(1u, r.Submit());
            Assert.Equal(1u, r.Flush(1u));

            Completion c = default;
            Assert.False(r.TryRead(ref c));

            r.TryPrepareNop(userData: 123);
            r.Flush(r.Submit());

            Assert.True(r.TryRead(ref c));
            Assert.Equal(0, c.result);
            Assert.Equal(123u, c.userData);

            Assert.True(r.TryRead(ref c));
            Assert.Equal(0, c.result);
        }
    }
}