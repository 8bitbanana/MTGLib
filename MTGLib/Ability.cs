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

    public class CharacteristicAbility
    {
        public CharacteristicAbility()
        {

        }
    }
}
