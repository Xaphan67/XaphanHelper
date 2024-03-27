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
    class Arrow : Solid
    {
        private Sprite sprite;

        public float arrowSpeed;

        public bool inWall;

        public bool canShoot;

        public bool destroyed;

        private bool destroyImmediate;

        public Coroutine DestroyRoutine = new();

        public Coroutine ShootRoutine = new();

        public ArrowTrap sourceTrap = null;

        public string side;

        public float noCollideDelay;

        private Solid attachedSolid;

        private bool collideHole;

        private ArrowHole arrowHole;

        StaticMover staticMover;

        private Level Level;

        public Arrow(Vector2 position, ArrowTrap trap, string side) : base(position, 4, 4, false)
        {
            sourceTrap = trap;
            this.side = side;
            inWall = sourceTrap == null;
            if (sourceTrap != null)
            {
                noCollideDelay = 0.01f;
                Add(new Coroutine(CollideDelayRoutine()));
            }
            if (side == "Right")
            {
                Collider = new Hitbox(22f, sourceTrap != null ? 2f : 4f, 3f, 3f);
            }
            else if (side == "Left")
            {
                Collider = new Hitbox(22f, sourceTrap != null ? 2f : 4f, 5f, 3f);
            }
            else if (side == "Top")
            {
                Collider = new Hitbox(2f, 22f, 3f, 5f);
            }
            else if (side == "Bottom")
            {
                Add(new LedgeBlocker());
                Collider = new Hitbox(2f, 22f, 3f, 3f);
            }
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/ArrowTrap" + "/"));
            sprite.AddLoop("arrow", "arrow", 0.08f, 0);
            sprite.Add("vibrating", "arrow", 0.04f, 1, 2, 3, 0, 1, 2, 3, 0);
            sprite.Origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            if (sourceTrap != null)
            {
                sprite.Play("arrow");
            }
            else
            {
                sprite.Play("vibrating");
            }
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
            Depth = -1;
            if (sourceTrap == null)
            {
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

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
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
            else if (side == "Top")
            {
                attachedSolid = CollideFirst<Solid>(Position - Vector2.UnitY * 2);
            }
            else if (side == "Bottom")
            {
                attachedSolid = CollideFirst<Solid>(Position + Vector2.UnitY * 2);
            }
            if (attachedSolid != null && attachedSolid.GetType() != typeof(SolidTiles) && staticMover != null)
            {
                DynData<Solid> solidData = new(attachedSolid);
                List<StaticMover> staticMovers = solidData.Get<List<StaticMover>>("staticMovers");
                if (!staticMovers.Contains(staticMover)) {
                    staticMover.OnMove = OnMove;
                    staticMovers.Add(staticMover);
                }
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

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            List<Entity> arrows = self.Scene.Tracker.GetEntities<Arrow>().ToList();
            foreach (Arrow arrow in arrows)
            {
                if (arrow.side == "Right" || arrow.side == "Left")
                {
                    if ((self.Bottom <= arrow.Top || arrow.HasRider()) && !arrow.destroyed && arrow.inWall)
                    {
                        arrow.Collidable = true;
                    }
                    else
                    {
                        arrow.Collidable = false;
                        if (self.Bottom > arrow.Top && self.Top + 8 < arrow.Bottom && self.Left >= arrow.Left && self.Right <= arrow.Right && self.Speed.Y >= 0 && !arrow.destroyed && arrow.inWall)
                        {
                            self.MoveV(-(self.Bottom - arrow.Top));
                        }
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
            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
            if (!inWall)
            {
                while ((side == "Right" && Left < sourceTrap.Left) || (side == "Left" && Right > sourceTrap.Right) || (side == "Top" && Bottom > sourceTrap.Bottom) || (side == "Bottom" && Top < sourceTrap.Top))
                {
                    MoveArrow();
                    yield return null;
                }
            }
            while (CollideCheck(sourceTrap) || CollideCheck<ArrowHole>() || !CollideCheck<Solid, Arrow, FlagDashSwitch>() || noCollideDelay > 0f)
            {
                MoveArrow();
                foreach (FlagDashSwitch flagDashSwitch in Scene.Tracker.GetEntities<FlagDashSwitch>())
                {
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
                if (CollideCheck<ArrowHole>() && !collideHole)
                {
                    collideHole = true;
                    Depth = -15001;
                    if (side == "Right")
                    {
                        arrowHole = CollideFirst<ArrowHole>(Position + Vector2.UnitX * 2);
                    }
                    else if (side == "Left")
                    {
                        arrowHole = CollideFirst<ArrowHole>(Position - Vector2.UnitX * 2);
                    }
                    else if (side == "Top")
                    {
                        arrowHole = CollideFirst<ArrowHole>(Position - Vector2.UnitY * 2);
                    }
                    else if (side == "Bottom")
                    {
                        arrowHole = CollideFirst<ArrowHole>(Position + Vector2.UnitY * 2);
                    }
                }
                yield return null;
            }
            Visible = false;
            if (!collideHole)
            {
                Audio.Play("event:/game/00_prologue/car_down");
                if (side == "Right")
                {
                    while (CollideCheck<Solid, ArrowTrap, Arrow>())
                    {
                        Position.X -= 1;
                    }
                    Scene.Add(new JumpThruArrow(Position, "Right"));
                }
                else if (side == "Left")
                {
                    while (CollideCheck<Solid, ArrowTrap, Arrow>())
                    {
                        Position.X += 1;
                    }
                    Scene.Add(new JumpThruArrow(Position, "Left"));
                }
                else if (side == "Top")
                {
                    while (CollideCheck<Solid, ArrowTrap, Arrow>())
                    {
                        Position.Y += 1;
                    }
                    Scene.Add(new Arrow(Position, null, "Top"));
                }
                else if (side == "Bottom")
                {
                    while (CollideCheck<Solid, ArrowTrap, Arrow>())
                    {
                        Position.Y -= 1;
                    }
                    Scene.Add(new Arrow(Position, null, "Bottom"));
                }
            }
            RemoveSelf();
        }

        private void MoveArrow()
        {
            if (side == "Right")
            {
                MoveTo(Position + Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                if (Left - 8f > Level.Bounds.Right)
                {
                    if (ShootRoutine.Active)
                    {
                        ShootRoutine.Cancel();
                    }
                    RemoveSelf();
                }
            }
            else if (side == "Left")
            {
                MoveTo(Position - Vector2.UnitX * arrowSpeed * Engine.DeltaTime, Vector2.UnitX * arrowSpeed);
                if (Right + 8f < Level.Bounds.Left)
                {
                    if (ShootRoutine.Active)
                    {
                        ShootRoutine.Cancel();
                    }
                    RemoveSelf();
                }
            }
            else if (side == "Top")
            {
                MoveTo(Position - Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                if (Bottom + 8f < Level.Bounds.Top)
                {
                    if (ShootRoutine.Active)
                    {
                        ShootRoutine.Cancel();
                    }
                    RemoveSelf();
                }
            }
            else if (side == "Bottom")
            {
                MoveTo(Position + Vector2.UnitY * arrowSpeed * Engine.DeltaTime, Vector2.UnitY * arrowSpeed);
                if (Top + 8f > Level.Bounds.Bottom)
                {
                    if (ShootRoutine.Active)
                    {
                        ShootRoutine.Cancel();
                    }
                    RemoveSelf();
                }
            }
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
                if (!collideHole)
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
                    if (side == "Right")
                    {
                        int value = (int)(arrowHole.Right - Left + 3);
                        if (value < 0)
                        {
                            value = 0;
                        }
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, value + 2, 8));
                    }
                    else if (side == "Left")
                    {
                        int value = (int)(Right - arrowHole.Right + 3);
                        if (value > sprite.Width)
                        {
                            value = (int)sprite.Width;
                        }
                        sprite.DrawSubrect(Vector2.UnitX * ((int)sprite.Width - value - 3), new Rectangle(0, 0, value + 3, 8));
                    }
                    else if (side == "Top")
                    {
                        int value = (int)(Bottom - arrowHole.Bottom + 3);
                        if (value > sprite.Width)
                        {
                            value = (int)sprite.Width;
                        }
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, value + 3, 8));
                    }
                    else if (side == "Bottom")
                    {
                        int value = (int)(arrowHole.Top - Top + 3);
                        if (value > sprite.Width)
                        {
                            value = (int)sprite.Width;
                        }
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, value + 3, 8));
                    }
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
