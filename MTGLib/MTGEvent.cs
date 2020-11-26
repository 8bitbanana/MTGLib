using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class AddManaEvent : MTGEvent
    {
        public readonly int player;
        public readonly ManaSymbol mana;
        internal AddManaEvent(OID source, int player, ManaSymbol mana) : base(source)
        {
            this.player = player;
            this.mana = mana;
        }

        protected override bool ApplyAction()
        {
            MTG.Instance.players[player].manaPool.AddMana(mana);
            return true;
        }

        protected override bool RevertAction()
        {
            MTG.Instance.players[player].manaPool.RemoveMana(mana);
            return true;
        }
    }

    public class RemoveManaEvent : MTGEvent
    {
        public readonly int player;
        public readonly ManaSymbol mana;
        internal RemoveManaEvent(OID source, int player, ManaSymbol mana) : base(source)
        {
            this.player = player;
            this.mana = mana;
        }

        protected override bool ApplyAction()
        {
            return MTG.Instance.players[player].manaPool.RemoveMana(mana);
        }

        protected override bool RevertAction()
        {
            MTG.Instance.players[player].manaPool.AddMana(mana);
            return true;
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

        protected override bool RevertAction()
        {
            return false;
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
            return ApplyChild(new MoveZoneEvent(source, library, hand, oid));
        }

        protected override bool RevertAction()
        {
            return true;
        }
    }

    public abstract class MTGEvent
    {
        private LinkedList<MTGEvent> children;

        public readonly OID source;

        public MTGEvent(OID source) { this.source = source; }

        protected abstract bool ApplyAction();
        protected abstract bool RevertAction();

        protected bool ApplyChild(MTGEvent child)
        {
            children.AddLast(child);
            return child.Apply();
        }

        protected void RevertAllChildren()
        {
            while (children.Last != null)
            {
                children.Last.Value.Revert();
                children.RemoveLast();
            }
        }

        public bool Apply()
        {
            return ApplyAction();
        }

        public bool Revert()
        {
            foreach (var child in children)
                if (!child.Revert())
                    return false;
            return RevertAction();
        }
    }
}
