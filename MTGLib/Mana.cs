using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    [Flags] public enum Color
    {
        Generic = 0,
        White = 1,
        Blue = 2,
        Black = 4,
        Red = 8,
        Green = 16,

        Azorius = White | Blue,
        Dimir = Blue | Black,
        Rakdos = Black | Red,
        Gruul = Red | Green,
        Selesnya = Green | White,

        Orzhov = White | Black,
        Izzet = Blue | Red,
        Golgari = Black | Green,
        Boros = Red | White,
        Simic = Green | Blue
    }

    public class ManaSymbol
    {
        public Color color { get; private set; } = Color.Generic;

        public ManaSymbol() : this(Color.Generic) { }
        public ManaSymbol(Color c) {
            color = c;
        }

        public int cmc { get { return 1; } }

        public static string GetColorSymbol(Color color)
        {
            switch (color)
            {
                case Color.Generic:
                    return "1";
                case Color.White:
                    return "W";
                case Color.Blue:
                    return "U";
                case Color.Black:
                    return "B";
                case Color.Red:
                    return "R";
                case Color.Green:
                    return "G";
                default:
                    return "H";
            }
        }

        public static Color[] GetBasicColors()
        {
            return new Color[5]
            {
                Color.White,
                Color.Blue,
                Color.Black,
                Color.Red,
                Color.Green
            };
        }

        public static int GetColorCount(Color color)
        {
            int count = 0;
            foreach (var basic in GetBasicColors())
                if (color.HasColor(basic)) count++;
            return count;
        }

        public virtual bool CanThisPayForMe(ManaSymbol manaBeingSpent)
        {
            int colorCount = GetColorCount(color);
            if (colorCount == 0)
                return true;
            else if (colorCount == 1)
            {
                return color.HasColor(manaBeingSpent.color);
            } else
            {
                throw new ArgumentException("The ManaSymbol being spent should only have one color");
            }
        }

        public override string ToString()
        {
            return "{" + GetColorSymbol(color) + "}";
        }

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (ManaSymbol)obj;
        }

        public static bool operator ==(ManaSymbol x, ManaSymbol y)
        {
            if (object.ReferenceEquals(x, null))
            {
                return object.ReferenceEquals(y, null);
            }
            if (object.ReferenceEquals(y, null))
            {
                return object.ReferenceEquals(x, null);
            }
            return (x.color == y.color);
        }

        public static bool operator !=(ManaSymbol x, ManaSymbol y)
        {
            return (x.color != y.color);
        }

        public static readonly ManaSymbol White = new ManaSymbol(Color.White);
        public static readonly ManaSymbol Blue = new ManaSymbol(Color.Blue);
        public static readonly ManaSymbol Black = new ManaSymbol(Color.Black);
        public static readonly ManaSymbol Red = new ManaSymbol(Color.Red);
        public static readonly ManaSymbol Green = new ManaSymbol(Color.Green);
        public static readonly ManaSymbol One = new ManaSymbol(Color.Generic);
        public static readonly ManaSymbol Generic = new ManaSymbol(Color.Generic);

        public static readonly ManaSymbol HybridAzorius = new ManaSymbol(Color.Azorius);
        public static readonly ManaSymbol HybridDimir = new ManaSymbol(Color.Dimir);
        public static readonly ManaSymbol HybridRakdos = new ManaSymbol(Color.Rakdos);
        public static readonly ManaSymbol HybridGruul = new ManaSymbol(Color.Gruul);
        public static readonly ManaSymbol HybridSelesnya = new ManaSymbol(Color.Selesnya);

        public static readonly ManaSymbol HybridOrzhov = new ManaSymbol(Color.Orzhov);
        public static readonly ManaSymbol HybridIzzet = new ManaSymbol(Color.Izzet);
        public static readonly ManaSymbol HybridGolgari = new ManaSymbol(Color.Golgari);
        public static readonly ManaSymbol HybridBoros = new ManaSymbol(Color.Boros);
        public static readonly ManaSymbol HybridSimic = new ManaSymbol(Color.Simic);

        public bool IsColored { get { return color != Color.Generic; } }
    }

    public class ManaPool
    {
        List<ManaSymbol> manaSymbols = new List<ManaSymbol>();

        /* Used to determine what mana has been added from the mana abilities
         while costs are being paid.
         */
        List<ManaSymbol> tempManaSymbols = new List<ManaSymbol>();

        public ManaPool() { }

        public void AddMana(ManaSymbol mana)
        {
            if (ManaSymbol.GetColorCount(mana.color) > 1)
                throw new ArgumentException("You can only add 0-1 color mana to your mana pool");
            manaSymbols.Add(mana);
            tempManaSymbols.Add(mana);
        }

        public void AddMana(params ManaSymbol[] mana)
        {
            foreach (var singlemana in mana)
                AddMana(singlemana);
        }

        public void AddMana(ManaCost manaCost)
        {
            foreach (var mana in manaCost)
                AddMana(mana);
        }

        public bool RemoveMana(ManaSymbol mana)
        {
            bool result = manaSymbols.Remove(mana);
            if (result)
                tempManaSymbols.Remove(mana);
            return result;
        }

        public bool RemoveMana(params ManaSymbol[] mana)
        {
            bool allRemoved = true;
            foreach (var manaSymbol in mana)
            {
                if (!RemoveMana(manaSymbol))
                    allRemoved = false;
            }
            return allRemoved;
        }

        public IEnumerator<ManaSymbol> GetEnumerator()
        {
            return manaSymbols.GetEnumerator();
        }

        public int Count { get { return manaSymbols.Count; } }

        public void Empty()
        {
            manaSymbols.Clear();
        }

        private int GetMyOwnerIndex()
        {
            var index = 0;
            foreach (var player in MTG.Instance.players)
            {
                if (player.manaPool == this)
                    return index;
                index++;
            }
            throw new InvalidOperationException("Manapool was unable to find it's owner.");
        }

        public bool PayFor(ManaCost cost)
        {
            tempManaSymbols = new List<ManaSymbol>(manaSymbols);

            // Track each activated mana ability, so they can be reversed if needed
            var activatedManaAbilities = new List<Tuple<ManaAbility, OID>>();

            // Quick little function to reverse any activated mana abilities if
            // this cost is cancelled.
            void reverseManaAbilities()
            {
                foreach (var x in activatedManaAbilities)
                {
                    var ability = x.Item1;
                    var source = x.Item2;
                    var abilityOid = ability.GenerateReverseAbility(source);
                    ability.RepayPaidCosts(source);
                    MTG.Instance.objects[abilityOid].Resolve();
                    MTG.Instance.DeleteObject(abilityOid);
                }
            }

            foreach (var manaToPay in cost)
            {
                bool paid = false;
                // Loop until mana is paid
                while (!paid)
                {
                    List<ManaSymbol> possManaSymbols = new List<ManaSymbol>();
                    foreach (var mana in tempManaSymbols)
                    {
                        if (manaToPay.CanThisPayForMe(mana))
                        {
                            possManaSymbols.Add(mana);
                        }
                    }

                    var choice = new ManaChoice(possManaSymbols, manaToPay, GetMyOwnerIndex());

                    MTG.Instance.PushChoice(choice);
                    if (choice.Cancelled)
                    {
                        reverseManaAbilities();
                        return false;
                    }

                    var chosen = choice.FirstChoice;

                    switch (chosen.type)
                    {
                        case (ManaChoiceOption.OptionType.UseMana):
                            tempManaSymbols.Remove(choice.FirstChoice.manaSymbol);
                            paid = true;
                            break;
                        case (ManaChoiceOption.OptionType.ActivateManaAbility):
                            bool result = chosen.manaAbility.PayCosts(chosen.manaAbilitySource);
                            if (result)
                            {
                                // Use lastAddedManaSymbols to track the mana this
                                // ability is adding.
                                OID abilityObj = chosen.manaAbility.GenerateAbility(chosen.manaAbilitySource);
                                MTG.Instance.objects[abilityObj].Resolve();
                                MTG.Instance.DeleteObject(abilityObj);
                                activatedManaAbilities.Add(
                                    new Tuple<ManaAbility, OID>(chosen.manaAbility, chosen.manaAbilitySource)
                                );
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            manaSymbols = new List<ManaSymbol>(tempManaSymbols);
            return true;
        }
    }

    public class ManaCost
    {
        public List<ManaSymbol> manaSymbols = new List<ManaSymbol>();

        public ManaCost() { }

        public ManaCost(int generic, params ManaSymbol[] mana)
        {
            for (int i=0;i<generic;i++)
            {
                manaSymbols.Add(ManaSymbol.One);
            }
            manaSymbols.AddRange(mana);
        }

        public IEnumerator<ManaSymbol> GetEnumerator()
        {
            return manaSymbols.GetEnumerator();
        }

        public ManaCost(params ManaSymbol[] mana)
        {
            manaSymbols.AddRange(mana);
        }

        public Color identity { get
            {
                Color id = Color.Generic;
                foreach(var mana in manaSymbols)
                {
                    id |= mana.color;
                }
                return id;
            }
        }
        public int cmc
        {
            get
            {
                int total = 0;
                foreach (var mana in manaSymbols)
                {
                    total += mana.cmc;
                }
                return total;
            }
        }

        public override string ToString()
        {
            string s = "";
            int generic = 0;
            foreach (var mana in manaSymbols)
            {
                if (mana.IsColored)
                    s += mana.ToString();
                else
                    generic += mana.cmc;
            }
            if (generic > 0)
                return $"{{{generic}}}{s}";
            else
                return s;
        }

        public static ManaCost operator +(ManaCost x, ManaCost y)
        {
            var allMana = new List<ManaSymbol>();
            allMana.AddRange(x.manaSymbols);
            allMana.AddRange(y.manaSymbols);

            var newManaCost = new ManaCost();
            newManaCost.manaSymbols.AddRange(allMana);
            return newManaCost;
        }

        public override bool Equals(object obj)
        {
            return this == (ManaCost)obj;
        }

        public override int GetHashCode()
        {
            return manaSymbols.GetHashCode();
        }

        public static bool operator ==(ManaCost x, ManaCost y)
        {
            if (object.ReferenceEquals(x, null))
            {
                return object.ReferenceEquals(y, null);
            }
            if (object.ReferenceEquals(y, null))
            {
                return object.ReferenceEquals(x, null);
            }
            // TODO Manacost should self-sort to make this faster
            if (x.manaSymbols.Count != y.manaSymbols.Count)
                return false;
            var manaX = new List<ManaSymbol>(x.manaSymbols);
            var manaY = new List<ManaSymbol>(y.manaSymbols);
            for (int ix = 0; ix<manaX.Count; ix++)
            {
                bool found = false;
                ManaSymbol mana = manaX[ix];
                for (int iy = 0; iy<manaY.Count; iy++)
                {
                    if (manaY[iy] == mana)
                    {
                        manaY.RemoveAt(iy);
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        public static bool operator !=(ManaCost x, ManaCost y)
        {
            return !(x == y);
        }

        public static ManaCost operator -(ManaCost x, ManaCost y)
        {
            var final = new ManaCost();  

            var colorValues = (Color[])Enum.GetValues(typeof(Color));
            foreach (Color color in colorValues)
            {
                int countX = 0;
                int countY = 0;
                foreach(var mana in x.manaSymbols)
                {
                    if (mana.color == color) { countX++; }
                }
                foreach(var mana in y.manaSymbols)
                {
                    if (mana.color == color) { countY++; }
                }
                var finalCount = countX - countY;
                for (int i = 0; i < finalCount; i++)
                {
                    final.manaSymbols.Add(new ManaSymbol(color));
                }
            }
            return final;
        }
    }
}
