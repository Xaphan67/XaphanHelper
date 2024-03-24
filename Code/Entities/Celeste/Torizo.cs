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
    [CustomEntity("XaphanHelper/Torizo")]
    public class Torizo : Actor
    {
        private enum Facings
        {
            Left,
            Right
        }

        private PlayerCollider pc;

        public ColliderList colliders;

        private Facings Facing;

        private Coroutine Routine = new();

        private Sprite Sprite;

        public bool Activated;

        private bool Defeated;

        public int Health;

        private float InvincibilityDelay;

        private bool Flashing;

        public Vector2 Speed;

        public Torizo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Torizo/"));
            Sprite.Add("sit", "standUp", 0f, 0);
            Sprite.Add("standUp", "standUp", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            Sprite.Add("walk", "walk", 0.08f);
            Sprite.Play("sit");
            Facing = Facings.Right;
            Health = 100;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Sprite.OnFrameChange = delegate (string anim)
            {
                if (Scene != null)
                {
                    int currentAnimationFrame = Sprite.CurrentAnimationFrame;
                    if (anim.Equals("walk") && (currentAnimationFrame == 7 || currentAnimationFrame == 14))
                    {
                        SceneAs<Level>().Shake(0.3f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    }
                }
            };
            Add(new Coroutine(SequenceRoutine()));
            Add(new Coroutine(InvincibilityRoutine()));
        }

        public override void Update()
        {
            List<Entity> crumbleBlocks = Scene.Tracker.GetEntities<CustomCrumbleBlock>().ToList();
            foreach (CustomCrumbleBlock crumbleBlock in crumbleBlocks)
            {
                crumbleBlock.Collidable = false;
            }
            base.Update();
            Sprite.FlipX = Facing == Facings.Left;
            if (Activated && pc == null)
            {
                Collider = new Hitbox(24, 80, 32, 16);
                colliders = new ColliderList(new Hitbox(8, 32, Position.X + 40, Position.Y + 24), new Hitbox(8, 8, Position.X + 48, Position.Y + 16));
                Add(pc = new PlayerCollider(onCollidePlayer, new Hitbox(24, 52, 32, 16)));
            }
            if (Flashing)
            {
                Sprite.Color = Color.Red;
            }
            else
            {
                Sprite.Color = Color.White;
            }
            if (Activated && !Defeated)
            {
                Speed.Y = Calc.Approach(Speed.Y, 200f, 800f * Engine.DeltaTime);
                MoveH(Speed.X * Engine.DeltaTime, null);
                MoveV(Speed.Y * Engine.DeltaTime, null);
                foreach (Collider collider in colliders.colliders)
                {
                    if (collider.Height == 32)
                    {
                        collider.Position.X = Position.X + 40;
                        collider.Position.Y = Position.Y + 24;
                    }
                    else
                    {
                        collider.Position.X = Position.X + 48;
                        collider.Position.Y = Position.Y + 16;
                    }
                }
            }
            if (Health <= 0)
            {
                Defeated = true;
                Collider = null;
                colliders = null;
                Remove(pc);
            }
            foreach (CustomCrumbleBlock crumbleBlock in crumbleBlocks)
            {
                crumbleBlock.Collidable = crumbleBlock.Destroyed ? false : true;
            }
        }

        public IEnumerator SequenceRoutine()
        {
            yield return 5f;
            StandUp();
            while (!Activated)
            {
                yield return null;
            }
            while (Health > 0)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && !Routine.Active)
                {
                    Add(Routine = new Coroutine(WalkRoutine(player)));
                }
                yield return null;
            }
        }

        public IEnumerator WalkRoutine(Player player)
        {
            Sprite.Position.X = 21;
            Sprite.Play("walk");
            bool walkLeft = player.Center.X < Center.X;
            Speed.X = 45f * (walkLeft ? -1 : 1);
            float walkDuration = Sprite.CurrentAnimationTotalFrames * 0.08f;
            while (walkDuration > 0)
            {
                walkDuration -= Engine.DeltaTime;
                yield return null;
            }
            Speed.X = 0f;
            yield return 1f;
        }

        public IEnumerator InvincibilityRoutine()
        {
            while (Health > 0)
            {
                while (InvincibilityDelay > 0)
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        Flashing = !Flashing;
                    }
                    InvincibilityDelay -= Engine.DeltaTime;
                    yield return null;
                }
                Flashing = false;
                yield return null;
            }
        }

        public void StandUp()
        {
            Sprite.Play("standUp");
            Sprite.OnLastFrame = onLastFrame;
        }

        public void GetHit()
        {
            if (Health > 0 && InvincibilityDelay <= 0)
            {
                Health -= 10;
                Logger.Log(LogLevel.Info, "XH", "Torizo hit - Remaining health : " + Health);
                InvincibilityDelay = 0.75f;
            }
        }

        private void onCollidePlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void onLastFrame(string s)
        {
            if (Sprite.LastAnimationID == "standUp")
            {
                Activated = true;
                Sprite.OnLastFrame = null;
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (colliders != null)
            {
                foreach (Collider collider in colliders.colliders)
                {
                    Draw.HollowRect(collider, Color.Red);
                }
            }
        }
    }
}
