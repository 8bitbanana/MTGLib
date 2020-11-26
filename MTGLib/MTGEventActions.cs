using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class ActivateAbilityEvent : MTGEvent
    {
        public readonly ActivatedAbility ability;

        public ActivateAbilityEvent(OID source, ActivatedAbility ability) : base(source)
        {
            this.ability = ability;
        }

        protected override bool ApplyAction()
        {
            throw new NotImplementedException();
        }

        protected override bool RevertAction()
        {
            throw new NotImplementedException();
        }
    }

    public class CastSpellEvent : MTGEvent
    {
        public OID oid;

        public CastSpellEvent(OID oid) : base(null)
        {
            this.oid = oid;
        }
        protected override bool ApplyAction()
        {
            throw new NotImplementedException();
        }

        protected override bool RevertAction()
        {
            throw new NotImplementedException();
        }
    }
}
