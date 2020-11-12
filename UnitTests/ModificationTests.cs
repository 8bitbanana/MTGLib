using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGLib;
using System.Collections.Generic;


namespace UnitTests
{
    [TestClass]
    public class ModificationTests
    {
        [TestMethod]
        public void TestModifications()
        {
            var mtg = new MTG();
            MTGObject testobj = new MTGObject(new MTGObject.BaseCardAttributes
            {
                name = "Test Creature",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Creature },
                manaCost = new ManaCost(2, ManaSymbol.Blue, ManaSymbol.Red),
                power = 1,
                toughness = 1
            }); ;
            mtg.CreateObject(testobj);
            mtg.CalculateBoardState();
            Assert.AreEqual(testobj.attr.power, 1);
            Assert.AreEqual(testobj.attr.toughness, 1);

            var contEffect = new ContinuousEffect(ContinuousEffect.Duration.Infinite);

            // All creatures get +2/+0
            contEffect.AddModification(
                new PowerMod {
                    value = 2, operation = Modification.Operation.Add,
                    condition = (obj) =>
                    {
                        if (!obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                            return false;
                        return true;
                    }
                }
            );
            // All creatures get -0/-1
            contEffect.AddModification(
                new ToughnessMod
                {
                    value = 1,
                    operation = Modification.Operation.Subtract,
                    condition = (obj) =>
                    {
                        if (!obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                            return false;
                        return true;
                    }
                }
            );

            mtg.continuousEffects.Add(contEffect);
            mtg.CalculateBoardState();
            Assert.AreEqual(testobj.attr.power, 3);
            Assert.AreEqual(testobj.attr.toughness, 0);

            // All blue creatures are green
            contEffect = new ContinuousEffect(ContinuousEffect.Duration.Infinite);
            contEffect.AddModification(
                new ColorMod
                {
                    value = Color.Green,
                    operation = Modification.Operation.Override,
                    condition = (obj) =>
                    {
                        if (!obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                            return false;
                        if (!obj.attr.color.HasColor(Color.Blue))
                            return false;
                        return true;
                    }
                }
            );
            mtg.continuousEffects.Clear();
            mtg.continuousEffects.Add(contEffect);
            mtg.CalculateBoardState();
            Assert.AreEqual(testobj.identity, Color.Green);

            // All creatures are artifacts
            contEffect.AddModification(
                new CardTypeMod
                {
                    value = new HashSet<MTGObject.CardType> { MTGObject.CardType.Artifact },
                    operation = Modification.Operation.Override
                }
            );
            mtg.continuousEffects.Clear();
            mtg.continuousEffects.Add(contEffect);

            // Type is before color, so the color mod should fail as
            // the object is now an artifact
            mtg.CalculateBoardState();
            Assert.AreEqual(testobj.identity, Color.Izzet);
        }
    }
}
