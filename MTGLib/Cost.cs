using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class CostTapSelf : Cost
    {
        public override bool CanPay(OID source)
        {
            // TODO - Summoning sickness

            MTG mtg = MTG.Instance;
            if (mtg.FindZoneFromOID(source) != mtg.battlefield)
                return false;
            if (SourceObject(source).permanentStatus.tapped)
                return false;
            return true;
        }

        protected override string GetString()
        {
            return "{Tap}";
        }

        protected override bool PayAction(OID source) 
        {
            SourceObject(source).permanentStatus.tapped = true;
            return true;
        }

        protected override void ReversePayAction(OID source)
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

        protected override string GetString()
        {
            return manaCost.ToString();
        }

        // You can always *try* to pay mana
        public override bool CanPay(OID source)
        {
            return true;
        }

        protected override bool PayAction(OID source)
        {
            Console.WriteLine($"Attempting to pay for {manaCost}");
            return SourcePlayer(source).manaPool.PayFor(manaCost);
        }

        protected override void ReversePayAction(OID source)
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

        public bool Pay(OID source)
        {
            if (!CanPay(source))
                return false;
            return PayAction(source);
        }

        public void ReversePay(OID source)
        {
            ReversePayAction(source);
        }

        public override sealed string ToString()
        {
            return GetString();
        }

        protected abstract string GetString();

        public abstract bool CanPay(OID source);

        protected abstract bool PayAction(OID source);

        protected abstract void ReversePayAction(OID source);
    }
}
