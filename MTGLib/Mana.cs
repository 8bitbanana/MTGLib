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

        public ManaPool() { }

        public void AddMana(params ManaSymbol[] mana)
        {
            foreach (var singlemana in mana)
                AddMana(singlemana);
        }

        public void AddMana(ManaSymbol mana)
        {
            if (ManaSymbol.GetColorCount(mana.color) > 1)
                throw new ArgumentException("You can only add 0-1 color mana to your mana pool");
            manaSymbols.Add(mana);
        }

        public void AddMana(ManaCost manaCost)
        {
            foreach (var mana in manaCost)
                AddMana(mana);
        }

        public void RemoveMana(params ManaSymbol[] mana)
        {
            foreach (var manaSymbol in mana)
            {
                manaSymbols.Remove(manaSymbol);
            }
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

        public bool PayFor(ManaCost cost)
        {
            var currentMana = new List<ManaSymbol>(manaSymbols);
            foreach (var manaToPay in cost)
            {
                List<ManaSymbol> possManaSymbols = new List<ManaSymbol>();
                foreach (var mana in currentMana) 
                {
                    if (manaToPay.CanThisPayForMe(mana))
                    {
                        possManaSymbols.Add(mana);
                    }
                }
                if (possManaSymbols.Count == 0)
                {
                    Console.WriteLine("Cannot pay for mana cost");
                    return false;
                }

                var choice = new Choice<ManaSymbol>()
                {
                    Title = $"Choose which mana to use to pay for {manaToPay}",
                    Min = 1, Max = 1,
                    Options = possManaSymbols
                };
                MTG.Instance.PushChoice(choice);
                currentMana.Remove(choice.FirstChoice);
            }
            manaSymbols = currentMana;
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
            return $"{{{generic}}}{s}";
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
