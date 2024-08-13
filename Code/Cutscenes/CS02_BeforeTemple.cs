using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_BeforeTemple : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        private Vector2 badelinEndPosition;

        public CS02_BeforeTemple(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch2_Before_Temple");
            level.Session.SetFlag("CS_Ch2_Before_Temple");
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
            player.StateMachine.State = 11;
            while (!player.OnGround())
            {
                yield return null;
            }
            player.Facing = Facings.Right;
            badeline = new BadelineDummy(player.Position);
            badelineSplit(badeline);
            badelineFloat(30, -18, badeline, -1, true, false, true);
            while (badeline.Position != badelinEndPosition)
            {
                yield return 0.1f;
            }
            badelineFloat(-1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple");
            Add(new Coroutine(CameraTo(new Vector2(level.Bounds.Left + 640, level.Bounds.Top), 2f, Ease.SineInOut)));
            yield return 2f;
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple_b");
            yield return new FadeWipe(Scene, wipeIn: false).Duration = 0.5f;
            level.Camera.Position = new Vector2(level.Bounds.Left, level.Bounds.Top);
            yield return new FadeWipe(Scene, wipeIn: true);
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple_c");
            badelineFloatToPlayer(badeline);
            while (badeline.Position != player.Position)
            {
                yield return 0.1f;
            }
            badelineMerge(badeline);
            EndCutscene(Level);
        }
    }
}
