using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class Choice<T>
    {
        protected Choice() { }
        public virtual bool Resolve(T chosen)
        {
            if (!Verify(chosen))
            {
                return false;
            }

            // ????

            return true;
        }
        protected abstract bool Verify(T chosen);
    }

    public class ObjectChoice : Choice<List<OID>>
    {
        enum ChoiceType {ANY, MINMAX}

        ChoiceType choiceType;
        int min; int max;

        protected override bool Verify(List<OID> oids)
        {
            switch (choiceType)
            {
                case ChoiceType.ANY:
                    return true;
                case ChoiceType.MINMAX:
                    if (oids.Count < min)
                        return false;
                    if (oids.Count > max)
                        return false;
                    return true;
                default:
                    throw new ArgumentException();
            }
        }

        public static ObjectChoice ChooseAny()
        {
            var obj = new ObjectChoice();
            obj.choiceType = ChoiceType.ANY;
            return obj;
        }
        public static ObjectChoice ChooseExact(int amount)
        {
            return ObjectChoice.ChooseMinMax(amount, amount);
        }
        public static ObjectChoice ChooseMinMax(int min, int max)
        {
            var obj = new ObjectChoice();
            obj.choiceType = ChoiceType.MINMAX;
            obj.min = min; obj.max = max;
            return obj;
        }
    }
}
