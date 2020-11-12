using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class Choice
    {
        public bool Resolved { get; protected set; } = false;

        public abstract void ConsoleResolve();

        protected static string Prompt(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
    public class Choice<T> : Choice
    {
        public List<T> Options;

        public int Min = 1;
        public int Max = 1;

        public string Title = "Make a choice";

        public List<T> Choices { get; protected set; }

        public T FirstChoice { get
            {
                if (Choices.Count > 0)
                    return Choices[0];
                throw new IndexOutOfRangeException();
            }
        }

        protected virtual string OptionString(T option)
        {
            return option.ToString();
        }

        protected virtual bool Verify(List<T> choices)
        {
            if (choices.Count < Min)
                return false;
            if (choices.Count > Max)
                return false;
            return true;
        }

        public override void ConsoleResolve()
        {
            var currentChoices = new List<T>();

            // Check resolve loop 
            while (true)
            {
                var currentOptions = new List<T>();
                currentOptions.AddRange(Options);

                var finished = false;

                // Making choices loop
                while (!finished)
                {
                    Console.WriteLine(Title);

                    if (currentChoices.Count >= Min)
                        Console.WriteLine("[n] - Stop choosing.");
                    int index = 0;
                    foreach (var option in currentOptions)
                    {
                        Console.WriteLine($"[{index}] - {OptionString(option)}");
                        index++;
                    }

                    string input = Prompt("> ");
                    if (input.ToLower() == "n")
                    {
                        // User quitting, stop making choices
                        finished = true;
                        break;
                    }

                    try
                    {
                        index = int.Parse(input);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid input");
                        continue;
                    }
                    if (index < 0 || index >= Options.Count)
                    {
                        Console.WriteLine("Input out of range");
                        continue;
                    }
                    currentChoices.Add(currentOptions[index]);
                    currentOptions.RemoveAt(index);
                    if (currentChoices.Count >= Max)
                    {
                        // Maximum reached, stop making choices
                        finished = true;
                    }
                }

                if (Resolve(currentChoices))
                    break;
                else
                    Console.WriteLine("Invalid choices, retrying");
            }
            
        }

        public bool Resolve(List<T> choices)
        {
            if (Verify(choices))
            {
                Resolved = true;
                Choices = choices;
            } else
            {
                Resolved = false;
            }
            return Resolved;
        }
    }
    public class OIDChoice : Choice<OID>
    {
        protected override string OptionString(OID option)
        {
            MTGObject obj = MTG.Instance.objects[option];
            string str = obj.attr.name + " -";
            foreach (var cardType in obj.attr.cardTypes)
            {
                str += " " + cardType.GetString();
            }
            return str;
        }
    }

    public struct PriorityOption
    {
        public enum OptionType
        {
            CastSpell,
            ActivateAbility,
            PassPriority,
            ManaAbility,

            // Special Actions
            PlayLand,
            TurnFaceUp,
            ExileSuspendCard,
            RetrieveCompanion

            // Special actions defined in rules 116.2c-d are very rare.
            // I won't be implementing them until needed.

        }

        public OptionType type;
        public OID source;

        public ActivatedAbility activatedAbility;

        public override string ToString()
        {
            string s = type.GetString();
            if (source != null)
            {
                s += " -> " + MTG.Instance.objects[source].attr.name;
            }
            return s;
        }
    }

    public class PriorityChoice : Choice<PriorityOption>
    {
        protected override string OptionString(PriorityOption option)
        {
            return option.ToString();
        }

        public PriorityChoice ()
        {
            Min = 1; Max = 1;
            Title = "Choose what to do with your priority!";
            var mtg = MTG.Instance;
            var player = mtg.players[mtg.turn.playerPriorityIndex];

            // What can a player do when they have priority?
            Options = new List<PriorityOption>();

            // They can pass, of course
            Options.Add(new PriorityOption
            {
                type = PriorityOption.OptionType.PassPriority
            });

            // 117.1a A player may cast an instant spell any time they have priority. A player may cast a noninstant spell during their main phase any time they have priority and the stack is empty.
            foreach (var oid in player.hand)
            {
                var cardtypes = mtg.objects[oid].attr.cardTypes;

                // TODO - This is an oversimplification
                if (cardtypes.Contains(MTGObject.CardType.Land))
                    continue;

                // TODO - Also an oversimplification :) (for the card type check at least)
                if (!mtg.CanCastSorceries && !cardtypes.Contains(MTGObject.CardType.Instant))
                    continue;

                Options.Add(new PriorityOption
                {
                    type = PriorityOption.OptionType.CastSpell,
                    source = oid,
                });
            }

            // 117.1b A player may activate an activated ability any time they have priority.
            foreach (var kvp in mtg.objects)
            {
                OID oid = kvp.Key; MTGObject obj = kvp.Value;
                if (MTG.Instance.battlefield.Has(oid))
                {
                    Console.Write("");
                }
                foreach (var ability in obj.attr.activatedAbilities)
                {
                    if (!(ability is ManaAbility) && ability.CanBeActivated(oid))
                    {
                        Options.Add(new PriorityOption
                        {
                            type = PriorityOption.OptionType.ActivateAbility,
                            source = oid,
                            activatedAbility = ability
                        });
                    }
                }
            }

            // 117.1c A player may take some special actions any time they have priority.A player may take other special actions during their main phase any time they have priority and the stack is empty.See rule 116, “Special Actions.”
            foreach(var oid in player.hand)
            {
                var cardtypes = mtg.objects[oid].attr.cardTypes;
                if (mtg.CanCastSorceries && cardtypes.Contains(MTGObject.CardType.Land))
                {
                    // TODO - One land per turn. Need the events log and a land drop total system
                    Options.Add(new PriorityOption
                    {
                        type = PriorityOption.OptionType.PlayLand,
                        source = oid
                    });
                }
            }

            // 117.1d A player may activate a mana ability whenever they have priority, whenever they are casting a spell or activating an ability that requires a mana payment, or whenever a rule or effect asks for a mana payment(even in the middle of casting or resolving a spell or activating or resolving an ability).
            foreach (var kvp in mtg.objects)
            {
                OID oid = kvp.Key; MTGObject obj = kvp.Value;
                foreach (var ability in obj.attr.activatedAbilities)
                {
                    if (ability is ManaAbility && ability.CanBeActivated(oid))
                    {
                        Options.Add(new PriorityOption
                        {
                            type = PriorityOption.OptionType.ManaAbility,
                            source = oid,
                            activatedAbility = ability
                        });
                    }
                }
            }
        }
    }
}