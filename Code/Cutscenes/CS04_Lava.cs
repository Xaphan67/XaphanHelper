using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_Lava : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_Lava(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (XaphanModule.ModSettings.AutoSkipCutscenes)
            {
                EndCutscene(Level);
                WasSkipped = true;
            }
            else
            {
                Add(new Coroutine(Cutscene(level)));
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (badeline != null)
                {
                    badeline.RemoveSelf();
                }
            }
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_Lava");
            level.Session.SetFlag("CS_Ch4_Lava");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkTo(Level.Bounds.Left + 44f, false, 2.5f);
            while (!player.OnSafeGround)
            {
                yield return null;
            }
            yield return 0.2f;
            player.Facing = Facings.Right;
            yield return Level.ZoomTo(new Vector2(110f, 110f), 1.5f, 1f);
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Lava");
            if (XaphanModule.ModSaveData.VisitedRooms.Contains("Xaphan/0/Ch4/R-14"))
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Lava_b_explored");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Lava_b");
            }
            yield return Textbox.Say("Xaphan_Ch4_A_Lava_c");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_X-Lore-00_4190"))
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Lava_d");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Lava_d_no_lore");
            }
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
