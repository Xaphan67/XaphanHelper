using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomSpinner")]
    class CustomSpinner : Entity
    {
        private class Border : Entity
        {
            private Entity[] drawing = new Entity[2];

            public Border(Entity parent, Entity filler)
            {
                drawing[0] = parent;
                drawing[1] = filler;
                Depth = parent.Depth + 2;
            }

            public override void Render()
            {
                if (drawing[0].Visible)
                {
                    DrawBorder(drawing[0]);
                    DrawBorder(drawing[1]);
                }
            }

            private void DrawBorder(Entity entity)
            {
                if (entity != null)
                {
                    foreach (Component component in entity.Components)
                    {
                        Image image = component as Image;
                        if (image != null)
                        {
                            Color color = image.Color;
                            Vector2 position = image.Position;
                            image.Color = Color.Black;
                            image.Position = position + new Vector2(0f, -1f);
                            image.Render();
                            image.Position = position + new Vector2(0f, 1f);
                            image.Render();
                            image.Position = position + new Vector2(-1f, 0f);
                            image.Render();
                            image.Position = position + new Vector2(1f, 0f);
                            image.Render();
                            image.Color = color;
                            image.Position = position;
                        }
                    }
                }
            }
        }

        [Tracked(true)]
        public class Filler : Entity
        {
            public Filler(Vector2 position) : base(position)
            {
                Collider = new Circle(4f);
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                foreach (Entity entity in Scene.Tracker.GetEntities<Filler>())
                {
                    Filler filler = (Filler)entity;
                    if (CollideCheck(filler) && filler != this)
                    {
                        filler.RemoveSelf();
                    }
                }
                //Add(new WeaponCollider(HitByBeam, HitByMissile, Collider));
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Depth = -8498;
            }

            private void HitByBeam(Beam beam)
            {
                beam.CollideSolid(beam.Direction);
            }

            private void HitByMissile(Missile missile)
            {
                missile.CollideImmune(missile.Direction);
            }

            public override void DebugRender(Camera camera)
            {
                //base.DebugRender(camera);
            }
        }

        public const float ParticleInterval = 0.02f;

        public bool AttachToSolid;

        public Filler filler;

        private Border border;

        private float offset;

        private bool expanded;

        private int randomSeed;

        public string type;

        public string bgDirectory;

        public string bgBoulderDirectory;

        public string fgDirectory;

        public int ID;

        public bool Hidden;

        private bool AlwaysCollidable;

        public bool CanDestroy;

        public bool CanAttach;

        public CustomSpinner(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            ID = data.ID;
            offset = Calc.Random.NextFloat();
            type = data.Attr("type");
            AlwaysCollidable = data.Bool("alwaysCollidable");
            CanDestroy = data.Bool("canDestroy");
            CanAttach = data.Bool("canAttachToBoulders");
            bgDirectory = "danger/crystal/Xaphan/" + type + "/bg";
            bgBoulderDirectory = "danger/crystal/Xaphan/" + type + "/bgBoulder";
            fgDirectory = "danger/crystal/Xaphan/" + type + "/fg";
            Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(new WeaponCollider(HitByBeam, HitByMissile, new Circle(8f)));
            Add(new LedgeBlocker());
            Depth = -8500;
            AttachToSolid = data.Bool("attachToSolid");
            if (AttachToSolid)
            {
                Add(new StaticMover
                {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = RemoveSelf
                });
            }
            randomSeed = Calc.Random.Next();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (InView() && !Hidden)
            {
                CreateSprites();
            }
        }

        public void ForceInstantiate()
        {
            CreateSprites();
            Visible = true;
        }

        public override void Update()
        {
            if (!Visible || Hidden)
            {
                Collidable = false;
                if (InView() && !Hidden)
                {
                    Visible = true;
                    if (!expanded)
                    {
                        CreateSprites();
                    }
                }
            }
            else
            {
                base.Update();
                if (SceneAs<Level>().Session.GetFlag("Using_Elevator"))
                {
                    Collidable = false;
                }
                else
                {
                    if (Scene.OnInterval(0.25f, offset) && !InView())
                    {
                        Visible = false;
                    }
                    if (Scene.OnInterval(0.05f, offset))
                    {
                        Player player = Scene.Tracker.GetEntity<Player>();
                        Beam beam = Scene.Tracker.GetEntity<Beam>();
                        Missile missile = Scene.Tracker.GetEntity<Missile>();
                        Collidable = AlwaysCollidable || (player != null && Math.Abs(player.X - X) < 128f && Math.Abs(player.Y - Y) < 128f) || (beam != null && Math.Abs(beam.X - X) < 32f && Math.Abs(beam.Y - Y) < 32f) || (missile != null && Math.Abs(missile.X - X) < 32f && Math.Abs(missile.Y - Y) < 32f);
                    }
                }
            }
            if (filler != null)
            {
                filler.Visible = !Hidden;
            }
        }

        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return base.X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
        }

        private void CreateSprites()
        {
            if (!expanded)
            {
                Calc.PushRandom(randomSeed);
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgDirectory);
                MTexture mtexture = Calc.Random.Choose(atlasSubtextures);
                if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
                {
                    Image image4 = new Image(mtexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f);
                    Add(image4);
                }
                if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
                {
                    Image image4 = new Image(mtexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f);
                    Add(image4);
                }
                if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
                {
                    Image image4 = new Image(mtexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f);
                    Add(image4);
                }
                if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
                {
                    Image image4 = new Image(mtexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f);
                    Add(image4);
                }
                foreach (CustomSpinner spinner in Scene.Entities.FindAll<CustomSpinner>())
                {
                    if (spinner.AttachToSolid == AttachToSolid && (spinner.Position - Position).LengthSquared() < 576f && (spinner.type == type || type != "hell"))
                    {
                        AddFiller((Position + spinner.Position) / 2f - Position);
                    }
                }
                if (CanAttach)
                {
                    foreach (ExplosiveBoulder boulder in Scene.Entities.FindAll<ExplosiveBoulder>())
                    {
                        if ((boulder.Position - Position).LengthSquared() < 576f)
                        {
                            AddFiller((Position + boulder.Position) / 2f - Position, true);
                        }
                    }
                }
                Scene.Add(border = new Border(this, filler));
                expanded = true;
                Calc.PopRandom();
            }
        }

        private void AddFiller(Vector2 offset, bool boulder = false)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(boulder ? bgBoulderDirectory : bgDirectory);
            Image image = new(Calc.Random.Choose(atlasSubtextures));
            image.Rotation = Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
            image.CenterOrigin();
            Scene.Add(filler = new Filler(Position + offset));
            filler.Add(image);
        }

        private bool SolidCheck(Vector2 position)
        {
            if (AttachToSolid)
            {
                return false;
            }
            List<Solid> list = Scene.CollideAll<Solid>(position);
            foreach (Solid item in list)
            {
                if (item is SolidTiles)
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearSprites()
        {
            if (filler != null)
            {
                filler.RemoveSelf();
            }
            filler = null;
            if (border != null)
            {
                border.RemoveSelf();
            }
            border = null;
            foreach (Image item in Components.GetAll<Image>())
            {
                item.RemoveSelf();
            }
            expanded = false;
        }

        private void OnShake(Vector2 pos)
        {
            foreach (Component component in Components)
            {
                if (component is Image)
                {
                    (component as Image).Position = pos;
                }
            }
        }

        private bool IsRiding(Solid solid)
        {
            return CollideCheck(solid);
        }

        private void OnPlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }

        private void HitByBeam(Beam beam)
        {
            beam.CollideImmune(beam.Direction);
        }

        private void HitByMissile(Missile missile)
        {
            missile.CollideImmune(missile.Direction);
        }

        public override void Removed(Scene scene)
        {
            if (filler != null && filler.Scene == scene)
            {
                filler.RemoveSelf();
            }
            if (border != null && border.Scene == scene)
            {
                border.RemoveSelf();
            }
            base.Removed(scene);
        }

        public void Destroy(bool boss = false)
        {
            if (InView())
            {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                Color color = Color.White;
                if (type == "mines")
                {
                    color = Calc.HexToColor("5B3311");
                }
                if (type != "magma")
                {
                    CrystalDebris.Burst(Position, color, boss, 8);
                }
                else
                {
                    color = Calc.HexToColor("792E00");
                    CrystalDebris.Burst(Position, color, boss, 4);
                    color = Calc.HexToColor("F0900F");
                    CrystalDebris.Burst(Position, color, boss, 4);
                }
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<Filler>())
            {
                Filler filler = (Filler)entity;
                if (CollideCheck(filler))
                {
                    filler.RemoveSelf();
                }
            }
            RemoveSelf();
        }

        public void Hide(bool boss = false)
        {
            if (InView() && !Hidden)
            {
                Hidden = true;
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                Color color = Color.White;
                if (type == "mines")
                {
                    color = Calc.HexToColor("5B3311");
                }
                CrystalDebris.Burst(Position, color, boss, 8);
            }
            Visible = Collidable = false;
        }

        public void Show()
        {
            Hidden = false;
            SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 8f, 32f, 0.5f);
            Visible = Collidable = true;
        }
    }
}
