using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Lever")]
    class Lever : Entity
    {
        private Sprite Sprite;

        private string Side;

        private string Flag;

        private bool CanSwapFlag;

        private bool PlayerOnTop;

        private StaticMover staticMover;

        public Lever(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, data.Attr("directory") + "/"));
            Sprite.Add("start", "lever", 0);
            Sprite.Add("toEnd", "lever", 0.08f, 1, 2);
            Sprite.Add("end", "lever", 0, 2);
            Sprite.Add("toStart", "lever", 0.08f, 1, 0);
            Flag = data.Attr("flag");
            CanSwapFlag = data.Bool("canSwapFlag", false);
            Side = data.Attr("side", "Up");
            staticMover = new StaticMover();
            switch (Side) {
                case "Up":
                    Collider = new Hitbox(12, 8, 2, 0);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitY * 4f));
                    Sprite.Position = new Vector2(-4f, -8f);
                    break;
                case "Down":
                    Collider = new Hitbox(12, 8, 2, -8);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitY * 4f));
                    Sprite.Position = new Vector2(-4f, -8f);
                    Sprite.FlipY = true;
                    break;
                case "Left":
                    Collider = new Hitbox(8, 12, 8, -6);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position + Vector2.UnitX * 2f));
                    Sprite.Position = new Vector2(0f, 12f);
                    Sprite.Rotation = -(float)Math.PI / 2;
                    Sprite.FlipX = true;
                    break;
                case "Right":
                    Collider = new Hitbox(8, 12, 0, -6);
                    staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitX * 2f));
                    Sprite.Position = new Vector2(16f, -12f);
                    Sprite.Rotation = (float)Math.PI / 2;
                    break;
            }
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.OnEnable = onEnable;
            staticMover.OnDisable = onDisable;
            staticMover.OnShake = onShake;
            staticMover.OnMove = onMove;
            Add(staticMover);
            Add(new PlayerCollider(onPlayer, Collider));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Session.GetFlag(Flag))
            {
                Sprite.Play("end");
            }
            else
            {
                Sprite.Play("start");
            }
        }

        private void onPlayer(Player player)
        {
            if (!PlayerOnTop)
            {
                Add(new Coroutine(AnimateRoutine(true)));
            }
            PlayerOnTop = true;
        }

        public void UpdateSprite()
        {
            Add(new Coroutine(AnimateRoutine(false)));
        }

        private IEnumerator AnimateRoutine(bool switchFlag)
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                if (switchFlag)
                {
                    if ((!CanSwapFlag && !SceneAs<Level>().Session.GetFlag(Flag)) || CanSwapFlag)
                    {
                        foreach (Lever lever in SceneAs<Level>().Tracker.GetEntities<Lever>())
                        {
                            if (lever != this && lever.Flag == Flag)
                            {
                                lever.UpdateSprite();
                            }
                        }
                    }
                }
                if (!CanSwapFlag)
                {
                    if (switchFlag)
                    {
                        if (!SceneAs<Level>().Session.GetFlag(Flag))
                        {
                            Sprite.Play("toEnd");
                            yield return 0.16f;
                            Sprite.Play("end");
                        }
                    }
                    else
                    {
                        if (SceneAs<Level>().Session.GetFlag(Flag))
                        {
                            Sprite.Play("toStart");
                            yield return 0.16f;
                            Sprite.Play("start");
                        }
                        else
                        {
                            Sprite.Play("toEnd");
                            yield return 0.16f;
                            Sprite.Play("end");
                        }
                    }
                }
                else
                {
                    if (SceneAs<Level>().Session.GetFlag(Flag))
                    {
                        Sprite.Play("toStart");
                        yield return 0.16f;
                        Sprite.Play("start");
                    }
                    else
                    {
                        Sprite.Play("toEnd");
                        yield return 0.16f;
                        Sprite.Play("end");
                    }
                }
                if (switchFlag)
                {
                    SwitchFlag();
                }
            }
        }

        private void SwitchFlag()
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                if (!CanSwapFlag)
                {
                    if (!SceneAs<Level>().Session.GetFlag(Flag))
                    {
                        SceneAs<Level>().Session.SetFlag(Flag, true);
                    }
                }
                else
                {
                    SceneAs<Level>().Session.SetFlag(Flag, !SceneAs<Level>().Session.GetFlag(Flag));
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = CollideFirst<Player>();
            if (player == null && PlayerOnTop)
            {
                PlayerOnTop = false;
            }
        }

        private void onEnable()
        {
            Visible = true;
            Collidable = true;
        }

        private void onDisable()
        {
            Collidable = false;
            Visible = false;
        }

        private void onShake(Vector2 amount)
        {
            Sprite.Position += amount;
        }

        private void onMove(Vector2 amount)
        {
            Position += amount;
        }

        public override void Render()
        {
            base.Render();
            Sprite.DrawOutline();
            Sprite.Render();
        }
    }
}
