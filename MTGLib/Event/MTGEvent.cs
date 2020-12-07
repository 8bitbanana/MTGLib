using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MTGLib
{
    public class TapEvent : MTGEvent
    {
        public TapEvent(OID source) : base(source) { }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;
            if (mtg.FindZoneFromOID(source) != mtg.battlefield)
                return false;
            var obj = mtg.objects[source];
            if (obj.permanentStatus.tapped)
                return false;
            obj.permanentStatus.tapped = true;
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.objects[source].permanentStatus.tapped = false;
        }
    }

    public class UntapEvent : MTGEvent
    {
        public UntapEvent(OID source) : base(source) { }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;
            if (mtg.FindZoneFromOID(source) != mtg.battlefield)
                return false;
            var obj = mtg.objects[source];
            if (!obj.permanentStatus.tapped)
                return false;
            obj.permanentStatus.tapped = false;
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.objects[source].permanentStatus.tapped = true;
        }
    }

    public class AddManaEvent : MTGEvent
    {
        public readonly int player;
        public readonly ManaSymbol mana;
        internal AddManaEvent(OID source, int player, ManaSymbol mana) : base(source)
        {
            this.player = player;
            this.mana = mana;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            MTG.Instance.players[player].manaPool.AddMana(mana);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.players[player].manaPool.RemoveMana(mana);
        }
    }

    public class RemoveManaEvent : MTGEvent
    {
        public readonly int player;
        public readonly ManaSymbol mana;
        public RemoveManaEvent(OID source, int player, ManaSymbol mana) : base(source)
        {
            this.player = player;
            this.mana = mana;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            return MTG.Instance.players[player].manaPool.RemoveMana(mana);
        }

        protected override void RevertAction()
        {
            MTG.Instance.players[player].manaPool.AddMana(mana);
        }
    }

    public class MoveZoneEvent : MTGEvent
    {
        public readonly Zone oldZone;
        public readonly Zone newZone;
        public readonly OID oid;
        public MoveZoneEvent(OID source, Zone oldZone, Zone newZone, OID oid) : base(source)
        {
            this.oldZone = oldZone;
            this.newZone = newZone;
            this.oid = oid;
        }
        protected override bool ApplyAction()
        {
            if (oldZone == newZone)
                return false;
            if (MTG.Instance.FindZoneFromOID(oid) != oldZone)
                return false;

            MTG.Instance.MoveZone(oid, oldZone, newZone);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.MoveZone(oid, newZone, oldZone);
        }

        protected override bool SelfRevertable
        {
            get {
                if (oldZone is Library)
                    return false;
                if (newZone is Library)
                    return false;
                return true;
            }
        }
    }

    public class GainLifeEvent : MTGEvent
    {
        public readonly int player;
        public readonly int amount;

        public GainLifeEvent(OID source, int player, int amount) : base(source)
        {
            this.player = player;
            this.amount = amount;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            if (amount <= 0)
                return false;

            MTG.Instance.players[player].ChangeLife(amount);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.players[player].ChangeLife(-amount);
        }
    }

    public class LoseLifeEvent : MTGEvent
    {
        public readonly int player;
        public readonly int amount;

        public LoseLifeEvent(OID source, int player, int amount) : base(source)
        {
            this.player = player;
            this.amount = amount;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            if (amount <= 0)
                return false;

            MTG.Instance.players[player].ChangeLife(-amount);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.players[player].ChangeLife(amount);
        }
    }

    public class DealDamageEvent : MTGEvent
    {
        public readonly PlayerOrOID target;
        public readonly int amount;

        public DealDamageEvent(OID source, PlayerOrOID target, int amount) : base(source)
        {
            this.target = target;
            this.amount = amount;
        }

        protected override bool SelfRevertable => false;

        protected override bool ApplyAction()
        {
            if (amount <= 0)
                return false;
            switch (target.type)
            {
                case (PlayerOrOID.ValueType.Player):
                    PushChild(new LoseLifeEvent(source, target.Player, amount));
                    break;
                case (PlayerOrOID.ValueType.OID):
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            return true;
        }
    }

    public class DrawCardEvent : MTGEvent
    {
        public readonly int player;

        internal DrawCardEvent(OID source, int player) : base(source)
        {
            this.player = player;
        }

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;
            var library = mtg.players[player].library;
            var hand = mtg.players[player].hand;
            OID oid = library.Get(0);
            if (oid == null)
                return false;
            return PushChild(new MoveZoneEvent(source, library, hand, oid));
        }

        protected override bool SelfRevertable => true;
    }

    public class DiscardCardEvent : MTGEvent
    {
        public readonly OID oid;

        internal DiscardCardEvent(OID source, OID oid) : base(source)
        {
            this.oid = oid;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;

            Zone currentHand = mtg.FindZoneFromOID(oid);
            Player currentPlayer = null;
            foreach (var player in mtg.players)
            {
                if (player.hand == currentHand)
                {
                    currentPlayer = player;
                    break;
                }
            }
            if (currentPlayer == null)
                return false;

            return PushChild(new MoveZoneEvent(source, currentHand, currentPlayer.graveyard, oid));
        }
    }

    public class EffectEvent : MTGEvent
    {
        public delegate bool PushEventCallback(MTGEvent newEvent);

        public bool callback(MTGEvent newEvent)
        {
            return PushChild(newEvent);
        }

        public delegate void Effect(OID source, List<Target> targets, PushEventCallback callback);

        public readonly Effect effect;

        public readonly List<Target> targets;

        protected override bool SelfRevertable => true;

        public EffectEvent(OID source, Effect effect, List<Target> targets) : base(source)
        {
            this.effect = effect;
            this.targets = targets;
        }

        protected override bool ApplyAction()
        {
            effect(source, targets, callback);
            return true;
        }
    }

    public abstract class MTGEvent
    {
        protected readonly LinkedList<MTGEvent> children = new LinkedList<MTGEvent>();

        public OID source { get; protected set; }

        public MTGEvent(OID source) { this.source = source; }

        protected abstract bool ApplyAction();
        protected virtual void RevertAction() { }

        protected bool PushChild(MTGEvent child)
        {
            var result = MTG.Instance.PushEvent(child);
            if (result)
                children.AddLast(child);
            return result;
        }

        protected void RevertAllChildren()
        {
            while (children.Last != null)
            {
                children.Last.Value.Revert();
                children.RemoveLast();
            }
        }

        protected void CheckTriggers()
        {
            foreach (var entry in MTG.Instance.TriggeredAbilities(this))
            {
                if (entry.ability.DoesTrigger(entry.source, this))
                {
                    PushChild(new PushTriggeredAbilityEvent(
                        source, entry
                    ));
                }
            }
        }

        public virtual bool Apply()
        {
            bool result = ApplyAction();
            if (result)
                CheckTriggers();
            return result;
        }

        protected abstract bool SelfRevertable { get; }

        protected virtual bool Revertable { get
            {
                if (!SelfRevertable)
                    return false;
                foreach (var child in children)
                    if (!child.Revertable)
                        return false;
                return true;
            }
        }

        public virtual void Revert()
        {
            if (!Revertable)
            {
                Console.WriteLine($"{GetType().Name} did not revert!");
                return;
            }
                
            RevertAction();
            RevertAllChildren();
            Console.WriteLine($"{GetType().Name} reverted!");
        }
    }
}
