using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;

namespace Celeste.Mod.XaphanHelper
{
    static class LorebookEntries
    {
        public static List<LorebookData> GenerateLorebookEntriesDataList()
        {
            List<LorebookData> list = new();

            // Locations
            list.Add(new LorebookData(
                entryID: "locc1",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc0",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc0",
                flag: "LorebookEntry_loc0",
                subCategoryID: "locc1"
            ));

            list.Add(new LorebookData(
                entryID: "locc2",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc1-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-1",
                flag: "LorebookEntry_loc1-1",
                subCategoryID: "locc2"
            ));

            list.Add(new LorebookData(
                entryID: "loc1-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-2",
                flag: "LorebookEntry_loc1-2",
                subCategoryID: "locc2"
            ));

            list.Add(new LorebookData(
                entryID: "loc1-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc1-3",
                flag: "LorebookEntry_loc1-3",
                subCategoryID: "locc2"
            ));

            list.Add(new LorebookData(
                entryID: "locc3",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc2-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-1",
                flag: "LorebookEntry_loc2-1",
                subCategoryID: "locc3"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-2",
                flag: "LorebookEntry_loc2-2",
                subCategoryID: "locc3"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-3",
                flag: "LorebookEntry_loc2-3",
                subCategoryID: "locc3"
            ));

            list.Add(new LorebookData(
                entryID: "loc2-4",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc2-4",
                flag: "LorebookEntry_loc2-4",
                subCategoryID: "locc3"
            ));

            list.Add(new LorebookData(
                entryID: "locc4",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc3-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc3-1",
                flag: "LorebookEntry_loc3-1",
                subCategoryID: "locc4"
            ));

            list.Add(new LorebookData(
                entryID: "locc5",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc4-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc4-1",
                flag: "LorebookEntry_loc4-1",
                subCategoryID: "locc5"
            ));

            list.Add(new LorebookData(
                entryID: "loc4-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc4-2",
                flag: "LorebookEntry_loc4-2",
                subCategoryID: "locc5"
            ));

            list.Add(new LorebookData(
                entryID: "loc4-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc4-3",
                flag: "LorebookEntry_loc4-3",
                subCategoryID: "locc5"
            ));

            list.Add(new LorebookData(
                entryID: "locc6",
                categoryID: 0,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "loc5-1",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-1",
                flag: "LorebookEntry_loc5-1",
                subCategoryID: "locc6"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-2",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-2",
                flag: "LorebookEntry_loc5-2",
                subCategoryID: "locc6"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-3",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-3",
                flag: "LorebookEntry_loc5-3",
                subCategoryID: "locc6"
            ));

            list.Add(new LorebookData(
                entryID: "loc5-4",
                categoryID: 0,
                picture: "lorebook/Xaphan/Locations/loc5-4",
                flag: "LorebookEntry_loc5-4",
                subCategoryID: "locc6"
            ));

            // Equipment

            list.Add(new LorebookData(
                entryID: "eqpc1",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "arm1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm1",
                flag: "Upgrade_PowerGrip",
                subCategoryID: "eqpc1"
            ));

            list.Add(new LorebookData(
                entryID: "arm2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm2",
                flag: "Upgrade_ClimbingKit",
                subCategoryID: "eqpc1"
            ));

            list.Add(new LorebookData(
                entryID: "arm3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/arm3",
                flag: "Upgrade_SpiderMagnet",
                subCategoryID: "eqpc1"
            ));

            list.Add(new LorebookData(
                entryID: "eqpc2",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "leg1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/leg1",
                flag: "Upgrade_DashBoots",
                subCategoryID: "eqpc2"
            ));

            list.Add(new LorebookData(
                entryID: "leg2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/leg2",
                flag: "Upgrade_SpaceJump",
                subCategoryID: "eqpc2"
            ));

            list.Add(new LorebookData(
                entryID: "eqpc4",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "bag1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/bag1",
                flag: "Upgrade_Bombs",
                subCategoryID: "eqpc4"
            ));

            list.Add(new LorebookData(
                entryID: "bag3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/bag3",
                flag: "Upgrade_RemoteDrone",
                subCategoryID: "eqpc4"
            ));

            list.Add(new LorebookData(
                entryID: "eqpc5",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "mis4",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/mis4",
                flag: "Upgrade_Binoculars",
                subCategoryID: "eqpc5"
            ));

            list.Add(new LorebookData(
                entryID: "mis6",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/mis6",
                flag: "Upgrade_PulseRadar",
                subCategoryID: "eqpc5"
            ));

            list.Add(new LorebookData(
                entryID: "eqpc6",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "dr-mod1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/dr-mod1",
                flag: "Upgrade_MissilesModule",
                subCategoryID: "eqpc6"
            ));

            list.Add(new LorebookData(
                entryID: "eqpc7",
                categoryID: 1,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "col1",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col1",
                flag: "XaphanHelper_StatFlag_EnergyTank",
                subCategoryID: "eqpc7"
            ));

            list.Add(new LorebookData(
                entryID: "col2",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col2",
                flag: "XaphanHelper_StatFlag_FireRateModule",
                subCategoryID: "eqpc7"
            ));

            list.Add(new LorebookData(
                entryID: "col3",
                categoryID: 1,
                picture: "lorebook/Xaphan/Equipment/col3",
                flag: "XaphanHelper_StatFlag_Missile",
                subCategoryID: "eqpc7"
            ));

            // Adventure

            list.Add(new LorebookData(
                entryID: "advc1",
                categoryID: 2,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "poi0-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/poi0-1",
                flag: "CS_Ch0_Gem_Room_B",
                subCategoryID: "advc1"
            ));

            list.Add(new LorebookData(
                entryID: "poi2-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/poi2-1",
                flag: "LorebookEntry_poi2-1",
                subCategoryID: "advc1"
            ));

            list.Add(new LorebookData(
                entryID: "poi5-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/poi5-1",
                flag: "LorebookEntry_poi5-1",
                subCategoryID: "advc1"
            ));

            list.Add(new LorebookData(
                entryID: "poi5-2",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/poi5-2",
                flag: "LorebookEntry_poi5-2",
                subCategoryID: "advc1"
            ));

            list.Add(new LorebookData(
                entryID: "advc2",
                categoryID: 2,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "boss-2-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Bestiary/boss-2-1",
                flag: "XaphanHelper_StatFlag_BossCh2",
                subCategoryID: "advc2"
            ));

            list.Add(new LorebookData(
                entryID: "boss-1-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Bestiary/boss-1-1",
                flag: "XaphanHelper_StatFlag_BossCh1",
                subCategoryID: "advc2"
            ));

            list.Add(new LorebookData(
                entryID: "boss-4-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Bestiary/boss-4-1",
                flag: "XaphanHelper_StatFlag_BossCh4",
                subCategoryID: "advc2"
            ));

            list.Add(new LorebookData(
                entryID: "boss-5-1",
                categoryID: 2,
                picture: "lorebook/Xaphan/Bestiary/boss-5-1",
                flag: "XaphanHelper_StatFlag_BossCh5",
                subCategoryID: "advc2"
            ));

            list.Add(new LorebookData(
                entryID: "advc3",
                categoryID: 2,
                picture: null,
                flag: null
            ));

            list.Add(new LorebookData(
                entryID: "strwb",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/strwb",
                flag: "XaphanHelper_StatFlag_Strawberry",
                subCategoryID: "advc3"
            ));

            list.Add(new LorebookData(
                entryID: "heart",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/heart",
                flag: "XaphanHelper_StatFlag_Heart",
                subCategoryID: "advc3"
            ));

            list.Add(new LorebookData(
                entryID: "gems",
                categoryID: 2,
                picture: "lorebook/Xaphan/Adventure/gems",
                flag: "XaphanHelper_StatFlag_GemCh1",
                subCategoryID: "advc3"
            ));

            return list;
        }
    }
}
