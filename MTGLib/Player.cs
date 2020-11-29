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
    }
}
