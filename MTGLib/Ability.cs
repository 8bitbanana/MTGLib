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

    public class CharacteristicAbility
    {
        public CharacteristicAbility()
        {

        }
    }
}
