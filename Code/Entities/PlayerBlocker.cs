using Celeste.Mod.XaphanHelper.Colliders;
using Celeste.Mod.XaphanHelper.Enemies;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class PlayerBlocker : Solid
    {
        public bool CanJumpThrough;

        public bool UpsideDown;

        public Enemy Enemy;

        public PlayerBlocker(Vector2 position, float width, float height, bool canClimb = false, int surfaceSoundIndex = 33, bool upsideDown = false, bool canJumpThrough = false, Enemy enemy = null) : base(position, width, height, safe: true)
        {
            Tag = Tags.TransitionUpdate;
            Collidable = true;
            Visible = false;
            if (!canClimb)
            {
                Add(new ClimbBlocker(edge: true));
            }
            SurfaceSoundIndex = surfaceSoundIndex;
            UpsideDown = upsideDown;
            CanJumpThrough = canJumpThrough;
            Enemy = enemy;
        }

        public PlayerBlocker(Collider collider, bool canClimb = false, int surfaceSoundIndex = 33, bool upsideDown = false, bool canJumpThrough = false) : base(collider.AbsolutePosition, collider.Width, collider.Height, safe: true)
        {
            Tag = Tags.TransitionUpdate;
            Collidable = true;
            Visible = false;
            if (!canClimb)
            {
                Add(new ClimbBlocker(edge: true));
            }
            SurfaceSoundIndex = surfaceSoundIndex;
            UpsideDown = upsideDown;
            CanJumpThrough = canJumpThrough;
        }

        public void HitByBeam(Beam beam)
        {
            Enemy.HitByBeam(beam);
        }

        public void HitByMissile(Missile missile)
        {
            Enemy.HitByMissile(missile);
        }

        public override void Update()
        {
            base.Update();
            if (CanJumpThrough)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null)
                {
                    if (!UpsideDown)
                    {
                        if (player.Bottom <= Top)
                        {
                            Collidable = true;
                        }
                        else
                        {
                            Collidable = false;
                        }
                    }
                    else
                    {
                        if (player.Top >= Bottom)
                        {
                            Collidable = true;
                        }
                        else
                        {
                            Collidable = false;
                        }
                    }
                }
            }
        }
    }
}
