using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public ClimbableVine(EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, safe: false)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(2f, data.Height - 3f, 3f, 0f);
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
        }

        public static void Unload()
        {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            List<Entity> vines = self.Scene.Tracker.GetEntities<ClimbableVine>().ToList();
            foreach (ClimbableVine vine in vines)
            {
                if (!self.SceneAs<Level>().Session.GetFlag(vine.flag))
                {
                    vine.Collidable = true;
                }
                else if (((self.Right <= vine.Left || self.Left >= vine.Right) && self.Bottom >= vine.Top + 1 && self.Top <= vine.Bottom) &&  Input.Grab.Check && self.Holding == null && vine.noCollideDelay <= 0f)
                {
                    if (Input.Jump.Pressed)
                    {
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

        private void onPlayer(Player player)
        {
            if (!SceneAs<Level>().Session.GetFlag(flag))
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag))
            {
                spriteA.Play("light");
                spriteB.Play("light");
                edgeSprite.Play("light");
            }
            else
            {
                spriteA.Play("dark");
                spriteB.Play("dark");
                edgeSprite.Play("dark");
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
