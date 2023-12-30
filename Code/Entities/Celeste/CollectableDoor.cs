using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CollectableDoor")]
    public class CollectableDoor : Entity
    {
        private struct Particle
        {
            public Vector2 Position;

            public float Speed;

            public Color Color;
        }

        private class WhiteLine : Entity
        {
            private float fade = 1f;

            private int blockSize;

            private string Color;

            private bool Vertical;

            public WhiteLine(Vector2 origin, int blockSize, string color, bool vertical = false)
                : base(origin)
            {
                Depth = -1000000;
                this.blockSize = blockSize;
                Color = color;
                Vertical = vertical;
            }

            public override void Update()
            {
                base.Update();
                fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
                if (!(fade <= 0f))
                {
                    return;
                }
                RemoveSelf();
                Level level = SceneAs<Level>();
                if (!Vertical)
                {
                    for (float num = (int)level.Camera.Left; num < level.Camera.Right; num += 1f)
                    {
                        if (num < X || num >= X + blockSize)
                        {
                            level.Particles.Emit(P_SliceH, new Vector2(num, Y));
                        }
                    }
                }
                else
                {
                    for (float num = (int)level.Camera.Top; num < level.Camera.Bottom; num += 1f)
                    {
                        if (num < Y || num >= Y + blockSize)
                        {
                            level.Particles.Emit(P_SliceV, new Vector2(X, num));
                        }
                    }
                }
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                float num = Math.Max(1f, 4f * fade);
                if (!Vertical)
                {
                    Draw.Rect(position.X - 10f, Y - num / 2f, 340f, num, Calc.HexToColor(Color));
                }
                else
                {
                    Draw.Rect(X - num / 2f, position.Y - 10f, num, 184f, Calc.HexToColor(Color));
                }
            }
        }

        public static ParticleType P_Shimmer = new();

        public static ParticleType P_SliceH = new();

        public static ParticleType P_SliceV = new();

        public readonly int Requires;

        public int Size;

        public int height;

        private readonly float openDistance;

        private float openPercent;

        private Solid TopSolid;

        private Solid BotSolid;

        private float offset;

        private Vector2 mist;

        private MTexture temp = new();

        private List<MTexture> icon;

        private Particle[] particles = new Particle[50];

        private float heartAlpha = 1f;

        public int Collectables
        {
            get
            {
                if (SaveData.Instance.CheatMode)
                {
                    return Requires;
                }
                string levelSetToCheck = (string.IsNullOrEmpty(levelSet) ? SceneAs<Level>().Session.Area.LevelSet : levelSet);
                if (mode == "TotalHearts")
                {
                    int TotalheartCount = 0;
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == levelSetToCheck)
                        {
                            int heartCount = 0;
                            for (int i = 0; i < item.Modes.Length; i++)
                            {
                                if (item.Modes[i].HeartGem)
                                {
                                    heartCount += 1;
                                }
                            }
                            TotalheartCount += heartCount;
                        }
                    }
                    return TotalheartCount;
                }
                if (mode == "CurrentChapterHeart")
                {
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == SceneAs<Level>().Session.Area.LevelSet && item.SID == SceneAs<Level>().Session.Area.SID)
                        {
                            if (item.Modes[(int)SceneAs<Level>().Session.Area.Mode].HeartGem)
                            {
                                return 1;
                            }
                        }
                    }
                    return 0;
                }
                if (mode == "CurrentSessionHeart")
                {
                    return SceneAs<Level>().Session.HeartGem ? 1 : 0;
                }
                if (mode == "TotalCassettes")
                {
                    int TotalCassetteCount = 0;
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == levelSetToCheck)
                        {
                            int cassetteCount = 0;
                            if (item.Cassette)
                            {
                                cassetteCount += 1;
                            }
                            TotalCassetteCount += cassetteCount;
                        }
                    }
                    return TotalCassetteCount;
                }
                if (mode == "CurrentChapterCassette")
                {
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == SceneAs<Level>().Session.Area.LevelSet && item.SID == SceneAs<Level>().Session.Area.SID)
                        {
                            if (item.Cassette)
                            {
                                return 1;
                            }
                        }
                    }
                    return 0;
                }
                if (mode == "CurrentSessionCassette")
                {
                    return SceneAs<Level>().Session.Cassette ? 1 : 0;
                }
                if (mode == "TotalStrawberries")
                {
                    int TotalStrawberryCount = 0;
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == levelSetToCheck)
                        {
                            AreaData areaData = AreaData.Get(item.ID_Safe);
                            int strawberryCount = 0;
                            if (areaData.Mode[0].TotalStrawberries > 0 || item.TotalStrawberries > 0)
                            {
                                strawberryCount = item.TotalStrawberries;
                            }
                            TotalStrawberryCount += strawberryCount;
                        }
                    }
                    return TotalStrawberryCount;
                }
                if (mode == "CurrentChapterStrawberries")
                {
                    int TotalStrawberryCount = 0;
                    foreach (AreaStats item in SaveData.Instance.Areas_Safe)
                    {
                        if (item.LevelSet == SceneAs<Level>().Session.Area.LevelSet && item.SID == SceneAs<Level>().Session.Area.SID)
                        {
                            AreaData areaData = AreaData.Get(item.ID_Safe);
                            int strawberryCount = 0;
                            if (areaData.Mode[0].TotalStrawberries > 0 || item.TotalStrawberries > 0)
                            {
                                strawberryCount = item.TotalStrawberries;
                            }
                            TotalStrawberryCount += strawberryCount;
                        }
                    }
                    return TotalStrawberryCount;
                }
                if (mode == "CurrentSessionStrawberries")
                {
                    return SceneAs<Level>().Session.Strawberries.Count;
                }
                if (mode == "GoldenStrawberry")
                {
                    foreach (Strawberry item in SceneAs<Level>().Entities.FindAll<Strawberry>())
                    {
                        if (item.Golden && item.Follower.Leader != null)
                        {
                            return 1;
                        }
                    }
                    return 0;
                }
                if (mode == "Flags")
                {
                    int TotalFlagCount = 0;
                    string[] flags = this.flags.Split(',');
                    foreach (string flag in flags)
                    {
                        int flagCount = 0;
                        if (SceneAs<Level>().Session.GetFlag(flag))
                        {
                            flagCount += 1;
                        }
                        TotalFlagCount += flagCount;
                    }
                    return TotalFlagCount;
                }
                return Requires;
            }
        }

        public float Counter
        {
            get;
            private set;
        }

        public bool Opened
        {
            get;
            private set;
        }

        private float openAmount => openPercent * openDistance;

        private string orientation;

        private string mode;

        private string flags;

        private string interiorColor;

        private string mistColor;

        private string edgesColor;

        private string iconsColor;

        private string interiorParticlesColor;

        private string sliceColor;

        private string directory;

        private int checkDistance;

        private int checkDisplaySpeed;

        private string doorID;

        private string unlockSound;

        private string fillSound;

        private int soundIndex;

        private float beforeSliceDelay;

        private float afterSliceDelay;

        private float openSpeedMultiplier;

        private string edges;

        private string edgesAnimationMode;

        private string mapIcon;

        private bool registerInSaveData;

        private string levelSet;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.LevelSet;
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Opened_Collectable_Door_" + doorID + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public CollectableDoor(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            doorID = id.Level + "_" + id.ID;
            orientation = data.Attr("orientation");
            Requires = data.Int("requires");
            height = data.Height;
            mode = data.Attr("mode");
            flags = data.Attr("flags");
            interiorColor = data.Attr("interiorColor");
            mistColor = data.Attr("mistColor");
            edgesColor = data.Attr("edgesColor");
            iconsColor = data.Attr("iconsColor");
            interiorParticlesColor = data.Attr("interiorParticlesColor");
            sliceColor = data.Attr("sliceColor");
            directory = data.Attr("directory", "objects/heartdoor");
            checkDistance = data.Int("checkDistance", 10);
            checkDisplaySpeed = data.Int("checkDisplaySpeed", 5);
            unlockSound = data.Attr("unlockSound", "event:/game/09_core/frontdoor_unlock");
            fillSound = data.Attr("fillSound", "event:/game/09_core/frontdoor_heartfill");
            soundIndex = data.Int("soundIndex", 32);
            beforeSliceDelay = data.Float("beforeSliceDelay", 0.5f);
            afterSliceDelay = data.Float("afterSliceDelay", 0.6f);
            openSpeedMultiplier = data.Float("openSpeedMultiplier", 1);
            edges = data.Attr("edges", "All");
            edgesAnimationMode = data.Attr("edgesAnimationMode", "Clockwise");
            mapIcon = data.Attr("mapIcon");
            registerInSaveData = data.Bool("registerInSaveData");
            levelSet = data.Attr("levelSet");
            Add(new CustomBloom(RenderBloom));
            Size = data.Width;
            openDistance = 32f;
            if (mode == "GoldenStrawberry")
            {
                Requires = 1;
            }
            Vector2? vector = data.FirstNodeNullable(offset);
            if (vector.HasValue)
            {
                if (orientation == "Vertical")
                {
                    openDistance = Math.Abs(vector.Value.Y - Y);
                }
                else
                {
                    openDistance = Math.Abs(vector.Value.X - X);
                }
            }
            if (!string.IsNullOrEmpty(directory))
            {
                icon = GFX.Game.GetAtlasSubtextures(directory + "/icon");
            }
            else
            {
                if (mode.Contains("Heart"))
                {
                    icon = GFX.Game.GetAtlasSubtextures("objects/XaphanHelper/CollectableDoor/Heart/icon");
                }
                else if (mode.Contains("Strawberries"))
                {
                    icon = GFX.Game.GetAtlasSubtextures("objects/XaphanHelper/CollectableDoor/Strawberry/icon");
                }
                else if (mode.Contains("Golden"))
                {
                    icon = GFX.Game.GetAtlasSubtextures("objects/XaphanHelper/CollectableDoor/GoldenStrawberry/icon");
                }
                else if (mode.Contains("Cassette"))
                {
                    icon = GFX.Game.GetAtlasSubtextures("objects/XaphanHelper/CollectableDoor/Cassette/icon");
                }
                else if (mode.Contains("Flag"))
                {
                    icon = GFX.Game.GetAtlasSubtextures("objects/XaphanHelper/CollectableDoor/Flag/icon");
                }
            }
            P_Shimmer = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(data.Attr("openingParticlesColor1", "baffff")),
                Color2 = Calc.HexToColor(data.Attr("openingParticlesColor2", "5abce2")),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                SpeedMin = 2f,
                SpeedMax = 5f,
                DirectionRange = (float)Math.PI / 3f,
                LifeMin = 1.4f,
                LifeMax = 2f
            };
            P_SliceH = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(data.Attr("sliceParticlesColor1", "ffffff")),
                Color2 = Calc.HexToColor(data.Attr("sliceParticlesColor2", "ffffff")) * 0.65f,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                SpeedMin = 0f,
                SpeedMax = 30f,
                Acceleration = Vector2.UnitY * 20f,
                DirectionRange = 0f,
                Direction = -(float)Math.PI / 2f,
                LifeMin = 0.4f,
                LifeMax = 1.8f
            };
            P_SliceV = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor(data.Attr("sliceParticlesColor1", "ffffff")),
                Color2 = Calc.HexToColor(data.Attr("sliceParticlesColor2", "ffffff")) * 0.65f,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                SpeedMin = 0f,
                SpeedMax = 30f,
                Acceleration = Vector2.UnitY * 20f,
                DirectionRange = 2f,
                Direction = (float)Math.PI / 2f,
                LifeMin = 0.4f,
                LifeMax = 1.8f
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(level.Bounds.Width), Calc.Random.NextFloat(level.Bounds.Height));
                particles[i].Speed = Calc.Random.Range(4, 12);
                particles[i].Color = Calc.HexToColor(interiorParticlesColor) * Calc.Random.Range(0.2f, 0.6f);
            }
            if (orientation == "Vertical")
            {
                level.Add(TopSolid = new Solid(new Vector2(X, Y - height), Size, height, safe: true));
                level.Add(BotSolid = new Solid(new Vector2(X, Y), Size, height, safe: true));
            }
            else
            {
                level.Add(TopSolid = new Solid(new Vector2(X - Size, Y), Size, height, safe: true));
                level.Add(BotSolid = new Solid(new Vector2(X, Y), Size, height, safe: true));
            }
            TopSolid.SurfaceSoundIndex = soundIndex;
            TopSolid.SquishEvenInAssistMode = true;
            TopSolid.EnableAssistModeChecks = false;
            BotSolid.SurfaceSoundIndex = soundIndex;
            BotSolid.SquishEvenInAssistMode = true;
            BotSolid.EnableAssistModeChecks = false;
            if (SceneAs<Level>().Session.GetFlag("XaphanHelper_Opened_Collectable_Door_" + doorID) || FlagRegiseredInSaveData())
            {
                Opened = true;
                Visible = true;
                openPercent = 1f;
                Counter = Requires;
                if (orientation == "Vertical")
                {
                    TopSolid.Y -= openDistance;
                    BotSolid.Y += openDistance;
                }
                else
                {
                    TopSolid.X -= openDistance;
                    BotSolid.X += openDistance;
                }
            }
            else
            {
                if (orientation == "Vertical")
                {
                    Add(new Coroutine(VerticalRoutine()));
                }
                else
                {
                    Add(new Coroutine(HorizontalRoutine()));
                }
            }
        }

        private IEnumerator VerticalRoutine()
        {
            Level level = Scene as Level;
            float botFrom;
            float topFrom;
            float botTo;
            float topTo;
            while (!Opened && Counter < Requires)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Math.Abs(player.Center.X - Position.X - Size / 2) < (checkDistance * 8 + Size / 2) && player.Top > TopSolid.Top - 32f && player.Bottom < BotSolid.Bottom + 32f)
                {
                    if (Counter == 0f && Collectables > 0)
                    {
                        Audio.Play(fillSound, Position);
                    }
                    int num = (int)Counter;
                    int target = Math.Min(Collectables, Requires);
                    Counter = Calc.Approach(Counter, target, Engine.DeltaTime * checkDisplaySpeed);
                    if (num != (int)Counter)
                    {
                        if (Counter < target)
                        {
                            Audio.Play(fillSound, Position);
                        }
                    }
                }
                else
                {
                    Counter = Calc.Approach(Counter, 0f, Engine.DeltaTime * 20f);
                }
                yield return null;
            }
            yield return beforeSliceDelay;
            Scene.Add(new WhiteLine(Position, Size, sliceColor));
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White * 0.5f);
            Audio.Play(unlockSound, Position);
            Opened = true;
            level.Session.SetFlag("XaphanHelper_Opened_Collectable_Door_" + doorID);
            if (registerInSaveData)
            {
                registerDoorOpenInSaveData();
            }
            MiniMap minimap = SceneAs<Level>().Tracker.GetEntity<MiniMap>();
            if (minimap != null && !level.Session.GetFlag("Map_Opened") && !level.InCutscene)
            {
                minimap.mapDisplay.GenerateIcons();
            }
            offset = 0f;
            yield return afterSliceDelay;
            botFrom = TopSolid.Y;
            topFrom = TopSolid.Y - openDistance;
            botTo = BotSolid.Y;
            topTo = BotSolid.Y + openDistance;
            for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * openSpeedMultiplier)
            {
                level.Shake();
                openPercent = Ease.CubeIn(p2);
                TopSolid.MoveToY(MathHelper.Lerp(botFrom, topFrom, openPercent));
                BotSolid.MoveToY(MathHelper.Lerp(botTo, topTo, openPercent));
                if (p2 >= 0.4f && level.OnInterval(0.1f))
                {
                    for (int i = 4; i < Size; i += 4)
                    {
                        level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(TopSolid.Left + i + 1f, TopSolid.Bottom - 2f), new Vector2(2f, 2f), -(float)Math.PI / 2f);
                        level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(BotSolid.Left + i + 1f, BotSolid.Top + 2f), new Vector2(2f, 2f), (float)Math.PI / 2f);
                    }
                }
                yield return null;
            }
            TopSolid.MoveToY(topFrom);
            BotSolid.MoveToY(topTo);
            openPercent = 1f;
        }

        private IEnumerator HorizontalRoutine()
        {
            Level level = Scene as Level;
            AreaKey area = level.Session.Area;
            int chapterIndex = area.ChapterIndex == -1 ? 0 : area.ChapterIndex;
            float botFrom;
            float topFrom;
            float botTo;
            float topTo;
            while (!Opened && Counter < Requires)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Math.Abs(player.Center.Y - Position.Y - height / 2) < (checkDistance * 8 + height / 2) && player.Left > TopSolid.Left - 32f && player.Right < BotSolid.Right + 32f)
                {
                    if (Counter == 0f && Collectables > 0)
                    {
                        Audio.Play(fillSound, Position);
                    }
                    int num = (int)Counter;
                    int target = Math.Min(Collectables, Requires);
                    Counter = Calc.Approach(Counter, target, Engine.DeltaTime * checkDisplaySpeed);
                    if (num != (int)Counter)
                    {
                        if (Counter < target)
                        {
                            Audio.Play(fillSound, Position);
                        }
                    }
                }
                else
                {
                    Counter = Calc.Approach(Counter, 0f, Engine.DeltaTime * 20f);
                }
                yield return null;
            }
            yield return beforeSliceDelay;
            Scene.Add(new WhiteLine(Position, height, sliceColor, true));
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White * 0.5f);
            Audio.Play(unlockSound, Position);
            Opened = true;
            level.Session.SetFlag("XaphanHelper_Opened_Collectable_Door_" + doorID);
            if (registerInSaveData)
            {
                registerDoorOpenInSaveData();
            }
            MiniMap minimap = SceneAs<Level>().Tracker.GetEntity<MiniMap>();
            if (minimap != null && !level.Session.GetFlag("Map_Opened") && !level.InCutscene)
            {
                minimap.mapDisplay.GenerateIcons();
            }
            offset = 0f;
            yield return afterSliceDelay;
            botFrom = TopSolid.X;
            topFrom = TopSolid.X - openDistance;
            botTo = BotSolid.X;
            topTo = BotSolid.X + openDistance;
            for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * openSpeedMultiplier)
            {
                level.Shake();
                openPercent = Ease.CubeIn(p2);
                TopSolid.MoveToX(MathHelper.Lerp(botFrom, topFrom, openPercent));
                BotSolid.MoveToX(MathHelper.Lerp(botTo, topTo, openPercent));
                if (p2 >= 0.4f && level.OnInterval(0.1f))
                {
                    for (int i = 4; i < height; i += 4)
                    {
                        level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(TopSolid.Right - 2f, TopSolid.Top + i + 1f), new Vector2(2f, 2f), -(float)Math.PI / 2f);
                        level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(BotSolid.Left + 2f, BotSolid.Top + i + 1f), new Vector2(2f, 2f), (float)Math.PI / 2f);
                    }
                }
                yield return null;
            }
            TopSolid.MoveToX(topFrom);
            BotSolid.MoveToX(topTo);
            openPercent = 1f;
        }

        public void registerDoorOpenInSaveData()
        {
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Opened_Collectable_Door_" + doorID))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_Opened_Collectable_Door_" + doorID);
            }
            if (XaphanModule.PlayerHasGolden)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_Opened_Collectable_Door_" + doorID + "_GoldenStrawberry"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_Opened_Collectable_Door_" + doorID + "_GoldenStrawberry");
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Opened)
            {
                offset += 12f * Engine.DeltaTime;
                mist.X -= 4f * Engine.DeltaTime;
                mist.Y -= 24f * Engine.DeltaTime;
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
                }
            }
        }

        public void RenderBloom()
        {
            if (!Opened && Visible)
            {
                if (orientation == "Vertical")
                {
                    DrawBloom(new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height)));
                }
                else
                {
                    DrawBloom(new Rectangle((int)TopSolid.X, (int)TopSolid.Y, (int)(TopSolid.Width + BotSolid.Width), height));
                }
            }
        }

        private void DrawBloom(Rectangle bounds)
        {
            Draw.Rect(bounds.Left - 4, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
            Draw.Rect(bounds.Left - 2, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
            Draw.Rect(bounds, Color.White * 0.75f);
            Draw.Rect(bounds.Right, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
            Draw.Rect(bounds.Right + 2, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
        }

        private void DrawMist(Rectangle bounds, Vector2 mist)
        {
            Color color = Calc.HexToColor(mistColor) * 0.6f;
            MTexture mTexture = GFX.Game["objects/heartdoor/mist"];
            if (!string.IsNullOrEmpty(directory))
            {
                mTexture = GFX.Game[directory + "/mist"];
            }
            int num = mTexture.Width / 2;
            int num2 = mTexture.Height / 2;
            for (int i = 0; i < bounds.Width; i += num)
            {
                for (int j = 0; j < bounds.Height; j += num2)
                {
                    mTexture.GetSubtexture((int)Mod(mist.X, num), (int)Mod(mist.Y, num2), Math.Min(num, bounds.Width - i), Math.Min(num2, bounds.Height - j), temp);
                    temp.Draw(new Vector2(bounds.X + i, bounds.Y + j), Vector2.Zero, color);
                }
            }
        }

        private void DrawInterior(Rectangle bounds)
        {
            Draw.Rect(bounds, Calc.HexToColor(interiorColor));
            DrawMist(bounds, mist);
            DrawMist(bounds, new Vector2(mist.Y, mist.X) * 1.5f);
            Vector2 value = (Scene as Level).Camera.Position;
            if (Opened)
            {
                value = Vector2.Zero;
            }
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 value2 = particles[i].Position + value * 0.2f;
                value2.X = Mod(value2.X, bounds.Width);
                value2.Y = Mod(value2.Y, bounds.Height);
                Draw.Pixel.Draw(new Vector2(bounds.X, bounds.Y) + value2, Vector2.Zero, particles[i].Color);
            }
        }

        private void DrawEdges(Rectangle bounds, Color color)
        {
            MTexture edges = GFX.Game["objects/heartdoor/edge"];
            MTexture top = GFX.Game["objects/heartdoor/top"];
            if (!string.IsNullOrEmpty(directory))
            {
                edges = GFX.Game[directory + "/edge"];
                top = GFX.Game[directory + "/top"];
            }
            int num = 0;
            if (this.edges != "None")
            {
                if (edgesAnimationMode != "Static")
                {
                    num = (int)(offset % 8f);
                }
                if (this.edges == "LeftRight" || this.edges == "All")
                {
                    if (edgesAnimationMode == "Clockwise")
                    {
                        if (num > 0)
                        {
                            edges.GetSubtexture(0, 8 - num, 8, num, temp);
                            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Bottom), new Vector2(0.5f, 0f), color, new Vector2(-1f, -1f));
                            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
                        }
                        for (int i = num; i < bounds.Height; i += 8)
                        {
                            edges.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - i), temp);
                            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Bottom - i), new Vector2(0.5f, 0f), color, new Vector2(-1f, -1f));
                            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top + i), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
                        }
                    }
                    else
                    {
                        if (num > 0)
                        {
                            edges.GetSubtexture(0, 8 - num, 8, num, temp);
                            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
                            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Bottom), new Vector2(0.5f, 0f), color, new Vector2(1f, -1f));
                        }
                        for (int i = num; i < bounds.Height; i += 8)
                        {
                            edges.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - i), temp);
                            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top + i), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
                            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Bottom - i), new Vector2(0.5f, 0f), color, new Vector2(1f, -1f));
                        }

                    }
                }
                if (this.edges == "TopBottom" || this.edges == "All")
                {
                    if (edgesAnimationMode == "Clockwise")
                    {
                        if (num > 0)
                        {
                            top.GetSubtexture(8 - num, 0, num, 8, temp);
                            temp.DrawJustified(new Vector2(bounds.Left, bounds.Top + 4), new Vector2(0f, 0.5f), color);
                            temp.DrawJustified(new Vector2(bounds.Right, bounds.Bottom - 4), new Vector2(0f, 0.5f), color, new Vector2(-1f, -1f));
                        }
                        for (int j = num; j < bounds.Width; j += 8)
                        {
                            top.GetSubtexture(0, 0, Math.Min(8, bounds.Width - j), 8, temp);
                            temp.DrawJustified(new Vector2(bounds.Left + j, bounds.Top + 4), new Vector2(0f, 0.5f), color);
                            temp.DrawJustified(new Vector2(bounds.Right - j, bounds.Bottom - 4), new Vector2(0f, 0.5f), color, new Vector2(-1f, -1f));
                        }
                    }
                    else
                    {
                        if (num > 0)
                        {
                            top.GetSubtexture(8 - num, 0, num, 8, temp);
                            temp.DrawJustified(new Vector2(bounds.Right, bounds.Top + 4), new Vector2(0f, 0.5f), color, new Vector2(-1f, 1f));
                            temp.DrawJustified(new Vector2(bounds.Left, bounds.Bottom - 4), new Vector2(0f, 0.5f), color, new Vector2(1f, -1f));
                        }
                        for (int j = num; j < bounds.Width; j += 8)
                        {
                            top.GetSubtexture(0, 0, Math.Min(8, bounds.Width - j), 8, temp);
                            temp.DrawJustified(new Vector2(bounds.Right - j, bounds.Top + 4), new Vector2(0f, 0.5f), color, new Vector2(-1f, 1f));
                            temp.DrawJustified(new Vector2(bounds.Left + j, bounds.Bottom - 4), new Vector2(0f, 0.5f), color, new Vector2(1f, -1f));
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            Color color = Opened ? (Calc.HexToColor(edgesColor) * 0.25f) : Calc.HexToColor(edgesColor);
            if (!Opened && TopSolid.Visible && BotSolid.Visible)
            {
                Rectangle bounds = new();
                if (orientation == "Vertical")
                {
                    bounds = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height));
                }
                else
                {
                    bounds = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, (int)(TopSolid.Width + BotSolid.Width), height);
                }
                DrawInterior(bounds);
                DrawEdges(bounds, color);
            }
            else
            {
                if (TopSolid.Visible)
                {
                    Rectangle bounds2 = new();
                    if (orientation == "Vertical")
                    {
                        bounds2 = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)TopSolid.Height);
                    }
                    else
                    {
                        bounds2 = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, (int)TopSolid.Width, height);
                    }
                    DrawInterior(bounds2);
                    DrawEdges(bounds2, color);
                }
                if (BotSolid.Visible)
                {
                    Rectangle bounds3 = new();
                    if (orientation == "Vertical")
                    {
                        bounds3 = new Rectangle((int)BotSolid.X, (int)BotSolid.Y, Size, (int)BotSolid.Height);
                    }
                    else
                    {
                        bounds3 = new Rectangle((int)BotSolid.X, (int)BotSolid.Y, (int)BotSolid.Width, height);
                    }
                    DrawInterior(bounds3);
                    DrawEdges(bounds3, color);
                }
            }
            if (!(heartAlpha > 0f))
            {
                return;
            }
            float num = 12f;
            if (orientation == "Vertical")
            {
                int num2 = (int)((Size - 8) / num);
                int num3 = (int)Math.Ceiling((float)Requires / num2);
                Color color2 = (Opened ? (Calc.HexToColor(iconsColor) * 0.25f) : Calc.HexToColor(iconsColor)) * heartAlpha;
                for (int i = 0; i < num3; i++)
                {
                    int num4 = ((i + 1) * num2 < Requires) ? num2 : (Requires - i * num2);
                    Vector2 value = new Vector2(X + Size * 0.5f, Y) + new Vector2((-num4) / 2f + 0.5f, (-num3) / 2f + i + 0.5f) * num;
                    if (Opened)
                    {
                        if (i < num3 / 2)
                        {
                            value.Y -= openAmount + 8f;
                        }
                        else
                        {
                            value.Y += openAmount + 8f;
                        }
                    }
                    for (int j = 0; j < num4; j++)
                    {
                        int num5 = i * num2 + j;
                        float num6 = Ease.CubeIn(Calc.ClampedMap(Counter, num5, num5 + 1f));
                        icon[(int)(num6 * (icon.Count - 1))].DrawCentered(value + new Vector2(j * num, 0f), color2);
                    }
                }
            }
            else
            {
                if (!Opened)
                {
                    int num2 = (int)((Size * 2 - 16) / num);
                    int num3 = (int)Math.Ceiling((float)Requires / num2);
                    Color color2 = (Opened ? (Calc.HexToColor(iconsColor) * 0.25f) : Calc.HexToColor(iconsColor)) * heartAlpha;
                    for (int i = 0; i < Requires; i++)
                    {
                        int num4 = ((i + 1) * num2 < Requires) ? num2 : (Requires - i * num2);
                        Vector2 value = new Vector2(X, Y + height * 0.5f) + new Vector2((-num4) / 2f + 0.5f, (-num3) / 2f + i + 0.5f) * num;
                        for (int j = 0; j < num4; j++)
                        {
                            int num5 = i * num2 + j;
                            float num6 = Ease.CubeIn(Calc.ClampedMap(Counter, num5, num5 + 1f));
                            icon[(int)(num6 * (icon.Count - 1))].DrawCentered(value + new Vector2(j * num, 0f), color2);
                        }
                    }
                }
                else
                {
                    int num2 = (int)((Size * 2 - 32) / num);
                    int num3 = (int)Math.Ceiling((float)Requires / num2);
                    Color color2 = (Opened ? (Calc.HexToColor(iconsColor) * 0.25f) : Calc.HexToColor(iconsColor)) * heartAlpha;
                    for (int i = 0; i < num3; i++)
                    {
                        int num4 = ((i + 1) * num2 < Requires) ? num2 : (Requires - i * num2);
                        Vector2 value = new Vector2(X, Y + height * 0.5f) + new Vector2((-num4) / 2f + 0.5f, (-num3) / 2f + i + 0.5f) * num;
                        if (Opened)
                        {
                            value.X -= openAmount + 8f;
                        }
                        for (int j = 0; j < (int)Math.Ceiling((float)num4 / 2); j++)
                        {
                            int num5 = i * num2 + j;
                            float num6 = Ease.CubeIn(Calc.ClampedMap(Counter, num5, num5 + 1f));
                            if (!Opened)
                            {
                                icon[(int)(num6 * (icon.Count - 1))].DrawCentered(value + new Vector2(j * num, 0f), color2);
                            }
                            else
                            {
                                icon[(int)(num6 * (icon.Count - 1))].Draw(new Vector2(TopSolid.Right - (j + 2) * num, value.Y), new Vector2(-5f, 5f), color2);
                            }
                        }
                        for (int j = 0; j < (int)Math.Floor((float)num4 / 2); j++)
                        {
                            int num5 = i * num2 + j;
                            float num6 = Ease.CubeIn(Calc.ClampedMap(Counter, num5, num5 + 1f));
                            if (!Opened)
                            {
                                icon[(int)(num6 * (icon.Count - 1))].DrawCentered(value + new Vector2(j * num, 0f), color2);
                            }
                            else
                            {
                                icon[(int)(num6 * (icon.Count - 1))].Draw(new Vector2(BotSolid.Left + (j + 1) * num, value.Y), new Vector2(3f, 5f), color2);
                            }
                        }
                    }
                }
            }
        }

        private float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
