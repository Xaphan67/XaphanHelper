using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Enemies;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Triggers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E03_BogPathOpen : CutsceneEntity
    {
        private Player player;

        private Message message;

        private HashSet<Entity> entities = new();

        public E03_BogPathOpen(Player player, Level level)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            RegisterExistingEntities(level);
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            WaterWheel waterwheel = level.Tracker.GetEntity<WaterWheel>();
            string flag = waterwheel.flag;

            string Prefix = level.Session.Area.LevelSet;
            int chapterIndex = level.Session.Area.ChapterIndex;

            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
            {
                while (waterwheel.acceleration == 0 || !player.OnSafeGround)
                {
                    yield return null;
                }
                player.StateMachine.State = 11;
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                int remainingSources = 2;
                foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (savedFlag.Contains(Prefix + "_Ch" + chapterIndex + "_Bog_Energy_Source"))
                    {
                        remainingSources--;
                    }
                }
                if (remainingSources > 0)
                {
                    level.Add(message = new Message(Vector2.Zero, "event:/game/xaphan/push_block_start_move", "Xaphan_Ch3_EnergySource", remainingSources, "Xaphan_Ch3_RemainingSource"));
                }
                else
                {
                    level.Add(message = new Message(Vector2.Zero, "event:/game/xaphan/push_block_start_move", "Xaphan_Ch3_AllEnergySource"));
                }
                while (!message.drawText || (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed))
                {
                    yield return null;
                }
                message.Close();
                yield return 0.2f;
                RegisterFlags(level, Prefix, chapterIndex);
                if (remainingSources == 0)
                {
                    AddEntitiesToNoLoad(level);
                    TeleportTrigger trigger = level.Tracker.GetEntity<TeleportTrigger>();
                    Vector2 triggerStartPosition = trigger.Position;
                    trigger.Position = player.Position - new Vector2(trigger.Width / 2, trigger.Height / 2);
                    yield return 0.1f;
                    trigger.Position = triggerStartPosition;
                }
                else
                {
                    player.StateMachine.State = 0;
                }
            }
        }

        public override void OnEnd(Level level)
        {

        }

        private void RegisterExistingEntities(Level level)
        {
            foreach (RotateFireflea fireflea in level.Tracker.GetEntities<RotateFireflea>())
            {
                entities.Add(fireflea);
            }
            foreach (CustomCrumbleBlock crumbleBlock in level.Tracker.GetEntities<CustomCrumbleBlock>())
            {
                entities.Add(crumbleBlock);
            }
        }

        private void RegisterFlags(Level level, string Prefix, int chapterIndex)
        {
            foreach (DroneSwitch droneSwitch in level.Tracker.GetEntities<DroneSwitch>())
            {
                if (!string.IsNullOrEmpty(droneSwitch.flag))
                {
                    droneSwitch.startSpawnPoint = level.Session.RespawnPoint;
                    droneSwitch.flagState = level.Session.GetFlag(droneSwitch.flag);
                    level.Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_true", false);
                    level.Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_false", false);
                    if (droneSwitch.wasPressed && droneSwitch.registerInSaveData && droneSwitch.saveDataOnlyAfterCheckpoint)
                    {
                        if (level.Session.GetFlag(droneSwitch.flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                        }
                        else if (level.Session.GetFlag(droneSwitch.flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                        }
                    }
                }
            }
        }

        private void AddEntitiesToNoLoad(Level level)
        {
            foreach (TrackFireflea fireflea in level.Tracker.GetEntities<TrackFireflea>())
            {
                level.Session.DoNotLoad.Add(fireflea.eid);
            }
            HashSet<Entity> toRemove = new();
            foreach (Entity entity in entities)
            {
                toRemove.Add(entity);
                if (entity.GetType() == typeof(RotateFireflea))
                {
                    RotateFireflea currentFireflea = (RotateFireflea)entity;
                    foreach (RotateFireflea fireflea in level.Tracker.GetEntities<RotateFireflea>())
                    {
                        if (fireflea == currentFireflea)
                        {
                            toRemove.Remove(fireflea);
                            break;
                        }
                    }
                }
                else if (entity.GetType() == typeof(CustomCrumbleBlock))
                {
                    CustomCrumbleBlock currentCrumbleBlock = (CustomCrumbleBlock)entity;
                    foreach (CustomCrumbleBlock crumbleBlock in level.Tracker.GetEntities<CustomCrumbleBlock>())
                    {
                        if (crumbleBlock == currentCrumbleBlock)
                        {
                            toRemove.Remove(crumbleBlock);
                            break;
                        }
                    }
                }
            }
            foreach (Entity entity in toRemove)
            {
                if (entity.GetType() == typeof(RotateFireflea))
                {
                    RotateFireflea fireflea = (RotateFireflea)entity;
                    level.Session.DoNotLoad.Add(fireflea.eid);
                }
                else if (entity.GetType() == typeof(CustomCrumbleBlock))
                {
                    CustomCrumbleBlock crumbleBlock = (CustomCrumbleBlock)entity;
                    level.Session.DoNotLoad.Add(crumbleBlock.eid);
                }
            }
            toRemove.Clear();
        }
    }
}
