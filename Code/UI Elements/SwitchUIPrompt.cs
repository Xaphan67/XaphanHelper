using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class SwitchUIPrompt : Entity
    {
        private class ScreenSelect : Entity
        {
            public int ID;

            public float width = 250f;

            public float height = 150f;

            private MTexture texture;

            private string text;

            public bool Selected;

            private float selectedAlpha = 0;

            private int alphaStatus = 0;

            SwitchUIPrompt prompt;

            public ScreenSelect(Vector2 position, int id, SwitchUIPrompt prompt) : base(position - new Vector2(125f, 75f))
            {
                Tag = Tags.HUD;
                ID = id;
                if (ID == 0)
                {
                    texture = GFX.Gui["common/upgradesScreen"];
                    text = Dialog.Clean("XaphanHelper_UI_abilities");
                }
                else if (ID == 1)
                {
                    texture = GFX.Gui["common/mapScreen"];
                    text = Dialog.Clean("XaphanHelper_UI_map");
                }
                else if (ID == 2)
                {
                    texture = GFX.Gui["common/achievementsScreen"];
                    text = Dialog.Clean("XaphanHelper_UI_achievements");
                }
                else if (ID == 3)
                {
                    texture = GFX.Gui["common/lorebookScreen"];
                    text = Dialog.Clean("XaphanHelper_UI_lorebook");
                }
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
                base.Render();
                if (prompt.drawContent)
                {
                    if (Selected)
                    {
                        Draw.Rect(Position, width, height, Color.Yellow * selectedAlpha);
                    }
                    texture.Draw(Position + new Vector2(width / 2 - texture.Width / 2, 25), Vector2.Zero, Color.White, Vector2.One);
                    ActiveFont.Draw(text, Position + new Vector2(width / 2 - ActiveFont.Measure(text).X * 0.5f / 2, 100), new Vector2(0f), new Vector2(0.5f), Color.White);
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

        private List<ScreenSelect> ScreenSelects = new();

        public SwitchUIPrompt(Vector2 position, int selection) : base(position)
        {
            Tag = (Tags.HUD | Tags.Persistent);
            Depth = -10003;
            maxSelection = XaphanModule.SoCMVersion >= new Version(3, 0, 0) ? 3 : 2;
            width = (maxSelection + 1) * 250 + 50;
            Selection = selection;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            OpenPrompt();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (ScreenSelect screenSelect in ScreenSelects)
            {
                screenSelect.RemoveSelf();
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
            if (XaphanModule.useIngameMap && XaphanModule.CanOpenMap(SceneAs<Level>()))
            {
                for (int i = 0; i <= maxSelection; i++)
                {
                    ScreenSelect select = new(PromptPos + new Vector2(150f + 250f * i, 101f), i, this);
                    ScreenSelects.Add(select);
                    SceneAs<Level>().Add(select);
                }
            }
            else
            {
                for (int i = 0; i <= maxSelection - 1; i++)
                {
                    ScreenSelect select = new(PromptPos + new Vector2(275f + 250f * i, 101f), i == 0 ? 0 : i + 1, this);
                    ScreenSelects.Add(select);
                    SceneAs<Level>().Add(select);
                }
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
            Draw.Rect(PromptPos.X, PromptPos.Y, width, height, Color.Black);
            Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, width + 10, 10, Color.White);
            Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5, 10, height + 10, Color.White);
            Draw.Rect(PromptPos.X - 5, PromptPos.Y - 5 + height, width + 10, 10, Color.White);
            Draw.Rect(PromptPos.X - 5 + width, PromptPos.Y - 5, 10, height + 10, Color.White);
        }
    }
}
