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
            ActivatedAbility,
            PassPriority,
            ManaAbility,

            // Special Actions
            PlayLand,
            TurnFaceUp,
            ExileSuspendCard,
            RetrieveCompanion,

            // Special actions defined in rules 116.2c-d are very rare.
            // I won't be implementing them until needed.

        }

        public OID source;
    }

    public class PriorityChoice : Choice<PriorityOption>
    {

    }
}
