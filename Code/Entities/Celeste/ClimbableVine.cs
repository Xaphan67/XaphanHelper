using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ClimbableVine")]
    class ClimbableVine : Solid
    {
        private Sprite spriteA;

        private Sprite spriteB;

        private Sprite edgeSprite;

        public string Directory;

        public string flag;

        public float noCollideDelay;

        public float playerStamina;

        private float fullHeight;

        public ClimbableVine(EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, safe: false)
        {
            Tag = Tags.TransitionUpdate;
            fullHeight = data.Height - 3f;
            Collider = new Hitbox(2f, fullHeight, 3f, 0f);
            Add(new PlayerCollider(onPlayer, new Hitbox(8f, data.Height - 3f)));
            Directory = data.Attr("directory");
            flag = data.Attr("flag");
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/ClimbableVine";
            }
            Add(spriteA = new Sprite(GFX.Game, Directory + "/"));
            spriteA.Position = Vector2.Zero;
            spriteA.AddLoop("light", "light_a", 0.08f);
            spriteA.AddLoop("dark", "dark_a", 0.08f);
            spriteA.Play("light");
            Add(spriteB = new Sprite(GFX.Game, Directory + "/"));
            spriteB.Position = new Vector2(0f, 8f);
            spriteB.AddLoop("light", "light_b", 0.08f);
            spriteB.AddLoop("dark", "dark_b", 0.08f);
            spriteB.Play("light");
            Add(edgeSprite = new Sprite(GFX.Game, Directory + "/"));
            edgeSprite.Position = new Vector2(0f, data.Height - 8f);
            edgeSprite.AddLoop("light", "light_edge", 0.08f);
            edgeSprite.AddLoop("dark", "dark_edge", 0.08f);
            edgeSprite.Play("light");
            AllowStaticMovers = false;
            Depth = -1;
        }

        public static void Load()
        {
            On.Celeste.Player.Update += onPlayerUpdate;
            On.Celeste.Player.ClimbJump += onPlayerClimbJump;
        }

        public static void Unload()
        {
            On.Celeste.Player.Update -= onPlayerUpdate;
            On.Celeste.Player.ClimbJump -= onPlayerClimbJump;
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            List<Entity> vines = self.Scene.Tracker.GetEntities<ClimbableVine>().ToList();
            foreach (ClimbableVine vine in vines)
            {
                if ((!string.IsNullOrEmpty(vine.flag) && !self.SceneAs<Level>().Session.GetFlag(vine.flag)) || XaphanModule.ModSession.LightMode != XaphanModuleSession.LightModes.Light)
                {
                    vine.Collidable = true;
                }
                else if (((self.Right <= vine.Left || self.Left >= vine.Right) && self.Bottom >= vine.Top + 1 && self.Top <= vine.Bottom) &&  Input.Grab.Check && self.Holding == null && vine.noCollideDelay <= 0f)
                {
                    if (Input.Jump.Pressed)
                    {
                        vine.playerStamina = self.Stamina;
                        vine.Collidable = false;
                        vine.noCollideDelay = 0.1f;
                        vine.Add(new Coroutine(vine.CollideDelayRoutine()));
                    }
                    else
                    {
                        vine.Collidable = true;
                    }
                }
                else
                {
                    vine.Collidable = false;
                }
            }
            orig(self);
        }

        private static void onPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);
            foreach (ClimbableVine vine in self.Scene.Tracker.GetEntities<ClimbableVine>())
            {
                if (self.CollideRect(new Rectangle((int)vine.Position.X + 3, (int)vine.Position.Y, 2, (int)vine.Height), self.Position + Vector2.UnitX * 2f * (float)self.Facing))
                {
                    if (vine != null && self.Facing == Facings.Left ? Input.Aim.Value.SafeNormalize().X < 0 : Input.Aim.Value.SafeNormalize().X > 0)
                    {
                        self.Stamina = vine.playerStamina;
                    }
                    break;
                }
            }
            
        }

        private void onPlayer(Player player)
        {
            if ((!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode != XaphanModuleSession.LightModes.Light)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        public override void Update()
        {
            base.Update();
            bool playerNotDead = SceneAs<Level>().Tracker.GetEntity<Player>() != null && !SceneAs<Level>().Tracker.GetEntity<Player>().Dead;
            if ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
            {
                spriteA.Play("light");
                spriteB.Play("light");
                edgeSprite.Play("light");
                Collider.Height = fullHeight;
            }
            else
            {
                spriteA.Play("dark");
                spriteB.Play("dark");
                edgeSprite.Play("dark");
                Collider.Height = fullHeight - 2f;
            }
        }

        private IEnumerator CollideDelayRoutine()
        {
            while (noCollideDelay > 0)
            {
                noCollideDelay -= Engine.DeltaTime;
                yield return null;
            }
        }
        public override void Render()
        {
            float num = Height / 8f - 1f;
            int i = 0;
            while (i < num)
            {
                spriteA.RenderPosition = Position + new Vector2(0f, i * 8);
                spriteA.DrawOutline();
                i = i + 2;
            }
            i = 0;
            while (i < num - 1)
            {
                spriteB.RenderPosition = Position + new Vector2(0f, 8f) + new Vector2(0f, i * 8);
                spriteB.DrawOutline();
                i = i + 2;
            }
            edgeSprite.RenderPosition = Position + new Vector2(0f, Height - 5f);
            edgeSprite.DrawOutline();
            i = 0;
            while (i < num)
            {
                spriteA.RenderPosition = Position + new Vector2(0f, i * 8);
                spriteA.Render();
                i = i + 2;
            }
            i = 0;
            while (i < num - 1)
            {
                spriteB.RenderPosition = Position + new Vector2(0f, 8f) + new Vector2(0f, i * 8);
                spriteB.Render();
                i = i + 2;
            }
            edgeSprite.RenderPosition = Position + new Vector2(0f, Height - 5f);
            edgeSprite.Render();
        }
    }
}
