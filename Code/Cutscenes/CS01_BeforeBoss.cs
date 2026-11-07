using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_BeforeBoss : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS01_BeforeBoss(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch1_Before_Boss");
            level.Session.SetFlag("CS_Ch1_Before_Boss");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return Textbox.Say("Xaphan_Ch5_A_Before_Boss");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, false, false, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Before_Boss_b");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            EndCutscene(Level);
        }
    }
}
