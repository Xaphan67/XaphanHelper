using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    public class Journal : Entity
    {
        private const float onScreenX = 0f;

        private const float offScreenX = -1920f;

        public bool PageTurningLocked;

        public List<JournalPage> Pages = new();

        public int PageIndex;

        public VirtualRenderTarget CurrentPageBuffer;

        public VirtualRenderTarget NextPageBuffer;

        private bool turningPage;

        private float turningScale;

        private Color backColor = Color.Lerp(Color.White, Color.Black, 0.2f);

        private float rotation;

        private MTexture arrow = MTN.Journal["pageArrow"];

        private float dot;

        private float dotTarget;

        private float dotEase;

        private float leftArrowEase;

        private float rightArrowEase;

        public string playerList;

        public JournalPage Page => Pages[PageIndex];

        public JournalPage NextPage
        {
            get
            {
                return Pages[PageIndex + 1];
            }
        }

        public JournalPage PrevPage
        {
            get
            {
                return Pages[PageIndex - 1];
            }
        }

        public Journal(Vector2 position) : base(position)
        {
            Tag = (Tags.HUD);
            Add(new Coroutine(Enter()));
            Depth = -10003;
        }

        public IEnumerator Enter()
        {
            SceneAs<Level>().PauseLock = true;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            player.StateMachine.State = Player.StDummy;
            PageIndex = 0;
            Visible = true;
            X = -1920f;
            turningPage = false;
            turningScale = 1f;
            rotation = 0f;
            dot = 0f;
            dotTarget = 0f;
            dotEase = 0f;
            leftArrowEase = 0f;
            rightArrowEase = 0f;
            NextPageBuffer = VirtualContent.CreateRenderTarget("journal-a", 1610, 1000);
            CurrentPageBuffer = VirtualContent.CreateRenderTarget("journal-b", 1610, 1000);
            Pages.Add(new JournalCover(this));
            Pages.Add(new JournalClears(this, 1));
            Pages.Add(new JournalClears(this, 2));
            Pages.Add(new JournalClears(this, 3));
            Pages.Add(new JournalClears(this, 4));
            int num = 0;
            foreach (JournalPage page in Pages)
            {
                page.PageIndex = num++;
            }
            Pages[0].Redraw(CurrentPageBuffer);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.4f)
            {
                rotation = -0.025f * Ease.BackOut(p);
                X = -1920f + 1920f * Ease.CubeInOut(p);
                dotEase = p;
                yield return null;
            }
            dotEase = 1f;
        }

        public IEnumerator Leave()
        {
            Audio.Play("event:/ui/world_map/journal/back");
            yield return EaseOut(0.4f);
            CurrentPageBuffer.Dispose();
            NextPageBuffer.Dispose();
            Pages.Clear();
            Visible = false;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            player.StateMachine.State = Player.StNormal;
            SceneAs<Level>().PauseLock = false;
            RemoveSelf();
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            if (Pages.Count > 0)
            {
                Page.Redraw(CurrentPageBuffer);
            }
        }

        public IEnumerator TurnPage(int direction)
        {
            turningPage = true;
            if (direction < 0)
            {
                PageIndex--;
                turningScale = -1f;
                dotTarget -= 1f;
                if (Page != null)
                {
                    Page.Redraw(CurrentPageBuffer);
                }
                NextPage.Redraw(NextPageBuffer);
                while ((turningScale = Calc.Approach(turningScale, 1f, Engine.DeltaTime * 8f)) < 1f)
                {
                    yield return null;
                }
            }
            else
            {
                NextPage.Redraw(NextPageBuffer);
                turningScale = 1f;
                dotTarget += 1f;
                while ((turningScale = Calc.Approach(turningScale, -1f, Engine.DeltaTime * 8f)) > -1f)
                {
                    yield return null;
                }
                PageIndex++;
                if (Page != null)
                {
                    Page.Redraw(CurrentPageBuffer);
                }
            }
            turningScale = 1f;
            turningPage = false;
        }

        private IEnumerator EaseOut(float duration)
        {
            float rotFrom = rotation;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                rotation = rotFrom * (1f - Ease.BackOut(p));
                X = 0f + -1920f * Ease.CubeInOut(p);
                dotEase = 1f - p;
                yield return null;
            }
            dotEase = 0f;
        }

        public override void Update()
        {
            base.Update();
            dot = Calc.Approach(dot, dotTarget, Engine.DeltaTime * 8f);
            leftArrowEase = Calc.Approach(leftArrowEase, (dotTarget > 0f) ? 1 : 0, Engine.DeltaTime * 5f) * dotEase;
            rightArrowEase = Calc.Approach(rightArrowEase, (dotTarget < (Pages.Count - 1)) ? 1 : 0, Engine.DeltaTime * 5f) * dotEase;
            if (turningPage || !Visible)
            {
                return;
            }
            Page.Update();
            if (Page.ForceRedraw)
            {
                Page.ForceRedraw = false;
                Page.Redraw(CurrentPageBuffer);
            }
            if (!PageTurningLocked)
            {
                if (Input.MenuLeft.Pressed && PageIndex > 0)
                {
                    if (PageIndex == 1)
                    {
                        Audio.Play("event:/ui/world_map/journal/page_cover_back");
                    }
                    else
                    {
                        Audio.Play("event:/ui/world_map/journal/page_main_back");
                    }
                    Add(new Coroutine(TurnPage(-1)));
                }
                else if (Input.MenuRight.Pressed && PageIndex < Pages.Count - 1)
                {
                    if (PageIndex == 0)
                    {
                        Audio.Play("event:/ui/world_map/journal/page_cover_forward");
                    }
                    else
                    {
                        Audio.Play("event:/ui/world_map/journal/page_main_forward");
                    }
                    Add(new Coroutine(TurnPage(1)));
                }
            }
            if (!PageTurningLocked && (Input.MenuJournal.Pressed || Input.MenuCancel.Pressed))
            {
                Add(new Coroutine(Leave()));
            }
        }

        public override void Render()
        {
            Vector2 vector = Position + new Vector2(128f, 120f);
            float num = Ease.CubeInOut(Math.Max(0f, turningScale));
            float num2 = Ease.CubeInOut(Math.Abs(Math.Min(0f, turningScale)));
            if (SaveData.Instance.CheatMode)
            {
                MTN.FileSelect["cheatmode"].DrawCentered(vector + new Vector2(80f, 360f), Color.White, 1f, (float)Math.PI / 2f);
            }
            if (SaveData.Instance.AssistMode)
            {
                MTN.FileSelect["assist"].DrawCentered(vector + new Vector2(100f, 370f), Color.White, 1f, (float)Math.PI / 2f);
            }
            MTexture mTexture = MTN.Journal["edge"];
            mTexture.Draw(vector + new Vector2(-mTexture.Width, 0f), Vector2.Zero, Color.White, 1f, rotation);
            if (PageIndex > 0)
            {
                MTN.Journal[PrevPage.PageTexture].Draw(vector, Vector2.Zero, backColor, new Vector2(-1f, 1f), rotation);
            }
            if (turningPage)
            {
                MTN.Journal[NextPage.PageTexture].Draw(vector, Vector2.Zero, Color.White, 1f, rotation);
                Draw.SpriteBatch.Draw((RenderTarget2D)NextPageBuffer, vector, NextPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            }
            if (turningPage && num2 > 0f)
            {
                MTN.Journal[Page.PageTexture].Draw(vector, Vector2.Zero, backColor, new Vector2(-1f * num2, 1f), rotation);
            }
            if (num > 0f)
            {
                MTN.Journal[Page.PageTexture].Draw(vector, Vector2.Zero, Color.White, new Vector2(num, 1f), rotation);
                Draw.SpriteBatch.Draw((RenderTarget2D)CurrentPageBuffer, vector, CurrentPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, new Vector2(num, 1f), SpriteEffects.None, 0f);
            }
            if (Pages.Count > 0)
            {
                int count = Pages.Count;
                MTexture mTexture2 = GFX.Gui["dot_outline"];
                int num3 = mTexture2.Width * count;
                Vector2 vector2 = new(960f, 1040f - 40f * Ease.CubeOut(dotEase));
                for (int i = 0; i < count; i++)
                {
                    mTexture2.DrawCentered(vector2 + new Vector2((-num3 / 2) + mTexture2.Width * (i + 0.5f), 0f), Color.White * 0.25f);
                }
                float x = 1f + Calc.YoYo(dot % 1f) * 4f;
                mTexture2.DrawCentered(vector2 + new Vector2((-num3 / 2) + mTexture2.Width * (dot + 0.5f), 0f), Color.White, new Vector2(x, 1f));
                GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2(-num3 / 2 - 50, 32f * (1f - Ease.CubeOut(leftArrowEase))), Color.White * leftArrowEase, new Vector2(-1f, 1f));
                GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2(num3 / 2 + 50, 32f * (1f - Ease.CubeOut(rightArrowEase))), Color.White * rightArrowEase);
            }
        }
    }
}