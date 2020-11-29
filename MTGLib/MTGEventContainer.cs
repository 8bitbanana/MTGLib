using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{

    public class EventContainerPayManaCost : MTGCostEventContainer<PayManaCostEvent>
    {
        public static EventContainerPayManaCost Auto(ManaCost manaCost)
        {
            int count = 0;
            foreach (var mana in manaCost)
                count++;

            var list = new PayManaCostEvent[count];
            int index = 0;
            foreach (var mana in manaCost)
            {
                list[index++] = new PayManaCostEvent(mana);
            }
            return new EventContainerPayManaCost(list);
        }


        public EventContainerPayManaCost(params PayManaCostEvent[] events) : base(events) { }
    }

    public class EventContainerDrawCards : MTGEventContainer<DrawCardEvent>
    {
        public static EventContainerDrawCards Auto(OID source, int player, int count)
        {
            var list = new DrawCardEvent[count];
            for (int i=0; i<count; i++)
                list[i] = new DrawCardEvent(source, player);
            return new EventContainerDrawCards(source, list);
        }

        public EventContainerDrawCards(OID source, params DrawCardEvent[] events) : base(source, events) { }
    }

    public class EventContainerDiscardCards : MTGEventContainer<DiscardCardEvent>
    {
        public static EventContainerDiscardCards Auto(OID source, params OID[] oids)
        {
            var list = new DiscardCardEvent[oids.Length];
            for (int i = 0; i < oids.Length; i++)
                list[i] = new DiscardCardEvent(source, oids[i]);
            return new EventContainerDiscardCards(source, list);
        }

        public static EventContainerDiscardCards Auto(OID source, int player, int count)
        {
            var mtg = MTG.Instance;
            OIDChoice choice = new OIDChoice
            {
                Options = new List<OID>(mtg.players[player].hand),
                Max = count,
                Min = count,
                Title = $"Discard {count} card(s)."
            };
            MTG.Instance.PushChoice(choice);
            return Auto(source, choice.Chosen.ToArray());
        }

        public EventContainerDiscardCards(OID source, params DiscardCardEvent[] events) : base(source, events) { }
    }

    public abstract class MTGCostEventContainer<T> : CostEvent where T : CostEvent
    {
        private readonly T[] events;

        public MTGCostEventContainer(params T[] events) : base()
        {
            this.events = events;
        }

        protected override bool IsPaymentPossible { get
            {
                foreach (var evnt in events)
                {
                    if (!evnt.CanPay(source)) return false;
                }
                return true;
            }
        }

        protected override bool ApplyAction()
        {
            if (events.Length == 0)
                return false;
            bool allDone = true;
            foreach (T evn in events)
            {
                if (!PushChild(evn))
                    allDone = false;
            }
            return allDone;
        }

        protected override bool SelfRevertable => true;
    }

    public abstract class MTGEventContainer<T> : MTGEvent where T : MTGEvent
    {
        private readonly T[] events;

        public MTGEventContainer(OID source, params T[] events) : base(source)
        {
            this.events = events;
        }

        protected override bool ApplyAction()
        {
            bool allDone = true;
            foreach (T evn in events)
            {
                if (!PushChild(evn))
                    allDone = false;
            }
            return allDone;
        }

        protected override bool SelfRevertable => true;
    }
}
