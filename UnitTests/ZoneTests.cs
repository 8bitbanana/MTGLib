﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGLib;

namespace UnitTests
{
    [TestClass]
    public class ZoneTests
    {
        [TestMethod]
        public void TestZoneGeneric()
        {
            Zone zone = new Zone();

            OID cid1 = new OID();
            OID cid2 = new OID();
            OID cid3 = new OID();

            zone.Add(cid1);
            zone.Add(cid2);
            zone.Add(cid3);
            Assert.IsTrue(zone.Has(cid1));
            Assert.AreEqual(zone.Count, 3);
            Assert.AreEqual(zone.Get(0), cid3);
            Assert.AreEqual(zone.Get(zone.Count - 1), cid1);
            Assert.ThrowsException<IndexOutOfRangeException>(()=>zone.Get(-1));
            Assert.ThrowsException<IndexOutOfRangeException>(()=>zone.Get(zone.Count));

            OID pop = zone.Pop();
            Assert.AreEqual(pop, cid3);
            Assert.AreEqual(zone.Count, 2);
            Assert.AreEqual(zone.Get(0), cid2);

            Assert.ThrowsException<ArgumentException>(() => zone.Add(cid2));
            Assert.ThrowsException<ArgumentException>(() => zone.Remove(cid3));

            zone.Push(pop);
            Assert.AreEqual(zone.Count, 3);
            Assert.AreEqual(zone.Get(0), cid3);
        }
    }
}
