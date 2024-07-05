using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/SoCMCutsceneTrigger")]
    class SoCMCutsceneTrigger : Trigger
    {
        private bool triggered;

        public string Cutscene;

        public bool playerHasCollectedOneGem()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) ||
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public SoCMCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Cutscene = data.Attr("cutscene");
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (triggered)
            {
                return;
            }
            triggered = true;
            Session session = SceneAs<Level>().Session;
            Level level = Scene as Level;
            switch (Cutscene)
            {
                case "Intro":
                    Scene.Add(new SoCMIntro(player));
                    break;
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
                                if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) && !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Sloted" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
                                {
                                    PlayerHasNotSlotedGems = true;
                                    break;
                                }
                            }
                            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) && !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Sloted" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
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
            }
        }
    }
}