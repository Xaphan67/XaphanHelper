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
                entryID: "loc0",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc0",
                flag: "LorebookEntry_loc0"
            ));

            list.Add(new LorebookData(
                entryID: "loc1-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-1",
                flag: "LorebookEntry_loc1-1"
            ));

            list.Add(new LorebookData(
                entryID: "loc1-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-2",
                flag: "LorebookEntry_loc1-2"
            ));

            list.Add(new LorebookData(
                entryID: "loc1-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-3",
                flag: "LorebookEntry_loc1-3"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-1",
                flag: "LorebookEntry_loc2-1"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-2",
                flag: "LorebookEntry_loc2-2"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-3",
                flag: "LorebookEntry_loc2-3"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-1",
                flag: "LorebookEntry_loc5-1"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-2",
                flag: "LorebookEntry_loc5-2"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-3",
                flag: "LorebookEntry_loc5-3"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-4",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-4",
                flag: "LorebookEntry_loc5-4"
            ));

            // Equipment

            list.Add(new LorebookData(
                entryID: "arm1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm1",
                flag: "Upgrade_PowerGrip"
            ));

            list.Add(new LorebookData(
                entryID: "arm2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm2",
                flag: "Upgrade_ClimbingKit"
            ));

            list.Add(new LorebookData(
                entryID: "arm3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm3",
                flag: "Upgrade_SpiderMagnet"
            ));

            list.Add(new LorebookData(
                entryID: "leg1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/leg1",
                flag: "Upgrade_DashBoots"
            ));

            list.Add(new LorebookData(
                entryID: "leg2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/leg2",
                flag: "Upgrade_SpaceJump"
            ));

            list.Add(new LorebookData(
                entryID: "bag1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/bag1",
                flag: "Upgrade_Bombs"
            ));

            list.Add(new LorebookData(
                entryID: "bag3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/bag3",
                flag: "Upgrade_RemoteDrone"
            ));

            list.Add(new LorebookData(
                entryID: "mis4",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/mis4",
                flag: "Upgrade_Binoculars"
            ));

            list.Add(new LorebookData(
                entryID: "mis6",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/mis6",
                flag: "Upgrade_PulseRadar"
            ));

            list.Add(new LorebookData(
                entryID: "dr-mod1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/dr-mod1",
                flag: "Upgrade_MissilesModule"
            ));

            list.Add(new LorebookData(
                entryID: "col1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col1",
                flag: "XaphanHelper_StatFlag_EnergyTank"
            ));

            list.Add(new LorebookData(
                entryID: "col2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col2",
                flag: "XaphanHelper_StatFlag_FireRateModule"
            ));

            list.Add(new LorebookData(
                entryID: "col3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col3",
                flag: "XaphanHelper_StatFlag_Missile"
            ));

            // Bestiary

            list.Add(new LorebookData(
                entryID: "boss-2-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Bestiary/boss-2-1",
                flag: "XaphanHelper_StatFlag_BossCh2"
            ));

            // Adventure

            list.Add(new LorebookData(
                entryID: "loc0",
                categoryID: 3,
                picture: "lorebook/Xaphan/Equipment/loc0",
                flag: "test-hint"
            ));

            return list;
        }
    }
}
