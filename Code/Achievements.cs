using System;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;

namespace Celeste.Mod.XaphanHelper
{
    static class Achievements
    {
        public static List<AchievementData> GenerateAchievementsList(Session session)
        {
            StatsFlags.GetStats(session);
            List<AchievementData> list = new();

            // General
            list.Add(new AchievementData(
                achievementID: "upg1",
                categoryID: 0,
                icon: "achievements/Xaphan/Upgrade1",
                flag: "Upgrade_DashBoots",
                currentValue: session.GetFlag("Upgrade_DashBoots") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "upg2",
                categoryID: 0,
                icon: "achievements/Xaphan/Upgrade2",
                flag: "Upgrade_PowerGrip",
                currentValue: session.GetFlag("Upgrade_PowerGrip") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "upg3",
                categoryID: 0,
                icon: "achievements/Xaphan/Upgrade3",
                flag: "Upgrade_ClimbingKit",
                currentValue: session.GetFlag("Upgrade_ClimbingKit") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "upg4",
                categoryID: 0,
                icon: "achievements/Xaphan/Upgrade4",
                flag: "Upgrade_SpaceJump",
                currentValue: session.GetFlag("Upgrade_SpaceJump") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "upg5",
                categoryID: 0,
                icon: "achievements/Xaphan/Upgrade5",
                flag: "Upgrade_Bombs",
                currentValue: session.GetFlag("Upgrade_Bombs") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "upg6",
                    categoryID: 0,
                    icon: "achievements/Xaphan/Upgrade6",
                    flag: "Upgrade_SpiderMagnet",
                    currentValue: session.GetFlag("Upgrade_SpiderMagnet") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "upg7",
                    categoryID: 0,
                    icon: "achievements/Xaphan/Upgrade7",
                    flag: "Upgrade_RemoteDrone",
                    currentValue: session.GetFlag("Upgrade_RemoteDrone") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "upg8",
                    categoryID: 0,
                    icon: "achievements/Xaphan/Upgrade8",
                    flag: "Upgrade_MissilesModule",
                    currentValue: session.GetFlag("Upgrade_MissilesModule") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "upgOpt1",
                    categoryID: 0,
                    icon: "achievements/Xaphan/UpgradeOptional1",
                    flag: "Upgrade_Binoculars",
                    currentValue: session.GetFlag("Upgrade_Binoculars") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "upgOpt2",
                    categoryID: 0,
                    icon: "achievements/Xaphan/UpgradeOptional2",
                    flag: "Upgrade_PulseRadar",
                    currentValue: session.GetFlag("Upgrade_PulseRadar") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
            }

            list.Add(new AchievementData(
                achievementID: "map0",
                categoryID: 0,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh0",
                currentValue: StatsFlags.CurrentTiles[0],
                maxValue: StatsFlags.TotalTiles[0],
                medals: 10
            ));
            list.Add(new AchievementData(
                achievementID: "strwb0",
                categoryID: 0,
                icon: "achievements/Xaphan/StrawberryBronze",
                flag: "XaphanHelper_StatFlag_Strawberry",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_Strawberry") ? 1 : 0,
                maxValue: 1,
                medals: 10
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "tank0",
                    categoryID: 0,
                    icon: "achievements/Xaphan/EnergyTankBronze",
                    flag: "XaphanHelper_StatFlag_EnergyTank",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_EnergyTank") ? 1 : 0,
                    maxValue: 1,
                    medals: 10,
                    reqID: "upg2"
                ));
                list.Add(new AchievementData(
                    achievementID: "dfrm0",
                    categoryID: 0,
                    icon: "achievements/Xaphan/FireRateModuleBronze",
                    flag: "XaphanHelper_StatFlag_FireRateModule",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_FireRateModule") ? 1 : 0,
                    maxValue: 1,
                    medals: 10,
                    reqID: "upg7"
                ));
                list.Add(new AchievementData(
                    achievementID: "dmiss0",
                    categoryID: 0,
                    icon: "achievements/Xaphan/MissileBronze",
                    flag: "XaphanHelper_StatFlag_Missile",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_Missile") ? 1 : 0,
                    maxValue: 1,
                    medals: 10,
                    reqID: "upg8"
                ));
            }

            int currentTotalStrawberries = 0;
            int currentTotalEnergyTanks = 0;
            int currentTotalFireRateModules = 0;
            int currentTotalMissiles = 0;
            int currentTotalSuperMissiles = 0;
            int maxTotalStrawberries = 0;
            int maxTotalEnergyTanks = 0;
            int maxTotalFireRateModules = 0;
            int maxTotalMissiles = 0;
            int maxTotalSuperMissiles = 0;

            for (int i = 1; i <= (XaphanModule.SoCMVersion >= new Version(3, 0, 0) ? 5 : 2); i++)
            {
                currentTotalStrawberries += (StatsFlags.CurrentStrawberries[i] - (session.GetFlag("XaphanHelper_StatFlag_GoldenCh" + i + "-1") ? 1 : 0));
                currentTotalEnergyTanks += StatsFlags.CurrentEnergyTanks[i];
                currentTotalFireRateModules += StatsFlags.CurrentFireRateModules[i];
                currentTotalMissiles += StatsFlags.CurrentMissiles[i];
                currentTotalSuperMissiles += StatsFlags.CurrentSuperMissiles[i];
                maxTotalStrawberries += StatsFlags.TotalStrawberries[i];
                maxTotalEnergyTanks += StatsFlags.TotalEnergyTanks[i];
                maxTotalFireRateModules += StatsFlags.TotalFireRateModules[i];
                maxTotalMissiles += StatsFlags.TotalMissiles[i];
                maxTotalSuperMissiles += StatsFlags.TotalSuperMissiles[i];
            }

            int currentTotalCassettes = StatsFlags.cassetteCount;
            int currentTotalASideHearts = StatsFlags.heartCount;
            int maxTotalCassettes = SaveData.Instance.GetLevelSetStatsFor(SaveData.Instance.LevelSet).MaxCassettes;
            int maxTotalASideHearts = StatsFlags.TotalASideHearts;

            list.Add(new AchievementData(
                achievementID: "strwb",
                categoryID: 0,
                icon: "achievements/Xaphan/StrawberryCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_Strawberries",
                currentValue: currentTotalStrawberries,
                maxValue: maxTotalStrawberries,
                medals: 25
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "tank",
                    categoryID: 0,
                    icon: "achievements/Xaphan/EnergyTankCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_EnergyTanks",
                    currentValue: currentTotalEnergyTanks,
                    maxValue: maxTotalEnergyTanks,
                    medals: 25,
                    reqID: "upg2"
                ));
                list.Add(new AchievementData(
                    achievementID: "dfrm",
                    categoryID: 0,
                    icon: "achievements/Xaphan/FireRateModuleCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_FireRateModules",
                    currentValue: currentTotalFireRateModules,
                    maxValue: maxTotalFireRateModules,
                    medals: 25,
                    reqID: "upg7"
                ));
            }

