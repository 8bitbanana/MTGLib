using System;
using System.Collections.Generic;
using MTGLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MTGTests
    {
        [TestMethod]
        public void TestMTG()
        {
            var mtg = new MTG();
            mtg.Start();
        }
    }
}
