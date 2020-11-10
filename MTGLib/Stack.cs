using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{

    public class TheStack : Zone
    {

        public void PopAndResolve()
        {
            var x = Pop();
            x.Resolve();
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
