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

        protected List<Action<OID>> reverseEffects = new List<Action<OID>>();

        public ManaAbility(Func<OID, bool> condition, Action<OID>[] effects, Action<OID>[] reverseEffects)
            : base(condition, effects)
        {
            this.reverseEffects.AddRange(reverseEffects);
        }
        public ManaAbility(Action<OID>[] effects, Action<OID>[] reverseEffects)
            : this(null, effects, reverseEffects) { }

        public OID GenerateReverseAbility(OID source)
        {
            ResolutionAbility resolution = new ResolutionAbility(reverseEffects.ToArray());
            AbilityObject obj = new AbilityObject(
                source, resolution, AbilityObject.AbilityType.Activated
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

        protected List<Action<OID>> effects = new List<Action<OID>>();
        protected Func<OID, bool> condition;

        public ActivatedAbility(Func<OID, bool> condition, params Action<OID>[] effects)
        {
            this.condition = condition;
            this.effects.AddRange(effects);
            SetConditionIfNull();
        }

        protected virtual void SetConditionIfNull()
        {
            if (condition == null)
                this.condition = ActivatedAbility.DefaultCondition;
        }

        public ActivatedAbility(params Action<OID>[] effects)
            : this(null, effects) { }

        public bool CanBeActivated(OID source)
        {
            return condition(source);
        }

        public OID GenerateAbility(OID source)
        {
            ResolutionAbility resolution = new ResolutionAbility(effects.ToArray());
            AbilityObject obj = new AbilityObject(
                source, resolution, AbilityObject.AbilityType.Activated
            );
            return MTG.Instance.CreateObject(obj);
        }
    }

    public class ResolutionAbility
    {
        List<Action<OID>> effects = new List<Action<OID>>();

        public ResolutionAbility() { }

        public ResolutionAbility(params Action<OID>[] resolutionEffects)
        {
            effects.AddRange(resolutionEffects);
        }

        public void AddEffect(Action<OID> effect)
        {
            effects.Add(effect);
        }

        public void Resolve(OID source)
        {
            foreach(var effect in effects)
            {
                effect(source);
            }
        }
    }
}
