using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    class SparkGenerator : Entity
    {
        private Sprite sprite;

        private SoundSource sound;

        public SparkGenerator(Vector2 position) : base(position)
        {
            Tag = Tags.TransitionUpdate;
            Visible = false;
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/SparkGenerator/"));
            sprite.AddLoop("main", "main", 0.05f);
            sprite.CenterOrigin();
            sprite.Play("main");
            Add(new VertexLight(Color.White, 1f, 24, 32));
            Add(sound = new SoundSource());
            Add(new Coroutine(GenerateSparks()));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
        }

        private IEnumerator GenerateSparks()
        {
            while (true && !SceneAs<Level>().InCredits)
            {
                float offTime = Calc.Random.NextFloat() * 2f + 1f;
                while (offTime > 0)
                {
                    offTime -= Engine.DeltaTime;
                    Visible = false;
                    yield return null;
                }
                float onTime = Calc.Random.NextFloat(0.4f) + 0.2f;
                sound.Play("event:/game/xaphan/spark");
                while (onTime > 0)
                {
                    onTime -= Engine.DeltaTime;
                    Visible = true;
                    sprite.OnLastFrame += onLastFrame;
                    yield return null;
                }
                sound.Stop();
            }
        }

        private void onLastFrame(string s)
        {
            bool shouldSwap = Calc.Random.Next(2) == 0 ? false : true;
            if (shouldSwap)
            {
                sprite.FlipX = !sprite.FlipX;
            }
        }
    }
}
