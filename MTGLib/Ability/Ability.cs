using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class StaticAbility
    {
        public delegate bool ActiveCondition(OID source);
        ActiveCondition activeCondition;

        List<Modification> modifications = new List<Modification>();

        public StaticAbility(ActiveCondition activeCondition, params Modification[] mods)
        {
            this.activeCondition = activeCondition;
            modifications.AddRange(mods);
            SetActiveConditionIfNull();
        }

        public StaticAbility(params Modification[] modifications)
            : this(null, modifications) { }

        public IReadOnlyList<Modification> GetModifications()
        {
            return modifications.AsReadOnly();
        }

        public bool IsActive(OID source)
        {
            return activeCondition(source);
        }

        protected static bool DefaultCondition(OID source)
        {
            return MTG.Instance.FindZoneFromOID(source) == MTG.Instance.battlefield;
        }

        protected virtual void SetActiveConditionIfNull()
        {
            if (activeCondition == null)
                activeCondition = DefaultCondition;
        }
    }

    public class ResolutionAbility
    {
        List<EffectEvent.Effect> effects = new List<EffectEvent.Effect>();

        List<Target> targets = new List<Target>();

        public ResolutionAbility() { }

        public ResolutionAbility(EffectEvent.Effect[] resolutionEffects)
            : this(resolutionEffects, null) { }

        public ResolutionAbility(EffectEvent.Effect[] resolutionEffects, Target[] targets)
        {
            effects.AddRange(resolutionEffects);
            if (targets != null)
                this.targets.AddRange(targets);
        }

        public IEnumerable<Target> Targets { get {
                foreach (var target in targets)
                    yield return target;
            }
        }

        public void Resolve(OID source)
        {
            foreach (var target in targets)
            {
                if (!target.Declared)
                {
                    throw new InvalidOperationException("Targets are not declared for resolution.");
                }
            }
            foreach (var effect in effects)
            {
                MTG.Instance.PushEvent(new EffectEvent(source, effect, targets));
            }
        }

        public IEnumerable<EffectEvent> GetResolutionEvents(OID source)
        {
            foreach (var target in targets)
            {
                if (!target.Declared)
                {
                    throw new InvalidOperationException("Targets are not declared for resolution.");
                }
            }
            foreach (var effect in effects)
            {
                yield return new EffectEvent(source, effect, targets);
            }
        }
    }
}
