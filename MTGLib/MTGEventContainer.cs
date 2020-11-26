using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
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
                if (!ApplyChild(evn))
                    allDone = false;
            }
            return allDone;
        }

        protected override bool RevertAction()
        {
            return true;
        }
    }
}
