using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class FakePlayer : Player
    {
        private bool startSleep;

        private const string SleepSpriteId = "XaphanHelper_player_dronesleep";

        public Sprite PlayerSprite;

        public Sprite PlayerHairSprite;

        private static MethodInfo smhGetPlayerSkin;

        private static bool smhChecked;

        public bool startedSleepAnim;

        public bool startedWakeUpAnim;

        public FakePlayer(Vector2 position, PlayerSpriteMode spriteMode, bool startSleep = false) : base(position, spriteMode)
        {
            this.startSleep = startSleep;
            SetupSleepSprite();
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
            if (XaphanModule.ModSaveData.fakePlayerFacing.ContainsKey(self.SceneAs<Level>().Session.Area.LevelSet) && XaphanModule.ModSaveData.fakePlayerFacing[self.SceneAs<Level>().Session.Area.LevelSet] != 0 && self.GetType() == typeof(FakePlayer))
            {
                self.Facing = XaphanModule.ModSaveData.fakePlayerFacing[self.SceneAs<Level>().Session.Area.LevelSet];
            }
        }

        public override void Update()
        {
            if (XaphanModule.ModSaveData.droneStartRoom.ContainsKey(SceneAs<Level>().Session.Area.LevelSet) && SceneAs<Level>().Session.Level == XaphanModule.ModSaveData.droneStartRoom[SceneAs<Level>().Session.Area.LevelSet])
            {
                List<Entity> fakePlayerPlatforms = Scene.Tracker.GetEntities<FakePlayerPlatform>().ToList();
                List<Entity> playerPlatforms = Scene.Tracker.GetEntities<PlayerPlatform>().ToList();
                fakePlayerPlatforms.ForEach(entity => entity.Collidable = true);
                playerPlatforms.ForEach(entity => entity.Collidable = false);
                base.Update();
                if (startSleep)
                {
                    startSleep = !startSleep;
                    StateMachine.State = 11;
                    DummyAutoAnimate = false;
                    PlaySleep();
                    Depth = 100;
                }
                fakePlayerPlatforms.ForEach(entity => entity.Collidable = false);
                playerPlatforms.ForEach(entity => (entity as PlayerPlatform).RestoreCollisionForPlayer());
            }
        }

        private static string GetActivePlayerSkin()
        {
            if (!smhChecked)
            {
                smhChecked = true;
                smhGetPlayerSkin = Everest.Modules.FirstOrDefault(m => m.GetType().FullName == "Celeste.Mod.SkinModHelper.SkinModHelperModule")?.GetType().GetMethod("GetPlayerSkin", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string) }, null);
            }
            if (smhGetPlayerSkin == null)
            {
                return null;
            }
            try
            {
                return smhGetPlayerSkin.Invoke(null, [null, null]) as string;
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Warn, "XaphanHelper", "Could not query SkinModHelper: " + e.Message);
                return null;
            }
        }

        private static string FirstFramePath(Sprite sprite)
        {
            if (sprite != null && sprite.Has("sleep") && sprite.Animations["sleep"].Frames.Length > 0)
            {
                return sprite.Animations["sleep"].Frames[0].ToString();
            }
            return null;
        }

        private void SetupSleepSprite()
        {
            if (!GFX.SpriteBank.Has(SleepSpriteId))
            {
                return;
            }

            Sprite sprite = GFX.SpriteBank.Create(SleepSpriteId);

            if (GetActivePlayerSkin() != null)
            {
                Sprite original = GFX.SpriteBank.SpriteData[SleepSpriteId].Create();
                string skinPath = FirstFramePath(sprite);
                if (skinPath == null || skinPath == FirstFramePath(original))
                {
                    return;
                }
            }

            sprite.Visible = false;
            PlayerSprite = sprite;
            PlayerSprite.Position -= new Vector2(16f, 32f);
            Add(PlayerSprite);
            PlayerHairSprite = GFX.SpriteBank.Create(SleepSpriteId);
            PlayerHairSprite.Position -= new Vector2(16f, 32f);
            PlayerHairSprite.Visible = false;
            Add(PlayerHairSprite);
        }

        public void PlaySleep()
        {
            startedSleepAnim = true;
            if (PlayerSprite != null && PlayerHairSprite != null)
            {
                Sprite.Visible = false;
                Hair.Visible = false;
                PlayerSprite.Rate = 2f;
                PlayerHairSprite.Rate = 2f;
                PlayerSprite.FlipX = Facing == Facings.Left;
                PlayerHairSprite.FlipX = Facing == Facings.Left;
                PlayerSprite.SetAnimationFrame(0);
                PlayerHairSprite.SetAnimationFrame(0);
                PlayerSprite.Visible = true;
                PlayerHairSprite.Visible = true;
                PlayerHairSprite.Color = Hair.Color;
                PlayerSprite.Play("sleep");
                PlayerHairSprite.Play("sleepHair");
            }
            else if (Sprite.Has("sleep"))
            {
                Sprite.Rate = 2f;
                Sprite.Visible = true;
                Sprite.Play("sleep");
            }
        }

        public void PlayWakeUp()
        {
            startedWakeUpAnim = true;
            if (PlayerSprite != null && PlayerHairSprite != null)
            {
                Sprite.Visible = false;
                Hair.Visible = false;
                PlayerSprite.Visible = true;
                PlayerHairSprite.Visible = true;
                PlayerSprite.Rate = 2f;
                PlayerHairSprite.Rate = 2f;
                PlayerSprite.FlipX = Facing == Facings.Left;
                PlayerHairSprite.FlipX = Facing == Facings.Left;
                PlayerHairSprite.Color = Hair.Color;
                PlayerSprite.Play("wakeUp");
                PlayerHairSprite.Play("wakeUpHair");
                PlayerSprite.SetAnimationFrame(0);
                PlayerHairSprite.SetAnimationFrame(0);
                PlayerSprite.OnLastFrame = delegate
                {
                    startedWakeUpAnim = false;
                };  
            }
            else if (Sprite.Has("wakeUp"))
            {
                Sprite.Visible = true;
                Sprite.Play("wakeUp");
                Sprite.OnLastFrame = delegate
                {
                    startedWakeUpAnim = false;
                };
            }
        }
    }
}
