using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/ThornBarrier")]
    class ThornBarrier : Entity
    {
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
        
        private Sprite Sprite;

        private string flag;

        public ThornBarrier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Circle(10f);
            flag = data.Attr("flag");
            Add(Sprite = new Sprite(GFX.Game, "danger/XaphanHelper/ThornBarrier/"));
            Add(new PlayerCollider(onPlayer, Collider));
            Sprite.AddLoop("light", "light", 0.08f);
            Sprite.AddLoop("dark", "dark", 0.08f);
            Sprite.CenterOrigin();
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

        public void Destroy()
        {
            Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
            ThornDebris.Burst(Position, 16);
            RemoveSelf();
        }
    }
}
