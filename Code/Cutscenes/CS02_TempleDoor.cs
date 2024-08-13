using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_TempleDoor : CutsceneEntity
    {
        private readonly Player player;

        public CS02_TempleDoor(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch2_TempleDoor");
            level.Session.SetFlag("CS_Ch2_TempleDoor");
        }

        public IEnumerator Cutscene(Level level)
        {
            level.Add(new MiniTextbox("Xaphan_Ch2_A_TempleDoor"));
            yield return null;
            EndCutscene(Level);
        }
    }
}