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

    public static class Util
    {
        public static bool ColorHas(Color col, Color test)
        {
            return ((test & col) == test);
        }
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

        public bool IsColored { get { return color != Color.Generic; } }
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
            foreach (var mana in manaSymbols)
            {
                s += mana.ToString();
            }
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

        public static (List<ManaSymbol> colored, int generic) SplitMana(ManaCost x)
        {
            var colored = new List<ManaSymbol>();
            var generic = 0;
            foreach (var mana in x.manaSymbols)
            {
                if (mana.IsColored)
                    colored.Add(mana);
                else
                    generic += mana.cmc;
            }
            return (colored, generic);
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
