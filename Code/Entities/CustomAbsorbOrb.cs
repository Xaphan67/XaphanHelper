using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    public class CustomAbsorbOrb : Entity
    {
        public Entity AbsorbInto;

        public Vector2? AbsorbTarget;

        private SimpleCurve curve;

        private float duration;

        private float percent;

        private float consumeDelay;

        private float burstSpeed;

        private Vector2 burstDirection;

        private Vector2 burstScale;

        private float alpha = 1f;

        private Image sprite;

        private BloomPoint bloom;

        private Color color;

        private bool startAbsorb;

        private bool fade;

        public CustomAbsorbOrb(Vector2 position, Entity into = null, Vector2? absorbTarget = null, string color = null, float consumeDelay = 0.7f, bool randomiseConsumeDelay = true, string direction = "All", bool fade = true)
        {
            AbsorbInto = into;
            AbsorbTarget = absorbTarget;
            Position = position;
            Tag = Tags.FrozenUpdate;
            Depth = -2000000;
            this.consumeDelay = consumeDelay + (randomiseConsumeDelay ? Calc.Random.NextFloat() * 0.3f : 0f);
            if (!randomiseConsumeDelay)
            {
                startAbsorb = true;
            }
            this.fade = fade;
            burstSpeed = 80f + Calc.Random.NextFloat() * 40f;
            if (direction == "Top")
            {
                burstDirection = Calc.AngleToVector((1 - Calc.Random.NextFloat(0.5f)) * ((float)Math.PI * 2f), 1f);
            } else if (direction == "Bottom")
            {
                burstDirection = Calc.AngleToVector(Calc.Random.NextFloat(0.5f) * ((float)Math.PI * 2f), 1f);
            }
            else
            {
                burstDirection = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
            }
            Add(sprite = new Image(GFX.Game["collectables/heartGem/orb"]));
            sprite.CenterOrigin();
            this.color = Calc.HexToColor(string.IsNullOrEmpty(color) ? "ffffff" : color);
            sprite.Color = this.color;
            Add(bloom = new BloomPoint(1f, 16f));
        }

        public override void Update()
        {
            base.Update();
            Vector2 vector = Vector2.Zero;
            bool flag = false;
            if (AbsorbInto != null)
            {
                vector = AbsorbInto.Center;
                flag = AbsorbInto.Scene == null || (AbsorbInto is Player && (AbsorbInto as Player).Dead);
            }
            else if (AbsorbTarget.HasValue)
            {
                vector = AbsorbTarget.Value;
            }
            else
            {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    vector = entity.Center;
                }

                flag = entity == null || entity.Scene == null || entity.Dead;
            }

            if (flag)
            {
                Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                burstSpeed = Calc.Approach(burstSpeed, 800f, Engine.RawDeltaTime * 200f);
                sprite.Rotation = burstDirection.Angle();
                sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                sprite.Color = color * (alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime));
            }
            else if (consumeDelay > 0f || startAbsorb)
            {
                if (!startAbsorb)
                {
                    Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                    burstSpeed = Calc.Approach(burstSpeed, 0f, Engine.RawDeltaTime * 120f);
                    sprite.Rotation = burstDirection.Angle();
                    sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                    consumeDelay -= Engine.RawDeltaTime;
                }
                else
                {
                    sprite.Scale = Vector2.Zero;
                }
                if (consumeDelay <= 0f)
                {
                    startAbsorb = false;
                    Vector2 position = Position;
                    Vector2 vector2 = vector;
                    Vector2 vector3 = (position + vector2) / 2f;
                    Vector2 vector4 = (vector2 - position).SafeNormalize().Perpendicular() * (position - vector2).Length() * (0.05f + Calc.Random.NextFloat() * 0.45f);
                    float value = vector2.X - position.X;
                    float value2 = vector2.Y - position.Y;
                    if ((Math.Abs(value) > Math.Abs(value2) && Math.Sign(vector4.X) != Math.Sign(value)) || (Math.Abs(value2) > Math.Abs(value2) && Math.Sign(vector4.Y) != Math.Sign(value2)))
                    {
                        vector4 *= -1f;
                    }

                    curve = new SimpleCurve(position, vector2, vector3 + vector4);
                    duration = 0.3f + Calc.Random.NextFloat(0.25f);
                    burstScale = sprite.Scale;
                }
            }
            else
            {
                curve.End = vector;
                if (percent >= 1f)
                {
                    RemoveSelf();
                }

                percent = Calc.Approach(percent, 1f, Engine.RawDeltaTime / duration);
                float num = Ease.CubeIn(percent);
                Position = curve.GetPoint(num);
                float num2 = Calc.YoYo(num) * curve.GetLengthParametric(10);
                if (!fade && percent >= 0.98f)
                {
                    sprite.Visible = false;
                }
                sprite.Scale = new Vector2(Math.Min(2f, 0.5f + num2 * 0.02f), Math.Max(0.05f, 0.5f - num2 * 0.004f));
                if (fade)
                {
                    sprite.Color = color * (1f - num);
                }
                sprite.Rotation = Calc.Angle(Position, curve.GetPoint(Ease.CubeIn(percent + 0.01f)));
            }
        }
    }
}