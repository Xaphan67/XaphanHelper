using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.XaphanHelper
{
    [SettingName("XaphanModuleSettings")]
    public class XaphanModuleSettings : EverestModuleSettings
    {
        // Mods Options Settings

        [SettingName("ModOptions_XaphanModule_ShowMiniMap")]
        [SettingSubText("ModOptions_XaphanModule_ShowMiniMap_Desc")]
        public bool ShowMiniMap { get; set; } = true;

        public int MiniMapOpacity { get; set; } = 10;

        public void CreateMiniMapOpacityEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_MiniMapOpacity"), (int i) => i switch
            {
                1 => Dialog.Clean("ModOptions_XaphanModule_10"),
                2 => Dialog.Clean("ModOptions_XaphanModule_20"),
                3 => Dialog.Clean("ModOptions_XaphanModule_30"),
                4 => Dialog.Clean("ModOptions_XaphanModule_40"),
                5 => Dialog.Clean("ModOptions_XaphanModule_50"),
                6 => Dialog.Clean("ModOptions_XaphanModule_60"),
                7 => Dialog.Clean("ModOptions_XaphanModule_70"),
                8 => Dialog.Clean("ModOptions_XaphanModule_80"),
                9 => Dialog.Clean("ModOptions_XaphanModule_90"),
                _ => Dialog.Clean("ModOptions_XaphanModule_100"),
            }, 1, 10, MiniMapOpacity).Change(delegate (int i)
            {
                MiniMapOpacity = i;
            }));
        }

        public int SpaceJumpIndicator { get; set; } = 2;

        public void CreateSpaceJumpIndicatorEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
                1 => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_Small"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_Large"),
            }, 0, 2, SpaceJumpIndicator).Change(delegate (int i)
            {
                SpaceJumpIndicator = i;
            }));
        }

        public int StaminaIndicator { get; set; } = 0;

        public void CreateStaminaIndicatorEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_UI_Only"),
                1 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Player_Only"),
                2 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Both"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
            }, 0, 3, StaminaIndicator).Change(delegate (int i)
            {
                StaminaIndicator = i;
            }));
        }

        public int OxygenIndicator { get; set; } = 0;

        public void CreateOxygenIndicatorEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Slider(Dialog.Clean("ModOptions_XaphanModule_OxygenIndicator"), (int i) => i switch
            {
                0 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_UI_Only"),
                1 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Player_Only"),
                2 => Dialog.Clean("ModOptions_XaphanModule_StaminaIndicator_Both"),
                _ => Dialog.Clean("ModOptions_XaphanModule_SpaceJumpIndicator_None"),
            }, 0, 3, OxygenIndicator).Change(delegate (int i)
            {
                OxygenIndicator = i;
            }));
        }

        [SettingName("ModOptions_XaphanModule_ShowCompleteSlopesHitboxes")]
        [SettingSubText("ModOptions_XaphanModule_ShowCompleteSlopesHitboxes_Desc")]
        public static bool ShowCompleteSlopesHitboxes { get; set; } = false;


        // Bindings

        [DefaultButtonBinding(Buttons.Back, Keys.Tab)]
        public ButtonBinding OpenMap { get; set; }

        [DefaultButtonBinding(Buttons.Y, Keys.A)]
        public ButtonBinding SelectItem { get; set; }

        [DefaultButtonBinding(Buttons.LeftShoulder, Keys.S)]
        public ButtonBinding UseBagItemSlot { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.D)]
        public ButtonBinding UseMiscItemSlot { get; set; }

        [DefaultButtonBinding(Buttons.Y, Keys.A)]
        public ButtonBinding MapScreenShowMapOrWorldMap { get; set; }

        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.Z)]
        public ButtonBinding MapScreenDisplayOptions { get; set; }

        [DefaultButtonBinding(Buttons.RightTrigger, Keys.C)]
        public ButtonBinding MapScreenPlaceMarker { get; set; }        

        // Celeste Upgrades

        [SettingIgnore]
        public bool PowerGrip { get; set; } = false;

        [SettingIgnore]
        public bool ClimbingKit { get; set; } = false;

        [SettingIgnore]
        public bool SpiderMagnet { get; set; } = false;

        [SettingIgnore]
        public bool DroneTeleport { get; set; } = false;

        [SettingIgnore]
        public bool JumpBoost { get; set; } = false;

        [SettingIgnore]
        public bool Bombs { get; set; } = false;

        [SettingIgnore]
        public bool MegaBombs { get; set; } = false;

        [SettingIgnore]
        public bool RemoteDrone { get; set; } = false;

        [SettingIgnore]
        public bool GoldenFeather { get; set; } = false;

        [SettingIgnore]
        public bool Binoculars { get; set; } = false;

        [SettingIgnore]
        public bool EtherealDash { get; set; } = false;

        [SettingIgnore]
        public bool PortableStation { get; set; } = false;

        [SettingIgnore]
        public bool PulseRadar { get; set; } = false;

        [SettingIgnore]
        public bool DashBoots { get; set; } = false;

        [SettingIgnore]
        public bool HoverJet { get; set; } = false;

        [SettingIgnore]
        public bool LightningDash { get; set; } = false;

        [SettingIgnore]
        public bool MissilesModule { get; set; } = false;

        [SettingIgnore]
        public bool SuperMissilesModule { get; set; } = false;

        // Metroid Upgrades

        [SettingIgnore]
        public bool Spazer { get; set; } = false;

        [SettingIgnore]
        public bool PlasmaBeam { get; set; } = false;

        [SettingIgnore]
        public bool MorphingBall { get; set; } = false;

        [SettingIgnore]
        public bool MorphBombs { get; set; } = false;

        [SettingIgnore]
        public bool SpringBall { get; set; } = false;

        [SettingIgnore]
        public bool HighJumpBoots { get; set; } = false;

        [SettingIgnore]
        public bool SpeedBooster { get; set; } = false;

        // Common Upgrades

        [SettingIgnore]
        public bool LongBeam { get; set; } = false;

        [SettingIgnore]
        public bool IceBeam { get; set; } = false;

        [SettingIgnore]
        public bool WaveBeam { get; set; } = false;

        [SettingIgnore]
        public bool VariaJacket { get; set; } = false;

        [SettingIgnore]
        public bool GravityJacket { get; set; } = false;

        [SettingIgnore]
        public bool ScrewAttack { get; set; } = false;

        [SettingIgnore]
        public int SpaceJump { get; set; } = 1;

        // SoCM only settings

        [SettingIgnore]
        public bool SoCMShowMiniMap { get; set; } = true;

        [SettingIgnore]
        public int SoCMMiniMapOpacity { get; set; } = 10;

        [SettingIgnore]
        public int SoCMSpaceJumpIndicator { get; set; } = 2;

        [SettingIgnore]
        public int SoCMStaminaIndicator { get; set; } = 0;

        [SettingIgnore]
        public int SoCMOxygenIndicator { get; set; } = 0;

        [SettingIgnore]
        public bool ShowAchievementsPopups { get; set; } = true;

        [SettingIgnore]
        public bool ShowLorebookPopups { get; set; } = true;

        [SettingIgnore]
        public bool AutoSkipCutscenes { get; set; } = false;

        [SettingIgnore]
        public bool WatchedCredits { get; set; } = false;

        [SettingIgnore]
        public bool AllowDebug { get; set; } = false;
    }
}
