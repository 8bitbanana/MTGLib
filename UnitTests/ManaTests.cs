using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGLib;

namespace UnitTests
{
    [TestClass]
    public class ManaTests
    {
        [TestMethod]
        public void TestManaSymbol()
        {
            Assert.AreEqual(
                new ManaSymbol(Color.Azorius),
                new ManaSymbol(Color.White | Color.Blue)
            );
            Assert.AreEqual(
                ManaSymbol.Generic.cmc, 1
            );
        }

        [TestMethod]
        public void TestManaCost()
        {
            ManaCost mana1 = new ManaCost(
                3,
                ManaSymbol.Red,
                ManaSymbol.Red,
                ManaSymbol.Red
            );
            ManaCost mana2 = new ManaCost(
                1,
                ManaSymbol.Red,
                ManaSymbol.Green
            );

            Assert.AreEqual(mana1.cmc, 6);
            Assert.AreEqual(mana2.cmc, 3);
            Assert.IsTrue(mana1.identity == Color.Red);
            Assert.IsTrue(mana2.identity == Color.Gruul);

            Assert.IsTrue((mana1 + mana2) - mana1 == mana2);
            Assert.AreEqual((mana1 + mana2) - mana1, mana2);

            Assert.AreEqual(mana1 + mana2, mana2 + mana1);
            Assert.AreEqual(mana1 + mana2, new ManaCost(
                4, ManaSymbol.Red, ManaSymbol.Red, ManaSymbol.Red, ManaSymbol.Red,
                ManaSymbol.Green
            ));
            Assert.AreEqual(mana2 - mana1, new ManaCost(ManaSymbol.Green));
        }

        [TestMethod]
        public void TestManaPool()
        {
            var manapool = new ManaPool();
            manapool.AddMana(ManaSymbol.Red);
            Assert.AreEqual(manapool.Count, 1);
            manapool.AddMana(ManaSymbol.Green, ManaSymbol.Green);
            manapool.RemoveMana(ManaSymbol.Red, ManaSymbol.Green);
            Assert.AreEqual(manapool.Count, 1);
            foreach (var mana in manapool)
            {
                Assert.AreEqual(mana, ManaSymbol.Green);
            }
            manapool.Empty();
            foreach (var mana in manapool)
            {
                Assert.Fail();
            }
        }
    }
}
