using System;
using System.Collections.Generic;
using MTGLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class MTGTests
    {
        [TestMethod]
        public void TestMTG()
        {
            var mtg = new MTG();
            Assert.ReferenceEquals(MTG.Instance, mtg);
        }

        [TestMethod]
        public void ReverseCastTest()
        {
            var mountain = new MTGLib.MTGObject.BaseCardAttributes
            {
                name = "Mountain",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Land },
                superTypes = new HashSet<MTGObject.SuperType> { MTGObject.SuperType.Basic },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Mountain },
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ManaAbility(
                        new CostEvent.CostGen[]
                        {
                            () => {return new TapSelfCostEvent(); }
                        },
                        new EffectEvent.Effect[] {
                            (source, targets, callback) => {
                                int controller = MTG.Instance.objects[source].attr.controller;
                                callback(EventContainerAddMana.Auto(source, controller, ManaSymbol.Red));
                            }
                        }
                    )
                }
            };

            var weirdartifact = new MTGLib.MTGObject.BaseCardAttributes
            {
                name = "Weird",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Artifact },
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ManaAbility(
                        new CostEvent.CostGen[]
                        {
                            () => {return new TapSelfCostEvent(); },
                            () => {return EventContainerPayManaCost.Auto(new ManaCost(1)); }
                        },
                        new EffectEvent.Effect[]
                        {
                            (source, targets, callback) =>
                            {
                                int controller = MTG.Instance.objects[source].attr.controller;
                                var mana = new ManaCost(3);
                                callback(EventContainerAddMana.Auto(source, controller, mana));
                                callback(EventContainerDrawCards.Auto(source, controller, 2));
                            }
                        }
                    )
                }
            };

            var megacreature = new MTGLib.MTGObject.BaseCardAttributes
            {
                name = "Mega Creature",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Creature },
                power = 20,
                toughness = 20,
                manaCost = new ManaCost(10)
            };

            var lib = new List<MTGObject.BaseCardAttributes>();
            for (int x=0;x<15;x++)
            {
                lib.Add(mountain);
                lib.Add(weirdartifact);
                lib.Add(megacreature);
            }

            var mtg = new MTG(lib, lib);

            var mountain1OID = mtg.DebugTutor("Mountain", 0, mtg.battlefield);
            var mountain2OID = mtg.DebugTutor("Mountain", 0, mtg.battlefield);
            var weirdOID = mtg.DebugTutor("Weird", 0, mtg.battlefield);
            var creatureOID = mtg.DebugTutor("Mega Creature", 0, mtg.players[0].hand);

            mtg.Start();

            Thread gameLoopThread = new Thread(mtg.GameLoop);
            gameLoopThread.Start();

            var pass = new PriorityOption { type = PriorityOption.OptionType.PassPriority };

            mtg.ChoiceNewEvent.WaitOne();

            Choice choice;

            // Pass until first main
            {
                while (mtg.turn.phase.type != Phase.PhaseType.Main1)
                {
                    choice = mtg.CurrentUnresolvedChoice;
                    if (choice is PriorityChoice cast)
                    {
                        cast.Resolve(pass);
                    }
                    else { Assert.Fail(); }
                    mtg.ResolveChoice(choice);
                    mtg.ChoiceNewEvent.WaitOne();
                }
                Assert.AreEqual(mtg.turn.phase.type, Phase.PhaseType.Main1);
            }

            // main1 priority choice - cast spell
            {
                choice = mtg.CurrentUnresolvedChoice;

                if (choice is PriorityChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == PriorityOption.OptionType.CastSpell &&
                            option.source == creatureOID)
                        {
                            cast.Resolve(option);
                            break;
                        }
                    }
                }
                else { Assert.Fail(); }
                
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            // Mana payment choice {1} for cast - activate weird
            {
                choice = mtg.CurrentUnresolvedChoice;

                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.ActivateManaAbility &&
                            option.manaAbilitySource == weirdOID)
                        {
                            cast.Resolve(option);
                            break;
                        }
                    }
                }
                else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            // Mana payment choice {1} for weird artifact - activate mountain 1
            {
                choice = mtg.CurrentUnresolvedChoice;

                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.ActivateManaAbility &&
                            option.manaAbilitySource == mountain1OID)
                        {
                            cast.Resolve(option);
                        }
                    }
                }
                else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            // Mana payment choice {1} for weird artifact - use {R}
            {
                choice = mtg.CurrentUnresolvedChoice;

                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.UseMana &&
                            option.manaSymbol == ManaSymbol.Red)
                        {
                            cast.Resolve(option);
                        }
                    }
                } else { Assert.Fail(); }
            }

            int cardcount = mtg.players[0].hand.Count;

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            // Weird artifact resolves and draws cards
            Assert.IsTrue(mtg.objects[weirdOID].permanentStatus.tapped);
            Assert.IsTrue(mtg.objects[mountain1OID].permanentStatus.tapped);
            Assert.IsFalse(mtg.objects[mountain2OID].permanentStatus.tapped);

            Assert.AreEqual(mtg.players[0].hand.Count, cardcount + 2);

            // Mana paymanet choice {1} - use mana from weird
            {
                choice = mtg.CurrentUnresolvedChoice;
                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.UseMana &&
                            option.manaSymbol == ManaSymbol.Generic)
                        {
                            cast.Resolve(option);
                            break;
                        }
                    }
                } else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            Assert.IsTrue(mtg.objects[weirdOID].permanentStatus.tapped);
            Assert.IsTrue(mtg.objects[mountain1OID].permanentStatus.tapped);
            Assert.IsFalse(mtg.objects[mountain2OID].permanentStatus.tapped);
            Assert.AreEqual(mtg.players[0].manaPool.Count, 2);

            // *NEW* Mana payment choice {1} - activate mountain 2
            {
                choice = mtg.CurrentUnresolvedChoice;
                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.ActivateManaAbility &&
                            option.manaAbilitySource == mountain2OID)
                        {
                            cast.Resolve(option);
                        }
                    }
                } else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            // Mana payment choice {1} - use {R}
            {
                choice = mtg.CurrentUnresolvedChoice;
                if (choice is ManaChoice cast)
                {
                    foreach (var option in cast.Options)
                    {
                        if (option.type == ManaChoiceOption.OptionType.UseMana &&
                            option.manaSymbol == ManaSymbol.Red)
                        {
                            cast.Resolve(option);
                            break;
                        }
                    }
                }
                else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();

            Assert.IsTrue(mtg.objects[weirdOID].permanentStatus.tapped);
            Assert.IsTrue(mtg.objects[mountain1OID].permanentStatus.tapped);
            Assert.IsTrue(mtg.objects[mountain2OID].permanentStatus.tapped);
            Assert.AreEqual(mtg.players[0].hand.Count, cardcount + 2);
            
            // Mana payment for {1} - cancel
            {
                choice = mtg.CurrentUnresolvedChoice;

                if (choice is ManaChoice cast)
                {
                    cast.Cancel();
                } else { Assert.Fail(); }
            }

            Assert.IsTrue(choice.Resolved);
            mtg.ResolveChoice(choice);
            mtg.ChoiceNewEvent.WaitOne();


            // Mountain 2 gets reverted
            Assert.IsFalse(mtg.objects[mountain2OID].permanentStatus.tapped);

            // Weird oid is not reverted - it drew cards
            Assert.IsTrue(mtg.objects[weirdOID].permanentStatus.tapped);
            Assert.IsTrue(mtg.objects[mountain1OID].permanentStatus.tapped);
            // The card draw isn't reverted, and the spell is returned to hand
            Assert.AreEqual(mtg.players[0].hand.Count, cardcount + 3);

            // We still have 3 mana
            Assert.AreEqual(mtg.players[0].manaPool.Count, 3);
        }
    }
}
