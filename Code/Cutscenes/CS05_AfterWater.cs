using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_AfterWater : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS05_AfterWater(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_After_Water");
            level.Session.SetFlag("CS_Ch5_After_Water");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkTo(player.Position.X - 21f, false, 1f);
            while (!player.OnSafeGround)
            {
                yield return null;
            }
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");
            yield return Level.ZoomTo(new Vector2(210f, 110f), 1.5f, 1f);
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch5_A_After_Water");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch5_A_After_Water_b");
            yield return 1f;
            player.Sprite.Play("idle");
            player.DummyAutoAnimate = true;
            yield return Textbox.Say("Xaphan_Ch5_A_After_Water_c");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_W-Lore-00_46"))
            {
                yield return Textbox.Say("Xaphan_Ch5_A_After_Water_d");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch5_A_After_Water_d_no_lore");
            }
            yield return 1f;
            yield return Textbox.Say("Xaphan_Ch5_A_After_Water_e");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
