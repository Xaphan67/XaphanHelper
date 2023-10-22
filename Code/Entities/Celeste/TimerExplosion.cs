using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    class TimerExplosion : Entity
    {
        Sprite explosionSprite;

        public TimerExplosion(Vector2 position) : base(position)
        {
            Tag = Tags.TransitionUpdate;
            Depth = -80000;
            Add(explosionSprite = new Sprite(GFX.Game, "countdown/"));
            explosionSprite.AddLoop("explosionA", "explosionA", 0.08f);
            explosionSprite.AddLoop("explosionB", "explosionB", 0.08f);
            explosionSprite.CenterOrigin();
            Random rand = Calc.Random;
            if (rand.Next(101) <= 50)
            {
                explosionSprite.Play("explosionA");
            }
            else
            {
                explosionSprite.Play("explosionB");
            }
            float rotation = rand.NextFloat();
            if (rand.Next(101) <= 50)
            {
                explosionSprite.Rotation = -(float)Math.PI / (2 - rotation);
            }
            else
            {
                explosionSprite.Rotation = (float)Math.PI / (2 - rotation);
            }
            if (rand.Next(101) <= 50)
            {
                explosionSprite.FlipX = true;
            }
            if (rand.Next(101) <= 50)
            {
                explosionSprite.FlipY = true;
            }
            explosionSprite.OnLastFrame = onLastFrame;
            Collider = new Circle(20f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CollideCheck<Player>() || CollideCheck<BubbleDoor>())
            {
                RemoveSelf();
            }
            else
            {
                Random rand = Calc.Random;
                if (rand.Next(101) <= 10)
                {
                    Audio.Play("event:/game/xaphan/explosion");
                }
            }
        }


        private void onLastFrame(string s)
        {
            Visible = false;
            RemoveSelf();
        }
    }
}
