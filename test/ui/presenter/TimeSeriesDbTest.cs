using System;
using Xunit;

namespace cynofield.mods.ui.presenter
{
    public class TimeSeriesDbTest
    {
        private float time = 0;

        [Fact]
        public void Test1()
        {
            TimeSeriesBuffer<double> tsb = new TimeSeriesBuffer<double>(new double[10], 0.5f, () => time);
            var rnd = new Random();
            double _v1;
            {
                time = 0;
                tsb.Add(rnd.NextDouble());
                var (m1, v1, p1) = tsb.GetCurrent();
                _v1 = v1;
                Assert.NotNull(m1);
                Assert.NotEqual(0, v1);
                Assert.Equal(0, p1);
                var (m2, v2, p2) = tsb.GetPrev();
                Assert.Null(m2);
                Assert.Equal(0, v2);
                Assert.Equal(-1, p2);

                var c1 = tsb.ChangesCount(1);
                var c2 = tsb.ChangesCount(0.5f);
                Assert.Equal(1, c1);
                Assert.Equal(1, c2);
            }

            {
                time = 1;
                tsb.Add(rnd.NextDouble());
                var (m1, v1, p1) = tsb.GetCurrent();
                Assert.NotNull(m1);
                Assert.NotEqual(0, v1);
                Assert.NotEqual(_v1, v1);
                Assert.Equal(1, p1);
                var (m2, v2, p2) = tsb.GetPrev();
                Assert.NotNull(m2);
                Assert.Equal(_v1, v2);
                Assert.Equal(0, p2);

                var c1 = tsb.ChangesCount(1);
                var c2 = tsb.ChangesCount(0.5f);
                Assert.Equal(2, c1);
                Assert.Equal(1, c2);
            }

            {
                time = 2;
                var c1 = tsb.ChangesCount(2);
                var c2 = tsb.ChangesCount(1);
                var c3 = tsb.ChangesCount(0.5f);
                Assert.Equal(2, c1);
                Assert.Equal(1, c2);
                Assert.Equal(0, c3);
            }
        }

        // private float GetTime()
        // {
        //     return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000f;
        // }
    }
}
