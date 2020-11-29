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

    public class ManaAbility : ActivatedAbility
    {
        protected new static bool DefaultCondition(OID oid)
        {
            return BattlefieldCondition(oid) && ControllerCondition(oid);
        }

        public ManaAbility(CostEvent[] costs, Func<OID, bool> condition, EffectEvent.Effect[] effects)
            : base(costs, condition, effects, null)
        {
            
        }
        public ManaAbility(CostEvent[] costs, EffectEvent.Effect[] effects)
            : this(costs, null, effects) { }



        protected override void SetConditionIfNull()
        {
            if (condition == null)
                this.condition = ManaAbility.DefaultCondition;
        }
    }

    public class ActivatedAbility
    {
        protected static bool SorceryCondition(OID oid)
        {
            return MTG.Instance.CanCastSorceries;
        }

        protected static bool BattlefieldCondition(OID oid)
        {
            MTG mtg = MTG.Instance;
            return mtg.FindZoneFromOID(oid) == mtg.battlefield;
        }

        protected static bool ControllerCondition(OID oid)
        {
            MTG mtg = MTG.Instance;
            return mtg.objects[oid].attr.controller == mtg.turn.playerPriorityIndex;
        }

        protected static bool DefaultCondition(OID oid)
        {
            return BattlefieldCondition(oid) && ControllerCondition(oid);
        }

        protected List<CostEvent> costs = new List<CostEvent>();

        public readonly ResolutionAbility resolution;
        protected Func<OID, bool> condition;

        public IEnumerable<CostEvent> Costs { get
            {
                foreach (var cost in costs) yield return cost;
            }
        }

        public ActivatedAbility(CostEvent[] costs, Func<OID, bool> condition, EffectEvent.Effect[] effects, Target[] targets)
        {
            this.costs.AddRange(costs);
            this.condition = condition;
            resolution = new ResolutionAbility(effects, targets);
            SetConditionIfNull();
        }

        protected virtual void SetConditionIfNull()
        {
            if (condition == null)
                this.condition = ActivatedAbility.DefaultCondition;
        }

        public bool CanBeActivated(OID source)
        {
            if (!condition(source))
                return false;
            foreach (var cost in costs)
                if (!cost.CanPay(source))
                    return false;
            return true;
        }

        public ActivatedAbility(CostEvent[] costs, EffectEvent.Effect[] effects)
            : this(costs, null, effects, null) { }

        public ActivatedAbility(CostEvent[] costs, EffectEvent.Effect[] effects, Target[] targets)
            : this(costs, null, effects, targets) { }

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
