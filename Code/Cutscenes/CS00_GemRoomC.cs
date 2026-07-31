using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Triggers;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_GemRoomC : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        private GemController gemController;

        private Coroutine EndRoutine;
        
        public EventInstance shakeSoundSource;

        public CS00_GemRoomC(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            player.StateMachine.State = 11;
            gemController = Scene.Entities.FindFirst<GemController>();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                foreach (GemSlot gem in Scene.Entities.FindAll<GemSlot>())
                {
                    if (!gem.Activated && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + gem.Chapter + "_Gem" + ((gem.Index != 1 ? gem.Index : "")) + "_Collected"))
                    {
                        gem.Activated = true;
                        gem.ActivateNoAnim();
                    }
                }
            }
            if (badeline != null)
                {
                badeline.RemoveSelf();
            }
            if (WasSkipped)
            {
                player.Position.X = gemController.X;
                player.Position.Y = 1056;
            }
            if (gemController.AllGemCollected())
            {
                XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Gem_Room_C");
                level.Session.SetFlag("CS_Ch0_Gem_Room_C");
                if (WasSkipped)
                {
                    bool playerRight = player.BottomCenter.X > gemController.BottomCenter.X;
                    level.Session.Inventory = new PlayerInventory(2);
                    level.Session.SetFlag("Ch-1_All_Gems_Active");
                    XaphanModule.ModSaveData.SavedFlags.Add("All_Gems_Active");
                    XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Double_Dash_Unlocked");
                }
            }
            else
            {
                player.StateMachine.State = 0;
            }
        }

        private IEnumerator startSkipping()
        {
            new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = true;
            yield break;
        }

        private IEnumerator stopSkipping()
        {
            new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = false;
            yield break;
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!gemController.AllGemCollected())
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch0_Gem_Room_First_Time"))
                {
                    player.Facing = Facings.Right;
                    XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch0_Gem_Room_First_Time");
                    if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                    {
                        yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C");
                    }
                }
                yield return player.DummyWalkToExact((int)gemController.X);
                gemController.PlayerPose = "XaphanHelper_turnAround";
                player.Sprite.Play(gemController.PlayerPose);
                player.Sprite.OnLastFrame = delegate
                {
                    gemController.PlayerPose = "XaphanHelper_turnAround_end";
                    player.Sprite.Play(gemController.PlayerPose);
                };
                yield return gemController.ActivateGems();
                if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                {
                    int missingGems = 0;
                    for (int i = 1; i <= (XaphanModule.SoCMVersion >= new Version(3, 0, 0) ? 5 : 4); i++)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Sloted"))
                        {
                            missingGems++;
                        }
                    }
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Sloted"))
                    {
                        missingGems++;
                    }
                    if (missingGems > 0)
                    {
                        yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_b_" + missingGems);
                    }
                }
                gemController.PlayerPose = "XaphanHelper_turnAround_reverse";
                player.Sprite.Play(gemController.PlayerPose);
                player.Sprite.OnLastFrame = delegate
                {
                    gemController.PlayerPose = "";
                };
            }
            else
            {
                foreach (SoCMCutsceneTrigger trigger in level.Tracker.GetEntities<SoCMCutsceneTrigger>())
                {
                    if (trigger.Cutscene == "Ch0 - Gem Room D")
                    {
                        trigger.Position = new Vector2(gemController.Position.X, 1048f) - new Vector2(trigger.Width / 2, trigger.Height / 2);
                        break;
                    }
                }
                bool playerRight = player.BottomCenter.X > gemController.BottomCenter.X;
                player.Facing = playerRight ? Facings.Left : Facings.Right;
                if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                {
                    yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_c_" + (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch0_Gem_Room_First_Time") ? "main" : "alt") + (playerRight ? "_flip" : ""));
                }
                yield return player.DummyWalkToExact((int)gemController.X);
                gemController.PlayerPose = "XaphanHelper_turnAround";
                player.Sprite.Play(gemController.PlayerPose);
                player.Sprite.OnLastFrame = delegate
                {
                    gemController.PlayerPose = "XaphanHelper_turnAround_end";
                    player.Sprite.Play(gemController.PlayerPose);
                };
                yield return gemController.ActivateGems();
                yield return 0.25f;
                gemController.PlayerPose = "XaphanHelper_turnAround_reverse";
                player.Sprite.Play(gemController.PlayerPose);
                player.Sprite.OnLastFrame = delegate
                {
                    gemController.PlayerPose = "";
                };
                Image white = new(GFX.Game["objects/Xaphan/heart_bwhite"]);
                white.CenterOrigin();
                white.Scale = Vector2.Zero;
                white.Color = Color.White * 0.7f;
                gemController.Add(white);
                BloomPoint glow = new(0f, 16f);
                gemController.Add(glow);
                List<Entity> absorbs = new();
                Audio.Play("event:/game/06_reflection/supersecret_heartappear");
                foreach (GemSlot slot in level.Tracker.GetEntities<GemSlot>())
                {
                    slot.ReleaseOrbs(gemController);
                }
                float timer = 2f;
                while (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                    yield return null;
                    if (timer <= 1f)
                    {
                        foreach (Component comp in gemController.Components)
                        {
                            if (comp.GetType() == typeof(Image))
                            {
                                Image image = (Image)comp;
                                image.Scale.X = 1 - timer;
                                image.Scale.Y = 1 - timer;
                            }
                        }
                    }
                }
                foreach (GemSlot slot in level.Tracker.GetEntities<GemSlot>())
                {
                    absorbs.AddRange(slot.absorbs);
                }
                foreach (Entity orb in absorbs)
                {
                    orb.RemoveSelf();
                }
                (Scene as Level).Flash(Color.White);
                gemController.Remove(white);
                gemController.Remove(glow);
                level.Session.SetFlag("Ch-1_All_Gems_Active");
                XaphanModule.ModSaveData.SavedFlags.Add("All_Gems_Active");
                if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                {
                    yield return 0.4f;
                    badeline = CutscenesHelper.BadelineSplit(Level, player);
                    yield return CutscenesHelper.BadelineFloat(this, -30 * (playerRight ? 1 : -1), -18, badeline, playerRight ? 1 : -1, false, false, true);
                    yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_d" + (playerRight ? "_flip" : ""));
                    yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
                }
                level.Session.Inventory = new PlayerInventory(2);
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Double_Dash_Unlocked");
                if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                {
                    yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_e" + (playerRight ? "_flip" : ""), startSkipping, stopSkipping);
                }
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/none");
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                XaphanModule.IgnoreShakeSettings = true;
                shakeSoundSource = Audio.Play("event:/game/xaphan/liquid_rise");
                level.DirectionalShake(new Vector2(0.5f, 0), 1f);
                Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
                yield return 0.1f;
                shakeSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
                if (!XaphanModule.ModSettings.AutoSkipCutscenes)
                {
                    yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_f" + (playerRight ? "_flip" : ""), startSkipping, stopSkipping);
                }
                shakeSoundSource = Audio.Play("event:/game/xaphan/liquid_rise");
                level.DirectionalShake(new Vector2(0.5f, 0), 2f);
                Input.Rumble(RumbleStrength.Light, RumbleLength.TwoSeconds);
                yield return 0.7f;
                yield return gemController.OpenEndArea();
                Add(EndRoutine = new Coroutine(LastDialog(playerRight)));
                yield return 0.1f;
                shakeSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
                XaphanModule.IgnoreShakeSettings = false;
            }
            if (EndRoutine != null)
            {
                while(EndRoutine.Active)
                {
                    yield return null;
                }
            }
            EndCutscene(Level);
        }

        public IEnumerator LastDialog(bool playerRight)
        {
            if (!XaphanModule.ModSettings.AutoSkipCutscenes)
            {
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_g" + (playerRight ? "_flip" : ""));
            }
            yield return 1f;
            Scene.Add(new TeleportCutscene(player, "A-15", new Vector2(0, 0), 0, 0, true, 0, "Fade", 1.35f));
        }
    }
}
