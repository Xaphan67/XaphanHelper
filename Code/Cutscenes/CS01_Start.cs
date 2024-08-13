using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_Start : CutsceneEntity
    {
        private readonly Player player;

        public CS01_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch1_Start");
            level.Session.SetFlag("CS_Ch1_Start");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch1_A_Start");
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
