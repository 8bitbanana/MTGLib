using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace MTGLib
{
    public abstract class BaseZone
    {

    }

    public abstract class BaseZone<T> : BaseZone, IEnumerable<T>
    {
        protected LinkedList<T> _objects = new LinkedList<T>();
        protected HashSet<T> _existenceMap = new HashSet<T>();

        public int Count
        {
            get { return _objects.Count; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_objects).GetEnumerator();
        }

        public virtual void Add(T item, int index = 0)
        {
            if (Has(item))
                throw new ArgumentException("OID is already in this zone");
            if (index == 0)
            {
                _objects.AddFirst(item);
            }
            else if (index >= _objects.Count)
            {
                _objects.AddLast(item);
            }
            else
            {
                _objects.AddBefore(GetNode(index), item);
            }
            _existenceMap.Add(item);
        }

        public void Shuffle()
        {
            int count = _objects.Count;
            Random rand = new Random();
            _objects = new LinkedList<T>(_objects.OrderBy(
                (o) => { return rand.Next() % count; }
            ));
        }

        private LinkedListNode<T> GetNode(int index)
        {
            int count = _objects.Count;
            if (index == 0)
                return _objects.First;
            else if (index == count - 1)
                return _objects.Last;
            else if (index < 0)
                throw new IndexOutOfRangeException();
            else if (index >= count)
                throw new IndexOutOfRangeException();
            else
            {
                var node = _objects.First;
                while (index > 0)
                {
                    node = node.Next;
                    index--;
                }
                return node;
            }
        }

        public virtual T Pop()
        {
            T item = _objects.First.Value;
            _objects.RemoveFirst();
            _existenceMap.Remove(item);
            return item;
        }

        public virtual void Push(T item)
        {
            Add(item, 0);
        }

        public virtual T Get(int index = 0)
        {
            return GetNode(index).Value;
        }

        public virtual bool Has(T item)
        {
            return _existenceMap.Contains(item);
        }

        public void Remove(T item)
        {
            bool result = _objects.Remove(item);
            if (!result)
                throw new ArgumentException("Item is not in this zone");
            _existenceMap.Remove(item);
        }
    }

    public class Zone : BaseZone<OID>
    {
        
    }

    public class Hand : Zone
    {
        int maxSize = 7;

        public int DiscardsNeeded { get
            {
                if (Count <= maxSize)
                {
                    return 0;
                }
                else
                {
                    return Count - maxSize;
                }
            }
        }
    }

    public class Library : Zone
    {

    }

    public class Battlefield : Zone
    {

    }

    public class Graveyard : Zone
    {

    }

    public class Exile : Zone
    {

    }
}
