using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Monocle;

namespace Celeste.Mod.XaphanHelper
{
    class Commands
    {
        private static Level level = Engine.Scene as Level;

        private static string prefix = level.Session.Area.LevelSet;

        private static int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;

        // In-Game Map commands

        [Command("clear_ingamemap", "Clear the In-Game map for the current chapter or all chapters")]
        public static void Cmd_Clear_InGameMap(bool allChapters = false, bool includeCurrentRoom = false)
        {
            List<string> TilesToRemove = new();
            List<string> ExtraUnexploredRoomsTilesToRemove = new();
            List<string> FlagsToRemove = new();
            if (!allChapters)
            {
                foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                {
                    if (visitedRoom.Contains(prefix + "/Ch" + chapterIndex + "/") && (!includeCurrentRoom ? !visitedRoom.Contains(level.Session.Level) : true))
                    {
                        TilesToRemove.Add(visitedRoom);
                    }
                }
                foreach (string visitedRoomTile in XaphanModule.ModSaveData.VisitedRoomsTiles)
                {
                    if (visitedRoomTile.Contains(prefix + "/Ch" + chapterIndex + "/"))
                    {
                        TilesToRemove.Add(visitedRoomTile);
                    }
                }
                foreach (string extraUnexploredRoomTile in XaphanModule.ModSaveData.ExtraUnexploredRooms)
                {
                    if (extraUnexploredRoomTile.Contains(prefix + "/Ch" + chapterIndex + "/"))
                    {
                        ExtraUnexploredRoomsTilesToRemove.Add(extraUnexploredRoomTile);
                    }
                }
                foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (flag.Contains(prefix + "/Ch" + chapterIndex + "_MapShard"))
                    {
                        FlagsToRemove.Add(flag);
                    }
                }
            }
            else
            {
                foreach (string visitedRoom in XaphanModule.ModSaveData.VisitedRooms)
                {
                    if (visitedRoom.Contains(prefix) && (!includeCurrentRoom ? !visitedRoom.Contains(level.Session.Level) : true))
                    {
                        TilesToRemove.Add(visitedRoom);
                    }
                }
                foreach (string visitedRoomTile in XaphanModule.ModSaveData.VisitedRoomsTiles)
                {
                    if (visitedRoomTile.Contains(prefix))
                    {
                        TilesToRemove.Add(visitedRoomTile);
                    }
                }
                foreach (string extraUnexploredRoomTile in XaphanModule.ModSaveData.ExtraUnexploredRooms)
                {
                    if (extraUnexploredRoomTile.Contains(prefix))
                    {
                        ExtraUnexploredRoomsTilesToRemove.Add(extraUnexploredRoomTile);
                    }
                }
                foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (flag.Contains(prefix) && flag.Contains("_MapShard"))
                    {
                        FlagsToRemove.Add(flag);
                    }
                }
            }
            foreach (string value in TilesToRemove)
            {
                XaphanModule.ModSaveData.VisitedRooms.Remove(value);
                XaphanModule.ModSaveData.VisitedRoomsTiles.Remove(value);
            }
            foreach (string value in ExtraUnexploredRoomsTilesToRemove)
            {
                XaphanModule.ModSaveData.ExtraUnexploredRooms.Remove(value);
            }
            foreach (string value in FlagsToRemove)
            {
                XaphanModule.ModSaveData.SavedFlags.Remove(value);
            }
        }

        [Command("reveal_ingamemap", "Reveal or hide all rooms on the In-Game map")]
        private static void Cmd_Reveal_InGameMap(bool state = true)
        {
            if (XaphanModule.useIngameMap)
            {
                MapDisplay.ForceRevealUnexploredRooms = state;
                MiniMap minimap = level.Tracker.GetEntity<MiniMap>();
                if (minimap != null && !level.Session.GetFlag("Map_Opened"))
                {
                    minimap.RemoveSelf();
                }
                if (level.Session.GetFlag("Map_Opened"))
                {
                    MapDisplay display = level.Tracker.GetEntity<MapDisplay>();
                    if (display != null)
                    {
                        display.UpdateMap(chapterIndex, level.Session.Level, 0);
                    }
                }
            }
        }

        // Warps Commands

        [Command("clear_warps", "Clear all warps unlocked in this campaign")]
        public static void Cmd_Clear_Warps()
        {
            List<string> ToRemove = new();
            foreach (string warp in XaphanModule.ModSaveData.UnlockedWarps)
            {
                if (warp.Contains(prefix))
                {
                    ToRemove.Add(warp);
                }
            }
            foreach (string value in ToRemove)
            {
                XaphanModule.ModSaveData.UnlockedWarps.Remove(value);
            }
        }

        // Upgrades Commands

        [Command("remove_upgrade", "Remove specified upgrades (Separated by a semicolon). If no upgrades are specified, remove all upgrades. Avaiable upgrades: PowerGrip, ClimbingKit, SpiderMagnet, DashBoots, SpaceJump, LightningDash, LongBeam, IceBeam, WaveBeam, MissilesModule, SuperMissilesModule, DroneTeleport, JumpBoost, HoverJet, VariaJacket, GravityJacket, Bombs, MegaBombs, RemoteDrone, GoldenFeather, EtherealDash, ScrewAttack, Binoculars, PortableStation, PulseRadar")]
        public static void Cmd_Remove_Upgrades(string upg = "")
        {
            if (XaphanModule.useUpgrades)
            {
                if (upg == "")
                {
                    upg = "PowerGrip;ClimbingKit;SpiderMagnet;DashBoots;SpaceJump;HoverJet;LightningDash;LongBeam;IceBeam;WaveBeam;DroneTeleport;VariaJacket;GravityJacket;Bombs;MegaBombs;RemoteDrone;GoldenFeather;EtherealDash;ScrewAttack;Binoculars;PortableStation;PulseRadar;MissilesModule;SuperMissilesModule;JumpBoost";
                }
                string[] upgrades = upg.Split(';');
                foreach (string u in upgrades)
                {
                    foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                    {
                        if (u == upgrade.ToString())
                        {
                            if (u == "SpaceJump")
                            {
                                XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                            }
                            else
                            {
                                XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(0);
                            }
                            level.Session.SetFlag("Upgrade_" + upgrade.ToString(), false);
                            if (XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade.ToString()))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(prefix + "_Upgrade_" + upgrade.ToString());
                            }
                        }
                    }
                }
            }
        }

        [Command("give_upgrade", "Give specified upgrades (Separated by a semicolon). If no upgrades are specified, give all upgrades. Avaiable upgrades: PowerGrip, ClimbingKit, SpiderMagnet, DashBoots, SpaceJump, LightningDash, LongBeam, IceBeam, WaveBeam, MissilesModule, SuperMissilesModule, DroneTeleport, JumpBoost, HoverJet, VariaJacket, GravityJacket, Bombs, MegaBombs, RemoteDrone, GoldenFeather, EtherealDash, ScrewAttack, Binoculars, PortableStation, PulseRadar")]
        private static void Cmd_Give_Upgrade(string upg = "")
        {
            if (XaphanModule.useUpgrades)
            {
                if (upg == "")
                {
                    upg = "PowerGrip;ClimbingKit;SpiderMagnet;DashBoots;SpaceJump;HoverJet;LightningDash;LongBeam;IceBeam;WaveBeam;DroneTeleport;VariaJacket;GravityJacket;Bombs;MegaBombs;RemoteDrone;GoldenFeather;EtherealDash;ScrewAttack;Binoculars;PortableStation;PulseRadar;MissilesModule;SuperMissilesModule;JumpBoost";
                }
                string[] upgrades = upg.Split(';');
                foreach (string u in upgrades)
                {
                    foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                    {
                        if (u == upgrade.ToString())
                        {
                            if (u == "SpaceJump")
                            {
                                XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(2);
                            }
                            else
                            {
                                XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                            }
                            level.Session.SetFlag("Upgrade_" + upgrade.ToString(), true);
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade.ToString()))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(prefix + "_Upgrade_" + upgrade.ToString());
                            }
                        }
                    }
                    ReActivateUpgrade(u);
                }
            }
        }

        public static void ReActivateUpgrade(string upg)
        {
            switch (upg)
            {
                case "PowerGrip":
                    if (XaphanModule.ModSaveData.PowerGripInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.PowerGripInactive.Remove(prefix);
                    }
                    break;
                case "ClimbingKit":
                    if (XaphanModule.ModSaveData.ClimbingKitInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.ClimbingKitInactive.Remove(prefix);
                    }
                    break;
                case "SpiderMagnet":
                    if (XaphanModule.ModSaveData.SpiderMagnetInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.SpiderMagnetInactive.Remove(prefix);
                    }
                    break;
                case "DashBoots":
                    if (XaphanModule.ModSaveData.DashBootsInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.DashBootsInactive.Remove(prefix);
                    }
                    break;
                case "SpaceJump":
                    if (XaphanModule.ModSaveData.SpaceJumpInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.SpaceJumpInactive.Remove(prefix);
                    }
                    break;
                case "HoverJet":
                    if (XaphanModule.ModSaveData.HoverJetInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.HoverJetInactive.Remove(prefix);
                    }
                    break;
                case "LightningDash":
                    if (XaphanModule.ModSaveData.LightningDashInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.LightningDashInactive.Remove(prefix);
                    }
                    break;
                case "DroneTeleport":
                    if (XaphanModule.ModSaveData.DroneTeleportInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.DroneTeleportInactive.Remove(prefix);
                    }
                    break;
                case "JumpBoost":
                    if (XaphanModule.ModSaveData.JumpBoostInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.JumpBoostInactive.Remove(prefix);
                    }
                    break;
                case "VariaJacket":
                    if (XaphanModule.ModSaveData.VariaJacketInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.VariaJacketInactive.Remove(prefix);
                    }
                    break;
                case "GravityJacket":
                    if (XaphanModule.ModSaveData.GravityJacketInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.GravityJacketInactive.Remove(prefix);
                    }
                    break;
                case "Bombs":
                    if (XaphanModule.ModSaveData.BombsInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.BombsInactive.Remove(prefix);
                    }
                    break;
                case "MegaBombs":
                    if (XaphanModule.ModSaveData.MegaBombsInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.MegaBombsInactive.Remove(prefix);
                    }
                    break;
                case "RemoteDrone":
                    if (XaphanModule.ModSaveData.RemoteDroneInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.RemoteDroneInactive.Remove(prefix);
                    }
                    break;
                case "GoldenFeather":
                    if (XaphanModule.ModSaveData.GoldenFeatherInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.GoldenFeatherInactive.Remove(prefix);
                    }
                    break;
                case "EtherealDash":
                    if (XaphanModule.ModSaveData.EtherealDashInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.EtherealDashInactive.Remove(prefix);
                    }
                    break;
                case "ScrewAttack":
                    if (XaphanModule.ModSaveData.ScrewAttackInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.ScrewAttackInactive.Remove(prefix);
                    }
                    break;
                case "Binoculars":
                    if (XaphanModule.ModSaveData.BinocularsInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.BinocularsInactive.Remove(prefix);
                    }
                    break;
                case "PortableStation":
                    if (XaphanModule.ModSaveData.PortableStationInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.PortableStationInactive.Remove(prefix);
                    }
                    break;
                case "PulseRadar":
                    if (XaphanModule.ModSaveData.PulseRadarInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.PulseRadarInactive.Remove(prefix);
                    }
                    break;
                case "MissilesModule":
                    if (XaphanModule.ModSaveData.MissilesModuleInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.MissilesModuleInactive.Remove(prefix);
                    }
                    break;
                case "SuperMissilesModule":
                    if (XaphanModule.ModSaveData.SuperMissilesModuleInactive.Contains(prefix))
                    {
                        XaphanModule.ModSaveData.SuperMissilesModuleInactive.Remove(prefix);
                    }
                    break;
                default:
                    break;
            }
        }

        [Command("reset_collectables_upgrades", "Remove all collectable upgrades (Energy Tanks, Missiles, Super Missiles and Fire Rate Modules) and allow to collect them again")]
        public static void Cmd_Reset_Collectables_Upgrades()
        {
            List<string> StaminaUpgradesToRemove = new();
            List<string> DroneMissilesUpgradesToRemove = new();
            List<string> DroneSuperMissilesUpgradesToRemove = new();
            List<string> DroneFireRateUpgradesToRemove = new();
            foreach (string staminaUpgrade in XaphanModule.ModSaveData.StaminaUpgrades)
            {
                if (staminaUpgrade.Contains(level.Session.Area.LevelSet))
                {
                    StaminaUpgradesToRemove.Add(staminaUpgrade);
                }
            }
            foreach (string droneMissileUpgrade in XaphanModule.ModSaveData.DroneMissilesUpgrades)
            {
                if (droneMissileUpgrade.Contains(level.Session.Area.LevelSet))
                {
                    DroneMissilesUpgradesToRemove.Add(droneMissileUpgrade);
                }
            }
            foreach (string droneSuperMissileUpgrade in XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
            {
                if (droneSuperMissileUpgrade.Contains(level.Session.Area.LevelSet))
                {
                    DroneSuperMissilesUpgradesToRemove.Add(droneSuperMissileUpgrade);
                }
            }
            foreach (string droneFireRateUpgrade in XaphanModule.ModSaveData.DroneFireRateUpgrades)
            {
                if (droneFireRateUpgrade.Contains(level.Session.Area.LevelSet))
                {
                    DroneFireRateUpgradesToRemove.Add(droneFireRateUpgrade);
                }
            }
            foreach (string value in StaminaUpgradesToRemove)
            {
                XaphanModule.ModSaveData.StaminaUpgrades.Remove(value);
            }
            foreach (string value in DroneMissilesUpgradesToRemove)
            {
                XaphanModule.ModSaveData.DroneMissilesUpgrades.Remove(value);
            }
            foreach (string value in DroneSuperMissilesUpgradesToRemove)
            {
                XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Remove(value);
            }
            foreach (string value in DroneFireRateUpgradesToRemove)
            {
                XaphanModule.ModSaveData.DroneFireRateUpgrades.Remove(value);
            }
        }

        // General Commands

        [Command("reset_campaign", "Delete all progress on the current campaign and restart it from the beggining (Campaigns that use a Merge Chapter Controller only)")]
        public static void Cmd_Reset_Campaign()
        {
            if (XaphanModule.isInLevel && XaphanModule.useMergeChaptersController)
            {
                Cmd_Clear_InGameMap(true, true);
                Cmd_Clear_Warps();
                Cmd_Remove_Upgrades();
                Cmd_Reset_Collectables_Upgrades();

                XaphanModule.ModSaveData.SavedRoom.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedChapter.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedSpawn.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedLightingAlphaAdd.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedBloomBaseAdd.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedCoreMode.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedMusic.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedAmbience.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedNoLoadEntities.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedTime.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedFromBeginning.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedSesionFlags.Remove(level.Session.Area.LevelSet);
                XaphanModule.ModSaveData.SavedSessionStrawberries.Remove(level.Session.Area.LevelSet);
                List<string> FlagsToRemove = new();
                List<string> CutscenesToRemove = new();
                List<string> GlobalFlagsToRemove = new();
                foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (savedFlag.Contains(level.Session.Area.LevelSet) && savedFlag != "Xaphan/0_Skip_Vignette")
                    {
                        FlagsToRemove.Add(savedFlag);
                    }
                }
                foreach (string cutscene in XaphanModule.ModSaveData.WatchedCutscenes)
                {
                    if (cutscene.Contains(level.Session.Area.LevelSet))
                    {
                        CutscenesToRemove.Add(cutscene);
                    }
                }
                foreach (string globalFlag in XaphanModule.ModSaveData.GlobalFlags)
                {
                    if (globalFlag.Contains(level.Session.Area.LevelSet))
                    {
                        GlobalFlagsToRemove.Add(globalFlag);
                    }
                }
                foreach (string value in FlagsToRemove)
                {
                    XaphanModule.ModSaveData.SavedFlags.Remove(value);
                }
                foreach (string value in CutscenesToRemove)
                {
                    XaphanModule.ModSaveData.WatchedCutscenes.Remove(value);
                }
                foreach (string value in GlobalFlagsToRemove)
                {
                    XaphanModule.ModSaveData.GlobalFlags.Remove(value);
                }
                LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + (XaphanModule.MergeChaptersControllerKeepPrologue ? 1 : 0), AreaMode.Normal)), fromSaveData: false);
            }
            else
            {
                Engine.Commands.Log("You are not currently in a campaign, or this campaign do not use a Merge Chapter Controller. Aborting...");
            }
        }
    }
}
