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

                controller = attr.owner;
                color = attr.manaCost.identity;

                Init();
            }

            public void Init()
            {
                if (cardTypes == null)
                    cardTypes = new HashSet<CardType>();
                if (superTypes == null)
                    superTypes = new HashSet<SuperType>();
                if (subTypes == null)
                    subTypes = new HashSet<SubType>();
                if (staticAbilities == null)
                    staticAbilities = new List<StaticAbility>();
            }

            public string name;
            public ManaCost manaCost;
            public int power;
            public int toughness;
            public int controller;
            public Color color;
            public HashSet<CardType> cardTypes;
            public HashSet<SuperType> superTypes;
            public HashSet<SubType> subTypes;
            public List<StaticAbility> staticAbilities;
        }

        public struct PermanentStatus
        {
            public void Reset()
            {
                tapped = flipped = facedown = phasedout = false;
            }

            public bool tapped;
            public bool flipped;
            public bool facedown;
            public bool phasedout;
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

        public enum SubType { Ogre, Warrior, Crab };

        protected readonly BaseCardAttributes baseCardAttributes;
        protected MTGObjectAttributes attributes;
        public PermanentStatus permanentStatus;

        public CounterStore counters;

        public MTGObjectAttributes attr { get { return attributes; } }

        public MTGObject(BaseCardAttributes baseAttr)
        {
            baseCardAttributes = baseAttr;
            ResetAttributes();
        }

        public void Resolve()
        {
            MTG mtg = MTG.Instance;
            if (attr.cardTypes.Contains(CardType.Instant) ||
                attr.cardTypes.Contains(CardType.Sorcery))
            {
                throw new NotImplementedException();
            } else
            {
                mtg.MoveZone(FindMyOID(), mtg.battlefield);
            }
        }

        public void ResetAttributes()
        {
            attributes = new MTGObjectAttributes();
            attributes.Import(baseCardAttributes);
        }

        public void ResetPermanentStatus()
        {
            permanentStatus.Reset();
        }

        public void ClearCounters()
        {
            counters.Clear();
        }

        public override string ToString()
        {
            if (attr.name.Length > 0)
                return attr.name;
            return "[[Unnamed Object]]";
        }

        public OID FindMyOID()
        {
            var mtg = MTG.Instance;
            foreach (var x in mtg.objects)
            {
                if (x.Value == this)
                    return x.Key;
            }
            return null;
        }

        public BaseZone FindMyZone()
        {
            var mtg = MTG.Instance;
            OID myOid = FindMyOID();
            return mtg.FindZoneFromOID(myOid);
        }

        public void CalculateAttributes()
        {
            ResetAttributes();
            
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
                    attributes.controller = cast.Modify(attributes.controller, this);
                }
            }

            //613.1c Layer 3: Text - changing effects are applied.See rule 612, “Text - Changing Effects.”

            //613.1d Layer 4: Type - changing effects are applied.These include effects that change an object’s card type, subtype, and / or supertype.

            //613.1e Layer 5: Color - changing effects are applied.
            for (int i = indexes.Count-1; i>=0; i--)
            {
                var mod = allMods[indexes[i]];
                if (mod is ColorMod cast)
                {
                    attributes.color = cast.Modify(attributes.color, this);
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
                    attributes.power = cast.Modify(attributes.power, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is ToughnessMod cast2)
                {
                    attributes.toughness = cast2.Modify(attributes.toughness, this);
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
                    attributes.power = cast.Modify(attributes.power, this);
                    indexes.RemoveAt(i);
                    continue;
                }
                if (mod is ToughnessMod cast2)
                {
                    attributes.toughness = cast2.Modify(attributes.toughness, this);
                    indexes.RemoveAt(i);
                    continue;
                }
            }
        }

        public Color identity { get
            {
                return attributes.color;
            } 
        }
    }
}
