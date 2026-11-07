using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Triggers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E05_EscapeStart : CutsceneEntity
    {
        private Player player;

        private bool playerHasMoved;

        private Point ReactorCenter;

        public EventInstance alarmSfx;

        private FieldInfo MiniTextboxRoutine = typeof(MiniTextbox).GetField("routine", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo MiniTextboxClosing = typeof(MiniTextbox).GetField("closing", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo MiniTextboxEase = typeof(MiniTextbox).GetField("ease", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo MiniTextboxText = typeof(MiniTextbox).GetField("text", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo MiniTextboxIndex = typeof(MiniTextbox).GetField("index", BindingFlags.Instance | BindingFlags.NonPublic);

        public E05_EscapeStart(Player player, Level level)
        {
            Tag = Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!level.Session.GetFlag("Ch4_Escape_Complete"))
            {
                if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Gem"))
                {
                    Scene.Add(new CS05_Gem(player));
                    yield return 0.1f;
                    while (player.StateMachine.State != 0)
                    {
                        yield return null;
                    }
                }

                level.InCutscene = false;
                level.CancelCutscene();
                while (!level.Session.GetFlag("reactor_glass_broken") || !playerHasMoved)
                {
                    if (player != null && player.Speed != Vector2.Zero)
                    {
                        playerHasMoved = true;
                    }
                    if (!level.Session.GetFlag("Lab_Escape"))
                    {
                        level.Session.SetFlag("Lab_Escape_Music", false);
                    }
                    yield return null;
                }
                if (!level.Session.GetFlag("Lab_Escape"))
                {
                    level.Session.SetFlag("XaphanHelper_Prevent_Drone", true);
                    List<EntityID> IDs = new();
                    List<EntityID> IDsToRemove = new();
                    IDs.Add(new EntityID("W-32", 2611));
                    IDs.Add(new EntityID("W-32", 2610));
                    IDs.Add(new EntityID("W-32", 2612));
                    IDs.Add(new EntityID("W-32", 2538));
                    IDs.Add(new EntityID("W-32", 2511));
                    IDs.Add(new EntityID("W-34", 95));
                    IDs.Add(new EntityID("W-37", 4787));
                    IDs.Add(new EntityID("W-38", 4977));
                    IDs.Add(new EntityID("W-38", 4983));
                    foreach (EntityID entity in level.Session.DoNotLoad)
                    {
                        foreach (EntityID id in IDs)
                        {
                            if (entity.Level == id.Level && entity.ID == id.ID)
                            {
                                IDsToRemove.Add(entity);
                            }
                        }
                    }
                    foreach (EntityID id in IDsToRemove)
                    {
                        level.Session.DoNotLoad.Remove(id);
                    }
                    alarmSfx = Audio.Play("event:/game/xaphan/alarm");
                    level.Session.SetFlag("Lab_Escape", true);
                    ReactorCenter = level.Bounds.Center;
                    if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_EscapeStart"))
                    {
                        MiniTextbox textBox = null;
                        level.Add(textBox = new MiniTextbox("Xaphan_Ch5_A_Gem_c"));
                        yield return CloseTextbox(textBox, 0.5f);
                        level.Add(textBox = new MiniTextbox("Xaphan_Ch5_A_Gem_d"));
                        yield return CloseTextbox(textBox, 1f);
                        XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_EscapeStart");
                        level.Session.SetFlag("CS_Ch5_EscapeStart");
                    }
                    StartCountdownTrigger trigger = level.Tracker.GetEntity<StartCountdownTrigger>();
                    Vector2 triggerStartPosition = trigger.Position;
                    trigger.Position = player.Position - new Vector2(trigger.Width / 2, trigger.Height / 2);
                    trigger.ChangeSpawnPosition(new Vector2(92f, 152f));
                    level.Session.RespawnPoint = level.Session.GetSpawnPoint(trigger.Center);
                    yield return 0.01f;
                    XaphanModule.ModSaveData.SavedSpawn[level.Session.Area.LevelSet] = (Vector2)level.Session.RespawnPoint - new Vector2(level.Bounds.Left, level.Bounds.Top);
                    trigger.Position = triggerStartPosition;
                    float timer = 2f;
                    bool countdownStarted = false;
                    while (timer > 0 && !countdownStarted)
                    {
                        timer -= Engine.DeltaTime;
                        yield return null;
                        if (level.Tracker.GetEntity<CountdownDisplay>() != null)
                        {
                            CountdownDisplay countdownDisplay = level.Tracker.GetEntity<CountdownDisplay>();
                            countdownDisplay.Immediate = true;
                            countdownStarted = true;
                        }
                    }
                    level.Session.SetFlag("Lab_Escape_Music", true);
                    level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_escape");
                    level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                    CountdownDisplay display = null;
                    bool lastSecond = false;
                    while (true)
                    {
                        if (Scene != null)
                        {
                            if (Scene.Tracker.GetEntities<CountdownDisplay>().Count == 1)
                            {
                                if (display == null)
                                {
                                    display = Scene.Tracker.GetEntity<CountdownDisplay>();
                                }
                                else if (display.GetRemainingTime() <= 2000000 && !lastSecond)
                                {
                                    lastSecond = true;
                                    TriggerExplosion();
                                }
                                else if (display.TimerRanOut)
                                {
                                    alarmSfx.stop(STOP_MODE.IMMEDIATE);
                                    break;
                                }
                            }
                        }
                        yield return null;
                    }
                }
            }
            else
            {
                level.InCutscene = false;
                level.CancelCutscene();
            }
        }

        private void TriggerExplosion()
        {
            Level CurrentLevel = SceneAs<Level>();
            Vector2 ExplosionCenter = new(CurrentLevel.Bounds.Center.X + CurrentLevel.Bounds.Width / 2 + 160f, CurrentLevel.Bounds.Center.Y);

            if (CurrentLevel.Session.Level == "X-07")
            {
                CurrentLevel.Add(new EscapeExplosion(new Vector2(ReactorCenter.X, ReactorCenter.Y)));
            }
            else if (ReactorCenter.Y - ExplosionCenter.Y == 0)
            {
                CurrentLevel.Add(new EscapeExplosion(new Vector2(ExplosionCenter.X, ExplosionCenter.Y)));
            }
            else if (Math.Abs(ReactorCenter.Y - ExplosionCenter.Y) <= 184)
            {
                CurrentLevel.Add(new EscapeExplosion(new Vector2(ExplosionCenter.X - 80f, ExplosionCenter.Y - CurrentLevel.Bounds.Height / 2)));
            }
            else
            {
                CurrentLevel.Add(new EscapeExplosion(new Vector2(ExplosionCenter.X - 80f, ExplosionCenter.Y - CurrentLevel.Bounds.Height / 2 - 92f)));
            }
        }

        private IEnumerator CloseTextbox(MiniTextbox textBox, float timer)
        {
            FancyText.Text textBoxtext = (FancyText.Text)MiniTextboxText.GetValue(textBox);
            while ((int)MiniTextboxIndex.GetValue(textBox) != textBoxtext.Count)
            {
                yield return null;
            }
            yield return timer;
            Coroutine TextBoxRoutine = (Coroutine)MiniTextboxRoutine.GetValue(textBox);
            TextBoxRoutine = (Coroutine)MiniTextboxRoutine.GetValue(textBox);
            TextBoxRoutine.Cancel();
            if (!(bool)MiniTextboxClosing.GetValue(textBox))
            {
                MiniTextboxClosing.SetValue(textBox, true);
                float ease = (float)MiniTextboxEase.GetValue(textBox);
                while ((ease -= Engine.DeltaTime * 4f) > 0f)
                {
                    MiniTextboxEase.SetValue(textBox, ease);
                    yield return null;
                }

                MiniTextboxEase.SetValue(textBox, 0f);
                textBox.RemoveSelf();
            }
        }

        public override void OnEnd(Level level)
        {
        }
    }
}
