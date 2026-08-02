using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS03_Bog_Start : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS03_Bog_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch3_Bog_Start");
            level.Session.SetFlag("CS_Ch3_Bog_Start");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return 0.5f;
            yield return Level.ZoomTo(new Vector2(210f, 104f), 1.5f, 1f);
            yield return Textbox.Say("Xaphan_Ch3_A_Bog_Start");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, false, false, true);
            yield return Textbox.Say("Xaphan_Ch3_A_Bog_Start_b");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