            list.Add(new AchievementData(
                achievementID: "cass",
                categoryID: 0,
                icon: "achievements/Xaphan/CassetteCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_Cassettes",
                currentValue: currentTotalCassettes,
                maxValue: maxTotalCassettes,
                medals: 25
            ));
            list.Add(new AchievementData(
                achievementID: "heart",
                categoryID: 0,
                icon: "achievements/Xaphan/HeartCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_Hearts",
                currentValue: currentTotalASideHearts,
                maxValue: maxTotalASideHearts,
                medals: 25
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "map",
                    categoryID: 0,
                    icon: "achievements/Xaphan/MapCheckmarkGold",
                    flag: "XaphanHelper_StatFlag_Map",
                    currentValue: StatsFlags.CurrentTiles[0] + StatsFlags.CurrentTiles[1] + StatsFlags.CurrentTiles[2] + StatsFlags.CurrentTiles[3] + StatsFlags.CurrentTiles[4] + StatsFlags.CurrentTiles[5],
                    maxValue: StatsFlags.TotalTiles[0] + StatsFlags.TotalTiles[1] + StatsFlags.TotalTiles[2] + StatsFlags.TotalTiles[3] + StatsFlags.TotalTiles[4] + StatsFlags.TotalTiles[5],
                    medals: 50
                    ));
                list.Add(new AchievementData(
                   achievementID: "items",
                   categoryID: 0,
                   icon: "achievements/Xaphan/ItemsCheckmarkGold",
                   flag: "XaphanHelper_StatFlag_Items",
                   currentValue: currentTotalStrawberries + currentTotalEnergyTanks + currentTotalFireRateModules + currentTotalMissiles + currentTotalSuperMissiles + currentTotalCassettes + currentTotalASideHearts,
                   maxValue: maxTotalStrawberries + maxTotalEnergyTanks + maxTotalFireRateModules + maxTotalMissiles + maxTotalSuperMissiles + maxTotalCassettes + maxTotalASideHearts,
                   medals: 50
                ));
                list.Add(new AchievementData(
                    achievementID: "golden",
                    categoryID: 0,
                    icon: "achievements/Xaphan/Golden",
                    flag: "XaphanHelper_StatFlag_SoCMGolden",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_SoCMGolden") ? 1 : 0,
                    maxValue: 1,
                    medals: 100
                ));
            }

