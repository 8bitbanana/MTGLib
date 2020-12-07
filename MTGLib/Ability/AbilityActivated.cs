using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class ManaAbility : ActivatedAbility
    {
        protected new static bool DefaultCondition(OID oid)
        {
            return BattlefieldCondition(oid) && ControllerCondition(oid);
        }

        public ManaAbility(CostEvent.CostGen[] costs, Func<OID, bool> condition, EffectEvent.Effect[] effects)
            : base(costs, condition, effects, null)
        {

        }
        public ManaAbility(CostEvent.CostGen[] costs, EffectEvent.Effect[] effects)
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

        protected List<CostEvent.CostGen> costs = new List<CostEvent.CostGen>();

        public readonly ResolutionAbility resolution;
        protected Func<OID, bool> condition;

        public IEnumerable<CostEvent> Costs
        {
            get
            {
                foreach (var cost in costs) yield return cost();
            }
        }

        public ActivatedAbility(CostEvent.CostGen[] costs, Func<OID, bool> condition, EffectEvent.Effect[] effects, Target[] targets)
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
                if (!cost().CanPay(source))
                    return false;
            return true;
        }

        public ActivatedAbility(CostEvent.CostGen[] costs, EffectEvent.Effect[] effects)
            : this(costs, null, effects, null) { }

        public ActivatedAbility(CostEvent.CostGen[] costs, EffectEvent.Effect[] effects, Target[] targets)
            : this(costs, null, effects, targets) { }

    }
}
