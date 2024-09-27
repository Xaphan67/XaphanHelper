using System;
using System.IO;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ThornBarrier")]
    class ThornBarrier : Entity
    {
        private class Border : Entity
        {
            private Entity drawing = new();

            public Border(Entity parent)
            {
                drawing = parent;
                Depth = parent.Depth + 2;
            }

            public override void Render()
            {
                if (drawing.Visible)
                {
                    DrawBorder(drawing);
                }
            }

            private void DrawBorder(Entity entity)
            {
                if (entity != null)
                {
                    foreach (Component component in entity.Components)
                    {
                        Sprite sprite = component as Sprite;
                        if (sprite != null)
                        {
                            Color color = sprite.Color;
                            Vector2 position = sprite.Position;
                            sprite.Color = Color.Black;
                            sprite.Position = position + new Vector2(0f, -1f);
                            sprite.Render();
                            sprite.Position = position + new Vector2(0f, 1f);
                            sprite.Render();
                            sprite.Position = position + new Vector2(-1f, 0f);
                            sprite.Render();
                            sprite.Position = position + new Vector2(1f, 0f);
                            sprite.Render();
                            sprite.Color = color;
                            sprite.Position = position;
                        }
                    }
                }
            }
        }

        [Pooled]
        public class ThornDebris : Actor
        {
            private Image image;

            private float percent;

            private float duration;

            private Vector2 speed;

            private Collision collideH;

            private Collision collideV;

            public ThornDebris() : base(Vector2.Zero)
            {
                Depth = -9990;
                Collider = new Hitbox(2f, 2f, -1f, -1f);
                collideH = OnCollideH;
                collideV = OnCollideV;
                image = new Image(GFX.Game["danger/XaphanHelper/ThornBarrier/debris"]);
                image.CenterOrigin();
                Add(image);
            }

            private void Init(Vector2 position)
            {
                Position = position;
                image.Scale = Vector2.One;
                percent = 0f;
                duration = Calc.Random.Range(0.25f, 1f);
                speed = Calc.AngleToVector(Calc.Random.NextAngle(), Calc.Random.Range(30, 100));
            }

            public override void Update()
            {
                speed.X = Calc.Clamp(speed.X, -100000f, 100000f);
                speed.Y = Calc.Clamp(speed.Y, -100000f, 100000f);
                base.Update();
                if (percent > 1f)
                {
                    RemoveSelf();
                    return;
                }

                percent += Engine.DeltaTime / duration;
                speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 20f);
                speed.Y += 200f * Engine.DeltaTime;

                if (speed.Length() > 0f)
                {
                    image.Rotation = speed.Angle();
                }

                image.Scale = Vector2.One * Calc.ClampedMap(percent, 0.8f, 1f, 1f, 0f);
                image.Scale.X *= Calc.ClampedMap(speed.Length(), 0f, 400f, 1f, 2f);
                image.Scale.Y *= Calc.ClampedMap(speed.Length(), 0f, 400f, 1f, 0.2f);
                MoveH(speed.X * Engine.DeltaTime, collideH);
                MoveV(speed.Y * Engine.DeltaTime, collideV);
            }

            public override void Render()
            {
                Color color = image.Color;
                image.Color = Color.Black;
                image.Position = new Vector2(-1f, 0f);
                image.Render();
                image.Position = new Vector2(0f, -1f);
                image.Render();
                image.Position = new Vector2(1f, 0f);
                image.Render();
                image.Position = new Vector2(0f, 1f);
                image.Render();
                image.Position = Vector2.Zero;
                image.Color = color;
                base.Render();
            }

            private void OnCollideH(CollisionData hit)
            {
                speed.X *= -0.8f;
            }

            private void OnCollideV(CollisionData hit)
            {
                if (Math.Sign(speed.X) != 0)
                {
                    speed.X += Math.Sign(speed.X) * 5;
                }
                else
                {
                    speed.X += Calc.Random.Choose(-1, 1) * 5;
                }

                speed.Y *= -1.2f;
            }

            public static void Burst(Vector2 position, int count = 1)
            {
                for (int i = 0; i < count; i++)
                {
                    ThornDebris debris = Engine.Pooler.Create<ThornDebris>();
                    Vector2 vector = position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                    debris.Init(vector);
                    Engine.Scene.Add(debris);
                }
            }
        }

        private Border border;

        private Sprite Sprite;

        public string Directory;

        private string flag;

        private int group;

        EntityID ID;

        public ThornBarrier(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            Tag = Tags.TransitionUpdate;
            Collider = new Circle(10f);
            flag = data.Attr("flag");
            Directory = data.Attr("directory");
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "danger/XaphanHelper/ThornBarrier";
            }
            Add(Sprite = new Sprite(GFX.Game, Directory + "/"));
            Add(new PlayerCollider(onPlayer, Collider));
            Sprite.AddLoop("light", "light", 0.08f);
            Sprite.AddLoop("dark", "dark", 0.08f);
            Sprite.CenterOrigin();
            int rotationIndex = Calc.Random.Range(0, 8);
            switch (rotationIndex)
            {
                case 1:
                    Sprite.Rotation = (float)Math.PI / 4f;
                    break;
                case 2:
                    Sprite.Rotation = (float)Math.PI / 2;
                    break;
                case 3:
                    Sprite.Rotation = (float)Math.PI / 4f * 3f;
                    break;
                case 4:
                    Sprite.Rotation = (float)Math.PI;
                    break;
                case 5:
                    Sprite.Rotation = -(float)Math.PI / 4f;
                    break;
                case 6:
                    Sprite.Rotation = -(float)Math.PI / 2f;
                    break;
                case 7:
                    Sprite.Rotation = -(float)Math.PI / 4f * 3f;
                    break;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (ThornBarrier barrier in SceneAs<Level>().Tracker.GetEntities<ThornBarrier>())
            {
                if (barrier != this && CollideCheck(barrier))
                {
                    if (barrier.group != 0)
                    {
                        group = barrier.group;
                    }
                }
            }
            if (group == 0)
            {
                group = ID.ID;
            }
            Scene.Add(border = new Border(this));
        }

        private void onPlayer(Player player)
        {
            if ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
            else
            {
                if (player.StateMachine.State == Player.StDash || player.DashAttacking)
                {
                    Destroy();
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Tracker.GetEntity<Player>() != null && !SceneAs<Level>().Tracker.GetEntity<Player>().Dead)
            {
                if ((!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag)) || XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
                {
                    Sprite.Play("light");
                }
                else
                {
                    Sprite.Play("dark");
                }
            }
        }

        public void Destroy(bool all = true)
        {
            Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
            ThornDebris.Burst(Position, 8);
            if (all)
            {
                foreach (ThornBarrier barrier in SceneAs<Level>().Tracker.GetEntities<ThornBarrier>())
                {
                    if (barrier != this && barrier.group == group)
                    {
                        barrier.Destroy(false);
                    }
                }
            }
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (border != null && border.Scene == scene)
            {
                border.RemoveSelf();
            }
        }
    }
}
