using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Conveyor")]
    class Conveyor : Solid
    {
        public int conveyorSpeed;

        public int direction;

        private string swapFlag;

        private string activeFlag;

        private string forceInactiveFlag;

        private bool swaped;

        private Coroutine moveRoutine = new();

        private Coroutine actorsMoveRoutine = new();

        private Coroutine moveSpritesRoutine = new();

        private int currentTotalActors;

        private List<Actor> actors = new();

        private List<Sprite> sprites = new();

        private Sprite bgSprite;

        private Sprite fgSprite;

        private string directory;

        public Conveyor(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, 8, safe: false)
        {
            Collider = new Hitbox(Width, 8);
            conveyorSpeed = data.Int("speed", 75);
            direction = data.Int("direction", -1);
            swapFlag = data.Attr("swapFlag", "");
            activeFlag = data.Attr("activeFlag", "");
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            directory = data.Attr("directory", "objects/XaphanHelper/Conveyor");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/Conveyor";
            }
            sprites = BuildSprite();
            Add(bgSprite = new Sprite(GFX.Game, directory + "/"));
            bgSprite.AddLoop("bgleft", "bg", 0.08f, 0);
            bgSprite.AddLoop("bgmid", "bg", 0.08f, 1);
            bgSprite.AddLoop("bgright", "bg", 0.08f, 2);
            Add(fgSprite = new Sprite(GFX.Game, directory + "/"));
            fgSprite.AddLoop("fgleft", "fg", 0.08f, 0);
            fgSprite.AddLoop("fgright", "fg", 0.08f, 1);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(activeFlag) || SceneAs<Level>().Session.GetFlag(activeFlag))
            {
                Add(moveSpritesRoutine = new Coroutine(MoveSprites()));
            }
        }

        public List<Sprite> BuildSprite()
        {
            List<Sprite> list = new();
            for (int i = -8; i <= Width + 8; i++)
            {
                Sprite sprite = new(GFX.Game, directory + "/");
                sprite.AddLoop("belt", "belt", 0f);
                sprite.Play("belt");
                sprite.Position = Vector2.UnitX * i;
                list.Add(sprite);
                Add(sprite);
            }
            return list;
        }

        public override void Update()
        {
            base.Update();
            if (!moveSpritesRoutine.Active && ((string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) && (string.IsNullOrEmpty(activeFlag) || SceneAs<Level>().Session.GetFlag(activeFlag))))
            {
                Add(moveSpritesRoutine = new Coroutine(MoveSprites()));
            }
            if (!string.IsNullOrEmpty(swapFlag) && ((SceneAs<Level>().Session.GetFlag(swapFlag) && !swaped) || (!SceneAs<Level>().Session.GetFlag(swapFlag) && swaped)))
            {
                direction = -direction;
                swaped = !swaped;
            }
            if ((string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) && (string.IsNullOrEmpty(activeFlag) || SceneAs<Level>().Session.GetFlag(activeFlag)))
            {
                if (HasPlayerOnTop())
                {
                    Player player = GetPlayerOnTop();
                    if (player != null && !moveRoutine.Active)
                    {
                        Add(moveRoutine = new Coroutine(MovePlayerRoutine(this)));
                    }
                }
                currentTotalActors = Scene.Tracker.GetEntities<Actor>().Count;
                if (currentTotalActors > 0 && !actorsMoveRoutine.Active)
                {
                    actors.Clear();
                    foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
                    {
                        actors.Add(actor);
                    }
                    Add(actorsMoveRoutine = new Coroutine(MoveActors(currentTotalActors)));
                }
            }
        }

        private IEnumerator MoveSprites()
        {
            while ((string.IsNullOrEmpty(forceInactiveFlag) || !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) && (string.IsNullOrEmpty(activeFlag) || SceneAs<Level>().Session.GetFlag(activeFlag)))
            {
                foreach (Sprite sprite in sprites)
                {
                    sprite.Position.X += conveyorSpeed * Engine.DeltaTime * direction;
                    if (sprite.Position.X > Width + 8)
                    {
                        sprite.Position.X -= (Width + 8);
                    }
                    else if (sprite.Position.X < -8)
                    {
                        sprite.Position.X += (Width + 8);
                    }
                }
                yield return null;
            }
        }

        private IEnumerator MovePlayerRoutine(Conveyor conveyor)
        {
            while (conveyor.HasPlayerRider())
            {
                conveyor.GetPlayerRider().LiftSpeed = Vector2.UnitX * conveyorSpeed * direction;
                conveyor.GetPlayerRider().MoveH(conveyorSpeed * Engine.DeltaTime * direction);
                yield return null;
            }
        }

        private IEnumerator MoveActors(int currentTotalActors)
        {
            while (currentTotalActors == this.currentTotalActors)
            {
                foreach (Actor actor in actors)
                {
                    if (actor.GetType() != typeof(Player) && actor.GetType() != typeof(FakePlayer) && actor.GetType() != typeof(Drone) && actor.GetType() != typeof(DroneDebris) && actor.GetType() != typeof(Debris) && actor.IsRiding(this) && actor.AllowPushing)
                    {
                        actor.MoveH(conveyorSpeed * Engine.DeltaTime * direction);
                        actor.Bottom = Top;
                    }
                }
                yield return null;
            }
        }

        public override void Render()
        {
            for (int i = 0; i < Width / 8; i++)
            {
                bgSprite.RenderPosition = Position + new Vector2(i * 8, 0);
                bgSprite.Play(i == 0 ? "bgleft" : (i > 0 && i < (Width / 8 - 1)) ? "bgmid" : "bgright");
                bgSprite.Render();
            }
            for (int i = 0; i < Width + 16; i++)
            {
                if (sprites[i].Position.X >= 0 && sprites[i].Position.X < (Width - 1))
                {
                    sprites[i].Visible = true;
                    sprites[i].DrawSubrect(Vector2.Zero, new Rectangle(i % 8, 0, 1, 8));
                }
                else
                {
                    sprites[i].Visible = false;
                }
            }
            fgSprite.RenderPosition = Position;
            fgSprite.Play("fgleft");
            fgSprite.Render();
            fgSprite.RenderPosition = Position + new Vector2(Width - 8, 0);
            fgSprite.Play("fgright");
            fgSprite.Render();
        }
    }
}
