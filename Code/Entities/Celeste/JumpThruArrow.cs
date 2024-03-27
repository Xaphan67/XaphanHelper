using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    class JumpThruArrow : JumpThru
    {
        private Sprite sprite;

        public bool inWall;

        public bool destroyed;

        private bool destroyImmediate;

        public Coroutine DestroyRoutine = new();

        public Coroutine ShootRoutine = new();

        public string side;

        private Solid attachedSolid;

        StaticMover staticMover;

        public JumpThruArrow(Vector2 position, string side) : base(position, 4, false)
        {
            this.side = side;
            inWall = true;
            if (side == "Right")
            {
                Collider = new Hitbox(22f, 4f, 3f, 3f);
            }
            else if (side == "Left")
            {
                Collider = new Hitbox(22f, 4f, 5f, 3f);
            }
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/ArrowTrap" + "/"));
            sprite.AddLoop("arrow", "arrow", 0.08f, 0);
            sprite.Add("vibrating", "arrow", 0.04f, 1, 2, 3, 0, 1, 2, 3, 0);
            sprite.Origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Play("vibrating");
            if (side == "Right")
            {
                sprite.Position = new Vector2(15f, 4f);
            }
            else if (side == "Left")
            {
                sprite.FlipX = true;
                sprite.Position = new Vector2(15f, 4f);
            }
            Depth = -1;
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitX));
            staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX));
            staticMover.OnDisable = OnDisable;
            Add(staticMover);
        }

        private void OnDisable()
        {
            destroyImmediate = true;
            Add(new Coroutine(Destroy()));
        }

        private void OnMove(Vector2 amount)
        {
            if (Collidable)
            {
                MoveV(amount.Y);
                MoveH(amount.X);
            }
            else
            {
                Position += amount;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (side == "Right")
            {
                attachedSolid = CollideFirst<Solid>(Position + Vector2.UnitX * 2);
            }
            else if (side == "Left")
            {
                attachedSolid = CollideFirst<Solid>(Position - Vector2.UnitX * 2);
            }
            if (attachedSolid != null && attachedSolid.GetType() != typeof(SolidTiles) && staticMover != null)
            {
                DynData<Solid> solidData = new(attachedSolid);
                List<StaticMover> staticMovers = solidData.Get<List<StaticMover>>("staticMovers");
                if (!staticMovers.Contains(staticMover))
                {
                    staticMover.OnMove = OnMove;
                    staticMovers.Add(staticMover);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null)
            {
                Collidable = false;
            }
            else
            {
                if (CollideCheck(player) && player.Bottom > Top && player.Bottom <= Bottom && player.Speed.Y > 0 && (side == "Right" || side == "Left") && !destroyed && inWall)
                {
                    player.MoveV(-(player.Bottom - Top));
                }
            }
            if (inWall && !DestroyRoutine.Active)
            {
                Add(DestroyRoutine = new Coroutine(Destroy()));
            }
            if (attachedSolid != null && attachedSolid.GetType() == typeof(FlagTempleGate))
            {
                FlagTempleGate gate = attachedSolid as FlagTempleGate;
                if (!gate.Collidable)
                {
                    destroyImmediate = true;
                }
            }
        }

        [Pooled]
        private class Debris : Actor
        {
            private Image sprite;

            private Vector2 speed;

            private bool shaking;

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
                    if (speed.Y > 0f && speed.Y < 40f)
                    {
                        speed.Y = 0f;
                    }
                    else
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public Debris Init(Vector2 position)
            {
                Collidable = true;
                Position = position;
                speed = position.SafeNormalize(-30f + Calc.Random.NextFloat(60f));
                directory = "particles/shard";
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(directory))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                alpha = 1f;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose(1, -1);
                fadeLerp = 0f;
                lifeTimer = Calc.Random.Range(0.6f, 2.6f);
                return this;
            }

            public override void Update()
            {
                List<Entity> arrows = SceneAs<Level>().Tracker.GetEntities<Arrow>().ToList();
                foreach (Arrow arrow in arrows)
                {
                    arrow.Collidable = false;
                }
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
                sprite.Color = Calc.HexToColor("404000") * alpha;
            }
        }

        private IEnumerator Destroy()
        {
            float timer = 1.3f;
            while (timer > 0f && !destroyImmediate)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            if (!destroyImmediate)
            {
                StartShaking(0.7f);
            }
            timer = 0.4f;
            while (timer > 0f && !destroyImmediate)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            Visible = false;
            destroyed = true;
            Collidable = false;
            // Audio.Play("event:/game/general/platform_disintegrate", Center);
            for (int i = 0; i <= 5; i++)
            {
                Vector2 vector = (side == "Right" || side == "Left") ? new(10 + i * 2f, 4f) : new(4f, 10 + i * 2f);
                Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector);
                Scene.Add(debris2);
            }
            yield return 0.3f;
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            sprite.DrawSubrect(Shake, new Rectangle(0, 0, (int)sprite.Width, 8));
        }
    }

}
