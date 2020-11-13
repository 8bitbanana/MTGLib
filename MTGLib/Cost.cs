using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class CostTapSelf : Cost
    {
        public override bool Pay(OID source)
        {
            MTG mtg = MTG.Instance;
            if (mtg.FindZoneFromOID(source) != mtg.battlefield)
                return false;
            if (SourceObject(source).permanentStatus.tapped)
                return false;

            // TODO - Summoning sickness

            SourceObject(source).permanentStatus.tapped = true;
            return true;
        }

        public override void ReversePay(OID source)
        {
            SourceObject(source).permanentStatus.tapped = false;
        }
    }

    public class CostPayMana : Cost
    {
        ManaCost manaCost;

        public CostPayMana(ManaCost manaCost)
        {
            this.manaCost = manaCost;
        }

        public override bool Pay(OID source)
        {
            return SourcePlayer(source).manaPool.PayFor(manaCost);
        }

        public override void ReversePay(OID source)
        {
            SourcePlayer(source).manaPool.AddMana(manaCost);
        }
    }

    public abstract class Cost
    {
        protected static MTGObject SourceObject(OID source)
        {
            return MTG.Instance.objects[source];
        }

        protected static Player SourcePlayer(OID source)
        {
            return MTG.Instance.players[SourceObject(source).attr.controller];
        }

        public abstract bool Pay(OID source);

        public abstract void ReversePay(OID source);
    }
}
