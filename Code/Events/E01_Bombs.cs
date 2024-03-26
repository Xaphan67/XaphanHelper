using System.Collections;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E01_Bombs : CutsceneEntity
    {
        private Player player;

        public E01_Bombs(Player player, Level level)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!level.Session.GetFlag("Torizo_Defeated"))
            {
                while (!level.Session.GetFlag("Upgrade_Bombs"))
                {
                    yield return null;
                }
                yield return 1f;
                level.Session.SetFlag("D-U0_Gate_1");
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_tension");
                level.Session.Audio.Apply();
            }
        }

        public override void OnEnd(Level level)
        {

        }
    }
}
