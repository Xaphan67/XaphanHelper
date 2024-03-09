using System;
using System.Data.SqlTypes;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/LaserEmitter")]
    public class LaserEmitter : Entity
    {
        public class LaserEmitterBase : Solid
        {
            private Sprite baseSprite;

            private LaserEmitter emitter;

            public LaserEmitterBase(LaserEmitter emitter, Vector2 offset) : base(emitter.Position + offset, 8, 8, safe: true)
            {
                Tag = Tags.TransitionUpdate;
                this.emitter = emitter;
                Add(baseSprite = new Sprite(GFX.Game, emitter.directory + "/"));
                baseSprite.Add("baseActiveLeft", "baseActiveLeft", 0.2f);
                baseSprite.Add("baseActiveRight", "baseActiveRight", 0.2f);
                baseSprite.Add("baseActiveTop", "baseActiveTop", 0.2f);
                baseSprite.Add("baseActiveBottom", "baseActiveBottom", 0.2f);
                baseSprite.Add("baseInactiveLeft", "baseInactiveLeft", 0.2f);
                baseSprite.Add("baseInactiveRight", "baseInactiveRight", 0.2f);
                baseSprite.Add("baseInactiveTop", "baseInactiveTop", 0.2f);
                baseSprite.Add("baseInactiveBottom", "baseInactiveBottom", 0.2f);
                baseSprite.Play((emitter.inverted ? "baseInactive" : "baseActive") + emitter.side);
                Collider = new Hitbox(8f, 8f);
                Add(new LightOcclude(0.5f));
            }

            public override void Update()
            {
                base.Update();
                if (!string.IsNullOrEmpty(emitter.flag))
                {
                    if (SceneAs<Level>().Session.GetFlag(emitter.flag))
                    {
                        baseSprite.Play((emitter.inverted ? "baseInactive" : "baseActive") + emitter.side);
                    }
                    else
                    {
                        baseSprite.Play((emitter.inverted ? "baseActive" : "baseInactive") + emitter.side);
                    }
                }
                else if (emitter.noBeam)
                {
                    foreach (LaserBeam beam in SceneAs<Level>().Tracker.GetEntities<LaserBeam>())
                    {
                        if (CollideCheck(beam))
                        {
                            baseSprite.Play("baseActive" + emitter.side);
                            return;
                        }
                    }
                    baseSprite.Play("baseInactive" + emitter.side);
                }
                else
                {
                    baseSprite.Play("baseActive" + emitter.side);
                }
            }
        }

        public string side;

        public string type;

        public string flag;

        private string forceInactiveFlag;

        public string directory;

        public bool inverted;

        private Sprite emiterSprite;

        private LaserBeam Beam;

        private StaticMover staticMover;

        private bool Base;

        private bool noBeam;

        public LaserEmitter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            side = data.Attr("side");
            type = data.Attr("type", "Kill");
            flag = data.Attr("flag");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            Base = data.Bool("base", false);
            noBeam = data.Bool("noBeam");
            inverted = data.Bool("inverted");
            directory = data.Attr("directory");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/LaserEmitter";
            }
            Add(emiterSprite = new Sprite(GFX.Game, directory + "/"));
            emiterSprite.Add("idle", "idle", 0.2f);
            emiterSprite.Origin = new Vector2(emiterSprite.Width / 2, emiterSprite.Height / 2);
            emiterSprite.Position = new Vector2(4f, 4f);
            emiterSprite.Play("idle");
            staticMover = new StaticMover();
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.OnMove = OnMove;
            switch (side)
            {
                case "Left":
                    Collider = new Hitbox(2f, 4f, 6f, 2f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX));
                    Add(staticMover);
                    emiterSprite.Rotation = -(float)Math.PI / 2f;
                    break;
                case "Right":
                    Collider = new Hitbox(2f, 4f, 0f, 2f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitX));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX));
                    Add(staticMover);
                    emiterSprite.Rotation = (float)Math.PI / 2f;
                    break;
                case "Top":
                    Collider = new Hitbox(4f, 2f, 2f, 6f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position + Vector2.UnitY));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY));
                    Add(staticMover);
                    break;
                case "Bottom":
                    Collider = new Hitbox(4f, 2f, 2f, 0f);
                    staticMover.SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitY));
                    staticMover.JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY));
                    Add(staticMover);
                    emiterSprite.Rotation = (float)Math.PI;
                    break;
            }
            staticMover.OnEnable = OnEnable;
            staticMover.OnDisable = OnDisable;
            Depth = -1;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Base)
            {
                switch (side)
                {
                    case "Left":
                        SceneAs<Level>().Add(new LaserEmitterBase(this, new Vector2(8f, 0f)));
                        break;
                    case "Right":
                        SceneAs<Level>().Add(new LaserEmitterBase(this, new Vector2(-8f, 0f)));
                        break;
                    case "Top":
                        SceneAs<Level>().Add(new LaserEmitterBase(this, new Vector2(0f, 8f)));
                        break;
                    case "Bottom":
                        SceneAs<Level>().Add(new LaserEmitterBase(this, new Vector2(0f, -8f)));
                        break;
                }
            }
        }

        private void OnEnable()
        {
            Active = (Visible = (Collidable = true));
            if (Beam != null)
            {
                Beam.Visible = (Beam.Collidable = true);
            }
        }

        private void OnDisable()
        {
            Active = (Visible = (Collidable = false));
            if (Beam != null)
            {
                Beam.Visible = (Beam.Collidable = false);
            }
        }

        private void OnMove(Vector2 amount)
        {
            Position += amount;
            if (Beam != null)
            {
                Beam.Position += amount;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag))
            {
                noBeam = true;
            }
            if (!noBeam)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    if (!inverted)
                    {
                        if (SceneAs<Level>().Session.GetFlag(flag) && Beam == null)
                        {
                            SceneAs<Level>().Add(Beam = new LaserBeam(this, type));
                        }
                        else if (!SceneAs<Level>().Session.GetFlag(flag) && Beam != null)
                        {
                            SceneAs<Level>().Remove(Beam);
                            Beam = null;
                        }
                    }
                    else
                    {
                        if (SceneAs<Level>().Session.GetFlag(flag) && Beam != null)
                        {
                            SceneAs<Level>().Remove(Beam);
                            Beam = null;
                        }
                        else if (!SceneAs<Level>().Session.GetFlag(flag) && Beam == null)
                        {
                            SceneAs<Level>().Add(Beam = new LaserBeam(this, type));
                        }
                    }
                }
                else
                {
                    if (Beam == null)
                    {
                        SceneAs<Level>().Add(Beam = new LaserBeam(this, type));
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            emiterSprite.Render();
        }
    }
}
