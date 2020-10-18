using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public enum Counter
    {
        PlusOnePlusOne,
        MinusOneMinusOne,
        Loyalty
    }

    public class CounterStore
    {
        private readonly Dictionary<Counter, int> counters = new Dictionary<Counter, int>();

        public void Add(Counter counter, int amount = 1)
        {
            if (counters.ContainsKey(counter))
            {
                counters[counter] += amount;
            } else
            {
                counters.Add(counter, amount);
            }
        }

        public bool Has(Counter counter)
        {
            return counters.ContainsKey(counter);
        }

        public int Count(Counter counter)
        {
            if (counters.ContainsKey(counter))
            {
                return counters[counter];
            } else
            {
                return 0;
            }
        }

        public bool Remove(Counter counter, int amount)
        {
            if (counters.ContainsKey(counter))
            {
                if (counters[counter] > amount)
                {
                    counters[counter] -= amount;
                    return true;
                } else if (counters[counter] == amount)
                {
                    counters.Remove(counter);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveAll(Counter counter)
        {
            if (counters.ContainsKey(counter))
            {
                counters.Remove(counter);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            counters.Clear();
        }
    }
}
