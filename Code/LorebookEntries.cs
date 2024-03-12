using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;

namespace Celeste.Mod.XaphanHelper
{
    static class LorebookEntries
    {
        public static List<LorebookData> GenerateLorebookEntriesDataList(Session session)
        {
            StatsFlags.GetStats(session);
            List<LorebookData> list = new();

            // Locations
            list.Add(new LorebookData(
                entryID: "loc1",
                categoryID: 0,
                picture: "lorebook/Xaphan/loc1"
            ));

            return list;
        }
    }
}
