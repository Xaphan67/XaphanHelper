using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_Fall : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS00_Fall(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.PauseLock = true;
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            level.PauseLock = false;
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Fall");
            level.Session.SetFlag("CS_Ch0_Fall");
            XaphanModule.ModSaveData.VisitedRoomsTiles.Add("Xaphan/0/Ch0/A-15-0-1");
            XaphanModule.ModSaveData.VisitedRoomsTiles.Add("Xaphan/0/Ch0/A-15-0-2");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.Position.X = level.Bounds.X + 96;
            player.Position.Y = level.Bounds.Y + 632;
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");
            yield return 1f;
            Add(new Coroutine(CameraTo(new Vector2(level.Bounds.Left, level.Bounds.Top + 538f), 4f, Ease.SineInOut)));
            yield return 4f;
            if (!XaphanModule.ModSettings.AutoSkipCutscenes)
            {
                level.PauseLock = false;
                yield return 0.5f;
                yield return Level.ZoomTo(new Vector2(96f, 64f), 1.5f, 1f);
                yield return Textbox.Say("Xaphan_Ch0_A_Fall");
                yield return 0.15f;
                player.Sprite.Play("idle");
                yield return 0.15f;
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, false, false, true);
                yield return Textbox.Say("Xaphan_Ch0_A_Fall_b");
                yield return 0.7f;
                yield return Textbox.Say("Xaphan_Ch0_A_Fall_c");
                yield return 0.4f;
                yield return Textbox.Say("Xaphan_Ch0_A_Fall_d");
                player.Sprite.Play("lookUp");
                yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, 1, false, false, true);
                yield return Textbox.Say("Xaphan_Ch0_A_Fall_e");
                yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
                player.Sprite.Play("idle");
                player.DummyAutoAnimate = true;
                yield return Level.ZoomBack(0.5f);
            }
            player.Sprite.Play("idle");
            EndCutscene(Level);
        }
    }
}
