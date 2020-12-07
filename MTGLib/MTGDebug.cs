using System;
using System.Collections.Generic;
using System.Text;

namespace MTGLib
{
    public partial class MTG
    {
        public OID DebugTutor(string name, int player, Zone zone)
        {
            foreach (var oid in players[player].library)
            {
                var obj = objects[oid];
                if (obj.attr.name == name)
                {
                    MoveZone(oid, players[player].library, zone);
                    return oid;
                }
            }
            throw new KeyNotFoundException($"Debug tutor - {name} not found");
        }

        public OID DebugFind(string name, Zone zone)
        {
            foreach (var oid in zone)
            {
                var obj = objects[oid];
                if (obj.attr.name == name)
                {
                    return oid;
                }
            }
            throw new KeyNotFoundException($"Card {name} not found in zone");
        }
    }
}
