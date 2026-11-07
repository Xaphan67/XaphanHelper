using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [CustomEntity("XaphanHelper/Powamp")]
    public class Powamp : Enemy
    {
        private Wiggler scaleWiggler;

        private Vector2 anchor;

        private SineWave sine;

        private Sprite Body;

        private Coroutine ReactionRoutine = new();

        public VertexLight Light;

        private float GrownDelay;

        private float GrownTime;

        private bool Grown;

        private Vector2 LastSinePosition;

        public Powamp(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new Circle(6f);
            pc.Collider = Collider;
            GrownDelay = data.Float("grownDelay", 0.5f);
            GrownTime = data.Float("grownTime", 0f);
            Collidable = false;
            Add(sine = new SineWave(0.44f, 0f).Randomize());
            anchor = Position;
            Position += new Vector2(sine.Value, sine.ValueOverTwo);
            LastSinePosition = Position;
            Body = new Sprite(GFX.Game, "enemies/Xaphan/Powamp/");
            Body.Add("idle", "idle", 0f);
            Body.Add("spiked", "spiked", 0.1f);
            Body.Play("idle");
            Body.CenterOrigin();
            sprites.Add(Body);
            foreach (Sprite sprite in sprites)
            {
                Add(sprite);
            }
            Add(Light = new VertexLight(new Vector2(0f, 6f), Color.White, 1f, 16, 24));
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                Body.Scale = Vector2.One * (1f + f * 0.3f);
            }));
        }
        private void UpdatePosition()
        {
            if (Position != LastSinePosition)
            {
                anchor += Position - LastSinePosition;
            }
            Vector2 vector = anchor + new Vector2(sine.Value, sine.ValueOverTwo);
            MoveToX(vector.X);
            MoveToY(vector.Y);
            LastSinePosition = Position;
        }

        public override void Update()
        {
            base.Update();
            UpdatePosition();
            if (SceneAs<Level>().CollideCheck<Actor>(new Rectangle((int)Position.X - 6, (int)Position.Y - 6, 12, 12)))
            {
                Actor actor = CollideFirst<Actor>();
                if (actor != null && !ReactionRoutine.Active && !Grown && actor.GetType() != typeof(Liquid.AirBubble))
                {
                    Add(ReactionRoutine = new Coroutine(GrowSpikes()));
                }
            }
        }

        private IEnumerator GrowSpikes()
        {
            yield return GrownDelay;

            Grown = true;
            scaleWiggler.Start();
            Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
            Collidable = true;
            Body.Rate = 1f;
            Body.Play("spiked");

            if (GrownTime > 0f)
            {
                yield return GrownTime;

                Body.Rate = -1f;
                Body.Play("spiked");
                scaleWiggler.Start();
                Audio.Play("event:/new_content/game/10_farewell/puffer_shrink", Position);
                Collidable = false;
                Body.Play("idle");
                Grown = false;
            }
        }
    }
}
