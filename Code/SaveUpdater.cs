using System;
using System.Collections.Generic;

namespace Celeste.Mod.XaphanHelper
{
    class SaveUpdater
    {
        private static List<string> upgradesToRemove = new();

        public static void UpdateSave(Level level)
        {
            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                if (XaphanModule.ModSaveData.SoCMVer < 300)
                {
                    // Remove Bombs upgrade

                    if (XaphanModule.BombsCollected(level))
                    {
                        upgradesToRemove.Add("Bombs");
                    }

                    // Remove Bombs upgrade Achievement

                    if (XaphanModule.ModSaveData.Achievements.Contains("upg5"))
                    {
                        XaphanModule.ModSaveData.Achievements.Remove("upg5");
                    }

                    Dictionary<string, string> RoomsNamesConversion = new();
                    for (int i = SaveData.Instance.LevelSetStats.AreaOffset; i < SaveData.Instance.LevelSetStats.AreaOffset + SaveData.Instance.LevelSetStats.Areas.Count; i++)
                    {
                        // Get list of new room names

                        RoomsNamesConversion.Clear();
                        if (i == SaveData.Instance.LevelSetStats.AreaOffset) // Prologue
                        {
                            RoomsNamesConversion.Add("A-00", "A-01");
                            RoomsNamesConversion.Add("A-01", "A-02");
                            RoomsNamesConversion.Add("A-02", "A-03");
                            RoomsNamesConversion.Add("A-03", "A-04");
                            RoomsNamesConversion.Add("A-04", "A-05");
                            RoomsNamesConversion.Add("A-05", "A-06");
                            RoomsNamesConversion.Add("A-06", "A-07");
                            RoomsNamesConversion.Add("A-07", "A-08");
                            RoomsNamesConversion.Add("A-08", "A-09");
                            RoomsNamesConversion.Add("A-09", "A-10");
                            RoomsNamesConversion.Add("A-10", "A-11");
                            RoomsNamesConversion.Add("N-Ch1-0", "A-Ch1-0");
                            RoomsNamesConversion.Add("N-Ch1-0-Arrow", "A-Ch1-0-Arrow");
                            RoomsNamesConversion.Add("N-Ch5-0", "A-Ch5-0");
                            RoomsNamesConversion.Add("N-Ch5-0-Arrow", "A-Ch5-0-Arrow");
                            RoomsNamesConversion.Add("S-Begin", "A-00");
                            RoomsNamesConversion.Add("S-End", "A-12");
                            RoomsNamesConversion.Add("U-00", "A-U0");
                        }
                        else if (i == SaveData.Instance.LevelSetStats.AreaOffset + 1) // Ancient Ruins
                        {
                            RoomsNamesConversion.Add("A-00", "B-01");
                            RoomsNamesConversion.Add("A-01", "B-02");
                            RoomsNamesConversion.Add("A-02", "B-15");
                            RoomsNamesConversion.Add("A-03", "B-08");
                            RoomsNamesConversion.Add("A-04", "B-09");
                            RoomsNamesConversion.Add("A-05", "B-16");
                            RoomsNamesConversion.Add("A-06", "B-10");
                            RoomsNamesConversion.Add("A-07", "B-11");
                            RoomsNamesConversion.Add("A-W0", "B-W0");
                            RoomsNamesConversion.Add("A-W1", "B-W1");
                            RoomsNamesConversion.Add("B-00", "B-12");
                            RoomsNamesConversion.Add("B-01", "B-13");
                            RoomsNamesConversion.Add("B-02", "B-14");
                            RoomsNamesConversion.Add("B-03", "B-04");
                            RoomsNamesConversion.Add("B-04", "B-18");
                            RoomsNamesConversion.Add("B-05", "B-19");
                            RoomsNamesConversion.Add("B-06", "B-20");
                            RoomsNamesConversion.Add("B-07", "B-21");
                            RoomsNamesConversion.Add("B-08", "B-22");
                            RoomsNamesConversion.Add("B-09", "B-23");
                            RoomsNamesConversion.Add("B-10", "B-24");
                            RoomsNamesConversion.Add("B-11", "B-25");
                            RoomsNamesConversion.Add("B-12", "B-26");
                            RoomsNamesConversion.Add("B-W0", "B-W2");
                            RoomsNamesConversion.Add("C-00", "B-27");
                            RoomsNamesConversion.Add("C-01", "B-28");
                            RoomsNamesConversion.Add("C-02", "B-29");
                            RoomsNamesConversion.Add("C-03", "B-30");
                            RoomsNamesConversion.Add("C-04", "B-35");
                            RoomsNamesConversion.Add("C-05", "B-39");
                            RoomsNamesConversion.Add("C-06", "B-38");
                            RoomsNamesConversion.Add("C-07", "B-32");
                            RoomsNamesConversion.Add("C-08", "B-33");
                            RoomsNamesConversion.Add("C-09", "B-34");
                            RoomsNamesConversion.Add("C-10", "B-37");
                            RoomsNamesConversion.Add("C-11", "B-43");
                            RoomsNamesConversion.Add("C-12", "B-44");
                            RoomsNamesConversion.Add("C-13", "B-45");
                            RoomsNamesConversion.Add("C-W0", "B-W3");
                            RoomsNamesConversion.Add("C-W1", "B-W4");
                            RoomsNamesConversion.Add("N-Ch0-0", "B-Ch0-0");
                            RoomsNamesConversion.Add("N-Ch0-0-Arrow", "B-Ch0-0-Arrow");
                            RoomsNamesConversion.Add("N-Ch2-0", "B-Ch2-0");
                            RoomsNamesConversion.Add("N-Ch2-0-Arrow", "B-Ch2-0-Arrow");
                            RoomsNamesConversion.Add("S-Begin", "B-00");
                            RoomsNamesConversion.Add("S-End", "B-46");
                            RoomsNamesConversion.Add("U-00", "B-U0");
                            RoomsNamesConversion.Add("U-01", "B-U1");
                        }
                        else if (i == SaveData.Instance.LevelSetStats.AreaOffset + 2) // Forgotten Abysses
                        {
                            RoomsNamesConversion.Add("A-00", "G-01");
                            RoomsNamesConversion.Add("A-01", "G-02");
                            RoomsNamesConversion.Add("A-02", "G-03");
                            RoomsNamesConversion.Add("A-03", "G-04");
                            RoomsNamesConversion.Add("A-04", "G-05");
                            RoomsNamesConversion.Add("A-05", "G-06");
                            RoomsNamesConversion.Add("A-06", "G-07");
                            RoomsNamesConversion.Add("A-07", "G-08");
                            RoomsNamesConversion.Add("A-08", "G-09");
                            RoomsNamesConversion.Add("A-09", "G-10");
                            RoomsNamesConversion.Add("A-10", "G-11");
                            RoomsNamesConversion.Add("A-11", "G-12");
                            RoomsNamesConversion.Add("A-W0", "G-W0");
                            RoomsNamesConversion.Add("A-W1", "G-W2");
                            RoomsNamesConversion.Add("B-00", "H-00");
                            RoomsNamesConversion.Add("B-01-L", "H-01-L");
                            RoomsNamesConversion.Add("B-01-R", "H-01-R");
                            RoomsNamesConversion.Add("B-02-L", "H-02-L");
                            RoomsNamesConversion.Add("B-02-R", "H-02-R");
                            RoomsNamesConversion.Add("B-03-L", "H-03-L");
                            RoomsNamesConversion.Add("B-03-R", "H-03-R");
                            RoomsNamesConversion.Add("B-04-L", "H-04-L");
                            RoomsNamesConversion.Add("B-04-R", "H-04-R");
                            RoomsNamesConversion.Add("B-05-L", "H-05-L");
                            RoomsNamesConversion.Add("B-05-R", "H-05-R");
                            RoomsNamesConversion.Add("B-06", "H-06");
                            RoomsNamesConversion.Add("B-07", "H-07");
                            RoomsNamesConversion.Add("B-08", "H-08");
                            RoomsNamesConversion.Add("B-09", "H-09");
                            RoomsNamesConversion.Add("B-10", "H-10");
                            RoomsNamesConversion.Add("B-11", "H-11");
                            RoomsNamesConversion.Add("B-W0", "H-W0");
                            RoomsNamesConversion.Add("B-W1", "H-W1");
                            RoomsNamesConversion.Add("C-00", "I-00");
                            RoomsNamesConversion.Add("C-01", "I-01");
                            RoomsNamesConversion.Add("C-02", "I-02");
                            RoomsNamesConversion.Add("C-03", "I-03");
                            RoomsNamesConversion.Add("C-04", "I-04");
                            RoomsNamesConversion.Add("C-05", "I-05");
                            RoomsNamesConversion.Add("C-06", "I-06");
                            RoomsNamesConversion.Add("C-07", "I-07");
                            RoomsNamesConversion.Add("C-08", "I-08");
                            RoomsNamesConversion.Add("C-09", "I-09");
                            RoomsNamesConversion.Add("C-10", "I-10");
                            RoomsNamesConversion.Add("C-11", "I-11");
                            RoomsNamesConversion.Add("C-12", "I-12");
                            RoomsNamesConversion.Add("C-13", "I-13");
                            RoomsNamesConversion.Add("C-14", "I-14");
                            RoomsNamesConversion.Add("C-15", "I-15");
                            RoomsNamesConversion.Add("C-16", "I-16");
                            RoomsNamesConversion.Add("C-17", "I-17");
                            RoomsNamesConversion.Add("C-W0", "I-W0");
                            RoomsNamesConversion.Add("C-W1", "I-W1");
                            RoomsNamesConversion.Add("D-00", "I-18");
                            RoomsNamesConversion.Add("D-01", "I-19");
                            RoomsNamesConversion.Add("D-02", "I-20");
                            RoomsNamesConversion.Add("D-03", "I-21");
                            RoomsNamesConversion.Add("D-04", "I-22");
                            RoomsNamesConversion.Add("D-05", "I-23");
                            RoomsNamesConversion.Add("D-06", "I-24");
                            RoomsNamesConversion.Add("D-07", "I-25");
                            RoomsNamesConversion.Add("D-08", "I-26");
                            RoomsNamesConversion.Add("D-09", "I-27");
                            RoomsNamesConversion.Add("D-10", "I-28");
                            RoomsNamesConversion.Add("D-11", "I-29");
                            RoomsNamesConversion.Add("D-12", "I-30");
                            RoomsNamesConversion.Add("D-13", "I-31");
                            RoomsNamesConversion.Add("D-14", "I-32");
                            RoomsNamesConversion.Add("D-15", "I-33");
                            RoomsNamesConversion.Add("D-16", "I-34");
                            RoomsNamesConversion.Add("D-17", "I-35");
                            RoomsNamesConversion.Add("D-18", "I-36");
                            RoomsNamesConversion.Add("D-19", "I-37");
                            RoomsNamesConversion.Add("D-20", "I-38");
                            RoomsNamesConversion.Add("D-21", "I-39");
                            RoomsNamesConversion.Add("D-W0", "I-W2");
                            RoomsNamesConversion.Add("D-W1", "I-W3");
                            RoomsNamesConversion.Add("D-W2", "I-W4");
                            RoomsNamesConversion.Add("N-Ch1-0", "G-Ch1-0");
                            RoomsNamesConversion.Add("N-Ch1-0-Arrow", "G-Ch1-0-Arrow");
                            RoomsNamesConversion.Add("dummy-14", "I-Ch2-0-Shaft");
                            RoomsNamesConversion.Add("N-Ch5-0", "I-Ch5-0");
                            RoomsNamesConversion.Add("N-Ch5-0-Arrow", "I-Ch5-0-Arrow");
                            RoomsNamesConversion.Add("S-Begin", "G-00");
                            RoomsNamesConversion.Add("S-End", "I-40");
                            RoomsNamesConversion.Add("U-00", "G-U0");
                            RoomsNamesConversion.Add("U-01", "I-U0");
                        }

                        // Adjust collected strawberries

                        HashSet<EntityID> oldStrawberries = new();
                        HashSet<EntityID> newStrawberries = new();
                        HashSet<EntityID> deletedStrawberries = new();
                        foreach (EntityID strawberry in SaveData.Instance.Areas_Safe[i].Modes[0].Strawberries)
                        {
                            // Removed strawberries
                            if (i == SaveData.Instance.LevelSetStats.AreaOffset + 1 && (strawberry.Level == "A-06" || strawberry.Level == "A-07"))
                            {
                                deletedStrawberries.Add(strawberry);
                            }
                            // Old boss strawberry
                            if (strawberry.Level == "D-03")
                            {
                                deletedStrawberries.Add(strawberry);
                                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch2_Boss_ShadowMonster_CM");
                            }
                            // Golden Strawberries
                            if ((i == SaveData.Instance.LevelSetStats.AreaOffset + 1 && strawberry.Level == "A-00") || (i == SaveData.Instance.LevelSetStats.AreaOffset + 2 && strawberry.Level == "A-01"))
                            {
                                deletedStrawberries.Add(strawberry);
                            }
                        }
                        if (deletedStrawberries.Count > 0)
                        {
                            foreach (EntityID strawberry in deletedStrawberries)
                            {
                                SaveData.Instance.Areas_Safe[i].Modes[0].Strawberries.Remove(strawberry);
                                AreaModeStats areaModeStats = SaveData.Instance.Areas_Safe[i].Modes[0];
                                areaModeStats.Strawberries.Remove(strawberry);
                                areaModeStats.TotalStrawberries--;
                                SaveData.Instance.TotalStrawberries_Safe--;
                            }
                        }
                        foreach (EntityID strawberry in SaveData.Instance.Areas_Safe[i].Modes[0].Strawberries)
                        {
                            foreach (KeyValuePair<string, string> oldRoom in RoomsNamesConversion)
                            {
                                if (strawberry.Level == oldRoom.Key)
                                {
                                    oldStrawberries.Add(strawberry);
                                    if (i == SaveData.Instance.LevelSetStats.AreaOffset + 1 && oldRoom.Key == "C-01") // New strawberry has a different ID -- Hardcode the new ID insead of using old one
                                    {
                                        newStrawberries.Add(new EntityID(oldRoom.Value, 1708));
                                    }
                                    else
                                    {
                                        newStrawberries.Add(new EntityID(oldRoom.Value, strawberry.ID));
                                    }
                                }
                            }
                        }
                        foreach (EntityID strawberry in oldStrawberries)
                        {
                            SaveData.Instance.Areas_Safe[i].Modes[0].Strawberries.Remove(strawberry);
                            level.Session.DoNotLoad.Remove(strawberry);
                        }
                        foreach (EntityID strawberry in newStrawberries)
                        {
                            SaveData.Instance.Areas_Safe[i].Modes[0].Strawberries.Add(strawberry);
                            level.Session.DoNotLoad.Add(strawberry);
                        }

                        // Adjust Lorebook entries

                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch0/A-03-1-0"))
                        {
                            level.Session.SetFlag("LorebookEntry_loc0");
                        }
                        if (XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_B"))
                        {
                            level.Session.SetFlag("CS_Ch0_Gem_Room_B");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/S-Begin-0-1"))
                        {
                            level.Session.SetFlag("LorebookEntry_loc1-1");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch2/S-Begin-0-1"))
                        {
                            level.Session.SetFlag("LorebookEntry_loc2-1");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch2/B-00-1-0"))
                        {
                            level.Session.SetFlag("LorebookEntry_loc2-2");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch2/C-00-0-0"))
                        {
                            level.Session.SetFlag("LorebookEntry_loc2-3");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch2/C-07-0-0"))
                        {
                            level.Session.SetFlag("LorebookEntry_poi2-1");
                        }

                        // Adjust in-game map explored and warps

                        int index = i - SaveData.Instance.LevelSetStats.AreaOffset;
                        HashSet<string> oldVisitedRooms = new();
                        HashSet<string> oldVisitedRoomsTiles = new();
                        HashSet<string> oldExtraUnexploredRooms = new();
                        HashSet<string> newVisitedRooms = new();
                        HashSet<string> newVisitedRoomsTiles = new();
                        HashSet<string> newExtraUnexploredRooms = new();
                        HashSet<string> oldUnlockedWarps = new();
                        HashSet<string> newUnlockedWarps = new();
                        foreach (KeyValuePair<string, string> oldRoom in RoomsNamesConversion)
                        {
                            foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                            {
                                if (visitedRoom == "Xaphan/0/Ch" + index + "/" + oldRoom.Key)
                                {
                                    oldVisitedRooms.Add(visitedRoom);
                                    newVisitedRooms.Add(visitedRoom.Replace(oldRoom.Key, oldRoom.Value));
                                }
                            }
                            foreach (string visitedRoomTile in XaphanModule.ModSaveData.VisitedRoomsTiles)
                            {
                                if (visitedRoomTile.Contains("Xaphan/0/Ch" + index + "/" + oldRoom.Key))
                                {
                                    oldVisitedRoomsTiles.Add(visitedRoomTile);
                                    newVisitedRoomsTiles.Add(visitedRoomTile.Replace(oldRoom.Key, oldRoom.Value));
                                }
                            }
                            foreach (string extraUnexploredRooms in XaphanModule.ModSaveData.ExtraUnexploredRooms)
                            {
                                if (extraUnexploredRooms == "Xaphan/0/Ch" + index + "/" + oldRoom.Key)
                                {
                                    oldExtraUnexploredRooms.Add(extraUnexploredRooms);
                                    newExtraUnexploredRooms.Add(extraUnexploredRooms.Replace(oldRoom.Key, oldRoom.Value));
                                }
                            }
                            foreach (string unlockedWarp in XaphanModule.ModSaveData.UnlockedWarps)
                            {
                                if (unlockedWarp == "Xaphan/0_Ch" + index + "_" + oldRoom.Key)
                                {
                                    oldUnlockedWarps.Add(unlockedWarp);
                                    newUnlockedWarps.Add(unlockedWarp.Replace(oldRoom.Key, oldRoom.Value));
                                }
                            }
                        }
                        foreach (string visitedRoom in oldVisitedRooms)
                        {
                            XaphanModule.ModSaveData.VisitedRooms.Remove(visitedRoom);
                        }
                        // Specific removed rooms
                        if (XaphanModule.ModSaveData.VisitedRooms.Contains("Xaphan/0/Ch1/A-06"))
                        {
                            XaphanModule.ModSaveData.VisitedRooms.Remove("Xaphan/0/Ch1/A-06");
                        }
                        if (XaphanModule.ModSaveData.VisitedRooms.Contains("Xaphan/0/Ch1/A-07"))
                        {
                            XaphanModule.ModSaveData.VisitedRooms.Remove("Xaphan/0/Ch1/A-07");
                        }
                        foreach (string visitedRoomTile in oldVisitedRoomsTiles)
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove(visitedRoomTile);
                        }
                        foreach (string extraUnexploredRooms in oldExtraUnexploredRooms)
                        {
                            XaphanModule.ModSaveData.ExtraUnexploredRooms.Remove(extraUnexploredRooms);
                        }
                        // Specific removed rooms tiles
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/A-06-0-0"))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove("Xaphan/0/Ch1/A-06-0-0");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/A-06-0-1"))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove("Xaphan/0/Ch1/A-06-0-1");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/A-06-0-2"))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove("Xaphan/0/Ch1/A-06-0-2");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/A-07-0-0"))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove("Xaphan/0/Ch1/A-07-0-0");
                        }
                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains("Xaphan/0/Ch1/A-07-0-1"))
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Remove("Xaphan/0/Ch1/A-07-0-1");
                        }
                        foreach (string unlockedWarp in oldUnlockedWarps)
                        {
                            XaphanModule.ModSaveData.UnlockedWarps.Remove(unlockedWarp);
                        }
                        foreach (string visitedRoom in newVisitedRooms)
                        {
                            XaphanModule.ModSaveData.VisitedRooms.Add(visitedRoom);
                        }
                        foreach (string visitedRoomTile in newVisitedRoomsTiles)
                        {
                            XaphanModule.ModSaveData.VisitedRoomsTiles.Add(visitedRoomTile);
                        }
                        foreach (string extraUnexploredRooms in newExtraUnexploredRooms)
                        {
                            XaphanModule.ModSaveData.ExtraUnexploredRooms.Add(extraUnexploredRooms);
                        }
                        foreach (string unlockedWarp in newUnlockedWarps)
                        {
                            XaphanModule.ModSaveData.UnlockedWarps.Add(unlockedWarp);
                        }
                    }
                }
            }
            XaphanModule.ModSaveData.SoCMVer = XaphanModule.SoCMVersion.Major * 100 + XaphanModule.SoCMVersion.Minor * 10 + XaphanModule.SoCMVersion.Build;
        }

        public static void RemoveUpgrades()
        {
            foreach (string upgrade in upgradesToRemove)
            {
                Commands.Cmd_Remove_Upgrades(upgrade);
            }
            upgradesToRemove.Clear();
        }
    }
}
