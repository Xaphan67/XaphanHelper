using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_StatueRoom : CutsceneEntity
    {
        private readonly Player player;

        public CS00_StatueRoom(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Statue_Room");
            level.Session.SetFlag("CS_Ch0_Statue_Room");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            if (!level.Session.GetFlag("CS_Ch0_Statue_Room_P1"))
            {
                yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
                yield return 0.2f;
                yield return Textbox.Say("Xaphan_Ch0_A_Statue_Room");
                yield return Level.ZoomBack(0.5f);
            }
            level.InCutscene = false;
            level.CancelCutscene();
            level.Session.SetFlag("CS_Ch0_Statue_Room_P1");
            player.StateMachine.State = 0;
            while (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                yield return null;
            }
            level.InCutscene = true;
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