using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class PlayerOrOID
    {
        public enum ValueType { Player, OID };

        public readonly ValueType type;
        readonly int player;
        readonly OID oid;

        public PlayerOrOID(int player)
        {
            this.player = player;
            type = ValueType.Player;
        }
        public PlayerOrOID(OID oid)
        {
            this.oid = oid;
            type = ValueType.OID;
        }

        public int Player { 
            get {
                if (type != ValueType.Player)
                    throw new InvalidOperationException("This object is not a player.");
                return player; 
            }
        }
        public OID OID {
            get {
                if (type != ValueType.OID)
                    throw new InvalidOperationException("This object is not an OID.");
                return oid;
            }
        }

        public bool IsPlayer { get { return type == ValueType.Player; } }
        public bool IsOID { get { return type == ValueType.OID; } }
    }


    public class Target
    {
        public static Target AnyTarget
        {
            get
            {
                return new Target(
                    (playeroroid) =>
                    {
                        return MTG.Instance.IsValidAnyTarget(playeroroid);
                    }
                );
            }
        }

        readonly Func<PlayerOrOID, bool> condition;

        readonly int Min = 1;
        readonly int Max = 1;

        public bool Declared { get; protected set; } = false;

        public List<PlayerOrOID> SetTargets { get; protected set; }

        public Target(Func<PlayerOrOID, bool> condition)
        {
            this.condition = condition;
        }

        public Target(Func<PlayerOrOID, bool> condition, int min, int max)
         : this(condition) 
        {
            Min = min; Max = max;
        }

        public bool Declare(OID source)
        {
            var targetOptions = GetAllTargets(source);
            PlayerOrOIDChoice choice = new PlayerOrOIDChoice()
            {
                Title = "Choose targets",
                Min = Min, Max = Max,
                Options = targetOptions,
                Cancellable = true
            };
            MTG.Instance.PushChoice(choice);
            if (choice.Cancelled) return false;
            else
            {
                SetTargets = new List<PlayerOrOID>(choice.Chosen);
                Declared = true;
                return true;
            }
        }

        private List<PlayerOrOID> GetAllTargets(OID source)
        {
            var targets = new List<PlayerOrOID>();

            foreach (var oid in MTG.Instance.objects.Keys)
            {
                if (oid == source) continue;
                var x = new PlayerOrOID(oid);
                if (condition(x))
                {
                    targets.Add(x);
                }
            }

            for (int player=0; player < MTG.Instance.players.Count; player++)
            {
                var x = new PlayerOrOID(player);
                if (condition(x))
                {
                    targets.Add(x);
                }
            }

            return targets;
        }
    }
}
