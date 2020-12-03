using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class TriggeredAbility
    {

    }

    public class TriggeredAbility<T> : TriggeredAbility where T : MTGEvent
    {

    }
}
