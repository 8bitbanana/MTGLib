using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading;

namespace MTGLib
{
    public class OID
    {
        private Guid guid;

        public OID()
        {
            guid = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            return guid == ((OID)obj).guid;
        }

        public override string ToString()
        {
            return guid.ToString();
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }

    public class Timestamp : IComparable
    {
        static int count = 0;

        public int value { get; private set; }

        public Timestamp() { Update(); }

        public void Update()
        {
            Interlocked.Increment(ref count);
            value = count;
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            Timestamp otherTimestamp = obj as Timestamp;
            if (otherTimestamp != null)
            {
                return value.CompareTo(otherTimestamp.value);
            } else
            {
                throw new ArgumentException("Object is not a Timestamp");
            }
        }
        public static bool operator >(Timestamp a, Timestamp b)
        {
            return a.CompareTo(b) > 0;
        }
        public static bool operator <(Timestamp a, Timestamp b)
        {
            return a.CompareTo(b) < 0;
        }
    }

    public class AbilityObject : MTGObject
    {
        readonly OID source;

        readonly ResolutionAbility resolutionAbility;

        public enum AbilityType { Activated, Triggered }
        public readonly AbilityType abilityType;

        public AbilityObject(OID source, ResolutionAbility resolutionAbility, AbilityType abilityType)
        {
            this.source = source;
            this.resolutionAbility = resolutionAbility;
            this.abilityType = abilityType;
        }

        public override Color identity => throw new NotImplementedException();
        public override MTGObjectAttributes attr => throw new NotImplementedException();
        public override BaseCardAttributes baseattr => throw new NotImplementedException();

        public override void Resolve()
        {
            resolutionAbility.Resolve(source);
        }
    }

    public class MTGObject
    { 
        public struct BaseCardAttributes
        {
            public string name;
            public HashSet<CardType> cardTypes;
            public HashSet<SuperType> superTypes;
            public HashSet<SubType> subTypes;
            public ManaCost manaCost;
            public int power;
            public int toughness;
            public int owner;
            public int loyalty;
            public List<StaticAbility> staticAbilities;
            public List<ResolutionAbility> spellAbilities;
            public List<ActivatedAbility> activatedAbilities;
            public List<Cost> additionalCastingCosts;
        }

        public struct MTGObjectAttributes
        {
            public void Import(BaseCardAttributes attr)
            {
                name = attr.name;
                manaCost = attr.manaCost;
                power = attr.power;
                toughness = attr.toughness;
                cardTypes = attr.cardTypes;
                superTypes = attr.superTypes;
                subTypes = attr.subTypes;
                staticAbilities = attr.staticAbilities;
                activatedAbilities = attr.activatedAbilities;
                spellAbilities = attr.spellAbilities;

                controller = attr.owner;
                if (attr.manaCost != null)
                {
                    color = attr.manaCost.identity;
                } else
                {
                    color = Color.Generic;
                }

                if (manaCost != null)
                {
                    castingCosts = new List<Cost>
                    {
                        new CostPayMana(manaCost)
                    };
                    if (attr.additionalCastingCosts != null)
                    {
                        foreach (var cost in attr.additionalCastingCosts)
                        {
                            castingCosts.Add(cost);
                        }
                    }
                }

                Init();
            }

            public void Init()
            {
                if (name == null)
                    name = "";
                if (cardTypes == null)
                    cardTypes = new HashSet<CardType>();
                if (superTypes == null)
                    superTypes = new HashSet<SuperType>();
                if (subTypes == null)
                    subTypes = new HashSet<SubType>();
                if (staticAbilities == null)
                    staticAbilities = new List<StaticAbility>();
                if (spellAbilities == null)
                    spellAbilities = new List<ResolutionAbility>();
                if (activatedAbilities == null)
                    activatedAbilities = new List<ActivatedAbility>();
                // Casting costs can stay null
            }

            public string name;
            public ManaCost manaCost; // manacost can be null
            public int power;
            public int toughness;
            public int controller;
            public Color color;
            public HashSet<CardType> cardTypes;
            public HashSet<SuperType> superTypes;
            public HashSet<SubType> subTypes;
            public List<StaticAbility> staticAbilities;
            public List<ResolutionAbility> spellAbilities;
            public List<ActivatedAbility> activatedAbilities;
            public List<Cost> castingCosts;
        }

        public struct PermanentStatus
        {
            public void Reset()
            {
                tapped = flipped = facedown = phasedout = false;
                damage = 0;
            }

            public bool tapped;
            public bool flipped;
            public bool facedown;
            public bool phasedout;
            public int damage;
        }

        public enum CardType
        {
            Artifact,
            Creature,
            Enchantment,
            Instant,
            Land,
            Planeswalker,
            Sorcery
        }

        public enum SuperType
        {
            Legendary, Basic, Token
        }

        public enum SubType {
            Ogre,
            Warrior,
            Crab,
            Human,
            Knight,
            Plains,
            Mountain,
            Swamp,
            Island,
            Forest
        };

        protected readonly BaseCardAttributes baseCardAttributes;
        protected MTGObjectAttributes attributes;
        public PermanentStatus permanentStatus;

        public CounterStore counters;

        public virtual MTGObjectAttributes attr { get { return attributes; } }

        public virtual BaseCardAttributes baseattr { get { return baseCardAttributes; } }

        public int owner { get { return baseCardAttributes.owner; } }

        public bool CanBePermanent { get
            {
                foreach (CardType cardType in attr.cardTypes)
                {
                    if (cardType.IsPermanentType()) return true;
                }
                return false;
            }
        }

        public bool CanAttack { get
            {
                // TODO - Determine if a creature can attack
                return true;
            }
        }

        public MTGObject()
        {
            baseCardAttributes = new BaseCardAttributes();
            ResetAttributes();
        }

