using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AncientDevice")]
    public class AncientDevice : Solid
    {
        public class SelectSymbolPrompt : Entity
        {
            private class SymbolSelect : Entity
            {
                public int ID;

                public float width = 100f;

                public float height = 100f;

                private Sprite sprite;

                public bool Selected;

                private float selectedAlpha = 0;

                private int alphaStatus = 0;

                SelectSymbolPrompt prompt;

                public SymbolSelect(Vector2 position, int id, SelectSymbolPrompt prompt) : base(position - new Vector2(50f, 37.5f))
                {
                    Tag = Tags.HUD;
                    ID = id;
                    Add(sprite = new Sprite(GFX.Gui, "symbols/"));
                    sprite.Position = new Vector2(10f, 10f);
                    sprite.AddLoop("symbol", "symbol0" + ID, 0.08f);
                    sprite.Play("symbol");
                    this.prompt = prompt;
                    Depth = prompt.Depth;
                }

                public override void Update()
                {
                    base.Update();
                    if (prompt.Selection == ID)
                    {
                        Selected = true;
                        if (alphaStatus == 0 || (alphaStatus == 1 && selectedAlpha != 0.9f))
                        {
                            alphaStatus = 1;
                            selectedAlpha = Calc.Approach(selectedAlpha, 0.9f, Engine.DeltaTime);
                            if (selectedAlpha == 0.9f)
                            {
                                alphaStatus = 2;
                            }
                        }
                        if (alphaStatus == 2 && selectedAlpha != 0.1f)
                        {
                            selectedAlpha = Calc.Approach(selectedAlpha, 0.1f, Engine.DeltaTime);
                            if (selectedAlpha == 0.1f)
                            {
                                alphaStatus = 1;
                            }
                        }
                    }
                    else
                    {
                        Selected = false;
                    }
                }

                public override void Render()
                {
                    if (prompt.drawContent && !SceneAs<Level>().Paused && !prompt.player.Dead)
                    {
                        if (Selected)
                        {
                            Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                        }
                        sprite.Render();
                    }
                }
            }

            private float height;

            private float width;

            public bool drawContent;

            public bool open;

            public int maxSelection;

            public int Selection = -1;

            public Vector2 PromptPos;

            public Coroutine OpenRoutine = new();

            private List<SymbolSelect> SymbolSelects = new();

            private Player player;

            public SelectSymbolPrompt(Vector2 position, int selection) : base(position)
            {
                Tag = (Tags.HUD | Tags.Persistent);
                Depth = -10003;
                maxSelection = 5;
                width = (maxSelection + 1) * 100 + 50;
                Selection = selection;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                player = SceneAs<Level>().Tracker.GetEntity<Player>();
                OpenPrompt();
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                foreach (SymbolSelect symbolSelect in SymbolSelects)
                {
                    symbolSelect.RemoveSelf();
                }
            }

            public void OpenPrompt()
            {
                open = true;
                Add(new Coroutine(Open()));
            }

            public IEnumerator Open()
            {
                while (height < 200)
                {
                    height += Engine.DeltaTime * 1200;
                    yield return null;
                }
                height = 200;
                AddSelections();
                drawContent = true;
            }

            public void AddSelections()
            {
                for (int i = 0; i <= maxSelection; i++)
                {
                    SymbolSelect select = new(PromptPos + new Vector2(75f + 100f * i, 113f), i, this);
                    SymbolSelects.Add(select);
                    SceneAs<Level>().Add(select);
                }
            }

            public void ClosePrompt()
            {
                Add(new Coroutine(Close()));
            }

            public IEnumerator Close()
            {
                drawContent = false;
                while (height > 1)
                {
                    height -= Engine.DeltaTime * 1200;
                    yield return null;
                }
                open = false;
                RemoveSelf();
            }

            public override void Update()
            {
                base.Update();
                PromptPos = new Vector2(Engine.Width / 2 - width / 2, (Engine.Height / 2 - 39) + 200 / 2 - height / 2);
            }

            public override void Render()
            {
                if (SceneAs<Level>().Paused || player.Dead)
                {
                    return;
                }
                Draw.Rect(PromptPos.X, PromptPos.Y, width, height, Color.Black);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, width + 10, 10, Color.White);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, 10, height + 10, Color.White);
                Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5 + height, width + 10, 10, Color.White);
                Draw.Rect(PromptPos.X - 5 + width, PromptPos.Y - 5, 10, height + 10, Color.White);
                if (drawContent)
                {
                    ActiveFont.Draw(Dialog.Clean("Xaphan_0_SelectSymbol"), new Vector2(PromptPos.X + width / 2, PromptPos.Y + 40f), new Vector2(0.5f), Vector2.One, Color.White);
                }
            }
        }

        public class Symbol : Entity
        {
            private Image symbol;

            private float alpha;

            public SineWave sine;

            private ParticleType p_glow;

            public Symbol(Vector2 Position) : base(Position)
            {
                Add(sine = new SineWave(0.6f, 0f));
                sine.Randomize();
                p_glow = new ParticleType(Refill.P_Glow)
                {
                    Color = Calc.HexToColor("BED6E9"),
                    Color2 = Calc.HexToColor("73A5CE")
                };
            }

            public override void Update()
            {
                base.Update();
                alpha += Engine.DeltaTime * 4f;
                if (symbol != null)
                {
                    symbol.Y = sine.Value * 2f;
                    symbol.Color = Color.White * (0.9f * (0.9f + ((float)Math.Sin(alpha) + 1f) * 0.125f));
                    if (Scene.OnInterval(0.1f))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(p_glow, 2, Position + Vector2.One * 8 + Vector2.UnitY * sine.Value * 2f, Vector2.One * 8f);
                    }
                }
            }

            public void DisplaySymbol(int index)
            {
                if (symbol != null)
                {
                    symbol.RemoveSelf();
                    p_glow = null;
                }
                Add(symbol = new Image(GFX.Game["objects/Xaphan/AncientDevice/symbol0" + index]));
                Color color = Color.White;
                Color color2 = Color.White;
                switch (index)
                {
                    case 0:
                        color = Calc.HexToColor("B0F890");
                        color2 = Calc.HexToColor("3CB41C");
                        break;
                    case 1:
                        color = Calc.HexToColor("FC0CFC");
                        color2 = Calc.HexToColor("74048C");
                        break;
                    case 2:
                        color = Calc.HexToColor("FCFCCC");
                        color2 = Calc.HexToColor("FCAC44");
                        break;
                    case 3:
                        color = Calc.HexToColor("B4FCFC");
                        color2 = Calc.HexToColor("3454D4");
                        break;
                    case 4:
                        color = Calc.HexToColor("F8E850");
                        color2 = Calc.HexToColor("D84800");
                        break;
                    case 5:
                        color = Calc.HexToColor("FC4C4C");
                        color2 = Calc.HexToColor("B4040C");
                        break;
                }
                p_glow = new ParticleType(Refill.P_Glow)
                {
                    Color = color,
                    Color2 = color2
                };
            }
        }

        private int index;

        private Image device;

        private float speed;

        private float start;

        private bool playerWasOn;

        private TalkComponent talk;

        private VertexLight light;

        private SelectSymbolPrompt symbolPrompt;

        private Symbol symbol;

        private int currentSymbol = -1;

        public AncientDevice(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            Collider = new Hitbox(22f, 8f, -11f, -8f);
            index = data.Int("index");
            Add(device = new Image(GFX.Game["objects/Xaphan/AncientDevice/device0" + index]));
            device.Position = new Vector2(-12f, -8f);
            Color color = Color.White;
            switch (index)
            {
                case 0:
                    color = Calc.HexToColor("B87000");
                    break;
                case 1:
                    color = Calc.HexToColor("4378E0");
                    break;
                case 2:
                    color = Calc.HexToColor("43AC31");
                    break;
                case 3:
                    color = Calc.HexToColor("B80000");
                    break;
            }
            Add(light = new VertexLight(color, 1f, 12, 24));
            light.Position = Collider.TopCenter - Vector2.UnitY;
            start = Y;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            bool hasSymbol = false;
            SceneAs<Level>().Add(symbol = new Symbol(Position + new Vector2(-8f, -32f)));
            foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
            {
                if (flag.Contains("Symbol_" + index))
                {
                    currentSymbol = Int32.Parse(flag.Split(':')[1]);
                    symbol.DisplaySymbol(currentSymbol);
                    hasSymbol = true;
                    break;
                }
            }
            Add(talk = new TalkComponent(new Rectangle(-8, -16, 16, 8), new Vector2(0, hasSymbol  ? -38f : - 28f), Interact));
            talk.PlayerMustBeFacing = false;
            talk.Enabled = false;
        }

        private void Interact(Player player)
        {
            Add(new Coroutine(PromptRoutine(player)));
        }

        private IEnumerator PromptRoutine(Player player)
        {
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)BottomCenter.X);
            player.Facing = index <= 1 ? Facings.Right  : Facings.Left;
            SceneAs<Level>().Add(symbolPrompt = new SelectSymbolPrompt(Vector2.Zero, currentSymbol == -1 ? 0 : currentSymbol));
            bool canceled = false;
            while (!Input.MenuConfirm.Pressed && !canceled)
            {
                if (Input.MenuLeft.Pressed && symbolPrompt.Selection > 0)
                {
                    symbolPrompt.Selection--;
                }
                if (Input.MenuRight.Pressed && symbolPrompt.Selection < symbolPrompt.maxSelection)
                {
                    symbolPrompt.Selection++;
                }
                if (Input.MenuCancel.Pressed)
                {
                    canceled = true;
                }
                yield return null;
            }
            symbolPrompt.ClosePrompt();
            if (canceled)
            {
                Audio.Play("event:/ui/main/button_toggle_off");
                symbolPrompt = null;
                yield return 0.1f;
                player.StateMachine.State = 0;
                yield break;
            }
            currentSymbol = symbolPrompt.Selection;
            Audio.Play("event:/game/06_reflection/supersecret_dashflavour", "dash_direction", currentSymbol);
            yield return 0.05f;
            symbolPrompt = null;
            if (currentSymbol != -1)
            {
                symbol.DisplaySymbol(currentSymbol);
                string oldFlag = "";
                foreach (string flag in XaphanModule.ModSaveData.SavedFlags)
                {
                    if (flag.Contains("Symbol_" + index))
                    {
                        oldFlag = flag;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(oldFlag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Remove(oldFlag);
                }
                XaphanModule.ModSaveData.SavedFlags.Add("Symbol_" + index + ":" + currentSymbol);
            }
            yield return 0.1f;
            player.StateMachine.State = 0;
        }

        public override void Update()
        {
            base.Update();
            Player playerOnTop = GetPlayerOnTop();
            if (playerOnTop != null)
            {
                if (speed < 0f)
                {
                    speed = 0f;
                }
                speed = Calc.Approach(speed, 70f, 200f * Engine.DeltaTime);
                MoveTowardsY(start + 2f, speed * Engine.DeltaTime);
                if (!playerWasOn)
                {
                    Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
                }
                if (Y == start + 2f)
                {
                    talk.Enabled = true;
                }
            }
            else 
            {
                if (speed > 0f)
                {
                    speed = 0f;
                }
                speed = Calc.Approach(speed, -150f, 200f * Engine.DeltaTime);
                MoveTowardsY(start, (0f - speed) * Engine.DeltaTime);
                if (playerWasOn)
                {
                    Audio.Play("event:/game/05_mirror_temple/button_return", Position);
                }
                talk.Enabled = false;
            }
            playerWasOn = (playerOnTop != null);
        }
    }
}
