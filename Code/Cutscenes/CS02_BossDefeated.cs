using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_BossDefeated : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        private Vector2 badelinEndPosition;

        public CS02_BossDefeated(Player player)
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
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated");
                if (XaphanModule.PlayerHasGolden)
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated_GoldenStrawberry");
                }
            }
            player.StateMachine.State = 0;
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

        public void badelineSplit(BadelineDummy badeline)
        {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(badeline);
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
        }

        public IEnumerator Cutscene(Level level)
        {
            while (!player.OnGround())
            {
                yield return null;
            }
            player.StateMachine.State = 11;
            if (player.Position.X <= level.Bounds.Left + level.Bounds.Width / 2 - 15)
            {
                yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2 - 15);
            }
            else
            {
                yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2 - 15, true, 1.5f);
            }
            player.Facing = Facings.Right;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated");
            badeline = new BadelineDummy(player.Position);
            badelineSplit(badeline);
            badelineFloat(30, -18, badeline, -1, true, false, true);
            while (badeline.Position != badelinEndPosition)
            {
                yield return 0.1f;
            }
            badelineFloat(-1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated_b");
            player.Sprite.Play("idle");
            player.DummyAutoAnimate = true;
            yield return 0.75f;
            player.Facing = Facings.Left;
            yield return 1f;
            player.Facing = Facings.Right;
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated_c");
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
