using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/ReactorGlass")]
    public class ReactorGlass : Solid
    {
        private Sprite Sprite;

        public int TimesDashed;

        [Pooled]
        private class Debris : Actor
        {
            private Image sprite;

            private Vector2 speed;

            private bool shaking;

            private bool firstHit;

            private float alpha;

            private Collision onCollideH;

            private Collision onCollideV;

            private float spin;

            private float lifeTimer;

            private float fadeLerp;

            private string directory;

            public Debris() : base(Vector2.Zero)
            {
                Tag = Tags.TransitionUpdate;
                Collider = new Hitbox(4f, 4f, -2f, -2f);

                onCollideH = delegate
                {
                    speed.X = (0f - speed.X) * 0.5f;
                };
                onCollideV = delegate
                {
                    if (firstHit || speed.Y > 50f)
                    {
                        Audio.Play("event:/game/06_reflection/fall_spike_smash", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
                    }
                    if (speed.Y > 0f && speed.Y < 40f)
                    {
                        speed.Y = 0f;
                    }
                    else
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                    firstHit = false;
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public Debris Init(Vector2 position, Vector2 center)
            {
                Collidable = true;
                Position = position;
                speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
                directory = "particles/shard";
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(directory))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose(1, -1);
                fadeLerp = 0f;
                lifeTimer = Calc.Random.Range(0.6f, 2.6f);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (Collidable)
                {
                    speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        speed.Y += 400f * Engine.DeltaTime;
                    }
                    MoveH(speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                }
                if (shaking && Scene.OnInterval(0.05f))
                {
                    sprite.X = -1 + Calc.Random.Next(3);
                    sprite.Y = -1 + Calc.Random.Next(3);
                }
                if ((Scene as Level).Transitioning)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                    sprite.Color = Color.White * alpha;
                }
                sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
                if (lifeTimer > 0f)
                {
                    lifeTimer -= Engine.DeltaTime;
                }
                else if (alpha > 0f)
                {
                    alpha -= 4f * Engine.DeltaTime;
                    if (alpha <= 0f)
                    {
                        RemoveSelf();
                    }
                }
                if (fadeLerp < 1f)
                {
                    fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
                }
                sprite.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
            }
        }

        public ReactorGlass(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = 32;
            Collider = new Hitbox(41f, 62f, -21f, -31f);
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/ReactorGlass/"));
            Sprite.AddLoop("idle", "idle", 0);
            Sprite.AddLoop("fissured", "fissured", 0);
            Sprite.AddLoop("fissured_b", "fissured_b", 0);
            Sprite.AddLoop("broken", "broken", 0);
            Sprite.Position = (new Vector2(-24f, -32f));
            Sprite.Play("idle");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_reactor_glass_dashed_1"))
            {
                Sprite.Play("fissured");
                TimesDashed = 1;
            }
            else if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_reactor_glass_dashed_2"))
            {
                Sprite.Play("fissured_b");
                TimesDashed = 2;
            }
            else if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_reactor_glass_broken") || SceneAs<Level>().Session.GetFlag("Ch4_Escape_Complete"))
            {
                Break();
            }
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            TimesDashed++;
            if (TimesDashed == 1)
            {
                Sprite.Play("fissured");
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_reactor_glass_dashed_1");
            }
            else if (TimesDashed == 2)
            {
                Sprite.Play("fissured_b");
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                XaphanModule.ModSaveData.SavedFlags.Remove("Xaphan/0_reactor_glass_dashed_1");
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_reactor_glass_dashed_2");
            }
            else if (TimesDashed == 3)
            {
                List<Debris> debris = new();
                for (int i = 0; i <= 4; i++)
                {
                    for (int j = 0; j <= 3; j++)
                    {
                        Vector2 vector2 = new(i * 8 + 4f, j * 8 + 4f);
                        Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + new Vector2(-21f, -15f) + vector2, Center);
                        debris.Add(debris2);
                        Scene.Add(debris2);
                    }
                }
                Audio.Play("event:/game/general/wall_break_ice", Position);
                XaphanModule.ModSaveData.SavedFlags.Remove("Xaphan/0_reactor_glass_dashed_2");
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_reactor_glass_broken");
                Break();
            }
            return DashCollisionResults.Bounce;
        }

        private void Break()
        {
            SceneAs<Level>().Session.SetFlag("reactor_glass_broken", true);
            Sprite.Play("broken");
            Collidable = false;
            SceneAs<Level>().Add(new CrumbleWallOnRumble(Position + new Vector2(-21f, -31f), '0', 3f, 14f, false, false, new EntityID()));
            SceneAs<Level>().Add(new CrumbleWallOnRumble(Position + new Vector2(17f, -31f), '0', 3f, 14f, false, false, new EntityID()));
            SceneAs<Level>().Add(new CrumbleWallOnRumble(Position + new Vector2(-21f, 17f), '0', 3f, 14f, false, false, new EntityID()));
            SceneAs<Level>().Add(new CrumbleWallOnRumble(Position + new Vector2(17f, 17f), '0', 3f, 14f, false, false, new EntityID()));
        }
    }
}
