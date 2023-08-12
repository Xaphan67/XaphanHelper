using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper
{
    public class XaphanModuleSaveData : EverestModuleSaveData
    {
        // Flags

        public HashSet<string> SavedFlags = new();

        public HashSet<string> GlobalFlags = new();

        public HashSet<string> WatchedCutscenes = new();

        // In-Game Map

        public List<string> VisitedChapters = new();

        public HashSet<string> VisitedRooms = new();

        public HashSet<string> VisitedRoomsTiles = new();

        public HashSet<string> ExtraUnexploredRooms = new();

        public Dictionary<string, bool> ShowHints = new();

        public Dictionary<string, int> ProgressMode = new();

        public Dictionary<string, int> WorldMapProgressMode = new();

        // Warps

        public HashSet<string> UnlockedWarps = new();

        public HashSet<string> SpeedrunModeUnlockedWarps = new();

        // Celeste Upgrades

        public HashSet<string> PowerGripInactive = new();

        public HashSet<string> ClimbingKitInactive = new();

        public HashSet<string> SpiderMagnetInactive = new();

        public HashSet<string> LightningDashInactive = new();

        public HashSet<string> DroneTeleportInactive = new();

        public HashSet<string> JumpBoostInactive = new();

        public HashSet<string> BombsInactive = new();

        public HashSet<string> MegaBombsInactive = new();

        public HashSet<string> RemoteDroneInactive = new();

        public HashSet<string> GoldenFeatherInactive = new();

        public HashSet<string> BinocularsInactive = new();

        public HashSet<string> EtherealDashInactive = new();

        public HashSet<string> PortableStationInactive = new();

        public HashSet<string> PulseRadarInactive = new();

        public HashSet<string> DashBootsInactive = new();

        public HashSet<string> HoverJetInactive = new();

        public HashSet<string> MissilesModuleInactive = new();

        public HashSet<string> SuperMissilesModuleInactive = new();

        // Metroid Upgrades

        public HashSet<string> SpazerInactive = new();

        public HashSet<string> PlasmaBeamInactive = new();

        public HashSet<string> MorphingBallInactive = new();

        public HashSet<string> MorphBombsInactive = new();

        public HashSet<string> SpringBallInactive = new();

        public HashSet<string> HighJumpBootsInactive = new();

        public HashSet<string> SpeedBoosterInactive = new();

        // Common Upgrades

        public HashSet<string> LongBeamInactive = new();

        public HashSet<string> IceBeamInactive = new();

        public HashSet<string> WaveBeamInactive = new();

        public HashSet<string> VariaJacketInactive = new();

        public HashSet<string> GravityJacketInactive = new();

        public HashSet<string> ScrewAttackInactive = new();

        public HashSet<string> SpaceJumpInactive = new();

        // Other Upgrades

        public HashSet<string> StaminaUpgrades = new();

        public HashSet<string> SpeedrunModeStaminaUpgrades = new();

        public HashSet<string> DroneMissilesUpgrades = new();

        public HashSet<string> SpeedrunModeDroneMissilesUpgrades = new();

        public HashSet<string> DroneSuperMissilesUpgrades = new();

        public HashSet<string> SpeedrunModeDroneSuperMissilesUpgrades = new();

        public HashSet<string> DroneFireRateUpgrades = new();

        public HashSet<string> SpeedrunModeDroneFireRateUpgrades = new();

        // Countdown

        public long CountdownCurrentTime = -1;

        public bool CountdownShake = false;

        public bool CountdownExplode = false;

        public string CountdownActiveFlag = "";

        public int CountdownStartChapter = -1;

        public string CountdownStartRoom = "";

        public Vector2 CountdownSpawn = new();

        public bool CountdownIntroType = false;

        public bool CountdownUseLevelWipe = false;

        public bool CountdownNotVisible = false;

        public string CountdownEventsFlags = "";

        // Teleport variables

        public bool TeleportFromElevator = false;

        public string DestinationRoom = "";

        public Vector2 Spawn = new();

        public string Wipe = "";

        public float WipeDuration = 1.35f;

        public bool ConsiderBeginning = false;

        public Dictionary<string, int> SavedChapter = new();

        public Dictionary<string, string> SavedRoom = new();

        public Dictionary<string, Vector2> SavedSpawn = new();

        public Dictionary<string, float> SavedLightingAlphaAdd = new();

        public Dictionary<string, float> SavedBloomBaseAdd = new();

        public Dictionary<string, Session.CoreModes> SavedCoreMode = new();

        public Dictionary<string, string> SavedMusic = new();

        public Dictionary<string, string> SavedAmbience = new();

        public Dictionary<string, HashSet<EntityID>> SavedNoLoadEntities = new();

        public Dictionary<string, long> SavedTime = new();

        public Dictionary<string, bool> SavedFromBeginning = new();

        public Dictionary<string, string> SavedSesionFlags = new();

        public Dictionary<string, HashSet<EntityID>> SavedSessionStrawberries = new();

        // Metroid variables

        public Dictionary<string, int> MaxHealth = new();

        public Dictionary<string, int> MaxMissiles = new();

        public Dictionary<string, int> MaxSuperMissiles = new();

        public Dictionary<string, int> MaxPowerBombs = new();

        public HashSet<string> DoorsOpened = new();

        public HashSet<string> AmmoCollected = new();

        public bool IgnoreSavedChapter = false;

        public bool IgnoreSavedRoom = false;

        // Sides Unlock variables

        public HashSet<string> CSideUnlocked = new();

        public int LastPlayedSide = 0;

        // Checkpoints (Merged chapters)

        public HashSet<string> Checkpoints = new();

        // Achievements

        public bool CanDisplayAchievementsPopups = false;

        public HashSet<string> Achievements = new();

        // Drone

        public Dictionary<string, bool> startAsDrone = new();

        public Dictionary<string, int> droneStartChapter = new();

        public Dictionary<string, string> droneStartRoom = new();

        public Dictionary<string, Vector2?> droneStartSpawn = new();

        public Dictionary<string, Vector2?> droneCurrentSpawn = new();

        public Dictionary<string, Facings> fakePlayerFacing = new();

        public Dictionary<string, Vector2> fakePlayerPosition = new();

        // Others

        public string CurrentSubArea = "";

        public bool SpeedrunMode = false;

        public int BagUIId1 = 0;

        public int BagUIId2 = 0;

        public bool LoadedPlayer = false;

        public int SoCMVer = 0;
    }
}