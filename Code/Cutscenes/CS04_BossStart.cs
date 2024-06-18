using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_BossStart : CutsceneEntity
    {
        private readonly Player player;

        private readonly AncientGuardian boss;

        public CS04_BossStart(Player player, AncientGuardian boss)
        {
            this.player = player;
            this.boss = boss;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
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
            player.DummyAutoAnimate = true;
            yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2);
            yield return 1f;
            player.Facing = Facings.Right;
            yield return 0.5f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookup");
            yield return 1f;
            yield return boss.EyesAlphaRoutine(true);
            yield return 0.5f;
            player.Facing = Facings.Left;
            level.Session.SetFlag("AncientGuardian_Start", true);
            EndCutscene(Level);
        }
    }
}
