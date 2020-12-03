using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class TapSelfCostEvent : CostEvent
    {
        public TapSelfCostEvent() : base() { }

        protected override bool IsPaymentPossible { get
            {
                return !MTG.Instance.objects[source].permanentStatus.tapped;
            }
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            return PushChild(new TapEvent(source));
        }
    }

    public class PayManaCostEvent : CostEvent
    {
        private ManaSymbol mana;

        public PayManaCostEvent(ManaSymbol mana) : base()
        {
            this.mana = mana;
        }

        protected override bool IsPaymentPossible => true;

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;
            int player = GetPlayerFromSource();
            ManaPool manaPool = mtg.players[player].manaPool;
            bool manaPaid = false;
            while (!manaPaid)
            {
                List<ManaSymbol> possibleMana = new List<ManaSymbol>();
                foreach (var manaPoolMana in manaPool)
                {
                    if (mana.CanThisPayForMe(manaPoolMana))
                        possibleMana.Add(manaPoolMana);
                }
                var choice = new ManaChoice(possibleMana, mana, player);
                mtg.PushChoice(choice);
                if (choice.Cancelled)
                {
                    RevertAllChildren();
                    return false;
                }

                var chosen = choice.FirstChoice;

                switch (chosen.type)
                {
                    case (ManaChoiceOption.OptionType.UseMana):
                        if (!PushChild(new RemoveManaEvent(source, player, chosen.manaSymbol)))
                        {
                            RevertAllChildren();
                            return false;
                        }
                        manaPaid = true;
                        break;
                    case (ManaChoiceOption.OptionType.ActivateManaAbility):
                        if (!PushChild(new ActivateAbilityEvent(chosen.manaAbilitySource, chosen.manaAbility)))
                        {
                            RevertAllChildren();
                            return false;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return true;
        }
    }

    // A cost is disconnected from it's source
    public abstract class CostEvent : MTGEvent
    {

        public CostEvent() : base(null)
        {
            
        }

        protected int GetPlayerFromSource()
        {
            return MTG.Instance.objects[source].attr.controller;
        }

        public void SetSource(OID source)
        {
            this.source = source;
        }

        public bool CanPay(OID source)
        {
            this.source = source;
            return IsPaymentPossible;
        }

        protected abstract bool IsPaymentPossible { get; }

        // A cost doesn't care that one of it's children was irreversable.
        public sealed override void Revert()
        {
            Console.WriteLine($"{GetType().Name} reverted!");
            RevertAllChildren();
            RevertAction();
        }
    }
}
