using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/BombBlock")]
    class BombBlock : Solid
    {
        private class PathRenderer : Entity
        {
            public BombBlock Block;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;

            public PathRenderer(BombBlock block)
            {
                base.Depth = 5000;
                Block = block;
                from = Block.start + new Vector2(Block.Width / 2f, Block.Height / 2f);
                to = Block.end + new Vector2(Block.Width / 2f, Block.Height / 2f);
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + (float)Math.PI / 8f;
                sparkDirFromB = num - (float)Math.PI / 8f;
                sparkDirToA = num + (float)Math.PI - (float)Math.PI / 8f;
                sparkDirToB = num + (float)Math.PI + (float)Math.PI / 8f;
                cog = GFX.Game["objects/zipmover/cog"];
            }

            public void CreateSparks()
            {
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
            }

            public override void Render()
            {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero);
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 vector2 = vector.Perpendicular() * 3f;
                Vector2 vector3 = -vector.Perpendicular() * 4f;
                float rotation = Block.percent * (float)Math.PI * 2f;
                Draw.Line(from + vector2 + offset, to + vector2 + offset, colorOverride.HasValue ? colorOverride.Value : Calc.HexToColor(Block.deactivateAllSidesAfterUse ? "4B4B4B" : "663931"));
                Draw.Line(from + vector3 + offset, to + vector3 + offset, colorOverride.HasValue ? colorOverride.Value : Calc.HexToColor(Block.deactivateAllSidesAfterUse ? "4B4B4B" : "663931"));
                for (float num = 4f - Block.percent * (float)Math.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
                {
                    Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
                    Vector2 vector5 = to + vector3 - vector * num;
                    Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : Calc.HexToColor(Block.deactivateAllSidesAfterUse ? "797979" : "9b6157"));
                    Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : Calc.HexToColor(Block.deactivateAllSidesAfterUse ? "797979" : "9b6157"));
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }

        [Tracked(true)]
        public class PushingSide : Entity
        {
            private BombBlock block;

            private float alpha = 0f;

            public string Side;

            public bool oneUse;

            public bool isActive;

            public bool WasActivated;

            public Sprite blackBg;

            public Sprite sprite;

            public PushingSide(BombBlock block, string side, bool active, bool one) : base(block.Position)
            {
                this.block = block;
                Side = side;
                isActive = active;
                oneUse = one;
                Depth = block.Depth - 1;
                Add(blackBg = new Sprite(GFX.Game, block.directory + "/"));
                blackBg.Add("bg", "bg", 0);
                blackBg.Play("bg");
                Add(sprite = new Sprite(GFX.Game, block.directory + "/"));
                sprite.AddLoop("active", "sideActive", 0.08f);
                sprite.AddLoop("oneUse", "sideOneUse", 0.08f);
                sprite.AddLoop("inactive", "sideInactive", 0.08f);
                sprite.Play(isActive ? (oneUse ? "oneUse" : "active") : "inactive");
                if (Side == "Left")
                {
                    blackBg.Rotation = sprite.Rotation = -(float)Math.PI / 2f;
                    blackBg.FlipX = sprite.FlipX = true;
                    Collider = new Hitbox(4, block.Height, 0, 0);
                }
                if (Side == "Right")
                {
                    blackBg.Rotation = sprite.Rotation = -(float)Math.PI / 2f;
                    blackBg.FlipX = sprite.FlipX = true;
                    blackBg.FlipY = sprite.FlipY = true;
                    Collider = new Hitbox(4, block.Height, (int)block.Width - 4, 0);
                }
                if (Side == "Up")
                {
                    Collider = new Hitbox(block.Width, 4, 0, 0);
                }
                if (Side == "Down")
                {
                    blackBg.FlipY = sprite.FlipY = true;
                    Collider = new Hitbox(block.Width, 4, 0, (int)block.Height - 4);
                }
            }

            public override void Update()
            {
                Position = block.Position;
                alpha += Engine.DeltaTime * 4f;
                sprite.Play(isActive ? (oneUse ? "oneUse" : "active") : "inactive");
            }

            public void swapActive()
            {
                Active = !Active;
            }

            public override void Render()
            {
                float opacity = isActive ? (0.9f * (0.9f + ((float)Math.Sin(alpha) + 1f) * 0.125f)) : 0.3f;
                sprite.Color = Color.White * opacity;
                if (Side == "Left")
                {
                    // Draw top tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitY * 8;
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));

                    // Draw middle tiles
                    for (int i = 1; i <= (block.Height - 16) / 8; i++)
                    {
                        blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitY * (i * 8 + 8);
                        blackBg.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                    }

                    // Draw bottom tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitY * block.Height;
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                }
                else if (Side == "Right")
                {
                    // Draw top tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + new Vector2(block.Width - 8, 8);
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));

                    // Draw middle tiles
                    for (int i = 1; i <= (block.Height - 16) / 8; i++)
                    {
                        blackBg.RenderPosition = sprite.RenderPosition = Position + new Vector2(block.Width - 8, (i * 8 + 8));
                        blackBg.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                    }

                    // Draw bottom tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + new Vector2(block.Width - 8, block.Height);
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                }
                else if (Side == "Up")
                {
                    // Draw left tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position;
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));

                    // Draw middle tiles
                    for (int i = 1; i <= (block.Width - 16) / 8; i++)
                    {
                        blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitX * i * 8;
                        blackBg.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                    }

                    // Draw right tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitX * (block.Width - 8);
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                }
                if (Side == "Down")
                {
                    // Draw left tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + Vector2.UnitY * (block.Height - 8);
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(0, 0, 8, 8));

                    // Draw middle tiles
                    for (int i = 1; i <= (block.Width - 16) / 8; i++)
                    {
                        blackBg.RenderPosition = sprite.RenderPosition = Position + new Vector2(i * 8, block.Height - 8);
                        blackBg.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                        sprite.DrawSubrect(Vector2.Zero, new Rectangle(8, 0, 8, 8));
                    }

                    // Draw right tile
                    blackBg.RenderPosition = sprite.RenderPosition = Position + new Vector2(block.Width - 8, block.Height - 8);
                    blackBg.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                    sprite.DrawSubrect(Vector2.Zero, new Rectangle(16, 0, 8, 8));
                }
            }
        }

        public ParticleType P_Move;

        private const float ReturnTime = 0.8f;

        public Vector2 Direction;

        public bool Swapping;

        private Vector2 start;

        private Vector2 end;

        private float lerp;

        private int target;

        private float percent;

        private float setSpeed;

        private float speed;

        private float maxForwardSpeed;

        private float maxBackwardSpeed;

        private float returnTimer;

        private float redAlpha = 1f;

        private MTexture[,] nineSliceGreen;

        private MTexture[,] nineSliceRed;

        private MTexture[,] nineSliceTarget;

        private Sprite middleGreen;

        private Sprite middleRed;

        private PathRenderer path;

        private EventInstance moveSfx;

        private EventInstance returnSfx;

        private float particlesRemainder;

        public string flag;

        public string directory = "objects/swapblock";

        public bool isActive;

        public bool toggle;

        public string leftSide;

        public string rightSide;

        public string topSide;

        public string bottomSide;

        public bool swapActiveSides;

        public bool deactivateAllSidesAfterUse;

        public List<PushingSide> pushSides = new();

        public BombBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            directory = data.Attr("directory", "objects/swapblock");
            if (directory == "objects/swapblock")
            {
                directory = data.Attr("sprite", "objects/swapblock");
            }
            flag = data.Attr("flag");
            toggle = data.Bool("toggle");
            leftSide = data.Attr("leftSide");
            rightSide = data.Attr("rightSide");
            topSide = data.Attr("topSide");
            bottomSide = data.Attr("bottomSide");
            swapActiveSides = data.Bool("swapActiveSides");
            deactivateAllSidesAfterUse = data.Bool("deactivateAllSidesAfterUse");
            P_Move = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(data.Attr("particleColor1", "fbf236")),
                Color2 = Calc.HexToColor(data.Attr("particleColor2", "6abe30")),
                ColorMode = ParticleType.ColorModes.Blink,
                DirectionRange = 0.6981317f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.3f,
                LifeMin = 0.3f,
                LifeMax = 0.5f
            };
            Vector2 node = data.Nodes[0] + offset;
            redAlpha = 1f;
            start = Position;
            end = node;
            setSpeed = data.Float("speed");
            maxForwardSpeed = data.Float("speed", 360f) / Vector2.Distance(start, end);
            maxBackwardSpeed = maxForwardSpeed * 0.4f;
            Direction.X = Math.Sign(end.X - start.X);
            Direction.Y = Math.Sign(end.Y - start.Y);
            int num = (int)MathHelper.Min(X, node.X);
            int num2 = (int)MathHelper.Min(Y, node.Y);
            int num3 = (int)MathHelper.Max(X + Width, node.X + Width);
            int num4 = (int)MathHelper.Max(Y + Height, node.Y + Height);
            MTexture mtexture = GFX.Game[directory + "/block"];
            MTexture mtexture2 = GFX.Game[directory + "/blockRed"];
            MTexture mtexture3 = GFX.Game[directory + "/target"];
            nineSliceGreen = new MTexture[3, 3];
            nineSliceRed = new MTexture[3, 3];
            nineSliceTarget = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSliceGreen[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceRed[i, j] = mtexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceTarget[i, j] = mtexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            middleGreen = new Sprite(GFX.Game, directory + "/midBlock");
            middleGreen.AddLoop("idle", "", 0.08f);
            middleGreen.Justify = new Vector2(0.5f, 0.5f);
            middleGreen.Play("idle");
            Add(middleGreen);
            middleRed = new Sprite(GFX.Game, directory + "/midBlockRed");
            middleRed.AddLoop("idle", "", 0.08f);
            middleRed.Justify = new Vector2(0.5f, 0.5f);
            middleRed.Play("idle");
            Add(middleRed);
            Add(new LightOcclude(0.2f));
            Add(new Coroutine(Sequence()));
            Depth = -9999;
        }

        private IEnumerator Sequence()
        {
            while (true)
            {
                yield return null;
                if (lerp != 0)
                {
                    percent = Ease.SineIn(lerp);
                    if (Scene.OnInterval(0.1f) && lerp != 0 && lerp != 1)
                    {
                        path.CreateSparks();
                    }
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(path = new PathRenderer(this));
            if (leftSide.ToLower().Contains("active"))
            {
                pushSides.Add(new PushingSide(this, "Left", (leftSide == "Start Active" || leftSide == "Active Only Once") ? true : false, leftSide == "Active Only Once" ? true : false));
            }
            if (rightSide.ToLower().Contains("active"))
            {
                pushSides.Add(new PushingSide(this, "Right", (rightSide  == "Start Active" || rightSide == "Active Only Once") ? true : false, rightSide == "Active Only Once" ? true : false));
            }
            if (topSide.ToLower().Contains("active"))
            {
                pushSides.Add(new PushingSide(this, "Up", (topSide == "Start Active" || topSide == "Active Only Once") ? true : false, topSide == "Active Only Once" ? true : false));
            }
            if (bottomSide.ToLower().Contains("active"))
            {
                pushSides.Add(new PushingSide(this, "Down", (bottomSide == "Start Active" || bottomSide == "Active Only Once") ? true : false, bottomSide == "Active Only Once" ? true : false));
            }

            foreach (PushingSide side in pushSides)
            {
                scene.Add(side);
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }

        public void Push()
        {
            if (isActive)
            {
                Swapping = (lerp < 1f);
                if (!toggle)
                {
                    target = 1;
                    returnTimer = 0.8f;
                    if (lerp >= 0.2f)
                    {
                        speed = maxForwardSpeed;
                    }
                    else
                    {
                        speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
                    }
                    Audio.Stop(returnSfx);
                    Audio.Stop(moveSfx);
                    if (!Swapping)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    }
                    else
                    {
                        moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
                    }
                }
                else
                {
                    Audio.Stop(returnSfx);
                    Audio.Stop(moveSfx);
                    if (target == 1)
                    {
                        returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
                        target = 0;
                    }
                    else
                    {
                        moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
                        target = 1;
                    }
                    returnTimer = 0.8f;
                    if (lerp >= 0.2f)
                    {
                        speed = maxForwardSpeed;
                    }
                    else
                    {
                        speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
                    }
                    if (!Swapping)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    }
                    
                }
                foreach (PushingSide side in pushSides)
                {
                    if (!deactivateAllSidesAfterUse)
                    {
                        if (swapActiveSides)
                        {
                            side.isActive = !side.isActive;
                        }
                        if (side.isActive && side.oneUse && side.WasActivated)
                        {
                            side.isActive = false;
                        }
                    }
                    else
                    {
                        side.isActive = false;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (flag == "" || SceneAs<Level>().Session.GetFlag(flag))
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            if (returnTimer > 0f)
            {
                returnTimer -= Engine.DeltaTime;
                if (returnTimer <= 0f)
                {
                    if (!toggle)
                    {
                        target = 0;
                        returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
                    }
                    speed = 0f;
                }
            }
            redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
            if (target == 0 && lerp == 0f)
            {
                middleRed.SetAnimationFrame(0);
                middleGreen.SetAnimationFrame(0);
            }
            if (target == 1)
            {
                speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
            }
            else
            {
                speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
            }
            float num = lerp;
            lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            if (lerp != num)
            {
                Vector2 liftSpeed = (end - start) * speed;
                Vector2 position = Position;
                if (target == 1)
                {
                    liftSpeed = (end - start) * maxForwardSpeed;
                }
                if (lerp < num)
                {
                    liftSpeed *= -1f;
                }
                if (target == 1 && Scene.OnInterval((setSpeed == 180f) ? 0.03f : ((setSpeed == 360f) ? 0.02f : 0.01f)))
                {
                    MoveParticles(end - start);
                }
                if (target == 0 && toggle && Scene.OnInterval((setSpeed == 180f) ? 0.03f : ((setSpeed == 360f) ? 0.02f : 0.01f)))
                {
                    MoveParticles(start - end);
                }
                MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
                if (position != Position)
                {
                    Audio.Position(moveSfx, Center);
                    Audio.Position(returnSfx, Center);
                    if (Position == start && target == 0)
                    {
                        Audio.SetParameter(returnSfx, "end", 1f);
                        Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                    }
                    else if (Position == end && target == 1)
                    {
                        if (toggle)
                        {
                            Audio.SetParameter(moveSfx, "end", 1f);
                        }
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    }
                }
            }
            if (Swapping && lerp >= 1f)
            {
                Swapping = false;
            }
            StopPlayerRunIntoAnimation = (lerp <= 0f || lerp >= 1f);
        }

        private void MoveParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 positionRange;
            float direction;
            float num;
            if (normal.X > 0f)
            {
                position = CenterLeft;
                positionRange = Vector2.UnitY * (Height - 6f);
                direction = (float)Math.PI;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.X < 0f)
            {
                position = CenterRight;
                positionRange = Vector2.UnitY * (Height - 6f);
                direction = 0f;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.Y > 0f)
            {
                position = TopCenter;
                positionRange = Vector2.UnitX * (Width - 6f);
                direction = -(float)Math.PI / 2f;
                num = Math.Max(2f, Width / 14f);
            }
            else
            {
                position = BottomCenter;
                positionRange = Vector2.UnitX * (Width - 6f);
                direction = (float)Math.PI / 2f;
                num = Math.Max(2f, Width / 14f);
            }
            particlesRemainder += num;
            int num2 = (int)particlesRemainder;
            particlesRemainder -= num2;
            positionRange *= 0.5f;
            SceneAs<Level>().Particles.Emit(P_Move, num2, position, positionRange, direction);
        }

        public override void Render()
        {
            Vector2 vector = Position + Shake;
            if (redAlpha < 1f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceGreen, middleGreen, Color.White);
            }
            if (redAlpha > 0f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceRed, middleRed, Color.White * redAlpha);
            }
        }

        private void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, Sprite middle, Color color)
        {
            int num = (int)(width / 8f);
            int num2 = (int)(height / 8f);
            ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
            ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
            ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
            ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
            for (int i = 1; i < num - 1; i++)
            {
                ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
            }
            for (int j = 1; j < num2 - 1; j++)
            {
                ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
            }
            for (int k = 1; k < num - 1; k++)
            {
                for (int l = 1; l < num2 - 1; l++)
                {
                    ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }
            if (middle != null)
            {
                middle.Color = color;
                middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
                middle.Render();
            }
        }
    }
}
