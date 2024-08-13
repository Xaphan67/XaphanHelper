using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_Start : CutsceneEntity
    {
        private readonly Player player;

        public CS00_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Start");
            level.Session.SetFlag("CS_Ch0_Start");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return Level.ZoomTo(new Vector2(210f, 90f), 1.5f, 1f);
            yield return player.DummyWalkTo(player.Position.X + 40f, false, 1);
            yield return Textbox.Say("Xaphan_Ch0_A_Start");
            yield return player.DummyWalkTo(player.Position.X - 15f, false, 1);
            yield return Textbox.Say("Xaphan_Ch0_A_Start_b");
            yield return player.DummyWalkTo(player.Position.X + 15f, false, 1);
            yield return Textbox.Say("Xaphan_Ch0_A_Start_c");
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
