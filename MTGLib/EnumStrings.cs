using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public static class EnumStrings
    {
        public static string GetString(this AbilityObject.AbilityType abilityType)
        {
            return abilityType switch
            {
                AbilityObject.AbilityType.Activated => "Activated Ability",
                AbilityObject.AbilityType.Triggered => "Triggered Ability",
                _ => throw new NotImplementedException()
            };
        }

        public static string GetString(this MTGObject.CardType cardType)
        {
            return cardType switch
            {
                MTGObject.CardType.Artifact => "Artifact",
                MTGObject.CardType.Creature => "Creature",
                MTGObject.CardType.Enchantment => "Enchantment",
                MTGObject.CardType.Instant => "Instant",
                MTGObject.CardType.Land => "Land",
                MTGObject.CardType.Planeswalker => "Planeswalker",
                MTGObject.CardType.Sorcery => "Sorcery",
                _ => throw new NotImplementedException()
            };
        }

        public static bool HasColor(this Color color, Color test)
        {
            return ((test & color) == test);
        }

        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }

        public static bool IsPermanentType(this MTGObject.CardType cardType)
        {
            switch (cardType)
            {
                case MTGObject.CardType.Instant:
                case MTGObject.CardType.Sorcery:
                    return false;
                default:
                    return true;
            }
        }

        public static string GetString(this MTGObject.SuperType superType)
        {
            return superType switch
            {
                MTGObject.SuperType.Basic => "Basic",
                MTGObject.SuperType.Legendary => "Legendary",
                MTGObject.SuperType.Token => "Token",
                _ => throw new NotImplementedException()
            };
        }

        public static string GetString(this MTGObject.SubType subType)
        {
            // This will do for now
            // TODO - Fill with user friendly values
            return Enum.GetName(typeof(MTGObject.SubType), subType);
        }

        public static string GetString(this PriorityOption.OptionType optionType)
        {
            return optionType switch
            {
                PriorityOption.OptionType.CastSpell => "Cast a spell",
                PriorityOption.OptionType.ActivateAbility => "Activate an ability",
                PriorityOption.OptionType.PassPriority => "Pass priority",
                PriorityOption.OptionType.ManaAbility => "Activate a mana ability",
                PriorityOption.OptionType.PlayLand => "Play a land",
                PriorityOption.OptionType.TurnFaceUp => "Turn a permanent face up",
                PriorityOption.OptionType.ExileSuspendCard => "Exile a suspend card",
                PriorityOption.OptionType.RetrieveCompanion => "Retrieve a companion",
                _ => throw new NotImplementedException()
            };
        }

        public static string GetString(this Phase.PhaseType phaseType)
        {
            return phaseType switch
            {
                Phase.PhaseType.Untap => "Untap step",
                Phase.PhaseType.Upkeep => "Upkeep step",
                Phase.PhaseType.Draw => "Draw step",
                Phase.PhaseType.Main1 => "Precombat main phase",
                Phase.PhaseType.CombatStart => "Beginning of combat step",
                Phase.PhaseType.CombatAttackers => "Declare attackers step",
                Phase.PhaseType.CombatBlockers => "Declare blockers step",
                Phase.PhaseType.CombatDamage => "Combat damage step",
                Phase.PhaseType.CombatEnd => "End of combat step",
                Phase.PhaseType.Main2 => "Postcombat main phase",
                Phase.PhaseType.End => "End step",
                Phase.PhaseType.Cleanup => "Cleanup step",
                _ => throw new NotImplementedException()
            };
        }
    }
}
