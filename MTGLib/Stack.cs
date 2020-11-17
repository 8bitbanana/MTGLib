using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{

    public class TheStack : Zone
    {

        public void ResolveTop()
        {
            MTG mtg = MTG.Instance;
            OID oid = Get(0);
            MTGObject obj = mtg.objects[oid];
            obj.Resolve();

            if (obj is AbilityObject)
            {
                mtg.DeleteObject(oid);
            } else
            {
                if (obj.CanBePermanent)
                {
                    mtg.MoveZone(oid, this, mtg.battlefield);
                } else
                {
                    mtg.MoveZone(oid, this, mtg.players[obj.owner].graveyard);
                }
            }
        }

        public void PPrint()
        {
            if (Count > 0)
            {
                Console.WriteLine(" == Stack ==");
                foreach (var x in this)
                {
                    Console.WriteLine(MTG.Instance.objects[x].ToString());
                }
                Console.WriteLine(" ==== ");
            } else
            {
                Console.WriteLine("Stack is empty");
            }
        }   
    }
}
