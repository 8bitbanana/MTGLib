using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    using effectdef = Action<OID, List<Target>>;

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

        protected ResolutionAbility reverseResolution;

        public ManaAbility(Cost[] costs, Func<OID, bool> condition, effectdef[] effects, effectdef[] reverseEffects)
            : base(costs, condition, effects, null)
        {
            reverseResolution = new ResolutionAbility(reverseEffects);
        }
        public ManaAbility(Cost[] costs, effectdef[] effects, effectdef[] reverseEffects)
            : this(costs, null, effects, reverseEffects) { }

        public OID GenerateReverseAbility(OID source)
        {
            AbilityObject obj = new AbilityObject(
                source, reverseResolution, AbilityObject.AbilityType.Activated
            );
            return MTG.Instance.CreateObject(obj);
        }

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

        protected List<Cost> costs = new List<Cost>();

        private List<int> paidCosts = new List<int>();

        protected ResolutionAbility resolution;
        protected Func<OID, bool> condition;

        public ActivatedAbility(Cost[] costs, Func<OID, bool> condition, effectdef[] effects, Target[] targets)
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

        public ActivatedAbility(Cost[] costs, effectdef[] effects)
            : this(costs, null, effects, null) { }

        public ActivatedAbility(Cost[] costs, effectdef[] effects, Target[] targets)
            : this(costs, null, effects, targets) { }

        public bool CanBeActivated(OID source)
        {
            if (!condition(source)) return false;
            if (!CanPayCosts(source)) return false;
            return true;
        }
        
        public bool DeclareTargets(OID source)
        {
            return resolution.DeclareTargets(source);
        }

        public bool CanPayCosts(OID source)
        {
            foreach (var cost in costs)
            {
                if (!cost.CanPay(source)) return false;
            }
            return true;
        }

        public bool PayCosts(OID source)
        {
            paidCosts.Clear();
            for (int i=0; i<costs.Count; i++)
            {
                var cost = costs[i];
                bool result = cost.Pay(source);
                if (result)
                {
                    paidCosts.Add(i);
                } else
                {
                    RepayPaidCosts(source);
                    return false;
                }
            }
            return true;
        }

        public void RepayPaidCosts(OID source)
        {
            foreach (int i in paidCosts)
            {
                var cost = costs[i];
                cost.ReversePay(source);
            }
        }

        public OID GenerateAbility(OID source)
        {
            AbilityObject obj = new AbilityObject(
                source, resolution, AbilityObject.AbilityType.Activated
            );
            return MTG.Instance.CreateObject(obj);
        }
    }

    public class ResolutionAbility
    {
        List<effectdef> effects = new List<effectdef>();

        List<Target> targets = new List<Target>();

        public ResolutionAbility() { }

        public ResolutionAbility(effectdef[] resolutionEffects)
            : this(resolutionEffects, null) { }

        public ResolutionAbility(effectdef[] resolutionEffects, Target[] targets)
        {
            effects.AddRange(resolutionEffects);
            if (targets != null)
                this.targets.AddRange(targets);
        }

        public bool DeclareTargets(OID source)
        {
            foreach (var target in targets)
            {
                if (!target.Declare(source))
                    return false;
            }
            return true;
        }

        public void Resolve(OID source)
        {
            foreach (var target in targets)
            {
                if (!target.Declared)
                {
                    throw new ArgumentException("One or more targets were not declared.");
                }
            }
            foreach (var effect in effects)
            {
                effect(source, targets);
            }
        }
    }
}
