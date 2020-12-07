using System;
using System.Collections.Generic;
using System.Threading;
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

            var knight = new MTGObject.BaseCardAttributes()
            {
                name = "Fireborn Knight",
                manaCost = new ManaCost(
                    ManaSymbol.HybridBoros,
                    ManaSymbol.HybridBoros,
                    ManaSymbol.HybridBoros,
                    ManaSymbol.HybridBoros
                ),
                power = 2,
                toughness = 3,
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Creature },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Human, MTGObject.SubType.Knight },
                // double strike goes here
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ActivatedAbility(
                        new CostEvent[]
                        {
                            EventContainerPayManaCost.Auto(new ManaCost(
                                ManaSymbol.HybridBoros,
                                ManaSymbol.HybridBoros,
                                ManaSymbol.HybridBoros,
                                ManaSymbol.HybridBoros
                            ))
                        },
                        new EffectEvent.Effect[]
                        {
                            (source, targets, callback) =>
                            {
                                MTG mtg_ = MTG.Instance;

                                mtg_.continuousEffects.Add(
                                    new ContinuousEffect(
                                        source,
                                        ContinuousEffect.Duration.EndOfTurn,
                                        new ContinuousEffect.DurationData
                                        {
                                            turn = mtg_.turn.turnCount
                                        },
                                        new Modification[]
                                        {
                                            new PowerMod
                                            {
                                                value = 1,
                                                operation = Modification.Operation.Add,
                                                specificOID = source
                                            },
                                            new ToughnessMod
                                            {
                                                value = 1,
                                                operation = Modification.Operation.Add,
                                                specificOID = source
                                            }
                                        }
                                    )
                                );
                            }
                        }
                    )
                }
            };

            var landfall = new MTGObject.BaseCardAttributes()
            {
                name = "Landfall Enchantment",
                manaCost = new ManaCost(),
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Enchantment },
                triggeredAbilities = new List<TriggeredAbility>
                {
                    new TriggeredAbility<MoveZoneEvent>(
                        (source, mtgevent) =>
                        {
                            if (mtgevent.newZone != MTG.Instance.battlefield)
                                return false;
                            var trigobj = MTG.Instance.objects[mtgevent.oid];
                            var sourceobj = MTG.Instance.objects[source];
                            return trigobj.attr.controller == sourceobj.attr.controller;
                        },
                        new EffectEvent.Effect[]
                        {
                            (source, targets, callback) =>
                            {
                                var damage = 1;
                                var target = targets[0].SetTargets[0];
                                callback(new DealDamageEvent(source, target, damage));
                            }
                        },
                        new Target[] { Target.AnyTarget }
                    )
                }
            };

            var island = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Island",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Land },
                superTypes = new HashSet<MTGObject.SuperType> { MTGObject.SuperType.Basic },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Island },
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ManaAbility(
                        new CostEvent[]
                        {
                            new TapSelfCostEvent()
                        },
                        new EffectEvent.Effect[] {
                            (source, targets, callback) => {
                                int controller = MTG.Instance.objects[source].attr.controller;
                                callback(EventContainerAddMana.Auto(source, controller, ManaSymbol.Blue));
                            }
                        }
                    )
                }
            };
            var mountain = new MTGLib.MTGObject.BaseCardAttributes()
            {
                name = "Mountain",
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Land },
                superTypes = new HashSet<MTGObject.SuperType> { MTGObject.SuperType.Basic },
                subTypes = new HashSet<MTGObject.SubType> { MTGObject.SubType.Mountain },
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ManaAbility(
                        new CostEvent[]
                        {
                            new TapSelfCostEvent()
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

            var izzetSignet = new MTGObject.BaseCardAttributes()
            {
                name = "Izzet Signet",
                manaCost = new ManaCost(2),
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Artifact },
                activatedAbilities = new List<ActivatedAbility>
                {
                    new ManaAbility(
                        new CostEvent[]
                        {
                            new TapSelfCostEvent(),
                            EventContainerPayManaCost.Auto(new ManaCost(1))
                        },
                        new EffectEvent.Effect[]
                        {
                            (source, targets, callback) =>
                            {
                                int controller = MTG.Instance.objects[source].attr.controller;
                                callback(EventContainerAddMana.Auto(source, controller, ManaSymbol.Red, ManaSymbol.Blue));
                            }
                        }
                    )
                }
            };

            var bolt = new MTGObject.BaseCardAttributes()
            {
                name = "Lightning Bolt",
                manaCost = new ManaCost(ManaSymbol.Red),
                cardTypes = new HashSet<MTGObject.CardType> { MTGObject.CardType.Instant },
                spellAbilities = new List<ResolutionAbility>
                {
                    new ResolutionAbility(
                        new EffectEvent.Effect[]
                        {
                            (source, targets, callback) =>
                            {
                                var damage = 3;
                                var target = targets[0].SetTargets[0];
                                callback(new DealDamageEvent(source, target, damage));
                            }
                        },
                        new Target[] {Target.AnyTarget}
                    )
                }
            };

            var lib1 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            var lib2 = new List<MTGLib.MTGObject.BaseCardAttributes>();
            for (int i=0; i<26; i++)
            {
                lib1.Add(mountain);
                lib2.Add(island);
            }

            for (int i=0; i<17; i++)
            {
                lib1.Add(ogre);
                lib1.Add(landfall);
                lib2.Add(crab);
                lib2.Add(izzetSignet);
            }

            var mtg = new MTG(lib1, lib2);
            mtg.Start();

            BoardViewer boardViewer = new BoardViewer();
            Thread boardViewerThread = new Thread(boardViewer.Run);
            boardViewerThread.Start();
            boardViewer.Update(mtg);

            Thread gameLoopThread = new Thread(mtg.GameLoop);
            gameLoopThread.Start();

            while (true)
            {
                mtg.ChoiceNewEvent.WaitOne();
                boardViewer.Update(mtg);

                Choice choice = mtg.CurrentUnresolvedChoice;
                
                if (choice is PriorityChoice cast)
                {
                    if (cast.Options.Count == 1)
                    {
                        Console.WriteLine("Single choice - autoresolving.");
                        cast.Resolve(new List<PriorityOption>(cast.Options));
                    }
                }

                if (!choice.Resolved)
                    choice.ConsoleResolve();

                mtg.ResolveChoice(choice);
            }
        }
    }
}
