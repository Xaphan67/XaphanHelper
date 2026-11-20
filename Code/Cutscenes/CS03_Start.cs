using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS03_Start : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS03_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch3_Start");
            level.Session.SetFlag("CS_Ch3_Start");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            if (Level.Session.Level == "L-23")
            {
                yield return Level.ZoomTo(new Vector2(210f, 88f), 1.5f, 1f);
            }
            else
            {
                yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            }
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch3_A_Start");
            if (Level.Session.Level == "L-00")
            {
                yield return player.DummyWalkTo(player.Position.X + 20f, false, 0.65f);
                player.Facing = Facings.Left;
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch3_A_Start_b_main");
            }
            else if (Level.Session.Level == "L-16")
            {
                yield return player.DummyWalkTo(player.Position.X - 20f, false, 0.65f);
                player.Facing = Facings.Right;
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch3_A_Start_b_alt");
            }
            else
            {
                player.Facing = Facings.Left;
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch3_A_Start_b_main");
            }
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
