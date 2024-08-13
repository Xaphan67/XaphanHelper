using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_FindCavern : CutsceneEntity
    {
        private readonly Player player;

        public CS00_FindCavern(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Find_Cavern");
            level.Session.SetFlag("CS_Ch0_Find_Cavern");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return Textbox.Say("Xaphan_Ch0_A_Find_Cavern");
            EndCutscene(Level);
        }
    }
}
