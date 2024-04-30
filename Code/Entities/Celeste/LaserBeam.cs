using System.Collections;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class LaserBeam : Entity
    {
        public LaserEmitter Emitter;

        public Vector2 Offset;

        public int alphaStatus;

        public float laserAlpha;

        public float maxRight;

        public string Type;

        private PlayerCollider pc;

        public ParticleType P_Collide;

        public Color color;

        public Color borderColor;

        private float colliderWidth;

        private float colliderHeight;

        private float colliderLeft;

        private float colliderTop;

        private bool canKillPlayer;

        public LaserBeam(LaserEmitter emitter, string type) : base(emitter.Position)
        {
            Tag = Tags.TransitionUpdate;
            laserAlpha = 0.5f;
            Emitter = emitter;
            Type = type;
            if (Emitter.side == "Left")
            {
                Offset = new Vector2(8f, 3f);
            }
            else if (Emitter.side == "Right")
            {
                Offset = new Vector2(0f, 3f);
            }
            else if (Emitter.side == "Top")
            {
                Offset = new Vector2(3f, 8f);
            }
            else if (Emitter.side == "Bottom")
            {
                Offset = new Vector2(3f, 0f);
            }
            Position = Emitter.Position + Offset;
            color = Type == "Must Dash" ? Color.Orange : (Type == "No Dash" ? Color.CadetBlue : Color.IndianRed);
            Color darker = Calc.HexToColor("798EB0");
            borderColor = new Color(darker.R / 255f * (color.R / 255f), darker.G / 255f * (color.G / 255f), darker.B / 255f * (color.B / 255f), 1f);
            Add(pc = new PlayerCollider(OnCollide));
            /*P_Collide = new ParticleType
            {
                Color = Type == "Must Dash" ? Color.Orange : (Type == "No Dash" ? Color.CadetBlue : Color.IndianRed),
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                Direction = (float)Math.PI / 2f,
                SpeedMin = 2f,
                SpeedMax = 10f,
                LifeMin = 0.6f,
                LifeMax = 0.8f,
                Acceleration = Vector2.UnitY * 35f
            };*/
            Depth = 101;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Emitter.side == "Left")
            {
                Collider = new Hitbox(Right - SceneAs<Level>().Bounds.Left, 2f, -(Right - SceneAs<Level>().Bounds.Left), 0f);
            }
            else if (Emitter.side == "Right")
            {
                Collider = new Hitbox(SceneAs<Level>().Bounds.Right - Left, 2f);
            }
            else if (Emitter.side == "Top")
            {
                Collider = new Hitbox(2f, Bottom - SceneAs<Level>().Bounds.Top, 0f, -(Bottom - SceneAs<Level>().Bounds.Top));
            }
            else if (Emitter.side == "Bottom")
            {
                Collider = new Hitbox(2f, SceneAs<Level>().Bounds.Bottom - Top);
            }
            Add(new Coroutine(WaitingRoutine()));
        }

        private IEnumerator WaitingRoutine()
        {
            float timer = 0.01f;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            canKillPlayer = true;
        }

        public override void Update()
        {
            foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
            {
                plateform.Collidable = false;
            }
            base.Update();
            if (alphaStatus == 0 || (alphaStatus == 1 && laserAlpha != 0.9f))
            {
                alphaStatus = 1;
                laserAlpha = Calc.Approach(laserAlpha, 0.9f, Engine.DeltaTime);
                if (laserAlpha == 0.9f)
                {
                    alphaStatus = 2;
                }
            }
            if (alphaStatus == 2 && laserAlpha != 0.1f)
            {
                laserAlpha = Calc.Approach(laserAlpha, 0.5f, Engine.DeltaTime);
                if (laserAlpha == 0.5f)
                {
                    alphaStatus = 1;
                }
            }
            Position = Emitter.Position + Offset;
            if (Emitter.side == "Left")
            {
                if ((CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>()) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>())
                    {
                        Collider.Left += 1;
                        Collider.Width -= 1;
                        colliderLeft = Collider.Left;
                        colliderWidth = Collider.Width;
                    }
                    /*if (Visible && SceneAs<Level>().OnRawInterval(0.15f))
                    {
                        for (int i = 0; i <= 90; i += 30)
                        {
                            SceneAs<Level>().Particles.Emit(P_Collide, 1, CenterLeft + new Vector2(3, 0) + new Vector2(0, 1), Vector2.One * 2, P_Collide.Color, i * ((float)Math.PI / 180f));
                        }
                        for (int i = 270; i <= 360; i += 30)
                        {
                            SceneAs<Level>().Particles.Emit(P_Collide, 1, CenterLeft + new Vector2(3, 0) + new Vector2(0, 1), Vector2.One * 2, P_Collide.Color, i * ((float)Math.PI / 180f));
                        }
                    }*/
                }
                else
                {
                    if (!CollideCheck<Solid>(Position - Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position - Vector2.UnitX))
                    {
                        while (!CollideCheck<Solid>(Position - Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position - Vector2.UnitX) && Collider.Width < (Right - SceneAs<Level>().Bounds.Left))
                        {
                            Collider.Left -= 1;
                            Collider.Width += 1;
                            colliderLeft = Collider.Left;
                            colliderWidth = Collider.Width;
                        }
                    }
                }
                if (CollideCheck<PlayerPlatform>())
                {
                    Collider.Left = colliderLeft;
                    Collider.Width = colliderWidth;
                }
                if (Collider.Width < (Right - SceneAs<Level>().Bounds.Left))
                {
                    Collider.Left -= 4;
                    Collider.Width += 4;
                }
            }
            else if (Emitter.side == "Right")
            {
                if ((CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>()) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>())
                    {
                        Collider.Width -= 1;
                        colliderWidth = Collider.Width;
                    }
                    /*if (Visible && SceneAs<Level>().OnRawInterval(0.15f))
                    {
                        for (int i = 90; i <= 270; i += 30)
                        {
                            SceneAs<Level>().Particles.Emit(P_Collide, 1, CenterRight - new Vector2(1, 0) + new Vector2(0, 1), Vector2.One * 2, P_Collide.Color, i * ((float)Math.PI / 180f));
                        }
                    }*/
                }
                else
                {
                    if (!CollideCheck<Solid>(Position + Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position + Vector2.UnitX))
                    {
                        while (!CollideCheck<Solid>(Position + Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position + Vector2.UnitX) && Collider.Width < SceneAs<Level>().Bounds.Right - Left)
                        {
                            Collider.Width += 1;
                            colliderWidth = Collider.Width;
                        }
                    }
                }
                if (CollideCheck<PlayerPlatform>())
                {
                    Collider.Width = colliderWidth;
                }
                if (Collider.Width < SceneAs<Level>().Bounds.Right - Left)
                {
                    Collider.Width += 4;
                }
            }
            else if (Emitter.side == "Top")
            {
                if ((CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>()) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>())
                    {
                        Collider.Top += 1;
                        Collider.Height -= 1;
                        colliderTop = Collider.Top;
                        colliderHeight = Collider.Height;
                    }
                    /*if (Visible && SceneAs<Level>().OnRawInterval(0.15f))
                    {
                        for (int i = 0; i <= 180; i += 30)
                        {
                            SceneAs<Level>().Particles.Emit(P_Collide, 1, TopCenter + new Vector2(0, 3) + new Vector2(1, 0), Vector2.One * 2, P_Collide.Color, i * ((float)Math.PI / 180f));
                        }
                    }*/
                }
                else
                {
                    if (!CollideCheck<Solid>(Position - Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position - Vector2.UnitX))
                    {
                        while (!CollideCheck<Solid>(Position - Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position - Vector2.UnitX) && Collider.Height < Bottom - SceneAs<Level>().Bounds.Top)
                        {
                            Collider.Top -= 1;
                            Collider.Height += 1;
                            colliderTop = Collider.Top;
                            colliderHeight = Collider.Height;
                        }
                    }
                }
                if (CollideCheck<PlayerPlatform>())
                {
                    Collider.Top = colliderTop;
                    Collider.Height = colliderHeight;
                }
                if (Collider.Height < Bottom - SceneAs<Level>().Bounds.Top)
                {
                    Collider.Top -= 4;
                    Collider.Height += 4;
                }
            }
            else if (Emitter.side == "Bottom")
            {
                if ((CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>()) && !CollideCheck<PlayerPlatform>())
                {
                    while (CollideCheck<Solid>() || CollideCheck<Crate.LaserBlocker>())
                    {
                        Collider.Height -= 1;
                        colliderHeight = Collider.Height;
                    }
                    /*if (Visible && SceneAs<Level>().OnRawInterval(0.15f))
                    {
                        for (int i = 180; i <= 360; i += 30)
                        {
                            SceneAs<Level>().Particles.Emit(P_Collide, 1, BottomCenter - new Vector2(0, 1) + new Vector2(1, 0), Vector2.One * 2, P_Collide.Color, i * ((float)Math.PI / 180f));
                        }
                    }*/
                }
                else
                {
                    if (!CollideCheck<Solid>(Position + Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position + Vector2.UnitX))
                    {
                        while (!CollideCheck<Solid>(Position + Vector2.UnitX) && !CollideCheck<Crate.LaserBlocker>(Position + Vector2.UnitX) && Collider.Height < SceneAs<Level>().Bounds.Bottom - Top)
                        {
                            Collider.Height += 1;
                            colliderHeight = Collider.Height;
                        }
                    }
                }
                if (CollideCheck<PlayerPlatform>())
                {
                    Collider.Height = colliderHeight;
                }
                if (Collider.Height < SceneAs<Level>().Bounds.Bottom - Top)
                {
                    Collider.Height += 4;
                }
            }
            foreach (PlayerPlatform plateform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
            {
                plateform.Collidable = true;
            }
        }

        private void OnCollide(Player player)
        {
            if ((XaphanModule.useUpgrades ? !ScrewAttackManager.isScrewAttacking : true) && canKillPlayer)
            {
                if (Type == "Kill")
                {
                    KillPlayer(player);
                }
                else if (Type == "Must Dash")
                {
                    if (!player.DashAttacking)
                    {
                        KillPlayer(player);
                    }
                }
                else if (Type == "No Dash")
                {
                    if (player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StDreamDash)
                    {
                        KillPlayer(player);
                    }
                }
            }
        }

        private void KillPlayer(Player player)
        {
            if (CollideCheck<Crate.LaserBlocker>())
            {
                if (Emitter.side == "Left")
                {
                    Crate.LaserBlocker blocker = SceneAs<Level>().Tracker.GetNearestEntity<Crate.LaserBlocker>(Collider.CenterLeft);
                    if (player.Right > blocker.Right)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }
                else if (Emitter.side == "Right")
                {
                    Crate.LaserBlocker blocker = SceneAs<Level>().Tracker.GetNearestEntity<Crate.LaserBlocker>(Collider.CenterRight);
                    if (player.Left < blocker.Left)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }
                else if (Emitter.side == "Top")
                {
                    Crate.LaserBlocker blocker = SceneAs<Level>().Tracker.GetNearestEntity<Crate.LaserBlocker>(Collider.TopCenter);
                    if (player.Bottom > blocker.Bottom)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }
                else if (Emitter.side == "Bottom")
                {
                    Crate.LaserBlocker blocker = SceneAs<Level>().Tracker.GetNearestEntity<Crate.LaserBlocker>(Collider.BottomCenter);
                    if (player.Top < blocker.Top)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }
            }
            else
            {
                player.Die(new Vector2(0f, -1f));
            }
        }

        public override void Render()
        {
            base.Render();
            if (Collider != null && Emitter != null)
            {
                Draw.Rect(Collider, (Type == "Must Dash" ? Color.Orange : (Type == "No Dash" ? Color.CadetBlue : Color.IndianRed)) * laserAlpha);
                if (Emitter.side == "Left" || Emitter.side == "Right")
                {
                    Draw.Rect(Collider.AbsoluteX, Collider.AbsoluteY - 1, Width, 1, borderColor * laserAlpha);
                    Draw.Rect(Collider.AbsoluteX, Collider.AbsoluteY + 2, Width, 1, borderColor * laserAlpha);
                }
                else
                {
                    Draw.Rect(Collider.AbsoluteX - 1, Collider.AbsoluteY, 1, Height, borderColor * laserAlpha);
                    Draw.Rect(Collider.AbsoluteX + 2, Collider.AbsoluteY, 1, Height, borderColor * laserAlpha);
                }
            }
        }
    }
}
