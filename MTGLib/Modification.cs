using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class ControllerMod : Modification<int> { }
    public class PowerMod : IntModification { }
    public class ToughnessMod : IntModification { }
    public class ColorMod : ColorModification { }

    public class CardTypeMod : HashSetModification<MTGObject.CardType> { }
    public class SuperTypeMod : HashSetModification<MTGObject.SuperType> { }
    public class SubTypeMod : HashSetModification<MTGObject.SubType> { }

    public abstract class Modification
    {
        public enum Operation { Override, Add, Subtract }
        public Operation operation = Operation.Override;

        public OID specificOID;

        public Func<MTGObject, bool> condition;
    }

    public abstract class Modification<T> : Modification
    {
        public T value;

        public virtual T Modify(T original, OID oid, MTGObject obj)
        {
            if (specificOID != null)
            {
                if (oid != specificOID)
                {
                    return original;
                }
            }
            if (condition != null)
            {
                if (obj == null)
                {
                    throw new ArgumentException();
                }
                if (!condition(obj))
                {
                    return original;
                }
            }
            switch (operation)
            {
                case Operation.Override:
                    return Override(original);
                case Operation.Add:
                    return Add(original);
                case Operation.Subtract:
                    return Subtract(original);
                default:
                    throw new ArgumentException();
            }
        }

        protected virtual T Override(T original)
        {
            return value;
        }
        protected virtual T Add (T original)
        {
            throw new NotImplementedException();
        }
        protected virtual T Subtract (T original)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class IntModification : Modification<int>
    {
        protected override int Add(int original)
        {
            return original + value;
        }
        protected override int Subtract(int original)
        {
            return original - value;
        }
    }

    public abstract class ManaCostModification : Modification<ManaCost>
    {
        protected override ManaCost Add(ManaCost original)
        {
            return original + value;
        }
        protected override ManaCost Subtract(ManaCost original)
        {
            return original - value;
        }
    }

    public abstract class ListModification<T> : Modification<List<T>>
    {
        protected override List<T> Add(List<T> original)
        {
            List<T> toReturn = new List<T>(original);
            toReturn.AddRange(value);
            return toReturn;
        }
        protected override List<T> Subtract(List<T> original)
        {
            List<T> toReturn = new List<T>(original);
            foreach (T x in value)
            {
                toReturn.Remove(x);
            }
            return toReturn;
        }
    }

    public abstract class HashSetModification<T> : Modification<HashSet<T>>
    {
        protected override HashSet<T> Add(HashSet<T> original)
        {
            HashSet<T> toReturn = new HashSet<T>(original);
            foreach (T x in value)
            {
                toReturn.Add(x);
            }
            return toReturn;
        }
        protected override HashSet<T> Subtract(HashSet<T> original)
        {
            HashSet<T> toReturn = new HashSet<T>(original);
            foreach (T x in value)
            {
                toReturn.Remove(x);
            }
            return toReturn;
        }
    }

    public abstract class ColorModification : Modification<Color>
    {
        protected override Color Add(Color original)
        {
            return original | value;
        }
        protected override Color Subtract(Color original)
        {
            return original & ~value;
        }
    }
}
