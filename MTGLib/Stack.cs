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
    }

    public class StackCard : StackItem
    {
        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }

    public class TheStack : Zone
    {
        public Stack<StackItem> stack = new Stack<StackItem>();

        public void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
