using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E03_WaterSource : CutsceneEntity
    {
        private Player player;

        private Message message;

        public E03_WaterSource(Player player, Level level)
        {
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
            WaterWheel waterwheel = level.Tracker.GetEntity<WaterWheel>();
            string flag = waterwheel.flag;

            string Prefix = level.Session.Area.LevelSet;
            int chapterIndex = level.Session.Area.ChapterIndex;

            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
            {
                while (waterwheel.acceleration < 100 || !level.Session.GetFlag(flag) || !player.OnSafeGround)
                {
                    yield return null;
                }
                player.StateMachine.State = 11;
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                int remainingSources = 3;
                foreach (string savedFlag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (savedFlag.Contains(Prefix + "_Ch" + chapterIndex + "_Bog_Main_Poison"))
                    {
                        remainingSources--;
                    }
                }
                if (remainingSources > 0)
                {
                    level.Add(message = new Message(Vector2.Zero, "event:/game/xaphan/push_block_start_move", "Xaphan_Ch3_WaterSource", remainingSources, "Xaphan_Ch3_RemainingSource" + (remainingSources > 1 ? "s" : "")));
                }
                else
                {
                    level.Add(message = new Message(Vector2.Zero, "event:/game/xaphan/push_block_start_move", "Xaphan_Ch3_AllWaterSource"));
                }
                while (!message.drawText || (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed))
                {
                    yield return null;
                }
                message.Close();
                level.Session.SetFlag(flag + "_Purified");
                yield return 0.2f;
                player.StateMachine.State = 0;
            }
        }

        public override void OnEnd(Level level)
        {

        }
    }
}
