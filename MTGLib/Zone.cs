using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MTGLib
{
    public class Zone
    {
        protected LinkedList<OID> _objects = new LinkedList<OID>();
        public Zone() { }

        public int Count
        {
            get { return _objects.Count; }
        }
       
        public IEnumerator<OID> Objects
        {
            get { return _objects.GetEnumerator(); }
        }

        public void Add(OID oid, int index=0)
        {
            if (Has(oid))
                throw new ArgumentException("OID is already in this zone");
            if (index == 0)
            {
                _objects.AddFirst(oid);
            } else if (index >= _objects.Count)
            {
                _objects.AddLast(oid);
            } else
            {
                _objects.AddBefore(GetNode(index), oid);
            }
        }

        public void Shuffle()
        {
            int count = _objects.Count;
            Random rand = new Random();
            _objects = new LinkedList<OID>(_objects.OrderBy(
                (o) => { return rand.Next() % count; }
            ));
        }

        private LinkedListNode<OID> GetNode(int index)
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

        public OID Pop()
        {
            OID cid = _objects.First.Value;
            _objects.RemoveFirst();
            return cid;
        }

        public void Push(OID oid)
        {
            _objects.AddFirst(oid);
        }

        public OID Get(int index=0)
        {
            return GetNode(index).Value;
        }

        public bool Has(OID oid)
        {
            return _objects.Contains(oid);
        }

        public void Remove(OID oid)
        {
            bool result = _objects.Remove(oid);
            if (!result)
                throw new ArgumentException("OID is not in this zone");
        }
    }

    public class Hand : Zone
    {
        int maxSize = 7;
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
