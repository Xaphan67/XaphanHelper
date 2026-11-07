using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked]
    [CustomEntity("XaphanHelper/BubbleBlock")]
    public class BubbleBlock : Solid
    {
        private class RespawnDebris : Entity
        {
            private Image sprite;

            private Vector2 from;

            private Vector2 to;

            private float percent;

            private float duration;

            private float timer;

            public Vector2 currentPos;

            public bool noMove;

            public float alpha;

            private SineWave sine;

            private bool Reform;

            public RespawnDebris Init(Vector2 from, Vector2 to, float duration, Image image, bool reform = true)
            {
                Add(sine = new SineWave(0.44f, 0f).Randomize());
                if (sprite == null)
                {
                    Add(sprite = new Image(image.Texture));
                    sprite.CenterOrigin();
                }
                else
                {
                    sprite.Texture = image.Texture;
                }
                Position = (this.from = from);
                percent = 0f;
                this.to = to;
                this.duration = duration;
                if (from == to)
                {
                    noMove = true;
                }
                Reform = reform;
                return this;
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                sine.Update();
                sprite.Position = new Vector2(sine.Value * (percent < 0.5f ? 2f : (percent < 0.75f ? 1f : 0f)), sine.ValueOverTwo * (percent < 0.5f ? 2f : (percent < 0.75f ? 1f : 0f)));
                if (Reform)
                {
                    if (percent < 0.3f)
                    {
                        percent += Engine.DeltaTime / duration;
                        Position = Vector2.Lerp(from, to, Ease.ElasticOut(percent * 2));
                        alpha = noMove ? 1 - percent * 33 : 1 - percent;
                        sprite.Color = Color.White * alpha;
                    }
                    else if (percent <= 0.5f)
                    {
                        percent += Engine.DeltaTime / duration;
                        currentPos = Position;
                    }
                    else
                    {
                        percent += Engine.DeltaTime / duration;
                        Position = Vector2.Lerp(currentPos, from, Ease.QuintOut(percent - 0.5f));
                        alpha = noMove ? (percent - 0.5f) * 2 : percent;
                        sprite.Color = Color.White * alpha;
                        if (percent >= 1f)
                        {
                            RemoveSelf();
                        }
                    }
                }
                else
                {
                    if (percent < 0.3f)
                    {
                        percent += Engine.DeltaTime / duration;
                        Position = Vector2.Lerp(from, to, Ease.ElasticOut(percent * 2));
                        alpha = noMove ? 1 - percent * 33 : 1 - percent;
                        sprite.Color = Color.White * alpha;
                    }
                    else
                    {
                        percent += Engine.DeltaTime / duration;
                        alpha = noMove ? 0 : 1 - percent;
                        sprite.Color = Color.White * alpha;
                        if (percent >= 1f)
                        {
                            RemoveSelf();
                        }
                    }
                }
            }
        }

        private EntityID ID;

        private int columns;

        private int lines;

        private float reappearFlash;

        private Vector2 anchor;

        private SineWave sine;

        private Coroutine breakRoutine = new();

        private string directory;

        private int[,] bubbles;

        private bool canPlaySounds;

        public BubbleBlock(EntityData data, Vector2 offset, EntityID ID) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            this.ID = ID;
            directory = data.Attr("directory", "objects/XaphanHelper/BubbleBlock");
            columns = data.Width / 8;
            lines = data.Height / 8;
            anchor = Position;
            Add(sine = new SineWave(0.44f, 0f).Randomize());
            Add(new DashListener(OnDashed));
            bubbles = new int[lines, columns];
            //OnDashCollide = OnDashed;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < lines; j++)
                {
                    if (bubbles[j, i] == 0)
                    {
                        bool bigBubble = false;
                        if (j < lines - 1 && i < columns - 1 && (columns == 2 ? lines > 2 : true) && (lines == 2 ? columns > 2 : true))
                        {
                            if (bubbles[j + 1, i] == 0 && bubbles[j, i + 1] == 0)
                            {
                                if (
                                    (j == 0 && i != 0 && bubbles[j, i - 1] == 1) ||
                                    (j != 0 && i == 0 && bubbles[j - 1, i] == 1) ||
                                    (j != 0 && i != 0 && bubbles[j - 1, i] == 1 && bubbles[j, i - 1] == 1)
                                    )
                                {
                                    bigBubble = true;
                                }
                                else
                                {
                                    bigBubble = Calc.Random.Next(0, 101) >= 70;
                                }
                            }
                        }
                        MTexture mTexture = GFX.Game[directory + "/" + (bigBubble ? "bigBubbles" : "bubbles")];
                        int size = bigBubble ? 16 : 8;
                        int textureColumns = mTexture.Width / size;
                        int textureLines = mTexture.Height / size;
                        int selectedTextureColumn = Calc.Random.Next(0, textureColumns - 1);
                        int selectedTextureLine = Calc.Random.Next(0, textureLines);
                        Image image = new(mTexture.GetSubtexture(selectedTextureColumn * size, selectedTextureLine * size, size, size));
                        image.CenterOrigin();
                        image.X = i * 8 + (bigBubble ? 8 : 4);
                        image.Y = j * 8 + (bigBubble ? 8 : 4);
                        Add(image);
                        if (bigBubble)
                        {
                            bubbles[j, i] = 2;
                            bubbles[j + 1, i] = 2;
                            bubbles[j, i + 1] = 2;
                            bubbles[j + 1, i + 1] = 2;
                        }
                        else
                        {
                            bubbles[j, i] = 1;
                        }
                    }
                }
            }
            foreach (BubbleBlock bubbleBlock in SceneAs<Level>().Tracker.GetEntities<BubbleBlock>())
            {
                if (bubbleBlock.ID.Level == SceneAs<Level>().Session.Level)
                {
                    if (bubbleBlock.canPlaySounds)
                    {
                        break;
                    }
                    else
                    {
                        bubbleBlock.canPlaySounds = true;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Collidable)
            {
                sine.Rate = 1f;
                MoveTo(new Vector2((float)(anchor.X + (double)sine.Value * 2f), (float)(anchor.Y + (double)sine.ValueOverTwo * 2f)));
            }
            else
            {
                sine.Rate = 0f;
            }
            reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);
        }

        /*private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Add(new Coroutine(Sequence()));
            return DashCollisionResults.Ignore;
        }*/

        public void OnDashed(Vector2 vector)
        {
            if (!breakRoutine.Active)
            {
                Add(breakRoutine = new Coroutine(Sequence()));
            }
        }

        public void Destroy()
        {
            if (!breakRoutine.Active)
            {
                Add(breakRoutine = new Coroutine(DestroyRoutine()));
            }
        }

        private IEnumerator Sequence()
        {
            float respawnTimer = 3.5f;
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Vector2 vector = new(X + i + 4f, Y + j + 4f);
                    Vector2 mult = new Vector2(Math.Abs(vector.X - Center.X) / (Width / 2), Math.Abs(vector.Y - Center.Y) / (Height / 2)) + Vector2.One * 1.25f;
                    Image debrisImage = null;
                    foreach (Component component in Components)
                    {
                        if (component.GetType() == typeof(Image))
                        {
                            Image image = (Image)component;
                            if (image.X == i + (image.Width == 16 ? 8 : 4) && image.Y == j + (image.Height == 16 ? 8 : 4))
                            {
                                debrisImage = image;
                                if (image.Width == 16 && image.Height == 16)
                                {
                                    vector += Vector2.One * 4f;
                                }
                                break;
                            }
                        }
                    }
                    if (debrisImage != null)
                    {
                        Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector, vector + (vector - Center).SafeNormalize() * 16f * mult, respawnTimer, debrisImage));
                    }
                }
            }
            Collidable = Visible = false;
            DisableStaticMovers();
            if (canPlaySounds)
            {
                yield return 0.05f;
                Audio.Play("event:/game/xaphan/bubble_block_break");
            }
            while (respawnTimer >= 0)
            {
                respawnTimer -= Engine.DeltaTime;
                yield return null;
            }
            reappearFlash = 1.6f;
            Collidable = Visible = true;
            EnableStaticMovers();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (CollideCheck(player))
                {
                    player.Die(Vector2.Zero);
                }
                else if (canPlaySounds)
                {
                    Audio.Play("event:/game/xaphan/bubble_block_reform");
                }
            }
            yield return 0.5f;
        }

        private IEnumerator DestroyRoutine()
        {
            float respawnTimer = 1.5f;
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Vector2 vector = new(X + i + 4f, Y + j + 4f);
                    Vector2 mult = new Vector2(Math.Abs(vector.X - Center.X) / (Width / 2), Math.Abs(vector.Y - Center.Y) / (Height / 2)) + Vector2.One * 1.25f;
                    Image debrisImage = null;
                    foreach (Component component in Components)
                    {
                        if (component.GetType() == typeof(Image))
                        {
                            Image image = (Image)component;
                            if (image.X == i + (image.Width == 16 ? 8 : 4) && image.Y == j + (image.Height == 16 ? 8 : 4))
                            {
                                debrisImage = image;
                                if (image.Width == 16 && image.Height == 16)
                                {
                                    vector += Vector2.One * 4f;
                                }
                                break;
                            }
                        }
                    }
                    if (debrisImage != null)
                    {
                        Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector, vector + (vector - Center).SafeNormalize() * 16f * mult, respawnTimer, debrisImage, false));
                    }
                }
            }
            Collidable = Visible = false;
            DisableStaticMovers();
            Audio.Play("event:/game/xaphan/bubble_block_break", Position);
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            while (respawnTimer >= 0)
            {
                respawnTimer -= Engine.DeltaTime;
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (reappearFlash > 0f)
            {
                float num = Ease.SineInOut(reappearFlash) * 0.75f;
                float num2 = num * 2f;
                foreach (Component component in Components)
                {
                    if (component.GetType() == typeof(Image))
                    {
                        Image image = (Image)component;
                        Draw.Circle(image.Position, 4f, Color.White * num, 8);
                    }
                }
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < lines; j++)
                    {
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 4f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 3f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 2f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 1f, Color.White * num, 8);
                    }
                }
            }
        }
    }
}