            // Area 1
            list.Add(new AchievementData(
                achievementID: "gem1-1",
                categoryID: 1,
                icon: "achievements/Xaphan/Gem1",
                flag: "XaphanHelper_StatFlag_GemCh1",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_GemCh1") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "map1-0s",
                categoryID: 1,
                icon: "achievements/Xaphan/MapBronze",
                flag: "XaphanHelper_StatFlag_MapCh1-0-Visited",
                currentValue: StatsFlags.CurrentSubAreaTiles[1][0] > 0 ? 1 : 0,
                maxValue: 1,
                medals: 5
            ));
            list.Add(new AchievementData(
                achievementID: "map1-1s",
                categoryID: 1,
                icon: "achievements/Xaphan/MapBronze",
                flag: "XaphanHelper_StatFlag_MapCh1-1-Visited",
                currentValue: StatsFlags.CurrentSubAreaTiles[1][1] > 0 ? 1 : 0,
                maxValue: 1,
                medals: 5
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "map1-2s",
                    categoryID: 1,
                    icon: "achievements/Xaphan/MapBronze",
                    flag: "XaphanHelper_StatFlag_MapCh1-2-Visited",
                    currentValue: StatsFlags.CurrentSubAreaTiles[1][2] > 0 ? 1 : 0,
                    maxValue: 1,
                    medals: 5
                ));
            }

            list.Add(new AchievementData(
                achievementID: "map1-0",
                categoryID: 1,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh1-0",
                currentValue: StatsFlags.CurrentSubAreaTiles[1][0],
                maxValue: StatsFlags.TotalSubAreaTiles[1][0],
                medals: 10,
                reqID: "map1-0s"
            ));
            list.Add(new AchievementData(
                achievementID: "map1-1",
                categoryID: 1,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh1-1",
                currentValue: StatsFlags.CurrentSubAreaTiles[1][1],
                maxValue: StatsFlags.TotalSubAreaTiles[1][1],
                medals: 10,
                reqID: "map1-1s"
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "map1-2",
                    categoryID: 1,
                    icon: "achievements/Xaphan/MapCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_MapCh1-2",
                    currentValue: StatsFlags.CurrentSubAreaTiles[1][2],
                    maxValue: StatsFlags.TotalSubAreaTiles[1][2],
                    medals: 10,
                    reqID: "map1-2s"
                ));
            }

            list.Add(new AchievementData(
                achievementID: "strwb1-0",
                categoryID: 1,
                icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_StrawberriesCh1-0",
                currentValue: StatsFlags.CurrentSubAreaStrawberries[1][0],
                maxValue: StatsFlags.TotalSubAreaStrawberries[1][0],
                medals: 10,
                reqID: "map1-0s"
            ));
            list.Add(new AchievementData(
                achievementID: "strwb1-1",
                categoryID: 1,
                icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_StrawberriesCh1-1",
                currentValue: StatsFlags.CurrentSubAreaStrawberries[1][1],
                maxValue: StatsFlags.TotalSubAreaStrawberries[1][1],
                medals: 10,
                reqID: "map1-1s"
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "strwb1-2",
                    categoryID: 1,
                    icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_StrawberriesCh1-2",
                    currentValue: StatsFlags.CurrentSubAreaStrawberries[1][2],
                    maxValue: StatsFlags.TotalSubAreaStrawberries[1][2],
                    medals: 10,
                    reqID: "map1-2s"
                ));
            }

            list.Add(new AchievementData(
                achievementID: "map1",
                categoryID: 1,
                icon: "achievements/Xaphan/MapCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_MapCh1",
                currentValue: StatsFlags.CurrentTiles[1],
                maxValue: StatsFlags.TotalTiles[1],
                medals: 15
            ));
            list.Add(new AchievementData(
                achievementID: "strwb1",
                categoryID: 1,
                icon: "achievements/Xaphan/StrawberryCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_StrawberriesCh1",
                currentValue: StatsFlags.CurrentStrawberries[1] - (session.GetFlag("XaphanHelper_StatFlag_GoldenCh1-1") ? 1 : 0),
                maxValue: StatsFlags.TotalStrawberries[1],
                medals: 15
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "tank1",
                    categoryID: 1,
                    icon: "achievements/Xaphan/EnergyTankCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_EnergyTanksCh1",
                    currentValue: StatsFlags.CurrentEnergyTanks[1],
                    maxValue: StatsFlags.TotalEnergyTanks[1],
                    medals: 15,
                    reqID: "upg2"
                ));
            }


            list.Add(new AchievementData(
                achievementID: "cass1",
                categoryID: 1,
                icon: "achievements/Xaphan/CassetteSilver",
                flag: "XaphanHelper_StatFlag_CassetteCh1",
                currentValue: StatsFlags.Cassettes[1] ? 1 : 0,
                maxValue: 1,
                medals: 20
            ));
            list.Add(new AchievementData(
                achievementID: "heart1",
                categoryID: 1,
                icon: "achievements/Xaphan/HeartSilver",
                flag: "XaphanHelper_StatFlag_HeartCh1",
                currentValue: StatsFlags.ASideHearts[1] ? 1 : 0,
                maxValue: 1,
                medals: 20
            ));
            list.Add(new AchievementData(
                achievementID: "bside1",
                categoryID: 1,
                icon: "achievements/Xaphan/BSide",
                flag: "XaphanHelper_StatFlag_BSideCh1",
                currentValue: StatsFlags.BSideHearts[1] ? 1 : 0,
                maxValue: 1,
                medals: 25,
                reqID: "cass1"
            ));

            if (XaphanModule.SoCMVersion < new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "golden1",
                    categoryID: 1,
                    icon: "achievements/Xaphan/Golden",
                    flag: "XaphanHelper_StatFlag_GoldenCh1-0",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_GoldenCh1-0") ? 1 : 0,
                    maxValue: 1,
                    medals: 50
                ));
            }

            list.Add(new AchievementData(
                achievementID: "golden1-b",
                categoryID: 1,
                icon: "achievements/Xaphan/Golden",
                flag: "XaphanHelper_StatFlag_GoldenCh1-1",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_GoldenCh1-1") ? 1 : 0,
                maxValue: 1,
                medals: 50,
                reqID: "bside1"
            ));

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "boss1-1",
                    categoryID: 1,
                    icon: "achievements/Xaphan/Boss",
                    flag: "XaphanHelper_StatFlag_BossCh1",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCh1") ? 1 : 0,
                    maxValue: 1,
                    medals: 25
                ));
                list.Add(new AchievementData(
                    achievementID: "boss1-1cm",
                    categoryID: 1,
                    icon: "achievements/Xaphan/BossCM",
                    flag: "XaphanHelper_StatFlag_BossCMCh1",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCMCh1") ? 1 : 0,
                    maxValue: 1,
                    medals: 50,
                    reqID: "boss1-1"
                ));
            }

            // Area 2
            list.Add(new AchievementData(
                achievementID: "gem2-1",
                categoryID: 2,
                icon: "achievements/Xaphan/Gem2",
                flag: "XaphanHelper_StatFlag_GemCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_GemCh2") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "lock2-0",
                categoryID: 2,
                icon: "achievements/Xaphan/LockTemple",
                flag: "XaphanHelper_StatFlag_TempleCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_TempleCh2") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "lock2-1",
                categoryID: 2,
                icon: "achievements/Xaphan/LockRed",
                flag: "XaphanHelper_StatFlag_LockRedCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_LockRedCh2") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "lock2-2",
                categoryID: 2,
                icon: "achievements/Xaphan/LockGreen",
                flag: "XaphanHelper_StatFlag_LockGreenCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_LockGreenCh2") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "lock2-3",
                categoryID: 2,
                icon: "achievements/Xaphan/LockYellow",
                flag: "XaphanHelper_StatFlag_LockYellowCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_LockYellowCh2") ? 1 : 0,
                maxValue: 1,
                medals: 5,
                hidden: true
            ));
            list.Add(new AchievementData(
                achievementID: "map2-0s",
                categoryID: 2,
                icon: "achievements/Xaphan/MapBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-0-Visited",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][0] > 0 ? 1 : 0,
                maxValue: 1,
                medals: 5
            ));
            list.Add(new AchievementData(
                achievementID: "map2-1s",
                categoryID: 2,
                icon: "achievements/Xaphan/MapBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-1-Visited",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][1] > 0 ? 1 : 0,
                maxValue: 1,
                medals: 5
            ));
            list.Add(new AchievementData(
                achievementID: "map2-2s",
                categoryID: 2,
                icon: "achievements/Xaphan/MapBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-2-Visited",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][2] > 0 ? 1 : 0,
                maxValue: 1,
                medals: 5
            ));
            list.Add(new AchievementData(
                achievementID: "map2-0",
                categoryID: 2,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-0",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][0],
                maxValue: StatsFlags.TotalSubAreaTiles[2][0],
                medals: 10,
                reqID: "map2-0s"
            ));
            list.Add(new AchievementData(
                achievementID: "map2-1",
                categoryID: 2,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-1",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][1],
                maxValue: StatsFlags.TotalSubAreaTiles[2][1],
                medals: 10,
                reqID: "map2-1s"
            ));
            list.Add(new AchievementData(
                achievementID: "map2-2",
                categoryID: 2,
                icon: "achievements/Xaphan/MapCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_MapCh2-2",
                currentValue: StatsFlags.CurrentSubAreaTiles[2][2],
                maxValue: StatsFlags.TotalSubAreaTiles[2][2],
                medals: 10,
                reqID: "map2-2s"
            ));
            list.Add(new AchievementData(
                achievementID: "strwb2-0",
                categoryID: 2,
                icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_StrawberriesCh2-0",
                currentValue: StatsFlags.CurrentSubAreaStrawberries[2][0],
                maxValue: StatsFlags.TotalSubAreaStrawberries[2][0],
                medals: 10,
                reqID: "map2-0s"
            ));
            list.Add(new AchievementData(
                achievementID: "strwb2-1",
                categoryID: 2,
                icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_StrawberriesCh2-1",
                currentValue: StatsFlags.CurrentSubAreaStrawberries[2][1],
                maxValue: StatsFlags.TotalSubAreaStrawberries[2][1],
                medals: 10,
                reqID: "map2-1s"
            ));
            list.Add(new AchievementData(
                achievementID: "strwb2-2",
                categoryID: 2,
                icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                flag: "XaphanHelper_StatFlag_StrawberriesCh2-2",
                currentValue: StatsFlags.CurrentSubAreaStrawberries[2][2],
                maxValue: StatsFlags.TotalSubAreaStrawberries[2][2],
                medals: 10,
                reqID: "map2-2s"
            ));
            list.Add(new AchievementData(
                achievementID: "map2",
                categoryID: 2,
                icon: "achievements/Xaphan/MapCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_MapCh2",
                currentValue: StatsFlags.CurrentTiles[2],
                maxValue: StatsFlags.TotalTiles[2],
                medals: 15
            ));
            list.Add(new AchievementData(
                achievementID: "strwb2",
                categoryID: 2,
                icon: "achievements/Xaphan/StrawberryCheckmarkSilver",
                flag: "XaphanHelper_StatFlag_StrawberriesCh2",
                currentValue: StatsFlags.CurrentStrawberries[2] - (session.GetFlag("XaphanHelper_StatFlag_GoldenCh2-1") ? 1 : 0),
                maxValue: StatsFlags.TotalStrawberries[2],
                medals: 15
            ));
            list.Add(new AchievementData(
                achievementID: "cass2",
                categoryID: 2,
                icon: "achievements/Xaphan/CassetteSilver",
                flag: "XaphanHelper_StatFlag_CassetteCh2",
                currentValue: StatsFlags.Cassettes[2] ? 1 : 0,
                maxValue: 1,
                medals: 20
            ));
            list.Add(new AchievementData(
                achievementID: "heart2",
                categoryID: 2,
                icon: "achievements/Xaphan/HeartSilver",
                flag: "XaphanHelper_StatFlag_HeartCh2",
                currentValue: StatsFlags.ASideHearts[2] ? 1 : 0,
                maxValue: 1,
                medals: 20
            ));
            list.Add(new AchievementData(
                achievementID: "boss2-1",
                categoryID: 2,
                icon: "achievements/Xaphan/Boss",
                flag: "XaphanHelper_StatFlag_BossCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCh2") ? 1 : 0,
                maxValue: 1,
                medals: 25
            ));
            list.Add(new AchievementData(
                achievementID: "boss2-1cm",
                categoryID: 2,
                icon: "achievements/Xaphan/BossCM",
                flag: "XaphanHelper_StatFlag_BossCMCh2",
                currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCMCh2") ? 1 : 0,
                maxValue: 1,
                medals: 50,
                reqID: "boss2-1"
            ));

            if (XaphanModule.SoCMVersion < new Version(3, 0, 0))
            {
                list.Add(new AchievementData(
                    achievementID: "golden2",
                    categoryID: 2,
                    icon: "achievements/Xaphan/Golden",
                    flag: "XaphanHelper_StatFlag_GoldenCh2-0",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_GoldenCh2-0") ? 1 : 0,
                    maxValue: 1,
                    medals: 50
                ));
            }

            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                // Area 3

                // Area 4
                list.Add(new AchievementData(
                    achievementID: "map4",
                    categoryID: 4,
                    icon: "achievements/Xaphan/MapCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_MapCh4",
                    currentValue: StatsFlags.CurrentTiles[4],
                    maxValue: StatsFlags.TotalTiles[4],
                    medals: 15
                ));

                // Area 5
                list.Add(new AchievementData(
                    achievementID: "gem5-1",
                    categoryID: 5,
                    icon: "achievements/Xaphan/Gem5",
                    flag: "XaphanHelper_StatFlag_GemCh5",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_GemCh5") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "escp5",
                    categoryID: 5,
                    icon: "achievements/Xaphan/Escape",
                    flag: "Ch4_Escape_Complete",
                    currentValue: XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Escape_Complete") ? 1 : 0,
                    maxValue: 1,
                    medals: 5,
                    hidden: true
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-0s",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-0-Visited",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][0] > 0 ? 1 : 0,
                    maxValue: 1,
                    medals: 5
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-1s",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-1-Visited",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][1] > 0 ? 1 : 0,
                    maxValue: 1,
                    medals: 5
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-2s",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-2-Visited",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][2] > 0 ? 1 : 0,
                    maxValue: 1,
                    medals: 5
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-3s",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-3-Visited",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][3] > 0 ? 1 : 0,
                    maxValue: 1,
                    medals: 5
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-0",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-0",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][0],
                    maxValue: StatsFlags.TotalSubAreaTiles[5][0],
                    medals: 10,
                    reqID: "map5-0s"
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-1",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-1",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][1],
                    maxValue: StatsFlags.TotalSubAreaTiles[5][1],
                    medals: 10,
                    reqID: "map5-1s"
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-2",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-2",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][2],
                    maxValue: StatsFlags.TotalSubAreaTiles[5][2],
                    medals: 10,
                    reqID: "map5-2s"
                ));
                list.Add(new AchievementData(
                    achievementID: "map5-3",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_MapCh5-3",
                    currentValue: StatsFlags.CurrentSubAreaTiles[5][3],
                    maxValue: StatsFlags.TotalSubAreaTiles[5][3],
                    medals: 10,
                    reqID: "map5-3s"
                ));
                list.Add(new AchievementData(
                    achievementID: "strwb5-0",
                    categoryID: 5,
                    icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_StrawberriesCh5-0",
                    currentValue: StatsFlags.CurrentSubAreaStrawberries[5][0],
                    maxValue: StatsFlags.TotalSubAreaStrawberries[5][0],
                    medals: 10,
                    reqID: "map5-0s"
                ));
                list.Add(new AchievementData(
                    achievementID: "strwb5-1",
                    categoryID: 5,
                    icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_StrawberriesCh5-1",
                    currentValue: StatsFlags.CurrentSubAreaStrawberries[5][1],
                    maxValue: StatsFlags.TotalSubAreaStrawberries[5][1],
                    medals: 10,
                    reqID: "map5-1s"
                ));
                list.Add(new AchievementData(
                    achievementID: "strwb5-3",
                    categoryID: 5,
                    icon: "achievements/Xaphan/StrawberryCheckmarkBronze",
                    flag: "XaphanHelper_StatFlag_StrawberriesCh5-3",
                    currentValue: StatsFlags.CurrentSubAreaStrawberries[5][3],
                    maxValue: StatsFlags.TotalSubAreaStrawberries[5][3],
                    medals: 10,
                    reqID: "map5-3s"
                ));
                list.Add(new AchievementData(
                    achievementID: "map5",
                    categoryID: 5,
                    icon: "achievements/Xaphan/MapCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_MapCh5",
                    currentValue: StatsFlags.CurrentTiles[5],
                    maxValue: StatsFlags.TotalTiles[5],
                    medals: 15
                ));
                list.Add(new AchievementData(
                    achievementID: "strwb5",
                    categoryID: 5,
                    icon: "achievements/Xaphan/StrawberryCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_StrawberriesCh5",
                    currentValue: StatsFlags.CurrentStrawberries[5] - (session.GetFlag("XaphanHelper_StatFlag_GoldenCh5-1") ? 1 : 0),
                    maxValue: StatsFlags.TotalStrawberries[5],
                    medals: 15
                ));
                list.Add(new AchievementData(
                    achievementID: "tank5",
                    categoryID: 5,
                    icon: "achievements/Xaphan/EnergyTankCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_EnergyTanksCh5",
                    currentValue: StatsFlags.CurrentEnergyTanks[5],
                    maxValue: StatsFlags.TotalEnergyTanks[5],
                    medals: 15,
                    reqID: "upg2"
                ));
                list.Add(new AchievementData(
                    achievementID: "dfrm5",
                    categoryID: 5,
                    icon: "achievements/Xaphan/FireRateModuleCheckmarkSilver",
                    flag: "XaphanHelper_StatFlag_FireRateModulesCh5",
                    currentValue: StatsFlags.CurrentFireRateModules[5],
                    maxValue: StatsFlags.TotalFireRateModules[5],
                    medals: 15,
                    reqID: "upg7"
                ));
                list.Add(new AchievementData(
                    achievementID: "boss5-1",
                    categoryID: 5,
                    icon: "achievements/Xaphan/Boss",
                    flag: "XaphanHelper_StatFlag_BossCh5",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCh5") ? 1 : 0,
                    maxValue: 1,
                    medals: 25
                ));
                list.Add(new AchievementData(
                achievementID: "boss5-1cm",
                    categoryID: 5,
                    icon: "achievements/Xaphan/BossCM",
                    flag: "XaphanHelper_StatFlag_BossCMCh5",
                    currentValue: session.GetFlag("XaphanHelper_StatFlag_BossCMCh5") ? 1 : 0,
                    maxValue: 1,
                    medals: 50,
                    reqID: "boss5-1"
                ));
            }

            return list;
        }
    }
}
