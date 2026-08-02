using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/SlipperyWall")]
    public class SlipperyWall : Entity
    {
        private StaticMover staticMover;

        private new bool Left;

        private MTexture[,] texture = new MTexture[2,4];

        private int Variation;

        public Vector2 spriteOffset;

        public SlipperyWall(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            if (data.Bool("left"))
            {
                Collider = new Hitbox(2f, data.Height + 4f, 8f);
                Left = true;
            }
            else
            {
                Collider = new Hitbox(2f, data.Height + 4f, -2f);
            }
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth - 1;
            };
            staticMover.OnMove = OnMove;
            staticMover.OnShake = onShake;
            staticMover.SolidChecker = IsRiding;
            staticMover.JumpThruChecker = IsRiding;
            staticMover.OnDisable = OnDisable;
            staticMover.OnEnable = OnEnable;
            Add(staticMover);
            Add(new PowerGripBlocker(edge: false));
            Variation = Calc.Random.Next(0, 3);
            Depth = -10999;
        }

        private void OnMove(Vector2 amount)
        {
            Position += amount;
        }

        private void onShake(Vector2 amount)
        {
            spriteOffset += amount;
        }

        private bool IsRiding(Solid solid)
        {
            if (Left)
            {
                return CollideCheck(solid, Position - Vector2.UnitX);
            }
            else if (!Left)
            {
                return CollideCheck(solid, Position + Vector2.UnitX);
            }
            return false;
        }

        private bool IsRiding(JumpThru jumpthru)
        {
            if (Left)
            {
                return CollideCheck(jumpthru, Position - Vector2.UnitX);
            }
            else if (!Left)
            {
                return CollideCheck(jumpthru, Position + Vector2.UnitX);
            }
            return false;
        }

        private void OnDisable()
        {
            Visible = Collidable = Active = false;
        }

        private void OnEnable()
        {
            Visible = Collidable = Active = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture mtexture = GFX.Game["objects/XaphanHelper/SlipperyWall/moss"];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    texture[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            spriteOffset = new Vector2(Left ? 4f : -4f, 0);
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StClimb)
            {
                if ((player.Top < Top && CollideCheck(player, Position + Vector2.UnitY * 4)) || (player.Bottom > Bottom && CollideCheck(player, Position - Vector2.UnitY * 4)))
                {
                    player.StateMachine.State = Player.StNormal;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (Height <= 12)
            {
                texture[0, 2].Draw(Position + Vector2.UnitX * (!Left ? 8 : 0) + spriteOffset, Vector2.Zero, Color.White, new Vector2(!Left ? -1 : 1, 1));
            }
            else
            {
                texture[0, 0].Draw(Position + Vector2.UnitX * (!Left ? 8 : 0) + spriteOffset, Vector2.Zero, Color.White, new Vector2(!Left ? -1 : 1, 1));
                for (int i = 1; i < (Height - 4) / 8f - 1f; i++)
                {
                    int remainder;
                    Math.DivRem((int)Position.Y + i, 4, out remainder);
                    int result = remainder + Variation;
                    if (result > 2)
                    {
                        result -= 3;
                    }
                    texture[1, result].Draw(Position + Vector2.UnitY * i * 8 + Vector2.UnitX * (!Left ? 8 : 0) + spriteOffset, Vector2.Zero, Color.White, new Vector2(!Left ? -1 : 1, 1));
                }
                texture[0, 1].Draw(Position + Vector2.UnitY * (Height - 12) + Vector2.UnitX * (!Left ? 8 : 0) + spriteOffset, Vector2.Zero, Color.White, new Vector2(!Left ? -1 : 1, 1));
            }
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Draw.Rect(new Rectangle((int)Position.X + (!Left ? -2 : 8), (int)Position.Y, (int)Collider.Width, (int)Collider.Height - 4), Collidable ? Color.Red : Color.DarkRed);
            }
        }
    }
}
