using System.Collections;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.Enemies;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class EscapeExplosion : Entity
    {
        private Sprite Sprite;

        public EscapeExplosion(Vector2 position) : base(position)
        {
            Add(Sprite = new Sprite(GFX.Game, "upgrades/powerBomb/"));
            Sprite.AddLoop("explode", "PowerBombExplode", 0.06f);
            Sprite.CenterOrigin();
            Sprite.Justify = new Vector2(0.5f, 0.5f);
            Sprite.Position.Y -= 1;
            Sprite.Play("explode");
            Depth = -100000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Flash(Color.White);
            Audio.Play("event:/game/xaphan/power_bomb_explode");
            Add(new Coroutine(Explosion()));
        }

        public IEnumerator Explosion()
        {
            Sprite.Color = Color.White * 0.9f;
            for (int i = 0; i <= 100; i++)
            {
                Sprite.Scale = new Vector2(0.04f * i, 0.04f * i);
                yield return null;
            }
            for (int j = 1; j <= 100; j++)
            {
                Sprite.Scale = new Vector2(4 + 0.03f * j, 4 + 0.03f * j);
                yield return null;
            }
            for (int k = 1; k <= 115; k++)
            {
                Sprite.Scale = new Vector2(7f + 0.02f * k, 7f + 0.02f * k);
                yield return null;
            }
            Visible = false;
            RemoveSelf();
        }
    }
}