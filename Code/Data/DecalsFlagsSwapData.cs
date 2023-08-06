using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.XaphanHelper.Data
{
    class DecalsFlagsSwapData
    {
        public string Decal;

        public string Flag;

        public string OffPath;

        public string OnPath;

        public string Room;

        public DecalsFlagsSwapData(string decal, string flag, string offPath, string onPath, string room)
        {
            Decal = decal;
            Flag = flag;
            OffPath = offPath;
            OnPath = onPath;
            Room = room;
        }
    }
}
