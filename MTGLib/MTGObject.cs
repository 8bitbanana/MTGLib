using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        }

        public struct MTGObjectAttributes
        {
            public void Import(BaseCardAttributes attr)
            {
                name = attr.name;
                manaCost = attr.manaCost;
                power = attr.power;
                toughness = attr.toughness;
                controller = attr.owner;
            }

            public string name;
            public ManaCost manaCost;
            public int power;
            public int toughness;
            public int controller;
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
        protected PermanentStatus permanentStatus;

        public CounterStore counters;


        public MTGObject(BaseCardAttributes baseAttr)
        {
            baseCardAttributes = baseAttr;
            
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

        public void CalculateAttributes()
        {
            ResetAttributes();
            var allMods = MTG.Instance.AllModifications;
            List<int> indexes = new List<int>(allMods.Count);
            for (int i=0;i<allMods.Count;i++) { indexes[i] = i; }

            //613.1a Layer 1: Rules and effects that modify copiable values are applied.

            //613.1b Layer 2: Control - changing effects are applied.

            //613.1c Layer 3: Text - changing effects are applied.See rule 612, “Text - Changing Effects.”

            //613.1d Layer 4: Type - changing effects are applied.These include effects that change an object’s card type, subtype, and / or supertype.

            //613.1e Layer 5: Color - changing effects are applied.

            //613.1f Layer 6: Ability - adding effects, keyword counters, ability-removing effects, and effects that say an object can’t have an ability are applied.

            //613.1g Layer 7: Power - and / or toughness - changing effects are applied
            foreach (int index in indexes)
            {
                var mod = allMods[index];
                
            }
        }

        public Color identity { get
            {
                return attributes.manaCost.identity;
            } 
        }
    }
}
