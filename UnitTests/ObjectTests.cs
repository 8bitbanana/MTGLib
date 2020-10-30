using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGLib;

namespace UnitTests
{
    
    [TestClass]
    public class TimestampTests
    {
        [TestMethod]
        public void TimestampTest()
        {
            var ts1 = new Timestamp();
            var ts2 = new Timestamp();
            Assert.IsTrue(ts1 < ts2);
            Assert.IsFalse(ts1 > ts2);
            ts1.Update();
            Assert.IsTrue(ts1 > ts2);

            var timestamps = new List<Timestamp>();
            for (int i=0; i<10; i++)
                timestamps.Add(new Timestamp());

            timestamps[9].Update();
            timestamps[2].Update();
            timestamps[4].Update();

            timestamps.Sort();
            for (int i=0; i<timestamps.Count-1;i++)
            {
                Assert.IsTrue(timestamps[i] < timestamps[i + 1]);
            }
        }
    }
}
