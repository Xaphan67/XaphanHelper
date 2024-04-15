using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class LorebookScreen : Entity
    {
        protected static XaphanModuleSettings XaphanSettings => XaphanModule.ModSettings;

        private bool NoInput;

        private Level level;

        private LorebookDisplay lorebookDisplay;

        private List<LorebookDisplay.EntryDisplay> Displays = new();

        public string Title;

        public BigTitle BigTitle;

        public SwitchUIPrompt prompt;

        public bool promptChoice;

        private Wiggler menuWiggle;

        private Wiggler closeWiggle;

        private float menuWiggleDelay;

        private float closeWiggleDelay;

        private float switchTimer;

        public int categorySelection;

        public int previousCategorySelection = -1;

        public int entrySelection = -1;

        public LorebookScreen(Level level)
        {
            this.level = level;
            Tag = Tags.HUD;
            Title = Dialog.Clean("XaphanHelper_UI_lorebook");
            Add(menuWiggle = Wiggler.Create(0.4f, 4f));
            Add(closeWiggle = Wiggler.Create(0.4f, 4f));
            categorySelection = 0;
            Depth = -10003;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            XaphanModule.UIOpened = true;
            Level level = Scene as Level;
            level.PauseLock = true;
            level.Session.SetFlag("Map_Opened", true);
            Add(new Coroutine(TransitionToLorebook(level)));
        }

        public override void Update()
        {
            if (XaphanModule.ShowUI)
            {
                foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>())
                {
                    player.StateMachine.State = Player.StDummy;
                    player.DummyAutoAnimate = false;
                }
            }
            if (prompt == null)
            {
                if (Input.Pause.Check && menuWiggleDelay <= 0f && switchTimer <= 0)
                {
                    menuWiggle.Start();
                    menuWiggleDelay = 0.5f;
                }
            }
            if (Input.MenuCancel.Check && closeWiggleDelay <= 0f)
            {
                closeWiggle.Start();
                closeWiggleDelay = 0.5f;
            }
            menuWiggleDelay -= Engine.DeltaTime;
            closeWiggleDelay -= Engine.DeltaTime;
            base.Update();
        }

        private IEnumerator TransitionToLorebook(Level level)
        {
            float duration = 0.5f;
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(true, false);
            }
            XaphanModule.ShowUI = true;
            duration = 0.25f;
            FadeWipe Wipe2 = new(SceneAs<Level>(), true)
            {
                Duration = duration
            };
            Add(new Coroutine(LorebookRoutine(level)));
            switchTimer = 0.35f;
            while (switchTimer > 0f)
            {
                yield return null;
                switchTimer -= Engine.DeltaTime;
            }
        }

        private IEnumerator TransitionToStatusScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseLorebook(true)));
                level.Add(new StatusScreen(level, true));
            }
        }

        private IEnumerator TransitionToMapScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseLorebook(true)));
                level.Add(new MapScreen(level, true));
            }
        }

        private IEnumerator TransitionToAchievementsScreen()
        {
            if (!NoInput)
            {
                NoInput = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                float duration = 0.5f;
                FadeWipe Wipe = new(SceneAs<Level>(), false)
                {
                    Duration = duration
                };
                SceneAs<Level>().Add(Wipe);
                duration = duration - 0.25f;
                while (duration > 0f)
                {
                    yield return null;
                    duration -= Engine.DeltaTime;
                }
                Add(new Coroutine(CloseLorebook(true)));
                level.Add(new AchievementsScreen(level));
            }
        }

        private IEnumerator TransitionToGame()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            float duration = 0.5f;
            FadeWipe Wipe = new(SceneAs<Level>(), false)
            {
                Duration = duration
            };
            SceneAs<Level>().Add(Wipe);
            duration = duration - 0.25f;
            while (duration > 0f)
            {
                yield return null;
                duration -= Engine.DeltaTime;
            }
            CountdownDisplay timerDisplay = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timerDisplay != null)
            {
                timerDisplay.StopTimer(false, true);
            }
            BagDisplay bagDisplay = SceneAs<Level>().Tracker.GetEntity<BagDisplay>();
            if (bagDisplay != null)
            {
                if (!bagDisplay.CheckIfUpgradeIsActive(bagDisplay.currentSelection))
                {
                    bagDisplay.SetToFirstActiveUpgrade();
                }
            }
            XaphanModule.ShowUI = false;
            duration = 0.25f;
            Wipe = new FadeWipe(SceneAs<Level>(), true)
            {
                Duration = duration
            };
            Add(new Coroutine(CloseLorebook(false)));
        }

        bool switchedToEntries;

        private IEnumerator LorebookRoutine(Level level)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            Scene.Add(BigTitle = new BigTitle(Title, new Vector2(960, 80), true));
            Scene.Add(lorebookDisplay = new LorebookDisplay(level));
            yield return lorebookDisplay.GenerateLorebookDisplay();
            while (switchTimer > 0)
            {
                yield return null;
            }
            while (!Input.ESC.Pressed && !Input.MenuCancel.Pressed && !XaphanSettings.OpenMap.Pressed && player != null)
            {
                if (prompt != null)
                {
                    if (!prompt.open)
                    {
                        prompt = null;
                        promptChoice = false;
                    }
                    else
                    {
                        if (Input.MenuLeft.Pressed && prompt.Selection > 0)
                        {
                            prompt.Selection--;
                            if ((!XaphanModule.useIngameMap || !XaphanModule.CanOpenMap(level)) && prompt.Selection == 1)
                            {
                                prompt.Selection--;
                            }
                        }
                        if (Input.MenuRight.Pressed && prompt.Selection < prompt.maxSelection)
                        {
                            prompt.Selection++;
                            if ((!XaphanModule.useIngameMap || !XaphanModule.CanOpenMap(level)) && prompt.Selection == 1)
                            {
                                prompt.Selection++;
                            }
                        }
                        if ((Input.MenuConfirm.Pressed || Input.Pause.Pressed) && prompt.drawContent && !promptChoice)
                        {
                            promptChoice = true;
                            if (prompt.Selection == 0)
                            {
                                Add(new Coroutine(TransitionToStatusScreen()));
                            }
                            else if (prompt.Selection == 1)
                            {
                                Add(new Coroutine(TransitionToMapScreen()));
                            }
                            else if (prompt.Selection == 2)
                            {
                                Add(new Coroutine(TransitionToAchievementsScreen()));
                            }
                            prompt.ClosePrompt();
                        }
                    }
                }
                else
                {
                    if (entrySelection == -1 && Input.MenuDown.Pressed)
                    {
                        GetEntryDisplays();
                        previousCategorySelection = categorySelection;
                        categorySelection = -1;
                        LorebookDisplay.EntryDisplay firstDisplay = Displays.Find(entry => entry.ID == 0);
                        entrySelection = firstDisplay.isSubCategory ? 1 : 0;
                        switchedToEntries = true;
                    }
                    else if (categorySelection == -1 && entrySelection == Displays.Find(entry => !entry.isSubCategory).ID && Input.MenuUp.Pressed)
                    {
                        entrySelection = -1;
                        categorySelection = previousCategorySelection;
                        previousCategorySelection = -1;
                        lorebookDisplay.GenerateEntryList(categorySelection);
                    }
                    if (Input.MenuLeft.Pressed && (categorySelection > 0 || previousCategorySelection > 0))
                    {
                        if (previousCategorySelection != -1)
                        {
                            categorySelection = previousCategorySelection - 1;
                            previousCategorySelection = -1;
                        } else
                        {
                            categorySelection--;
                        }
                        entrySelection = -1;
                        lorebookDisplay.GenerateEntryList(categorySelection);
                        Audio.Play("event:/ui/main/rollover_up");
                    }
                    if (Input.MenuRight.Pressed && ((categorySelection >= 0 && categorySelection < 2) || (previousCategorySelection >= 0 && previousCategorySelection <= 1)))
                    {
                        if (previousCategorySelection != -1)
                        {
                            categorySelection = previousCategorySelection + 1;
                            previousCategorySelection = -1;
                        }
                        else
                        {
                            categorySelection++;
                        }
                        entrySelection = -1;
                        lorebookDisplay.GenerateEntryList(categorySelection);
                        Audio.Play("event:/ui/main/rollover_down");
                    }
                    if (entrySelection != -1 && !switchedToEntries)
                    {
                        if (Input.MenuUp.Pressed && entrySelection > Displays.Find(entry => !entry.isSubCategory).ID)
                        {
                            LorebookDisplay.EntryDisplay previousDisplay = Displays.Find(entry => entry.ID == entrySelection - 1);
                            entrySelection -= previousDisplay.isSubCategory ? 2 : 1;
                            if (entrySelection <= Displays.Count - 6 && entrySelection >= 5)
                            {
                                foreach (LorebookDisplay.EntryDisplay display in Displays)
                                {
                                    display.Position.Y += display.height * (previousDisplay.isSubCategory ? entrySelection == Displays.Count - 6 ? 1 : 2 : 1);
                                }
                            }
                            LorebookDisplay.EntryDisplay firstDisplay = Displays.Find(entry => entry.ID == 0);
                            if (firstDisplay.Position.Y <= 480f && entrySelection < 5)
                            {
                                while (firstDisplay.Position.Y < 480f)
                                {
                                    foreach (LorebookDisplay.EntryDisplay display in Displays)
                                    {
                                        display.Position.Y += display.height;
                                    }
                                }
                            }
                            if (firstDisplay.Position.Y == 531f && entrySelection < 6)
                            {
                                while (firstDisplay.Position.Y > 481f)
                                {
                                    foreach (LorebookDisplay.EntryDisplay display in Displays)
                                    {
                                        display.Position.Y -= display.height;
                                    }
                                }
                            }
                            
                            Audio.Play("event:/ui/main/rollover_up");
                        }
                        if (Input.MenuDown.Pressed && entrySelection < Displays.Count - 1)
                        {
                            LorebookDisplay.EntryDisplay nextDisplay = Displays.Find(entry => entry.ID == entrySelection + 1);
                            entrySelection += nextDisplay.isSubCategory ? 2 : 1;
                            if (entrySelection >= 6 && Displays.Count - 1 >= entrySelection + (nextDisplay.isSubCategory ? 3 : 4))
                            {
                                foreach (LorebookDisplay.EntryDisplay display in Displays)
                                {
                                    display.Position.Y -= display.height;
                                }
                            }
                            LorebookDisplay.EntryDisplay lastDisplay = Displays.Find(entry => entry.ID == Displays.Count - 1);
                            if (lastDisplay.Position.Y > 933f && entrySelection > Displays.Count - 5)
                            {
                                while (lastDisplay.Position.Y > 933f)
                                {
                                    foreach (LorebookDisplay.EntryDisplay display in Displays)
                                    {
                                        display.Position.Y -= display.height;
                                    }
                                }
                            }
                            Audio.Play("event:/ui/main/rollover_down");
                        }
                    }
                    switchedToEntries = false;
                }
                if (Input.Pause.Check && switchTimer <= 0)
                {
                    if (prompt == null)
                    {
                        Scene.Add(prompt = new SwitchUIPrompt(Vector2.Zero, 3));
                    }
                }
                yield return null;
            }
            Audio.Play("event:/ui/game/unpause");
            Add(new Coroutine(TransitionToGame()));
        }

        private void GetEntryDisplays()
        {
            List<Entity> DisplaysEntities = SceneAs<Level>().Tracker.GetEntities<LorebookDisplay.EntryDisplay>();
            Displays.Clear();
            foreach (Entity entryDisplay in DisplaysEntities)
            {
                LorebookDisplay.EntryDisplay display = entryDisplay as LorebookDisplay.EntryDisplay;
                Displays.Add(display);
            }
        }

        private IEnumerator CloseLorebook(bool switchtoMap)
        {
            Level level = Scene as Level;
            Player player = Scene.Tracker.GetEntity<Player>();
            level.Remove(BigTitle);
            level.Remove(lorebookDisplay);
            level.Remove(prompt);
            if (!switchtoMap)
            {
                if (player != null)
                {
                    player.StateMachine.State = Player.StNormal;
                    player.DummyAutoAnimate = true;
                }
                level.PauseLock = false;
                yield return 0.1f;
                level.Session.SetFlag("Map_Opened", false);
            }
            XaphanModule.UIOpened = false;
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (XaphanModule.ShowUI)
            {
                Draw.Rect(new Vector2(-10, -10), 1940, 182, Color.Black);
                Draw.Rect(new Vector2(-10, 172), 100, 856, Color.Black);
                Draw.Rect(new Vector2(1830, 172), 100, 856, Color.Black);
                Draw.Rect(new Vector2(-10, 1028), 1940, 62, Color.Black);
                Draw.Rect(new Vector2(90, 172), 1740, 8, Color.White);
                Draw.Rect(new Vector2(90, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(1820, 180), 10, 840, Color.White);
                Draw.Rect(new Vector2(90, 1020), 1740, 8, Color.White);
                float inputEase = 0f;
                inputEase = Calc.Approach(inputEase, 1, Engine.DeltaTime * 4f);
                if (inputEase > 0f)
                {
                    float scale = 0.5f;
                    string label = Dialog.Clean("XaphanHelper_UI_close");
                    string label2 = Dialog.Clean("XaphanHelper_UI_menu");
                    float num = ButtonUI.Width(label, Input.MenuCancel);
                    float num2 = ButtonUI.Width(label2, Input.Pause);
                    Vector2 position = new(1830f, 1055f);
                    ButtonUI.Render(position, label, Input.MenuCancel, scale, 1f, closeWiggle.Value * 0.05f);
                    position.X -= num / 2 + 32;
                    ButtonUI.Render(position, label2, Input.Pause, scale, 1f, menuWiggle.Value * 0.05f);
                    position.X -= num2 / 2 + 32;
                }
            }
        }
    }
}
