using System.Collections;
using System.Collections.Generic;
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

        public EventInstance alarmSfx;

        public E05_EscapeStart(Player player, Level level)
        {
            Tag = Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!level.Session.GetFlag("Ch4_Escape_Complete"))
            {
                while (!level.Session.GetFlag("Gem_Collected") || !playerHasMoved)
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
                    List<EntityID> IDs = new();
                    List<EntityID> IDsToRemove = new();
                    IDs.Add(new EntityID("W-32", 2611));
                    IDs.Add(new EntityID("W-32", 2610));
                    IDs.Add(new EntityID("W-32", 2612));
                    IDs.Add(new EntityID("W-32", 2538));
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
                    StartCountdownTrigger trigger = level.Tracker.GetEntity<StartCountdownTrigger>();
                    Vector2 triggerStartPosition = trigger.Position;
                    trigger.Position = player.Position - new Vector2(trigger.Width / 2, trigger.Height / 2);
                    trigger.ChangeSpawnPosition(new Vector2(92f, 152f));
                    level.Session.RespawnPoint = level.Session.GetSpawnPoint(trigger.Center);
                    yield return 0.01f;
                    XaphanModule.ModSaveData.SavedSpawn[level.Session.Area.LevelSet] = (Vector2)level.Session.RespawnPoint - new Vector2(level.Bounds.Left, level.Bounds.Top);
                    level.Session.SetFlag("Lab_Escape", true);
                    trigger.Position = triggerStartPosition;
                    float timer = 2f;
                    bool countdownStarted = false;
                    while (timer > 0 && !countdownStarted)
                    {
                        timer -= Engine.DeltaTime;
                        yield return null;
                        if (level.Tracker.GetEntity<CountdownDisplay>() != null)
                        {
                            countdownStarted = true;
                        }
                    }
                    level.Session.SetFlag("Lab_Escape_Music", true);
                    level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_escape");
                    level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                    CountdownDisplay display = null;
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
        }

        public override void OnEnd(Level level)
        {

        }
    }
}
