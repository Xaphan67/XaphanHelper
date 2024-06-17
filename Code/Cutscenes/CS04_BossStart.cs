

using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_BossStart : CutsceneEntity
    {
        private readonly Player player;

        public CS04_BossStart(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            //level.Session.SetFlag("D-07_Gate_1", true);
            level.Session.SetFlag("AncientGuardian_Start", true);
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_BossStart");
            level.Session.SetFlag("CS_04_BossStart", true);
            player.Facing = Facings.Left;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Sprite.Play("idle");
            //level.Session.SetFlag("D-07_Gate_1", true);
            player.DummyAutoAnimate = true;
            yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2);
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookup");
            yield return 1.5f;
            player.Facing = Facings.Left;
            level.Session.SetFlag("AncientGuardian_Start", true);
            EndCutscene(Level);
        }
    }
}
