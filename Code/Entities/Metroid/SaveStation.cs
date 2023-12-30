using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/SaveStation")]
    public class SaveStation : Solid
    {
        [Tracked(true)]
        public class StationBeam : Entity
        {
            private MTexture texture = GFX.Game["util/lightbeam"];

            private Color color;

            public int LightWidth;

            public int LightLength;

            public float Rotation;

            private float timer = Calc.Random.NextFloat(1000f);

            private ParticleType P_Glow;

            public StationBeam(Vector2 position, string beamColor) : base(position)
            {
                Tag = Tags.TransitionUpdate;
                Depth = -9998;
                Position = position;
                LightWidth = 14;
                LightLength = 20;
                color = Calc.HexToColor(beamColor);
                Rotation = 180 * ((float)Math.PI / 180f);
                P_Glow = new ParticleType
                {
                    Source = GFX.Game["particles/rect"],
                    Color = Calc.HexToColor("fcf8de") * 0.4f,
                    FadeMode = ParticleType.FadeModes.InAndOut,
                    Size = 1f,
                    SpeedMin = 16f,
                    SpeedMax = 20f,
                    LifeMin = 0.7f,
                    LifeMax = 1.4f,
                    RotationMode = ParticleType.RotationModes.SameAsDirection
                };
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                Level level = Scene as Level;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    Vector2 value = Calc.AngleToVector(Rotation + (float)Math.PI / 2f, 1f);
                    Vector2 value2 = Calc.ClosestPointOnLine(Position, Position + value * 10000f, entity.Center);
                    float target = Math.Min(1f, Math.Max(0f, (value2 - Position).Length() - 8f) / LightLength);
                    if ((value2 - entity.Center).Length() > LightWidth / 2f)
                    {
                        target = 1f;
                    }
                    if (level.Transitioning)
                    {
                        target = 0f;
                    }
                }
                if (level.OnInterval(0.5f))
                {
                    Vector2 vector = Calc.AngleToVector(Rotation + (float)Math.PI / 2f, 1f);
                    Vector2 position = Position - vector * 4f;
                    float scaleFactor = Calc.Random.Next(LightWidth - 4) + 2 - LightWidth / 2;
                    position += scaleFactor * vector.Perpendicular();
                    level.Particles.Emit(P_Glow, position, Rotation + (float)Math.PI / 2f);
                }
                base.Update();
            }

            public override void Render()
            {
                if (Visible)
                {
                    DrawTexture(0f, LightWidth, (LightLength - 4) + (float)Math.Sin(timer * 2f) * 4f, 0.4f);
                    for (int i = 0; i < LightWidth; i += 4)
                    {
                        float num = timer + i * 0.3f;
                        float num2 = 4f + (float)Math.Sin(num * 0.5f + 1.2f) * 4f;
                        float offset = (float)Math.Sin(((num + (i * 32)) * 0.1f) + Math.Sin(num * 0.05f + i * 0.1f) * 0.25) * (LightWidth / 2f - num2 / 2f);
                        float length = LightLength + (float)Math.Sin(num * 0.25f) * 8f;
                        float a = 0.6f + (float)Math.Sin(num + 0.8f) * 0.3f;
                        DrawTexture(offset, num2, length, a);
                    }
                }
            }

            private void DrawTexture(float offset, float width, float length, float a)
            {
                float rotation = Rotation + (float)Math.PI / 2f;
                if (width >= 1f)
                {
                    texture.Draw(Position + Calc.AngleToVector(Rotation, 1f) * offset, new Vector2(0f, 0.5f), color * a, new Vector2(1f / texture.Width * length, width), rotation);
                }
            }
        }

        public class SavePrompt : Entity
        {
            private string Text;

            private Vector2 TextSize;

            private float height;

            public bool drawText;

            public SavePrompt(Vector2 position, string text) : base(position)
            {
                Tag = (Tags.HUD | Tags.Persistent);
                Text = text;
                TextSize = ActiveFont.Measure(Dialog.Clean(text));
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(new Coroutine(OpenRoutine()));
            }

            public IEnumerator OpenRoutine()
            {
                while (height < TextSize.Y + 200)
                {
                    height += Engine.DeltaTime * 1200;
                    yield return null;
                }
                drawText = true;
            }

            public void ClosePrompt()
            {
                Add(new Coroutine(CloseRoutine()));
            }

            public IEnumerator CloseRoutine()
            {
                drawText = false;
                while (height > 1)
                {
                    height -= Engine.DeltaTime * 1200;
                    yield return null;
                }
                RemoveSelf();
            }

            public override void Render()
            {
                Draw.Rect(Engine.Width / 2 - TextSize.X / 2 - 50, (Engine.Height / 2 - TextSize.Y / 2 - 125) + ((TextSize.Y + 200) / 2) - height / 2, TextSize.X + 100, height, Color.Black);
                if (drawText)
                {
                    ActiveFont.Draw(Dialog.Clean(Text), new Vector2(Engine.Width / 2 - TextSize.X / 2, Engine.Height / 2 - TextSize.Y / 2 - 75), new Vector2(0f, 0.5f), Vector2.One * 1f, Calc.HexToColor("AA00AA"));
                }
            }
        }

        private static FieldInfo playerFlash = typeof(Player).GetField("flash", BindingFlags.NonPublic | BindingFlags.Instance);

        private TalkComponent talk;

        private StationBeam beam;

        private Sprite botSprite;

        private bool CanSave = true;

        private bool Saving;

        private TextMenu menu;

        private SavePrompt prompt;

        private Level level;

        private Coroutine GameLoadCoroutine = new();

        public SaveStation(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            SurfaceSoundIndex = 7;
            Collider = new Hitbox(4f, 8f, -2f, 0f);
            Add(botSprite = new Sprite(GFX.Game, "objects/XaphanHelper/saveStation/"));
            botSprite.AddLoop("idle", "idle", 0.2f);
            botSprite.Add("standIn", "stand", 0.1f, "standLoop", 0, 1, 2, 3);
            botSprite.AddLoop("standLoop", "stand", 0.2f, 4, 5, 6, 7, 8);
            botSprite.Add("standOut", "stand", 0.1f, "idle", 3, 2, 1, 0);
            botSprite.Position += new Vector2(-17f, 0f);
            botSprite.Play("idle");
            Depth = -10002;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            level.Add(new Slope(Position, Vector2.UnitX, false, "Left", 7, 1, "Horizontal", "Horizontal", "cement", "cement", false, "", "", false, true, false, false, false, "", true));
            level.Add(new Slope(Position, new Vector2(-25f, 0f), false, "Right", 7, 1, "Horizontal", "Horizontal", "cement", "cement", false, "", "", false, true, false, false, false, "", true));
            Add(talk = new TalkComponent(new Rectangle(-4, -8, 8, 8), new Vector2(0f, -16f), Interact));
            talk.PlayerMustBeFacing = false;
            talk.Enabled = false;
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (player.Left >= Left - 9 && player.Right <= Right + 9 && player.Bottom == Top && !MetroidGameplayController.MorphMode)
                {
                    talk.Enabled = true;
                    if ((botSprite.LastAnimationID != "standIn" && botSprite.CurrentAnimationID != "standIn" && botSprite.LastAnimationID != "standLoop" && botSprite.CurrentAnimationID != "standLoop") && CanSave)
                    {
                        botSprite.Play("standIn");
                    }
                    if (MetroidGameplayController.isLoadingFromSave)
                    {
                        string LoadRoom = "";
                        if (XaphanModule.ModSaveData.SavedRoom.ContainsKey(SceneAs<Level>().Session.Area.LevelSet))
                        {
                            LoadRoom = XaphanModule.ModSaveData.SavedRoom[SceneAs<Level>().Session.Area.LevelSet];
                        }

                        if (SceneAs<Level>().Session.Level == LoadRoom && !GameLoadCoroutine.Active && !Saving)
                        {
                            Add(GameLoadCoroutine = new Coroutine(LoadRoutine()));
                        }
                    }
                }
                else
                {
                    talk.Enabled = false;
                    if (botSprite.LastAnimationID == "standIn")
                    {
                        int currentFrame = botSprite.CurrentAnimationFrame;
                        botSprite.Play("standOut");
                        botSprite.SetAnimationFrame(currentFrame);
                    }
                    else if (botSprite.LastAnimationID == "standLoop")
                    {
                        botSprite.Play("standOut");
                    }
                }
            }
        }

        public IEnumerator LoadRoutine()
        {
            CanSave = false;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            string music = Audio.CurrentMusic;
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/game/xaphan/load_game");
            SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.Add(beam = new StationBeam(Position, "457E65"));
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            float timer = 5.4f;
            while (timer > 0f)
            {
                yield return null;
                player.Facing = Facings.Right;
                player.BottomCenter = TopCenter;
                player.Sprite.Play("front");
                player.Speed = Vector2.Zero;
                timer -= Engine.DeltaTime;
                if (player != null)
                {
                    if ((bool)playerFlash.GetValue(player))
                    {
                        player.Visible = false;
                    }
                    else
                    {
                        player.Visible = true;
                    }
                }
            }
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle(music);
            SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.Remove(beam);
            player.Visible = true;
            while (Input.MoveX == 0 && Input.MoveY == 0)
            {
                yield return null;
            }
            player.StateMachine.State = 0;
            while (player.IsRiding(this))
            {
                yield return null;
            }
            MetroidGameplayController.IsLoadingFromSave(false);
            CanSave = true;
        }

        private void Interact(Player player)
        {
            Add(new Coroutine(InteractRoutine(player)));
        }

        public IEnumerator InteractRoutine(Player player)
        {
            CanSave = false;
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            yield return player.DummyWalkToExact((int)X, false, 1f, true);
            player.Facing = Facings.Right;
            HealthDisplay healthDisplay = SceneAs<Level>().Tracker.GetEntity<HealthDisplay>();
            AmmoDisplay ammoDisplay = SceneAs<Level>().Tracker.GetEntity<AmmoDisplay>();
            if (healthDisplay.CurrentHealth < healthDisplay.MaxHealth || ammoDisplay.CurrentMissiles < ammoDisplay.MaxMissiles || ammoDisplay.CurrentSuperMissiles < ammoDisplay.MaxSuperMissiles || ammoDisplay.CurrentPowerBombs < ammoDisplay.MaxPowerBombs)
            {
                level.Add(beam = new StationBeam(Position, "F9C462"));
                if (healthDisplay != null)
                {
                    if (healthDisplay.CurrentHealth < healthDisplay.MaxHealth - 100)
                    {
                        healthDisplay.CurrentHealth = healthDisplay.MaxHealth - 100;
                    }
                    while (healthDisplay.CurrentHealth < healthDisplay.MaxHealth)
                    {
                        healthDisplay.CurrentHealth += 1;
                        healthDisplay.GetEnergyTanks();
                        player.Facing = Facings.Right;
                        player.BottomCenter = TopCenter;
                        player.Sprite.Play("front");
                        float tickTimer = 0.01f;
                        while (tickTimer > 0)
                        {
                            tickTimer -= Engine.DeltaTime;
                            player.Speed = Vector2.Zero;
                            if ((bool)playerFlash.GetValue(player))
                            {
                                player.Visible = false;
                            }
                            else
                            {
                                player.Visible = true;
                            }
                            yield return null;
                        }
                    }
                }
                if (ammoDisplay != null)
                {
                    if (ammoDisplay.CurrentMissiles < ammoDisplay.MaxMissiles - 50)
                    {
                        ammoDisplay.CurrentMissiles = ammoDisplay.MaxMissiles - 50;
                    }
                    while (ammoDisplay.CurrentMissiles < ammoDisplay.MaxMissiles)
                    {
                        ammoDisplay.CurrentMissiles += 1;
                        player.Facing = Facings.Right;
                        player.BottomCenter = TopCenter;
                        player.Sprite.Play("front");
                        float tickTimer = 0.02f;
                        while (tickTimer > 0)
                        {
                            tickTimer -= Engine.DeltaTime;
                            player.Speed = Vector2.Zero;
                            if ((bool)playerFlash.GetValue(player))
                            {
                                player.Visible = false;
                            }
                            else
                            {
                                player.Visible = true;
                            }
                            yield return null;
                        }
                    }
                    if (ammoDisplay.CurrentSuperMissiles < ammoDisplay.MaxSuperMissiles - 15)
                    {
                        ammoDisplay.CurrentSuperMissiles = ammoDisplay.MaxSuperMissiles - 15;
                    }
                    while (ammoDisplay.CurrentSuperMissiles < ammoDisplay.MaxSuperMissiles)
                    {
                        ammoDisplay.CurrentSuperMissiles += 1;
                        player.Facing = Facings.Right;
                        player.BottomCenter = TopCenter;
                        player.Sprite.Play("front");
                        float tickTimer = 0.033f;
                        while (tickTimer > 0)
                        {
                            tickTimer -= Engine.DeltaTime;
                            player.Speed = Vector2.Zero;
                            if ((bool)playerFlash.GetValue(player))
                            {
                                player.Visible = false;
                            }
                            else
                            {
                                player.Visible = true;
                            }
                            yield return null;
                        }
                    }
                    if (ammoDisplay.CurrentPowerBombs < ammoDisplay.MaxPowerBombs - 15)
                    {
                        ammoDisplay.CurrentPowerBombs = ammoDisplay.MaxPowerBombs - 15;
                    }
                    while (ammoDisplay.CurrentPowerBombs < ammoDisplay.MaxPowerBombs)
                    {
                        ammoDisplay.CurrentPowerBombs += 1;
                        player.Facing = Facings.Right;
                        player.BottomCenter = TopCenter;
                        player.Sprite.Play("front");
                        float tickTimer = 0.033f;
                        while (tickTimer > 0)
                        {
                            tickTimer -= Engine.DeltaTime;
                            player.Speed = Vector2.Zero;
                            if ((bool)playerFlash.GetValue(player))
                            {
                                player.Visible = false;
                            }
                            else
                            {
                                player.Visible = true;
                            }
                            yield return null;
                        }
                    }
                }
                player.Visible = true;
                level.Remove(beam);
            }
            SceneAs<Level>().CameraOffset.X = MathHelper.Lerp(SceneAs<Level>().CameraOffset.X, 0, 0.1f);
            Audio.Stop(MetroidGameplayController.jumpSfx, false);
            level.PauseLock = true;
            Audio.Play("event:/ui/game/pause");
            menu = new TextMenu();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            menu.Add(new TextMenu.SubHeader(""));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_continue")).Pressed(delegate
            {
                menu.RemoveSelf();
                Add(new Coroutine(SaveRoutine(player)));
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("ASSIST_NO")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnCancel = delegate
            {
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
            };
            level.Add(prompt = new SavePrompt(Vector2.Zero, "XaphanHelper_UI_Save_Game"));
            while (!prompt.drawText)
            {
                player.Facing = Facings.Right;
                player.BottomCenter = TopCenter;
                player.Sprite.Play("front");
                player.Speed = Vector2.Zero;
                yield return null;
            }
            level.Add(menu);
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            prompt.ClosePrompt();
            level.PauseLock = false;
            yield return 0.15f;
            while (Saving)
            {
                level.PauseLock = true;
                yield return null;
            }
            level.PauseLock = false;
            while (Input.MoveX == 0 && Input.MoveY == 0)
            {
                yield return null;
            }
            player.StateMachine.State = 0;
            while (player.IsRiding(this))
            {
                yield return null;
            }
            CanSave = true;
        }

        public IEnumerator SaveRoutine(Player player)
        {
            string prefix = level.Session.Area.LevelSet;
            Saving = true;
            MetroidGameplayController.IsLoadingFromSave(false);
            Audio.Play("event:/game/xaphan/save_game");
            level.Add(beam = new StationBeam(Position, "457E65"));
            SavePosition(prefix);
            level.AutoSave();
            float timer = 2.85f;
            while (timer > 0f)
            {
                yield return null;
                player.Facing = Facings.Right;
                player.BottomCenter = TopCenter;
                player.Sprite.Play("front");
                player.Speed = Vector2.Zero;
                timer -= Engine.DeltaTime;
                if (player != null)
                {
                    if ((bool)playerFlash.GetValue(player))
                    {
                        player.Visible = false;
                    }
                    else
                    {
                        player.Visible = true;
                    }
                }
            }
            level.Remove(beam);
            player.Speed = Vector2.Zero;
            player.Visible = true;
            Saving = false;
        }

        private void SavePosition(string prefix)
        {
            if (!XaphanModule.ModSaveData.SavedChapter.ContainsKey(prefix))
            {
                XaphanModule.ModSaveData.SavedChapter.Add(prefix, level.Session.Area.ChapterIndex);
            }
            else
            {
                XaphanModule.ModSaveData.SavedChapter[prefix] = level.Session.Area.ChapterIndex;
            }
            if (!XaphanModule.ModSaveData.SavedRoom.ContainsKey(prefix))
            {
                XaphanModule.ModSaveData.SavedRoom.Add(prefix, level.Session.Level);
            }
            else
            {
                XaphanModule.ModSaveData.SavedRoom[prefix] = level.Session.Level;
            }
        }
    }
}
