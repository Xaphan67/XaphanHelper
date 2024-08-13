using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_Gem : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        private Vector2 badelinEndPosition;

        public CS01_Gem(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch1_Gem");
            level.Session.SetFlag("CS_Ch1_Gem");
            player.StateMachine.State = 0;
        }

        public void badelineAppears(BadelineDummy badeline)
        {
            Level.Add(badeline);
            badeline.Appear(Level);
        }

        public void badelineMerge(BadelineDummy badeline)
        {
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", badeline.Position);
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            badeline.RemoveSelf();
        }

        public void badelineFloat(int x, int y, BadelineDummy badeline, int? turnAtEndTo, bool faceDirection, bool fadeLight, bool quickEnd)
        {
            badelinEndPosition = new Vector2(badeline.Position.X + x, badeline.Position.Y + y);
            Add(new Coroutine(badeline.FloatTo(badelinEndPosition, turnAtEndTo, faceDirection, fadeLight, quickEnd)));
        }

        public void badelineFloatToPlayer(BadelineDummy badeline)
        {
            Add(new Coroutine(badeline.FloatTo(player.Position, null, true, false, true)));
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return Level.ZoomTo(new Vector2(165f, 110f), 1.5f, 1f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem");
            badeline = new BadelineDummy(new Vector2(player.Position.X - 60, player.Position.Y - 24));
            badelineAppears(badeline);
            badelineFloat(1, 0, badeline, null, true, false, false);
            yield return 0.5;
            yield return player.DummyWalkTo(player.Position.X - 5f, false, 0.5f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_b");
            yield return player.DummyRunTo(badeline.X + 30, false);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_c");
            badelineFloat(60, -15, badeline, null, true, false, false);
            while (badeline.Position != badelinEndPosition)
            {
                yield return 0.1f;
            }
            badelineFloat(0, 0, badeline, null, true, false, false);
            yield return player.DummyWalkTo(player.Position.X + 5f, false, 0.5f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_d");
            badelineFloat(-1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_e");
            badelineFloatToPlayer(badeline);
            while (badeline.Position != player.Position)
            {
                yield return 0.1f;
            }
            badelineMerge(badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}