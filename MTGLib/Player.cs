using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class Player
    {
        public bool hasLost = false;

        public Library library = new Library();
        public Hand hand = new Hand();
        public Graveyard graveyard = new Graveyard();

        public int life = 20;

        public ManaPool manaPool = new ManaPool();

        public CounterStore counters = new CounterStore();

        public Player()
        {

        }

        public void ChangeLife(int delta)
        {
            life += delta;
        }

        public void Draw(int count = 1)
        {
            while (count-- > 0)
            {
                OID oid = library.Get(0);
                if (oid != null)
                    MTG.Instance.MoveZone(oid, library, hand);
            }
        }

        public void Mill(int count = 1)
        {
            while (count-- > 0)
            {
                OID oid = library.Get(0);
                if (oid != null)
                    MTG.Instance.MoveZone(oid, library, graveyard);
            }
        }

        public void Discard(int count = 1)
        {
            if (count < 1) return;
            if (count < hand.Count)
            {
                OIDChoice choice = new OIDChoice
                {
                    Options = new List<OID>(),
                    Min = count,
                    Max = count,
                    Title = $"Discard {count} card(s)."
                };
                choice.Options.AddRange(hand);
                MTG.Instance.PushChoice(choice);
                foreach (var card in choice.Chosen)
                {
                    MTG.Instance.MoveZone(card, hand, graveyard);
                }
            } else
            {
                foreach (var card in new List<OID>(hand))
                {
                    MTG.Instance.MoveZone(card, hand, graveyard);
                }
            }
            
        }
    }
}
