using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_BossStart : CutsceneEntity
    {
        private readonly Player player;

        private readonly Torizo boss;

        public CS01_BossStart(Player player, Torizo boss)
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
            level.Session.SetFlag("D-07_Gate_1", true);
            level.Session.SetFlag("Torizo_Wakeup", true);
            level.Session.SetFlag("Torizo_Start", true);
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch1_BossStart");
            level.Session.SetFlag("CS_01_BossStart", true);
            player.Facing = Facings.Left;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Sprite.Play("idle");
            level.Session.SetFlag("D-07_Gate_1", true);
            player.DummyAutoAnimate = true;
            yield return 0.75f;
            player.Facing = Facings.Right;
            yield return 1f;
            player.Facing = Facings.Left;
            Add(new Coroutine(CameraTo(new Vector2(level.Bounds.Left + 160, level.Bounds.Top), 2f, Ease.SineInOut)));
            yield return 3f;
            level.Session.SetFlag("Torizo_Wakeup", true);
            while (!boss.Activated)
            {
                yield return null;
            }
            yield return 1f;
            level.Session.SetFlag("Torizo_Start", true);
            EndCutscene(Level);
        }
    }
}
