using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class ContinuousEffect
    {
        public enum Duration
        {
            EndOfTurn,
            ObjectInZone,
            Infinite,
            EndOfPhase,
            ObjectTapped,
            ObjectControlledByPlayer
        }

        public struct DurationData
        {
            public int? turn;
            public int? player;
            public OID oid;
            public Phase.PhaseType? phase;
            public Zone? zone;
        }

        protected readonly int startTurn;
        protected readonly DurationData durationData;

        protected Duration duration;
        protected List<Modification> modifications = new List<Modification>();

        public IReadOnlyList<Modification> GetModifications()
        {
            return modifications.AsReadOnly();
        }

        public ContinuousEffect(Duration effectduration, DurationData? data = null)
        {
            duration = effectduration;

            if (data.HasValue)
            {
                durationData = data.Value;
            } else
            {
                durationData = new DurationData();
            }

            if (!IsDurationDataValid())
                throw new ArgumentException($"Duration data invalid for {duration}");
        }

        private bool IsDurationDataValid()
        {
            switch (duration)
            {
                case Duration.EndOfTurn:
                    if (durationData.turn == null)
                        return false;
                    return true;
                case Duration.EndOfPhase:
                    if (durationData.turn == null)
                        return false;
                    if (durationData.phase == null)
                        return false;
                    return true;
                case Duration.ObjectTapped:
                    if (durationData.oid == null)
                        return false;
                    return true;
                case Duration.ObjectInZone:
                    if (durationData.oid == null)
                        return false;
                    if (durationData.zone == null)
                        return false;
                    return true;
                case Duration.ObjectControlledByPlayer:
                    if (durationData.oid == null)
                        return false;
                    if (durationData.player == null)
                        return false;
                    return false;
                case Duration.Infinite:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddModification(Modification modification)
        {
            modifications.Add(modification);
        }

        public bool IsActive()
        {
            switch (duration)
            {
                case Duration.EndOfTurn:
                    return MTG.Instance.turn.turnCount <= durationData.turn;
                case Duration.EndOfPhase:
                    if (MTG.Instance.turn.turnCount > durationData.turn)
                        return false;
                    if (MTG.Instance.turn.phase.type > durationData.phase)
                        return false;
                    return true;
                case Duration.ObjectInZone:
                    throw new NotImplementedException();
                    return MTG.Instance.battlefield.Has(durationData.oid);
                case Duration.ObjectTapped:
                    //return MTG.Instance.objects[durationData.oid].
                    throw new NotImplementedException();
                case Duration.Infinite:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
