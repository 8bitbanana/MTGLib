using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public class PlayLandEvent : MTGEvent
    {
        public PlayLandEvent(OID source) : base(source) { }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            Zone currentZone = MTG.Instance.FindZoneFromOID(source);
            return PushChild(new MoveZoneEvent(null, currentZone, MTG.Instance.battlefield, source));
        }
    }

    public class PushTriggeredAbilityEvent : MTGEvent
    {
        public readonly MTG.TriggeredAbilityEntry ability;

        public PushTriggeredAbilityEvent(OID source, MTG.TriggeredAbilityEntry ability)
            : base(source)
        {
            this.ability = ability;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            if (MTG.Instance.PendingTriggeredAbilities.Contains(ability))
                throw new InvalidOperationException("This ability is already pending.");
            MTG.Instance.PendingTriggeredAbilities.Add(ability);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.PendingTriggeredAbilities.Remove(ability);
        }
    }

    public class GenerateAbilityObjectEvent : MTGEvent
    {
        public readonly ResolutionAbility resolution;
        public readonly AbilityObject.AbilityType abilityType;

        OID objectOID;

        public GenerateAbilityObjectEvent(OID source, ResolutionAbility resolution, AbilityObject.AbilityType abilityType)
            : base(source)
        {
            this.resolution = resolution;
            this.abilityType = abilityType;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            AbilityObject obj = new AbilityObject(
                source, resolution, abilityType
            );
            objectOID = MTG.Instance.CreateObject(obj);
            MTG.Instance.theStack.Push(objectOID);
            return true;
        }

        protected override void RevertAction()
        {
            MTG.Instance.DeleteObject(objectOID);
        }
    }

    public class ResolveManaAbilityEvent : MTGEvent
    {
        ManaAbility ability;

        public ResolveManaAbilityEvent(OID source, ManaAbility ability) : base(source)
        {
            this.ability = ability;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            ability.resolution.Resolve(source);
            return true;
        }
    }

    public class ActivateAbilityEvent : MTGEvent
    {
        public readonly ActivatedAbility ability;

        private GenerateAbilityObjectEvent objevent;

        public ActivateAbilityEvent(OID source, ActivatedAbility ability) : base(source)
        {
            this.ability = ability;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;

            if (!(ability is ManaAbility))
            {
                // Put ability on stack
                objevent = new GenerateAbilityObjectEvent(
                    source, ability.resolution, AbilityObject.AbilityType.Activated
                );
                if (!PushChild(objevent))
                    return false;

                // Determine targets
                foreach (var target in objevent.resolution.Targets)
                {
                    if (!PushChild(new DeclareTargetEvent(source, target)))
                    {
                        Console.WriteLine("Target not declared, reverting ability.");
                        RevertAllChildren();
                        return false;
                    }
                }
            }

            // Pay ability costs
            foreach (var cost in ability.Costs)
            {
                cost.SetSource(source);
                if (!PushChild(cost))
                {
                    Console.WriteLine("Cost not paid, reverting ability.");
                    RevertAllChildren();
                    return false;
                }
            }

            if (ability is ManaAbility)
            {
                foreach (var evnt in ability.resolution.GetResolutionEvents(source))
                {
                    PushChild(evnt);
                }
            }

            return true;
        }
    }

    public class DeclareTargetEvent : MTGEvent
    {
        public readonly Target target;

        public DeclareTargetEvent(OID source, Target target) : base(source)
        {
            this.target = target;
        }

        protected override bool SelfRevertable => true;

        protected override bool ApplyAction()
        {
            return target.Declare(source);
        }

        protected override void RevertAction()
        {
            target.Reset();
        }

    }

    public class CastSpellEvent : MTGEvent
    {

        private MoveZoneEvent moveZoneEvent;

        public CastSpellEvent(OID oid) : base(oid) { }

        protected override bool ApplyAction()
        {
            var mtg = MTG.Instance;

            // Move card to stack
            var currentZone = mtg.FindZoneFromOID(source);
            moveZoneEvent = new MoveZoneEvent(null, currentZone, mtg.theStack, source);
            if (!PushChild(moveZoneEvent))
                return false;

            var obj = mtg.objects[source];

            // Determine targets
            foreach (var target in obj.Targets)
            {
                if (!PushChild(new DeclareTargetEvent(source, target)))
                {
                    Console.WriteLine("Target not declared, reverting cast.");
                    RevertAllChildren();
                    return false;
                }
            }

            // Pay casting costs
            foreach (var cost in obj.Costs)
            {
                cost.SetSource(source);
                if (!PushChild(cost))
                {
                    Console.WriteLine("Cost not paid, reverting cast.");
                    RevertAllChildren();
                    return false;
                }
            }

            return true;
        }

        protected override bool SelfRevertable => true;
    }
}
