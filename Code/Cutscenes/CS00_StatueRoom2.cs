using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_StatueRoom2 : CutsceneEntity
    {
        private readonly Player player;

        public CS00_StatueRoom2(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Statue_Room2");
            level.Session.SetFlag("CS_Ch0_Statue_Room2");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            while (!player.OnGround())
            {
                yield return null;
            }
            yield return player.DummyWalkTo(player.Position.X - 32f, true, 1.5f);
            yield return Textbox.Say("Xaphan_Ch0_A_Statue_Room_b");
            EndCutscene(Level);
        }
    }
}