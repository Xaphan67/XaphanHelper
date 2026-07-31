using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/SoCMCutsceneTrigger")]
    class SoCMCutsceneTrigger : Trigger
    {
        public string Cutscene;

        public bool playerHasCollectedOneGem()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected") ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected") ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected") ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected") ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected") ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Gem_Collected");
        }

        public SoCMCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Cutscene = data.Attr("cutscene");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Cutscene == "Intro" && !player.SceneAs<Level>().InCutscene)
            {
                Scene.Add(new SoCMIntro(player));
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (Cutscene != "Intro")
            {
                Level level = Scene as Level;
                if (!level.InCutscene)
                {
                    switch (Cutscene)
                    {
                        case "Ch0 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Start"))
                            {
                                Scene.Add(new CS00_Start(player));
                            }
                            break;
                        case "Ch0 - Find Cavern":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Find_Cavern"))
                            {
                                Scene.Add(new CS00_FindCavern(player));
                            }
                            break;
                        case "Ch0 - Gem Room A":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_A"))
                            {
                                Scene.Add(new CS00_GemRoomA(player));
                            }
                            break;
                        case "Ch0 - Statue Room":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Statue_Room"))
                            {
                                Scene.Add(new CS00_StatueRoom(player));
                            }
                            break;
                        case "Ch0 - Statue Room 2":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Statue_Room2") && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots"))
                            {
                                Scene.Add(new CS00_StatueRoom2(player));
                            }
                            break;
                        case "Ch0 - Gem Room B":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_B"))
                            {
                                Scene.Add(new CS00_GemRoomB(player));
                            }
                            break;
                        case "Ch0 - Gem Room C":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_C"))
                            {
                                if (playerHasCollectedOneGem())
                                {
                                    bool PlayerHasNotSlotedGems = false;
                                    for (int i = 1; i <= 5; i++)
                                    {
                                        if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Collected") && !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Sloted"))
                                        {
                                            PlayerHasNotSlotedGems = true;
                                            break;
                                        }
                                    }
                                    if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected") && !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Sloted"))
                                    {
                                        PlayerHasNotSlotedGems = true;
                                    }
                                    if (PlayerHasNotSlotedGems)
                                    {
                                        Scene.Add(new CS00_GemRoomC(player));
                                    }
                                }
                            }
                            break;
                        case "Ch0 - Gem Room D":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_D"))
                            {
                                Scene.Add(new CS00_GemRoomD(player));
                            }
                            break;
                        case "Ch0 - Fall":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Fall"))
                            {
                                Scene.Add(new CS00_Fall(player));
                            }
                            break;
                        case "Ch1 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Start"))
                            {
                                Scene.Add(new CS01_Start(player));
                            }
                            break;
                        case "Ch1 - Main Hall":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Main_Hall"))
                            {
                                Scene.Add(new CS01_MainHall(player));
                            }
                            break;
                        case "Ch1 - Checkpoint Statue":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Checkpoint_Statue"))
                            {
                                Scene.Add(new CS01_CheckpointStatue(player));
                            }
                            break;
                        case "Ch1 - Gem":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Gem"))
                            {
                                Scene.Add(new CS01_Gem(player));
                            }
                            break;
                        case "Ch1 - Gem 2":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Gem_2"))
                            {
                                Scene.Add(new CS01_Gem2(player));
                            }
                            break;
                        case "Ch2 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch2_Start"))
                            {
                                Scene.Add(new CS02_Start(player));
                            }
                            break;
                        case "Ch2 - Before Temple":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch2_Before_Temple"))
                            {
                                Scene.Add(new CS02_BeforeTemple(player));
                            }
                            break;
                        case "Ch2 - Temple Door":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch2_TempleDoor") && !level.Session.GetFlag("CS_Ch2_TempleDoor") && !level.Session.GetFlag("Temple_Activated"))
                            {
                                Scene.Add(new CS02_TempleDoor(player));
                            }
                            break;
                        case "Ch2 - Gem":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch2_Gem"))
                            {
                                Scene.Add(new CS02_Gem(player));
                            }
                            break;
                        case "Ch3 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch3_Start"))
                            {
                                Scene.Add(new CS03_Start(player));
                            }
                            break;
                        case "Ch4 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_Start"))
                            {
                                Scene.Add(new CS04_Start(player));
                            }
                            break;
                        case "Ch4 - Gem":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_Gem") && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected"))
                            {
                                Scene.Add(new CS04_Gem(player));
                            }
                            break;
                        case "Ch4 - After Escape":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_After_Escape"))
                            {
                                Scene.Add(new CS04_AfterEscape(player));
                            }
                            break;
                        case "Ch4 - Lava":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_Lava"))
                            {
                                Scene.Add(new CS04_Lava(player));
                            }
                            break;
                        case "Ch4 - Warp":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_Warp"))
                            {
                                Scene.Add(new CS04_Warp(player));
                            }
                            break;
                        case "Ch4 - Water":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_Water"))
                            {
                                Scene.Add(new CS04_Water(player));
                            }
                            break;
                        case "Ch5 - Start":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Start"))
                            {
                                Scene.Add(new CS05_Start(player));
                            }
                            break;
                        case "Ch5 - After Water":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_After_Water"))
                            {
                                Scene.Add(new CS05_AfterWater(player));
                            }
                            break;
                        case "Ch5 - Generator":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator"))
                            {
                                Scene.Add(new CS05_Generator(player));
                            }
                            break;
                        case "Ch5 - Generator 2":
                            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator2") && XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch5_Auxiliary_Power"))
                            {
                                Scene.Add(new CS05_Generator2(player));
                            }
                            break;
                    }
                }
            }
        }
    }
}