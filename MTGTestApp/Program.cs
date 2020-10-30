using System;
using System.Collections.Generic;
using MTGLib;

namespace MTGTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var ogre = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Onakke Ogre",
                manaCost = new ManaCost(
                    2, ManaSymbol.Red
                ),
                power = 4,
                toughness = 2,
                cardTypes = new HashSet<MTGLib.MTGObject.CardType> { MTGLib.MTGObject.CardType.Creature },
                subTypes = new HashSet<MTGLib.MTGObject.SubType> { MTGLib.MTGObject.SubType.Ogre, MTGLib.MTGObject.SubType.Warrior }
            };
            
            var crab = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Wishcoin Crab",
                manaCost = new ManaCost(
                    3, ManaSymbol.Blue
                ),
                power = 2,
                toughness = 5,
                cardTypes = new HashSet<MTGObject.CardType> { MTGLib.MTGObject.CardType.Creature },
                subTypes = new HashSet<MTGObject.SubType> { MTGLib.MTGObject.SubType.Crab }
            };

            var redStatic = new StaticAbility(new PowerMod
            {
                value = 1,
                operation = Modification.Operation.Add,
                condition = (obj) =>
                {
                    if (!obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                        return false;
                    if (!Util.ColorHas(obj.identity, Color.Red))
                        return false;
                    if (obj.FindMyZone() != MTG.Instance.battlefield)
                        return false;
                    return true;
                }
            });
            var whiteStatic = new StaticAbility(new ToughnessMod
            {
                value = 1,
                operation = Modification.Operation.Add,
                condition = (obj) =>
                {
                    if (!obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                        return false;
                    if (!Util.ColorHas(obj.identity, Color.White))
                        return false;
                    if (obj.FindMyZone() != MTG.Instance.battlefield)
                        return false;
                    return true;
                }
            });

            var allPermsAreRed = new StaticAbility(new ColorMod
            {
                value = Color.Red,
                operation = Modification.Operation.Override,
                condition = (obj) =>
                {
                    if (obj.FindMyZone() != MTG.Instance.battlefield)
                        return false;
                    return true;
                }
            });

            var legionsInitiative = new MTGObject.BaseCardAttributes()
            {
                name = "Legion's Initiative",
                manaCost = new ManaCost(ManaSymbol.Red, ManaSymbol.White),
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Enchantment },
                staticAbilities = new List<StaticAbility> { redStatic, whiteStatic }
            };

            var redEnchantment = new MTGObject.BaseCardAttributes()
            {
                name = "Red Enchantment",
                manaCost = new ManaCost(1, ManaSymbol.Red, ManaSymbol.Red),
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Enchantment },
                staticAbilities = new List<StaticAbility> { allPermsAreRed }
            };

            var lib1 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            var lib2 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            for (int i=0; i<30; i++)
            {
                lib1.Add(ogre);
                lib1.Add(crab);
                lib2.Add(ogre);
                lib2.Add(crab);
            }
            lib1.Add(legionsInitiative);
            lib2.Add(redEnchantment);

            var mtg = new MTG(lib1, lib2);

            mtg.Start();

            foreach (var x in mtg.objects)
            {
                if (x.Value.attr.name == "Legion's Initiative")
                {
                    mtg.MoveZone(x.Key, mtg.battlefield);
                }
                if (x.Value.attr.name == "Red Enchantment")
                {
                    mtg.MoveZone(x.Key, mtg.battlefield);
                }
            }

            mtg.MoveZone(mtg.players[0].hand.Get(0), mtg.players[0].hand, mtg.battlefield);
            mtg.MoveZone(mtg.players[0].hand.Get(0), mtg.players[0].hand, mtg.battlefield);
            mtg.MoveZone(mtg.players[0].hand.Get(0), mtg.players[0].hand, mtg.battlefield);
            mtg.MoveZone(mtg.players[1].hand.Get(0), mtg.players[1].hand, mtg.battlefield);
            mtg.MoveZone(mtg.players[1].hand.Get(0), mtg.players[1].hand, mtg.battlefield);
            mtg.MoveZone(mtg.players[1].hand.Get(0), mtg.players[1].hand, mtg.battlefield);

            mtg.CalculateBoardState();
            foreach (OID oid in mtg.battlefield)
            {
                var obj = mtg.objects[oid];
                Console.WriteLine($"{obj.attr.name} {obj.attr.power}/{obj.attr.toughness}");
            }
        }
    }
}
