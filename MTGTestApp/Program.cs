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

            var island = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Island",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Land },
                superTypes = new HashSet<MTGObject.SuperType> { MTGObject.SuperType.Basic },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Island }
            };
            var mountain = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Mountain",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Land },
                superTypes = new HashSet<MTGObject.SuperType> { MTGObject.SuperType.Basic },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Mountain }
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

            var lib1 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            var lib2 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            for (int i=0; i<30; i++)
            {
                lib1.Add(ogre);
                lib1.Add(crab);
                lib2.Add(ogre);
                lib2.Add(crab);
            }

            var mtg = new MTG(lib1, lib2);

            mtg.Start();
            mtg.GameLoop();
        }
    }
}
