using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CratesSpawner")]
    public class CratesSpawner : Solid
    {
        private MTexture Texture;

        public Vector2 spriteOffset;

        private int MaxCrates;

        private string Flag;

        private string ForceInactiveFlag;

        private float Cooldown;

        private string Type;

        private StaticMover staticMover;

        public CratesSpawner(EntityData data, Vector2 position) : base(data.Position + position, data.Width, data.Height, false)
        {
            Collider = new Hitbox(24, 6);
            Texture = GFX.Game[data.Attr("directory") + "/spawner00"];
            MaxCrates = data.Int("maxCrates", 1);
            Flag = data.Attr("flag");
            ForceInactiveFlag = data.Attr("forceInactiveFlag");
            Cooldown = data.Float("cooldwon", 3f);
            Type = data.Attr("type", "wood");
            staticMover = new StaticMover();
            staticMover.SolidChecker = ((Solid s) => CollideCheckOutside(s, Position - Vector2.UnitY * 4f));
            staticMover.OnAttach = delegate (Platform p)
            {
                Depth = p.Depth + 1;
            };
            staticMover.OnEnable = onEnable;
            staticMover.OnDisable = onDisable;
            staticMover.OnShake = onShake;
            staticMover.OnMove = onMove;
            Add(staticMover);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(SpawnRoutine()));
        }

        private IEnumerator SpawnRoutine()
        {
            float cooldown = 0f;
            while (true)
            {
                int currentCrates = 0;
                foreach (Crate crate in SceneAs<Level>().Tracker.GetEntities<Crate>())
                {
                    if (crate.SourceSpawner == this)
                    {
                        currentCrates++;
                    }
                }
                while (cooldown >= 0f)
                {
                    cooldown -= Engine.DeltaTime;
                    yield return null;
                }
                while ((!string.IsNullOrEmpty(Flag) && !SceneAs<Level>().Session.GetFlag(Flag)) || (!string.IsNullOrEmpty(ForceInactiveFlag) && SceneAs<Level>().Session.GetFlag(ForceInactiveFlag)))
                {
                    yield return null;
                }
                if (currentCrates < MaxCrates)
                {
                    SceneAs<Level>().Add(new Crate(Position + new Vector2(12f, 15f), Type, this));
                    cooldown = Cooldown;
                }
                yield return null;
            }
        }

        private void onEnable()
        {
            Visible = Collidable = true;
        }

        private void onDisable()
        {
            Collidable = Visible = false;
        }

        private void onShake(Vector2 amount)
        {
            spriteOffset += amount;
        }

        private void onMove(Vector2 amount)
        {
            Position += amount;
        }

        public override void Render()
        {
            base.Render();
            Texture.Draw(Position + spriteOffset);
        }
    }
}
