using Microsoft.Xna.Framework;
using System;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    public class RotateEnemy : Enemy
    {
        private float RotationTime;

        public bool Moving;

        private Vector2 center;

        private float rotationPercent;

        private float length;

        private bool fallOutOfScreen;

        private bool fixAngle;

        private Vector2 startCenter;

        private Vector2 startPosition;

        public float Angle
        {
            get
            {
                if (!fixAngle)
                {
                    return MathHelper.Lerp(4.712389f, -(float)Math.PI / 2f, Easer(rotationPercent));
                }
                return MathHelper.Lerp((float)Math.PI, -(float)Math.PI, Easer(rotationPercent));
            }
        }

        public bool Clockwise { get; private set; }

        public RotateEnemy(EntityData data, Vector2 offset) : base(data, offset)
        {
            Moving = true;
            center = data.Nodes[0] + offset;
            Clockwise = data.Bool("clockwise");
            RotationTime = data.Float("rotationTime", 1.8f);
            StaticMover staticMover = new StaticMover();
            staticMover.SolidChecker = (Solid s) => s.CollidePoint(center);
            staticMover.JumpThruChecker = (JumpThru jt) => jt.CollidePoint(center);
            staticMover.OnMove = delegate (Vector2 v)
            {
                center += v;
                Position += v;
            };
            staticMover.OnDestroy = delegate
            {
                fallOutOfScreen = true;
            };
            Add(staticMover);
            float angleRadians = Calc.Angle(center, Position);
            angleRadians = Calc.WrapAngle(angleRadians);
            rotationPercent = EaserInverse(Calc.Percent(angleRadians, -(float)Math.PI / 2f, 4.712389f));
            length = (Position - center).Length();
            Position = center + Calc.AngleToVector(Angle, length);
            startCenter = data.Nodes[0] + offset;
            startPosition = data.Position + offset;
        }

        private float Easer(float v)
        {
            return v;
        }

        private float EaserInverse(float v)
        {
            return v;
        }

        public override void Update()
        {
            base.Update();
            if (Moving && !Freezed)
            {
                if (Clockwise)
                {
                    rotationPercent -= Engine.DeltaTime / RotationTime;
                    rotationPercent += 1f;
                }
                else
                {
                    rotationPercent += Engine.DeltaTime / RotationTime;
                }
                rotationPercent %= 1f;
                Position = center + Calc.AngleToVector(Angle, length);
            }
            if (fallOutOfScreen)
            {
                center.Y += 160f * Engine.DeltaTime;
                if (Y > ((Scene as Level).Bounds.Bottom + 32))
                {
                    RemoveSelf();
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            fixAngle = (Scene as Level).Session.Area.GetLevelSet() != "Celeste";
            if (fixAngle)
            {
                float angleRadians = Calc.Angle(startCenter, startPosition);
                angleRadians = Calc.WrapAngle(angleRadians);
                rotationPercent = EaserInverse(Calc.Percent(angleRadians, (float)Math.PI, -(float)Math.PI));
                Position = center + Calc.AngleToVector(Angle, length);
            }
        }
    }
}
