using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/TriggerBlock")]
    public class TriggerBlock : Solid
    {
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        private char fillTile;

        private TileGrid tiles;

        private Coroutine SequenceRoutine = new();

        private Vector2 start;

        private Vector2 target;

        private float percent = 0f;

        private string flag;

        private Direction direction;

        private float moveSpeed;

        private float returnSpeed;

        private SoundSource sfx;

        public TriggerBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            Tag = Tags.TransitionUpdate;
            start = Position;
            fillTile = data.Char("tiletype", '3');
            flag = data.Attr("flag");
            direction = data.Enum("direction", Direction.Left);
            moveSpeed = data.Float("moveSpeed", 3f);
            returnSpeed = data.Float("returnSpeed", 0.25f);
            switch (direction)
            {
                case Direction.Left:
                    target = Position - Vector2.UnitX * (Width - 16);
                    break;
                case Direction.Right:
                    target = Position + Vector2.UnitX * (Width - 16);
                    break;
                case Direction.Up:
                    target = Position - Vector2.UnitY * (Height - 16);
                    break;
                case Direction.Down:
                    target = Position + Vector2.UnitY * (Height - 16);
                    break;
            }
            Add(sfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Level level = SceneAs<Level>();
            tiles = GFX.FGAutotiler.GenerateBox(fillTile, tilesX, tilesY).TileGrid;
            Add(tiles);
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag) && !SequenceRoutine.Active)
            {
                Add(SequenceRoutine = new Coroutine(Sequence()));
            }
        }

        private IEnumerator Sequence()
        {
            float at = percent;
            while (at < 1f)
            {
                if (!sfx.Playing && !SceneAs<Level>().Transitioning)
                {
                    sfx.Play("event:/game/03_resort/platform_vert_down_loop");
                }
                if (moveSpeed > returnSpeed)
                {
                    sfx.Param("ducking", 1);
                }
                yield return null;
                at = Calc.Approach(at, 1f, (SceneAs<Level>().Transitioning ? 9999 : moveSpeed) * Engine.DeltaTime);
                percent = at;
                Vector2 to = Vector2.Lerp(start, target, percent);
                MoveTo(to);
            }
            sfx.Stop();
            if (!SceneAs<Level>().Transitioning)
            {
                Audio.Play("event:/game/03_resort/platform_vert_start", Position);
                StartShaking(0.1f);
            }
            while (SceneAs<Level>().Session.GetFlag(flag))
            {
                yield return null;
            }
            at = 0f;
            while (at < 1f && !SceneAs<Level>().Session.GetFlag(flag))
            {
                if (!sfx.Playing)
                {
                    sfx.Play("event:/game/03_resort/platform_vert_down_loop");
                }
                if (returnSpeed > moveSpeed)
                {
                    sfx.Param("ducking", 1);
                }
                yield return null;
                at = Calc.Approach(at, 1f, returnSpeed * Engine.DeltaTime);
                percent = 1f - at;
                Vector2 to = Vector2.Lerp(target, start, at);
                MoveTo(to);
            }
            sfx.Stop();
            Audio.Play("event:/game/03_resort/platform_vert_end", Position);
        }
    }
}
