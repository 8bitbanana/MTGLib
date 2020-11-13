using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    // https://stackoverflow.com/a/643438/8708443
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }

    public class Phase
    {
        public enum PhaseType
        {
            Untap,
            Upkeep,
            Draw,
            Main1,
            CombatStart,
            CombatAttackers,
            CombatBlockers,
            CombatDamage,
            CombatEnd,
            Main2,
            End,
            Cleanup
        }

        public const PhaseType StartingPhase = PhaseType.Untap;
        public const PhaseType FinalPhase = PhaseType.Cleanup;

        public PhaseType type = StartingPhase;

        public void StartCurrentPhase()
        {
            var mtg = MTG.Instance;
            switch (type)
            {
                // "At the beginning of" effects

                case PhaseType.Untap:
                    // Phasing happens

                    // All permanents untap
                    foreach (OID oid in mtg.battlefield)
                    {
                        var mtgobj = mtg.objects[oid];
                        mtgobj.permanentStatus.tapped = false;
                    }

                    // No priority
                    break;
                case PhaseType.Upkeep:
                    // AP Priority
                    break;
                case PhaseType.Draw:
                    // The active player draws a card
                    mtg.players[mtg.turn.playerTurnIndex].Draw();

                    // AP Priority
                    break;
                case PhaseType.Main1:
                    // Increment and resolve sagas
                    // AP Priority
                    break;
                case PhaseType.CombatStart:
                    // AP Priority
                    break;
                case PhaseType.CombatAttackers:
                    // Attackers are declared by AP
                    // AP Priority
                    break;
                case PhaseType.CombatBlockers:
                    // Blockers are declared
                    // Attacking player chooses damage assignment order of attackers
                    // Defending player chooses damage assignment order of blockers
                    // AP Priority
                    break;
                case PhaseType.CombatDamage:
                    // Attacking player assigns combat damage
                    // Defending player assigns combat damage
                    // Damage is dealt (simultaneously)
                    // AP Priority
                    break;
                case PhaseType.CombatEnd:
                    // AP Priority
                    break;
                case PhaseType.Main2:
                    // AP Priority
                    break;
                case PhaseType.End:
                    // AP Priority
                    break;
                case PhaseType.Cleanup:
                    // Players discard to max hand size
                    foreach (var player in mtg.players)
                    {
                        player.Discard(player.hand.DiscardsNeeded);
                    }

                    // All marked damage is removed
                    // No priority
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void EndCurrentPhase()
        {
            // Empty mana pools
            foreach (var player in MTG.Instance.players)
            {
                player.manaPool.Empty();
            }

            // "Until the end of" effects end
        }

        public bool SorceryPhase { get
            {
                switch (type)
                {
                    case PhaseType.Main1:
                    case PhaseType.Main2:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool GivesPriority { get
            {
                switch (type)
                {
                    case PhaseType.Untap:
                    case PhaseType.Cleanup:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }
}
