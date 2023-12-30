using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.XaphanHelper.UI_Elements.StatusDisplay;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class StatusScreen : Entity
    {
        protected static XaphanModuleSettings XaphanSettings => XaphanModule.ModSettings;

        private Level level;

        private StatusDisplay statusDisplay;

        private bool NoInput;

        public string Title;

        public BigTitle BigTitle;

        public SwitchUIPrompt prompt;

        public bool promptChoice;

        private Wiggler mapWiggle;

        private Wiggler closeWiggle;

        private Wiggler actionWiggle;

        private float mapWiggleDelay;

        private float closeWiggleDelay;

        private float actionWiggleDelay;

        private bool fromMap;

        private float switchTimer;

        public int SelectedCol;

        public int SelectedRow;

        public int SelectedSide;

        public StatusScreen(Level level, bool fromMap)
        {
            this.level = level;
            this.fromMap = fromMap;
            Tag = Tags.HUD;
            Title = (SaveData.Instance == null || !Dialog.Language.CanDisplay(SaveData.Instance.Name)) ? "FILE_DEFAULT" : SaveData.Instance.Name;
            Add(mapWiggle = Wiggler.Create(0.4f, 4f));
            Add(closeWiggle = Wiggler.Create(0.4f, 4f));
            Add(actionWiggle = Wiggler.Create(0.4f, 4f));
            SelectedSide = -1;
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            XaphanModule.UIOpened = true;
            Level level = Scene as Level;
            level.PauseLock = true;
            level.Session.SetFlag("Map_Opened", true);
            if (!fromMap)
            {
                Audio.Play("event:/ui/game/pause");
            }
            Add(new Coroutine(TransitionToStatus(level)));
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
                if (XaphanModule.useIngameMap)
                {
                    if (Input.Pause.Check && mapWiggleDelay <= 0f && switchTimer <= 0)
                    {
                        mapWiggle.Start();
                        mapWiggleDelay = 0.5f;
                    }
                }
                if (Input.MenuConfirm.Check && actionWiggleDelay <= 0f)
                {
                    actionWiggle.Start();
                    actionWiggleDelay = 0.5f;
                }
            }
            if (Input.MenuCancel.Check && closeWiggleDelay <= 0f)
            {
                closeWiggle.Start();
                closeWiggleDelay = 0.5f;
            }
            mapWiggleDelay -= Engine.DeltaTime;
            closeWiggleDelay -= Engine.DeltaTime;
            actionWiggleDelay -= Engine.DeltaTime;
            base.Update();
        }

        private IEnumerator TransitionToStatus(Level level)
        {
            float duration = 0.5f;
            if (!fromMap)
            {
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
            }
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
            Add(new Coroutine(StatusRoutine(level)));
            switchTimer = 0.35f;
            while (switchTimer > 0f)
            {
                yield return null;
                switchTimer -= Engine.DeltaTime;
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
            Add(new Coroutine(CloseStatus(false)));
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
                Add(new Coroutine(CloseStatus(true)));
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
                Add(new Coroutine(CloseStatus(true)));
                level.Add(new AchievementsScreen(level));
            }
        }

        private IEnumerator StatusRoutine(Level level)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            Scene.Add(BigTitle = new BigTitle(Title, new Vector2(960, 80), true));
            Scene.Add(statusDisplay = new StatusDisplay(level, XaphanModule.useIngameMap));
            yield return statusDisplay.GennerateUpgradesDisplay();
            int TopLeftDisplayRow = 11;
            int TopRightDisplayRow = 11;
            int BottomLeftDisplayRow = 0;
            int BottomRightDisplayRow = 0;
            foreach (UpgradeDisplay display in statusDisplay.LeftDisplays)
            {
                if (display.row < TopLeftDisplayRow)
                {
                    TopLeftDisplayRow = display.row;
                }
                if (display.row > BottomLeftDisplayRow)
                {
                    BottomLeftDisplayRow = display.row;
                }
            }
            foreach (UpgradeDisplay display in statusDisplay.RightDisplays)
            {
                if (display.row < TopRightDisplayRow)
                {
                    TopRightDisplayRow = display.row;
                }
                if (display.row > BottomRightDisplayRow)
                {
                    BottomRightDisplayRow = display.row;
                }
            }
            if (statusDisplay.LeftDisplays.Count > 0)
            {
                SelectedSide = 0;
                SelectedCol = statusDisplay.LeftDisplays[0].col;
                SelectedRow = statusDisplay.LeftDisplays[0].row;
            }
            else if (statusDisplay.RightDisplays.Count > 0)
            {
                SelectedSide = 1;
                SelectedCol = statusDisplay.RightDisplays[0].col;
                SelectedRow = statusDisplay.RightDisplays[0].row;
            }
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
                        if (Input.MenuRight.Pressed && prompt.Selection < 2)
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
                            if (prompt.Selection == 1)
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
                else if (SelectedSide != -1)
                {
                    if (Input.MenuLeft.Pressed)
                    {
                        int firstDisplayCol = 9;
                        foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                        {
                            if (display.row == SelectedRow && display.col < firstDisplayCol)
                            {
                                firstDisplayCol = display.col;
                            }
                        }
                        if (SelectedCol > firstDisplayCol)
                        {
                            SelectedCol--;
                        }
                        else if (SelectedCol == firstDisplayCol && SelectedSide == 1 && statusDisplay.LeftDisplays.Count > 0)
                        {
                            SelectedSide = 0;
                            if (SelectedRow <= TopLeftDisplayRow)
                            {
                                SelectedRow = TopLeftDisplayRow;
                            }
                            else if (SelectedRow >= BottomLeftDisplayRow)
                            {
                                SelectedRow = BottomLeftDisplayRow;
                            }
                            else
                            {
                                SelectedRow = GetRowPosition(BottomLeftDisplayRow);
                            }
                            SelectedCol = GetColPosition(9);
                        }
                    }
                    if (Input.MenuRight.Pressed)
                    {
                        int lastDisplayCol = 0;
                        foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                        {
                            if (display.row == SelectedRow && display.col > lastDisplayCol)
                            {
                                lastDisplayCol = display.col;
                            }
                        }
                        if (SelectedCol < lastDisplayCol)
                        {
                            SelectedCol++;
                        }
                        else if (SelectedCol == lastDisplayCol && SelectedSide == 0 && statusDisplay.RightDisplays.Count > 0)
                        {
                            SelectedSide = 1;
                            if (SelectedRow <= TopRightDisplayRow)
                            {
                                SelectedRow = TopRightDisplayRow;
                            }
                            else if (SelectedRow >= BottomRightDisplayRow)
                            {
                                SelectedRow = BottomRightDisplayRow;
                            }
                            else
                            {
                                SelectedRow = GetRowPosition(BottomRightDisplayRow);
                            }
                            SelectedCol = 0;
                        }
                    }
                    if (Input.MenuUp.Pressed)
                    {
                        if (SelectedRow > (SelectedSide == 0 ? TopLeftDisplayRow : TopRightDisplayRow))
                        {
                            bool foundDisplay = false;
                            while (SelectedRow > (SelectedSide == 0 ? TopLeftDisplayRow : TopRightDisplayRow) && !foundDisplay)
                            {
                                SelectedRow--;
                                foundDisplay = GetDisplay();
                            }
                            SelectedCol = GetColPosition(SelectedCol);
                        }
                    }
                    if (Input.MenuDown.Pressed)
                    {
                        if (SelectedRow < (SelectedSide == 0 ? BottomLeftDisplayRow : BottomRightDisplayRow))
                        {
                            bool foundDisplay = false;
                            while (SelectedRow < (SelectedSide == 0 ? BottomLeftDisplayRow : BottomRightDisplayRow) && !foundDisplay)
                            {
                                SelectedRow++;
                                foundDisplay = GetDisplay();
                            }
                            SelectedCol = GetColPosition(SelectedCol);
                        }
                    }
                }
                if (Input.Pause.Check && switchTimer <= 0)
                {
                    if (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0")
                    {
                        if (prompt == null)
                        {
                            Scene.Add(prompt = new SwitchUIPrompt(Vector2.Zero, 0));
                        }
                    }
                    else if (XaphanModule.useIngameMap && XaphanModule.CanOpenMap(level))
                    {
                        Add(new Coroutine(TransitionToMapScreen()));
                    }
                }
                yield return null;
            }
            Audio.Play("event:/ui/game/unpause");
            Add(new Coroutine(TransitionToGame()));
        }

        private bool GetDisplay()
        {
            foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
            {
                if (display.row == SelectedRow)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetColPosition(int current)
        {
            int Col = -1;
            foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
            {
                if (display.row == SelectedRow && display.col == current)
                {
                    Col = display.col;
                    break;
                }
            }
            if (Col == -1)
            {
                for (int i = current - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                        {
                            if (display.row == SelectedRow && display.col == i)
                            {
                                Col = i;
                                break;
                            }
                        }
                    }
                    if (Col != -1)
                    {
                        break;
                    }
                }
                if (Col == -1)
                {
                    int firstDisplayCol = 9;
                    foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                    {
                        if (display.row == SelectedRow && display.col < firstDisplayCol)
                        {
                            firstDisplayCol = display.col;
                        }
                    }
                    Col = firstDisplayCol;
                }
            }
            return Col;
        }

        private int GetRowPosition(int BottomDisplayRow)
        {
            int Row = -1;
            foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
            {
                if (display.row == SelectedRow)
                {
                    Row = display.row;
                    break;
                }
            }
            if (Row == -1)
            {
                int ClosestAbove = 11;
                int ClosestBelow = 11;
                for (int i = 1; i <= SelectedRow; i++)
                {
                    if (i < BottomDisplayRow)
                    {
                        foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                        {
                            if (display.row == SelectedRow - i && ClosestAbove > i)
                            {
                                ClosestAbove = i;
                            }
                        }
                    }
                }
                for (int i = 1; i <= BottomDisplayRow; i++)
                {
                    if (i < BottomDisplayRow)
                    {
                        foreach (UpgradeDisplay display in (SelectedSide == 0 ? statusDisplay.LeftDisplays : statusDisplay.RightDisplays))
                        {
                            if (display.row == SelectedRow + i && ClosestBelow > i)
                            {
                                ClosestBelow = i;
                            }
                        }
                    }
                }
                Row = ClosestAbove < ClosestBelow ? SelectedRow - ClosestAbove : SelectedRow + ClosestBelow;
            }
            return Row;
        }

        private IEnumerator CloseStatus(bool switchtoMap)
        {
            Level level = Scene as Level;
            Player player = Scene.Tracker.GetEntity<Player>();
            level.Remove(BigTitle);
            level.Remove(statusDisplay);
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
                    string label2 = SceneAs<Level>().Session.Area.LevelSet != "Xaphan/0" ? Dialog.Clean("XaphanHelper_UI_map") : Dialog.Clean("XaphanHelper_UI_menu");
                    string label3 = Dialog.Clean("XaphanHelper_UI_activate");
                    string label4 = Dialog.Clean("XaphanHelper_UI_deactivate");
                    float num = ButtonUI.Width(label, Input.MenuCancel);
                    float num2 = ButtonUI.Width(label2, Input.Pause);
                    Vector2 position = new(1830f, 1055f);
                    ButtonUI.Render(position, label, Input.MenuCancel, scale, 1f, closeWiggle.Value * 0.05f);
                    if (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" || (XaphanModule.useIngameMap && XaphanModule.CanOpenMap(level)))
                    {
                        position.X -= num / 2 + 32;
                        ButtonUI.Render(position, label2, Input.Pause, scale, 1f, mapWiggle.Value * 0.05f);
                    }
                    if (SelectedSide != -1 && statusDisplay.SelectedDisplay != null)
                    {
                        position.X -= num2 / 2 + 32;
                        ButtonUI.Render(position, statusDisplay.SelectedDisplay.InactiveList.Contains(level.Session.Area.LevelSet) ? label3 : label4, Input.MenuConfirm, scale, 1f, actionWiggle.Value * 0.05f);
                    }
                }
            }
        }
    }
}
