using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class StaticAbility
    {
        List<Modification> modifications = new List<Modification>();

        public StaticAbility() {}

        public StaticAbility(params Modification[] mods)
        {
            modifications.AddRange(mods);
        }

        public void AddModification(Modification mod)
        {
            modifications.Add(mod);
        }

        public IReadOnlyList<Modification> GetModifications()
        {
            return modifications.AsReadOnly();
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
