using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Stele")]
    public class Stele : Entity
    {
        private Sprite mainSprite;

        private Sprite leftOrb;

        private Sprite rightOrb;

        private Player player;

        public Sprite PlayerSprite;

        public Sprite PlayerHairSprite;

        private TalkComponent talk;

        public EventInstance shakeSoundSource;

        private Coroutine appearRoutine = new();

        private bool shake;

        private List<Entity> absorbs = new();

        private CustomCollectable semantograph;

        public Stele(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(mainSprite = new Sprite(GFX.Game, "objects/Xaphan/Stele/"));
            mainSprite.Add("idle", "stele", 0, 0);
            mainSprite.AddLoop("interact", "stele", 0.2f, 1, 2, 3, 4, 3, 2);
            mainSprite.Add("activePurple", "stele", 0.2f, 5, 6, 7, 8);
            mainSprite.Add("inactivePurple", "stele", 0.2f, 7, 6, 5, 0);
            mainSprite.Add("activeRed", "stele", 0.2f, 9, 10, 11, 12);
            mainSprite.Add("inactiveRed", "stele", 0.2f, 11, 10, 9, 0);
            mainSprite.CenterOrigin();
            mainSprite.Play("idle");
            Add(leftOrb = new Sprite(GFX.Game, "objects/Xaphan/Stele/"));
            leftOrb.AddLoop("active", "orb", 0.04f, 0, 1, 2, 1);
            leftOrb.CenterOrigin();
            leftOrb.Position += new Vector2(-52f, -32f);
            leftOrb.Color = Color.White * 0.7f;
            leftOrb.Scale = Vector2.Zero;
            leftOrb.FlipX = true;
            Add(rightOrb = new Sprite(GFX.Game, "objects/Xaphan/Stele/"));
            rightOrb.AddLoop("active", "orb", 0.04f, 0, 1, 2, 1);
            rightOrb.Position += new Vector2(52f, -32f);
            rightOrb.CenterOrigin();
            rightOrb.Color = Color.White * 0.7f;
            rightOrb.Scale = Vector2.Zero;
            Add(PlayerSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerSprite.CenterOrigin();
            PlayerSprite.Position += new Vector2(0f, 2f);
            PlayerSprite.Visible = false;
            Add(PlayerHairSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerHairSprite.CenterOrigin();
            PlayerHairSprite.Position += new Vector2(0f, 2f);
            PlayerHairSprite.Visible = false;
        }

        public static void Load()
        {
            On.Celeste.Player.Die += OnPlayerDie;
        }

        public static void Unload()
        {
            On.Celeste.Player.Die -= OnPlayerDie;
        }

        private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            foreach (Stele stele in self.SceneAs<Level>().Tracker.GetEntities<Stele>())
            {
                if (stele.PlayerSprite.Visible)
                {
                    stele.PlayerSprite.Visible = false;
                    stele.PlayerHairSprite.Visible = false;
                    self.Sprite.Visible = true;
                }
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(new Slope(Position, new Vector2(1f, 16f), true, "Right", 8, 1, "Horizontal", "Horizontal", "cement", "cement", false, false, "", "", false, true, false, false, false, "", true));
            SceneAs<Level>().Add(new Slope(Position, new Vector2(-25f, 16f), true, "Left", 8, 1, "Horizontal", "Horizontal", "cement", "cement", false, false, "", "", false, true, false, false, false, "", true));
            SceneAs<Level>().Add(new InvisibleBarrier(Position + new Vector2(-16f, 18f), 32f, 6f));

            Add(talk = new TalkComponent(new Rectangle(-12, 10, 24, 8), new Vector2(0, -24f), Interact));
            talk.PlayerMustBeFacing = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            semantograph = SceneAs<Level>().Entities.FindFirst<CustomCollectable>();
            if (semantograph != null && !SceneAs<Level>().Session.GetFlag("Ch0_Semantograph_appeared") || !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Semantograph_appeared"))
            {
                semantograph.Visible = semantograph.Collidable = false;
            }
        }

        private void Interact(Player player)
        {
            this.player = player;
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            PlayerSprite.OnLastFrame = null;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.DummyAutoAnimate = false;
            yield return player.DummyWalkToExact((int)BottomCenter.X);
            PlayerSprite.FlipX = player.Facing == Facings.Left;
            PlayerHairSprite.FlipX = player.Facing == Facings.Left;
            player.Sprite.Visible = false;
            player.Hair.Visible = false;
            PlayerSprite.Visible = true;
            PlayerHairSprite.Visible = true;
            PlayerSprite.Play("turnBackpack");
            PlayerHairSprite.Play("hairBackpack");
            yield return 0.5f;

            mainSprite.Play("interact");
            yield return 1.2f;

            if ((!SceneAs<Level>().Session.GetFlag("Ch0_Semantograph_appeared") || !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Semantograph_appeared")) && semantograph != null)
            {
                mainSprite.Play("activePurple");
                yield return 1.3f;
                XaphanModule.IgnoreShakeSettings = true;
                SceneAs<Level>().CanRetry = false;
                shakeSoundSource = Audio.Play("event:/game/xaphan/liquid_rise");
                shake = true;
                Add(appearRoutine = new Coroutine(AppearRoutine()));
                while (!SceneAs<Level>().Paused && shake)
                {
                    SceneAs<Level>().DirectionalShake(new Vector2(0.5f, 0), 0.05f);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                    yield return 0.05f;
                }
                shakeSoundSource.stop(STOP_MODE.ALLOWFADEOUT);
                SceneAs<Level>().CanRetry = true;
                XaphanModule.IgnoreShakeSettings = false;               
            }
            else
            {
                mainSprite.Play("activeRed");
                yield return 1.3f;
                mainSprite.Play("inactiveRed");
            }

            yield return 0.5f;
            PlayerSprite.Play("turnBackpack_reverse");
            PlayerHairSprite.Play("hairBackpack_reverse");
            PlayerSprite.OnLastFrame = delegate
            {
                PlayerSprite.Visible = false;
                PlayerHairSprite.Visible = false;
                player.Sprite.Visible = true;
                player.Hair.Visible = true;
                player.StateMachine.State = 0;
            };
            yield return 0.2f;
            player.StateMachine.Locked = false;
            player.DummyAutoAnimate = true;
            player.StateMachine.State = 0;
        }

        private IEnumerator AppearRoutine()
        {
            yield return 2f;
            leftOrb.Play("active");
            rightOrb.Play("active");
            float timer = 0.1f;
            while (timer > 0f)
            {
                leftOrb.Scale.X = 1.1f - timer * 10;
                leftOrb.Scale.Y = 1.1f - timer * 10;
                rightOrb.Scale.X = 1.1f - timer * 10;
                rightOrb.Scale.Y = 1.1f - timer * 10;
                timer -= Engine.DeltaTime;
                yield return null;
            }
            yield return 1.5f;
            Audio.Play("event:/game/06_reflection/supersecret_heartappear");
            Entity dummy = new(Position + new Vector2(0, -64f))
            {
                Depth = 1
            };
            Scene.Add(dummy);
            Image white = new(GFX.Game["objects/Xaphan/Stele/white00"]);
            white.CenterOrigin();
            white.Scale = Vector2.Zero;
            dummy.Add(white);
            BloomPoint glow = new(0f, 16f);
            dummy.Add(glow);
            for (int i = 0; i < 20; i++)
            {
                CustomAbsorbOrb orbLeft = new(Position + new Vector2(-52f, -32f), dummy, color: "3FD6CA");
                CustomAbsorbOrb orbRight = new(Position + new Vector2(52f, -32f), dummy, color: "3FD6CA");
                Scene.Add(orbLeft);
                Scene.Add(orbRight);
                absorbs.Add(orbLeft);
                absorbs.Add(orbRight);
                yield return null;
            }
            yield return 0.7f;
            timer = 0.1f;
            while (timer > 0f)
            {
                leftOrb.Scale.X = timer * 10;
                leftOrb.Scale.Y = timer * 10;
                rightOrb.Scale.X = timer * 10;
                rightOrb.Scale.Y = timer * 10;
                timer -= Engine.DeltaTime;
                yield return null;
            }
            leftOrb.Scale = Vector2.Zero;
            rightOrb.Scale = Vector2.Zero;
            float duration = 0.6f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                white.Scale = Vector2.One * p;
                glow.Alpha = p;
                (Scene as Level).Shake();
                yield return null;
            }
            foreach (Entity orb in absorbs)
            {
                orb.RemoveSelf();
            }
            SceneAs<Level>().Flash(Color.White);
            Scene.Remove(dummy);
            SceneAs<Level>().Session.SetFlag("Ch0_Semantograph_appeared", true);
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Semantograph_appeared");
            semantograph.Visible = semantograph.Collidable = true;
            SceneAs<Level>().Displacement.AddBurst(semantograph.Center, 0.5f, 8f, 32f, 0.5f);
            mainSprite.Play("inactivePurple");
            yield return 0.8f;
            shake = false;
        }

        public override void Render()
        {
            base.Render();
            if (PlayerSprite != null && PlayerSprite.Visible)
            {
                PlayerSprite.Render();
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (PlayerHairSprite != null && PlayerHairSprite.Visible && player != null)
            {
                PlayerHairSprite.Color = player.Hair.Color;
                PlayerHairSprite.Render();
            }
        }
    }
}
