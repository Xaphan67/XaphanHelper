using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Arrow")]
    class Arrow : Solid
    {
        private Sprite sprite;

        public float arrowSpeed;

        public bool inWall;

        public bool canShoot;

        public bool destroyed;

        public Coroutine DestroyRoutine = new();

        public ArrowTrap sourceTrap = null;

        public string side;

        public float noCollideDelay;

        //public StaticMover staticMover;

        public Arrow(EntityData data, Vector2 position, ArrowTrap trap, string side) : base(data.Position + position, data.Width, data.Height, false)
        {
            sourceTrap = trap;
            this.side = side;
            inWall = sourceTrap == null;
            if (sourceTrap != null)
            {
                noCollideDelay = 0.01f;
                Add(new Coroutine(CollideDelayRoutine()));
            }
            Init();
        }

        private IEnumerator CollideDelayRoutine()
        {
            while (noCollideDelay > 0)
            {
                noCollideDelay -= Engine.DeltaTime;
                yield return null;
            }
        }

        public void Init()
        {
            if (side == "Right")
            {
                Collider = new Hitbox(22f, 3f, 3f, 3f);
            }
            else if (side == "Left")
            {
                Collider = new Hitbox(22f, 3f, 5f, 3f);
            }
            else if (side == "Top")
            {
                Collider = new Hitbox(2f, 22f, 3f, 5f);
            }
            else if (side == "Bottom")
            {
                Collider = new Hitbox(2f, 22f, 3f, 3f);
            }
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/ArrowTrap" + "/"));
            sprite.AddLoop("arrow", "arrow", 0.08f);
            sprite.Origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Play("arrow");
            if (side == "Right")
            {
                sprite.Position = new Vector2(15f, 4f);
            }
            else if (side == "Left")
            {
                sprite.FlipX = true;
                sprite.Position = new Vector2(15f, 4f);
            }
            else if (side == "Top")
            {
                sprite.Rotation = -(float)Math.PI / 2f;
                sprite.Position = new Vector2(4f, 15f);
            }
            else if (side == "Bottom")
            {
                sprite.Rotation = (float)Math.PI / 2f;
                sprite.Position = new Vector2(4f, 15f);
            }
            Depth = -100;

            /*staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            //staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitX));
            staticMover.SolidChecker = IsRiding;
            staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX));
            Add(staticMover);*/
        }

        /*private bool IsRiding(Solid solid)
        {
            return Scene.CollideCheck(new Rectangle((int)(X + Width), (int)Y, 8, (int)Width), solid);
        }*/

        public static void Load()
        {
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null || !inWall)
            {
                Collidable = false;
            }
            if (inWall && !DestroyRoutine.Active)
            {
                Add(DestroyRoutine = new Coroutine(Destroy()));
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
            yield return 1.3f;
            StartShaking(0.7f);
            yield return 0.4f;
            Visible = false;
            destroyed = true;
            Collidable = false;
            Audio.Play("event:/game/general/platform_disintegrate", Center);
            for (int i = 0; i <= 5; i++)
            {
                Vector2 vector = (side == "Right" || side == "Left") ? new(10 + i * 2f, 4f) : new(4f, 10 + i * 2f);
                Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector);
                Scene.Add(debris2);
            }
            yield return 0.3f;
            RemoveSelf();
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            List<Entity> arrows = self.Scene.Tracker.GetEntities<Arrow>().ToList();
            foreach (Arrow arrow in arrows)
            {
                if (arrow.side == "Right" || arrow.side == "Left")
                {
                    if (self.Bottom <= arrow.Top && !arrow.destroyed && arrow.inWall)
                    {
                        arrow.Collidable = true;
                    }
                    else
                    {
                        arrow.Collidable = false;
                    }
                }
                else
                {
                    if ((self.Right <= arrow.Left || self.Left >= arrow.Right) && !arrow.destroyed && arrow.inWall && Input.Grab.Check && self.Holding == null && arrow.noCollideDelay <= 0f)
                    {
                        if (Input.Jump.Pressed)
                        {
                            arrow.Collidable = false;
                            arrow.noCollideDelay = 0.1f;
                            arrow.Add(new Coroutine(arrow.CollideDelayRoutine()));
                        }
                        else
                        {
                            arrow.Collidable = true;
                        }
                    }
                    else
                    {
                        arrow.Collidable = false;
                    }
                }
                if (!arrow.inWall)
                {
                    if ((arrow.side == "Right" && self.CollideRect(new Rectangle((int)arrow.Right - 7, (int)arrow.Top - 1, 7, 4))) || (arrow.side == "Left" && self.CollideRect(new Rectangle((int)arrow.Left, (int)arrow.Top - 1, 7, 4))))
                    {
                        self.Die(new Vector2(0f, -1f));
                    }
                    else if ((arrow.side == "Top" && self.CollideRect(new Rectangle((int)arrow.Position.X + 2, (int)arrow.Position.Y, 4, 7))) || (arrow.side == "Bottom" && self.CollideRect(new Rectangle((int)arrow.Position.X + 2, (int)arrow.Position.Y + 18, 4, 7))))
                    {
                        self.Die(new Vector2(0f, -1f));
                    }
                }

            }
            orig(self);
        }

        public IEnumerator Shoot()
        {
            arrowSpeed = 460f;
            while (CollideCheck(sourceTrap) || !CollideCheck<Solid, Arrow, FlagDashSwitch>() || noCollideDelay > 0f)
            {
                if (side == "Right")
                {
                    MoveTo(Position + Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                }
                else if (side == "Left")
                {
                    MoveTo(Position - Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                }
                else if (side == "Top")
                {
                    MoveTo(Position - Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                }
                else if (side == "Bottom")
                {
                    MoveTo(Position + Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                }
                foreach (Entity entity in Scene.Tracker.GetEntities<FlagDashSwitch>())
                {
                    FlagDashSwitch flagDashSwitch = (FlagDashSwitch)entity;
                    if (CollideCheck(flagDashSwitch))
                    {
                        if (side == "Right")
                        {
                            flagDashSwitch.OnDashCollide(null, Vector2.UnitX);
                        }
                        else if (side == "Left")
                        {
                            flagDashSwitch.OnDashCollide(null, -Vector2.UnitX);
                        }
                        else if (side == "Top")
                        {
                            flagDashSwitch.OnDashCollide(null, -Vector2.UnitY);
                        }
                        else if (side == "Bottom")
                        {
                            flagDashSwitch.OnDashCollide(null, Vector2.UnitY);
                        }
                        break;
                    }
                }
                yield return null;
            }
            Visible = false;
            if (side == "Right")
            {

                while (CollideCheck<Solid, ArrowTrap, Arrow>())
                {
                    Position.X -= 1;
                }
                Scene.Add(new Arrow(new EntityData(), Position, null, "Right"));
            }
            else if (side == "Left")
            {
                while (CollideCheck<Solid, ArrowTrap, Arrow>())
                {
                    Position.X += 1;
                }
                Scene.Add(new Arrow(new EntityData(), Position, null, "Left"));
            }
            else if (side == "Top")
            {
                while (CollideCheck<Solid, ArrowTrap, Arrow>())
                {
                    Position.Y += 1;
                }
                Scene.Add(new Arrow(new EntityData(), Position, null, "Top"));
            }
            else if (side == "Bottom")
            {
                while (CollideCheck<Solid, ArrowTrap, Arrow>())
                {
                    Position.Y -= 1;
                }
                Scene.Add(new Arrow(new EntityData(), Position, null, "Bottom"));
            }
            RemoveSelf();
        }

        public IEnumerator Reload()
        {
            if (sourceTrap != null)
            {
                arrowSpeed = 115f;
                if (side == "Right")
                {
                    while (Math.Abs((int)(sourceTrap.Left - Left + 3)) > 11)
                    {
                        MoveTo(Position + Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                        yield return null;
                    }
                }
                else if (side == "Left")
                {
                    while (Math.Abs((int)(sourceTrap.Right - Right - 3)) > 11)
                    {
                        MoveTo(Position - Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                        yield return null;
                    }
                }
                else if (side == "Top")
                {
                    while (Math.Abs((int)(sourceTrap.Bottom - Bottom - 3)) > 11)
                    {
                        MoveTo(Position - Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                        yield return null;
                    }
                }
                else if (side == "Bottom")
                {
                    while (Math.Abs((int)(sourceTrap.Top - Top + 3)) > 11)
                    {
                        MoveTo(Position + Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                        yield return null;
                    }
                }
                canShoot = true;
            }
        }

        public override void Render()
        {
            if (sourceTrap != null)
            {
                if (side == "Right")
                {
                    int value = (int)(sourceTrap.Left - Left + 3);
                    if (value < 0)
                    {
                        value = 0;
                    }
                    sprite.DrawSubrect(Vector2.UnitX * value + Shake, new Rectangle(value, 0, (int)(sprite.Width - value), 8));
                }
                else if (side == "Left")
                {
                    int value = (int)(sourceTrap.Right - Left + 5);
                    if (value > sprite.Width)
                    {
                        value = (int)sprite.Width;
                    }
                    sprite.DrawSubrect(Shake, new Rectangle((int)(sprite.Width - value), 0, value, 8));
                }
                else if (side == "Top")
                {
                    int value = (int)(sourceTrap.Bottom - Top + 5);
                    if (value > sprite.Width)
                    {
                        value = (int)sprite.Width;
                    }
                    sprite.DrawSubrect(Vector2.UnitY * -(sprite.Width - value) + Shake, new Rectangle((int)(sprite.Width - value), 0, value, 8));
                }
                else if (side == "Bottom")
                {
                    int value = (int)(sourceTrap.Top - Top + 3);
                    if (value > sprite.Width)
                    {
                        value = (int)sprite.Width;
                    }
                    sprite.DrawSubrect(Vector2.UnitY * value + Shake, new Rectangle(value, 0, (int)(sprite.Width - value), 8));
                }
            }
            else
            {
                base.Render();
                sprite.DrawSubrect(Shake, new Rectangle(0, 0, (int)sprite.Width, 8));
            }
        }
    }

}