        public MTGObject(BaseCardAttributes baseAttr)
        {
            baseCardAttributes = baseAttr;
            ResetAttributes();
        }

        private List<int> paidCosts = new List<int>();

        public bool DeclareTargets()
        {
            var oid = FindMyOID();
            foreach (var res in attr.spellAbilities)
            {
                if (!res.DeclareTargets(oid))
                    return false;
            }
            return true;
        }

        public bool CanPayCosts()
        {
            OID myOID = FindMyOID();
            foreach(var cost in attr.castingCosts)
            {
                if (!cost.CanPay(myOID)) return false;
            }
            return true;
        }

        public bool PayCastingCosts()
        {
            OID myOid = FindMyOID();
            paidCosts.Clear();
            for (int i = 0; i < attr.castingCosts.Count; i++)
            {
                var cost = attr.castingCosts[i];
                bool result = cost.Pay(myOid);
                if (result)
                {
                    paidCosts.Add(i);
                }
                else
                {
                    RepayPaidCosts();
                    return false;
                }
            }
            return true;
        }

        private void RepayPaidCosts()
        {
            var myOid = FindMyOID();
            foreach (int i in paidCosts)
            {
                var cost = attr.castingCosts[i];
                cost.ReversePay(myOid);
            }
        }

        public virtual void Resolve()
        {
            foreach (var ability in attr.spellAbilities)
            {
                ability.Resolve(FindMyOID());
            }
        }

        protected void ResetAttributes()
        {
            attributes = new MTGObjectAttributes();
            attributes.Import(baseCardAttributes);
        }

        protected void ResetPermanentStatus()
        {
            permanentStatus.Reset();
        }

        protected void ClearCounters()
        {
            counters.Clear();
        }

        public override string ToString()
        {
            if (attr.name.Length > 0)
                return attr.name;
            return "[[Unnamed Object]]";
        }

        protected OID FindMyOID()
        {
            var mtg = MTG.Instance;
            foreach (var x in mtg.objects)
            {
                if (x.Value == this)
                    return x.Key;
            }
            return null;
        }

        protected Zone FindMyZone()
        {
            var mtg = MTG.Instance;
            OID myOid = FindMyOID();
            return mtg.FindZoneFromOID(myOid);
        }

        public void CalculateAttributes()
        {
            ResetAttributes();

            OID myOid = FindMyOID();

            // TODO - Make sure effects are applied in timestamp order
            // TODO - How the hell does dependency work
            var allMods = MTG.Instance.AllModifications;
            List<int> indexes = new List<int>(Enumerable.Range(0, allMods.Count));

            //613.1a Layer 1: Rules and effects that modify copiable values are applied.
            //613.2a Layer 1a: Copiable effects are applied

            //613.2b Layer 1b: Face-down spells and permanents have their characteristics modified as defined in rule 707.2.

            //613.1b Layer 2: Control - changing effects are applied.
            for (int i = indexes.Count-1; i >= 0; i--) {
                var mod = allMods[indexes[i]];
                if (mod is ControllerMod cast)
                {
                    attributes.controller = cast.Modify(attributes.controller, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }

            //613.1c Layer 3: Text - changing effects are applied.See rule 612, “Text - Changing Effects.”

            //613.1d Layer 4: Type - changing effects are applied.These include effects that change an object’s card type, subtype, and / or supertype.
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                var mod = allMods[indexes[i]];
                if (mod is CardTypeMod cast)
                {
                    attributes.cardTypes = cast.Modify(attributes.cardTypes, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is SubTypeMod cast1)
                {
                    attributes.subTypes = cast1.Modify(attributes.subTypes, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is SuperTypeMod cast2)
                {
                    attributes.superTypes = cast2.Modify(attributes.superTypes, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }

            //613.1e Layer 5: Color - changing effects are applied.
            for (int i = indexes.Count-1; i>=0; i--)
            {
                var mod = allMods[indexes[i]];
                if (mod is ColorMod cast)
                {
                    attributes.color = cast.Modify(attributes.color, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }

            //613.1f Layer 6: Ability - adding effects, keyword counters, ability-removing effects, and effects that say an object can’t have an ability are applied.

            //613.1g Layer 7: Power - and / or toughness - changing effects are applied
            //613.4a Layer 7a: Effects from characteristic - defining abilities that define power and / or toughness are applied.See rule 604.3.

            //613.4b Layer 7b: Effects that set power and / or toughness to a specific number or value are applied.Effects that refer to the base power and/ or toughness of a creature apply in this layer.
            for (int i = indexes.Count-1; i >= 0; i--)
            {
                var mod = allMods[indexes[i]];
                if (mod.operation != Modification.Operation.Override)
                    continue;
                if (mod is PowerMod cast)
                {
                    attributes.power = cast.Modify(attributes.power, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is ToughnessMod cast2)
                {
                    attributes.toughness = cast2.Modify(attributes.toughness, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }
            //613.4c Layer 7c: Effects and counters that modify power and / or toughness(but don’t set power and / or toughness to a specific number or value) are applied.
            for (int i = indexes.Count-1; i >= 0; i--)
            {
                var mod = allMods[indexes[i]];
                if (!(mod.operation == Modification.Operation.Add || mod.operation == Modification.Operation.Subtract))
                    continue;
                if (mod is PowerMod cast)
                {
                    attributes.power = cast.Modify(attributes.power, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is ToughnessMod cast2)
                {
                    attributes.toughness = cast2.Modify(attributes.toughness, myOid, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }
        }

        public virtual Color identity { get
            {
                return attributes.color;
            } 
        }
        public virtual int cmc { get
            {
                if (attr.manaCost != null)
                    return attr.manaCost.cmc;
                else return 0;
            }
        }
    }
}
