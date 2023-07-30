using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class FakePlayer : Player
    {
        private bool startSleep;
        public FakePlayer(Vector2 position, PlayerSpriteMode spriteMode, bool startSleep = false) : base(position, spriteMode)
        {
            this.startSleep = startSleep;
        }

        public static void Load()
        {
            On.Celeste.Player.Added += OnPlayerAdded;
        }

        public static void Unload()
        {
            On.Celeste.Player.Added -= OnPlayerAdded;
        }

        private static void OnPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            if (XaphanModule.ModSaveData.fakePlayerFacing.ContainsKey(self.SceneAs<Level>().Session.Area.GetLevelSet()) && XaphanModule.ModSaveData.fakePlayerFacing[self.SceneAs<Level>().Session.Area.GetLevelSet()] != 0 && self.GetType() == typeof(FakePlayer))
            {
                self.Facing = XaphanModule.ModSaveData.fakePlayerFacing[self.SceneAs<Level>().Session.Area.GetLevelSet()];
            }
        }

        public override void Update()
        {
            List<Entity> fakePlayerPlatforms = Scene.Tracker.GetEntities<FakePlayerPlatform>().ToList();
            List<Entity> playerPlatforms = Scene.Tracker.GetEntities<PlayerPlatform>().ToList();
            fakePlayerPlatforms.ForEach(entity => entity.Collidable = true);
            playerPlatforms.ForEach(entity => entity.Collidable = false);
            base.Update();
            if (startSleep)
            {
                StateMachine.State = 11;
                DummyAutoAnimate = false;
                Sprite.Play("sleep");
                Sprite.SetAnimationFrame(Sprite.CurrentAnimationTotalFrames - 1);
                Depth = 100;
            }
            fakePlayerPlatforms.ForEach(entity => entity.Collidable = false);
            playerPlatforms.ForEach(entity => entity.Collidable = true);
        }
    }
}
