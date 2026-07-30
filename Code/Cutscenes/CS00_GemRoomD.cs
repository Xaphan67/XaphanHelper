using System.Collections;
using Celeste.Mod.XaphanHelper.Controllers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_GemRoomD : CutsceneEntity
    {
        private readonly Player player;

        private GemController gemController;

        public CS00_GemRoomD(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Gem_Room_D");
            level.PauseLock = true;
            player.StateMachine.State = 11;
            gemController = Scene.Entities.FindFirst<GemController>();
            Add(new Coroutine(Cutscene(level)));
        }

        public override void OnEnd(Level level)
        {

        }

        public IEnumerator Cutscene(Level level)
        {
            yield return gemController.OpenEndArea();    
            yield return 1f;
            Scene.Add(new TeleportCutscene(player, "A-15", new Vector2(0, 0), 0, 0, true, 0, "Fade"));
        }
    }
}
