using System;
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

            OID oid = zone.Get(0);
            Assert.IsNull(oid);

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

        [TestMethod]
        public void TestHand()
        {
            Hand hand = new Hand();
            Assert.IsTrue(hand.DiscardsNeeded == 0);
            for (int i = 0; i < 7; i++)
                hand.Add(new OID());
            Assert.IsTrue(hand.DiscardsNeeded == 0);
            hand.Add(new OID());
            Assert.IsTrue(hand.DiscardsNeeded == 1);
            for (int i = 0; i < 7; i++)
                hand.Add(new OID());
            Assert.IsTrue(hand.DiscardsNeeded == 8);
            hand.maxSize = 5;
            Assert.IsTrue(hand.DiscardsNeeded == 10);
            hand.maxSize = 50;
            Assert.IsTrue(hand.DiscardsNeeded == 0);
            hand.maxSize = -1;
            Assert.IsTrue(hand.DiscardsNeeded == 0);
        }
    }
}
