using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class Modification
    {
        public enum Operation { Subtract = -1, Override = 0, Add = 1 }
    }

    public abstract class Modification<T> : Modification
    {
        protected readonly T value;
        public virtual T Modify(T input)
        {
            return value;
        }
    }

    public class TypeMod<T> : Modification<HashSet<T>>
    {
        protected readonly T[] typesToAdd;
        protected readonly T[] typesToRemove;

        public override HashSet<T> Modify(HashSet<T> input)
        {
            var output = new HashSet<T>(input);
            foreach (T type in typesToAdd)
            {
                output.Add(type);
            }
            foreach(T type in typesToRemove)
            {
                output.Remove(type);
            }
            return output;
        }
    }

    public class NameMod : Modification<string> { }

    public class ManaCostMod : Modification<ManaCost>
    {
        protected readonly Operation op = Operation.Override;
        public override ManaCost Modify(ManaCost input)
        {
            switch (op)
            {
                case Operation.Override:
                    return value;
                case Operation.Add:
                    return input + value;
                case Operation.Subtract:
                    return input - value;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class IntMod : Modification<int>
    {
        protected readonly Operation op = Operation.Override;
        public override int Modify(int input)
        {
            switch (op)
            {
                case Operation.Override:
                    return value;
                case Operation.Add:
                    return input + value;
                case Operation.Subtract:
                    return input - value;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
