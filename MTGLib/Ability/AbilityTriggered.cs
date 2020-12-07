using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public abstract class TriggeredAbility {

        public delegate bool ActiveCondition(OID source);
        protected ActiveCondition activeCondition;
        public virtual bool IsActive(OID source)
        {
            return activeCondition(source);
        }

        public abstract Type EventType { get; }

        public abstract Type MyType { get; }

        protected static bool DefaultCondition(OID source)
        {
            return MTG.Instance.FindZoneFromOID(source) == MTG.Instance.battlefield;
        }

        protected virtual void SetActiveConditionIfNull()
        {
            if (activeCondition == null)
                activeCondition = DefaultCondition;
        }

        public abstract bool DoesTrigger(OID source, MTGEvent mtgevent);

        public ResolutionAbility resolution { get; protected set; }
    }

    public class TriggeredAbility<T> : TriggeredAbility where T : MTGEvent
    {
        public delegate bool TriggerCondition(OID source, T mtgevent);

        protected TriggerCondition triggerCondition;
         
        public override Type EventType
        {
            get { return typeof(T); }
        }

        public override Type MyType
        {
            get { return typeof(TriggeredAbility<T>); }
        }

        public TriggeredAbility(ActiveCondition activeCondition, TriggerCondition triggerCondition, EffectEvent.Effect[] effects, Target[] targets)
        {
            this.activeCondition = activeCondition;
            this.triggerCondition = triggerCondition;
            resolution = new ResolutionAbility(effects, targets);
            SetActiveConditionIfNull();
        }

        public override bool DoesTrigger(OID source, MTGEvent mtgevent)
        {
            return triggerCondition(source, (T)mtgevent);
        }

        public TriggeredAbility(TriggerCondition triggerCondition, EffectEvent.Effect[] effects, Target[] targets)
            : this(null, triggerCondition, effects, targets) { }

        public TriggeredAbility(TriggerCondition triggerCondition, EffectEvent.Effect[] effects)
            : this(null, triggerCondition, effects, null) { }

        public TriggeredAbility(ActiveCondition activeCondition, TriggerCondition triggerCondition, EffectEvent.Effect[] effects)
            : this(activeCondition, triggerCondition, effects, null) { }
    }
}
