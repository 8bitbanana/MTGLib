using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class StackItem {
        public abstract void Resolve();
    }

    public class StackAbility : StackItem
    {
        public override void Resolve()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class StackCard : StackItem
    {
        public readonly OID oid;

        public StackCard(OID newoid) { oid = newoid; }

        public override void Resolve()
        {
            MTG.Instance.objects[oid].Resolve();
        }

        public override string ToString()
        {
            return MTG.Instance.objects[oid].ToString();
        }
    }

    public class TheStack : BaseZone<StackItem>
    {

        public void PopAndResolve()
        {
            var x = Pop();
            x.Resolve();
        }

        private StackCard GetStackCardFromOID(OID oid)
        {
            foreach(var x in _objects)
            {
                if (x is StackCard cast)
                {
                    if (cast.oid == oid)
                        return cast;
                }
            }
            return null;
        }

        public void Push(OID oid)
        {
            Push(new StackCard(oid));
        }

        public bool Has(OID oid)
        {
            return GetStackCardFromOID(oid) != null;
        }

        public void Remove(OID oid)
        {
            Remove(GetStackCardFromOID(oid));
        }

        public void PPrint()
        {
            if (Count > 0)
            {
                Console.WriteLine(" == Stack ==");
                foreach (var x in this)
                {
                    Console.WriteLine(x.ToString());
                }
            } else
            {
                Console.WriteLine("Stack is empty");
            }
        }
    }
}
