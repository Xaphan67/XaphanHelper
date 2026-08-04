using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CollectableNote")]
    public class CollectableNote : Entity
    {
        private class NotePage : Entity
        {
            private MTexture paper;

            private VirtualRenderTarget target;

            private FancyText.Text text;

            private float alpha = 1f;

            private float scale = 1f;

            private float textScale = 0.7f;

            private float rotation;

            private float timer;

            private bool easingOut;

            public NotePage(string dialogID, float scale = 0.7f)
            {
                Tag = Tags.HUD;
                paper = GFX.Gui["poempage"];
                textScale = scale;
                text = FancyText.Parse(Dialog.Get(dialogID), (int)((paper.Width - 120) / textScale), -1, 1f, Color.Black * 0.7f);
                Add(new BeforeRenderHook(BeforeRender));
            }

            public IEnumerator EaseIn()
            {
                Audio.Play("event:/game/03_resort/memo_in");
                Vector2 vector = new Vector2(Engine.Width, Engine.Height) / 2f;
                Vector2 from = vector + new Vector2(0f, 200f);
                Vector2 to = vector;
                float rFrom = -0.1f;
                float rTo = 0.05f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime)
                {
                    Position = from + (to - from) * Ease.CubeOut(p);
                    alpha = Ease.CubeOut(p);
                    rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                    yield return null;
                }
            }

            public IEnumerator EaseOut()
            {
                Audio.Play("event:/game/03_resort/memo_out");
                easingOut = true;
                Vector2 from = Position;
                Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0f, -200f);
                float rFrom = rotation;
                float rTo = rotation + 0.1f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)
                {
                    Position = from + (to - from) * Ease.CubeIn(p);
                    alpha = 1f - Ease.CubeIn(p);
                    rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                    yield return null;
                }
                RemoveSelf();
            }

            public void BeforeRender()
            {
                if (target == null)
                {
                    target = VirtualContent.CreateRenderTarget("journal-poem", paper.Width, paper.Height);
                }
                Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                paper.Draw(Vector2.Zero);
                text.DrawJustifyPerLine(new Vector2(paper.Width, paper.Height) / 2f, new Vector2(0.5f, 0.5f), Vector2.One * textScale, 1f);
                Draw.SpriteBatch.End();
            }

            public override void Removed(Scene scene)
            {
                if (target != null)
                {
                    target.Dispose();
                }
                target = null;
                base.Removed(scene);
            }

            public override void SceneEnd(Scene scene)
            {
                if (target != null)
                {
                    target.Dispose();
                }
                target = null;
                base.SceneEnd(scene);
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            public override void Render()
            {
                if ((!(Scene is Level level) || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)) && target != null)
                {
                    Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0f);
                    if (!easingOut)
                    {
                        GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height / 2 + ((timer % 1f < 0.25f) ? 6 : 0)));
                    }
                }
            }
        }

        private Player player;

        private NotePage note;

        private TalkComponent talk;

        private Image sprite;

        private string dialogId;

        private float scale;

        private EntityID eid;

        public CollectableNote(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset)
        {
            sprite = new Image(GFX.Game[data.Attr("sprite") + "/Note00"]);
            sprite.Position = Position;
            dialogId = data.Attr("dialogId");
            scale = data.Float("textScale", 0.7f);
            this.eid = eid;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (XaphanModule.ModSaveData.SavedFlags.Contains(SceneAs<Level>().Session.Area.LevelSet + "_" + SceneAs<Level>().Session.Level + "_" + eid.ID))
            {
                RemoveSelf();
            }
            else
            {
                Add(talk = new TalkComponent(new Rectangle(0, 0, 16, 8), new Vector2(8, -4f), Interact));
                talk.PlayerMustBeFacing = false;
            }
        }

        private void Interact(Player player)
        {
            this.player = player;
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkToExact((int)BottomCenter.X);
            player.Facing = Facings.Right;
            yield return 0.2f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("duck");
            yield return 0.4f;
            note = new NotePage(dialogId, scale);
            Scene.Add(note);
            SceneAs<Level>().Session.SetFlag(SceneAs<Level>().Session.Level + "_" + eid.ID);
            XaphanModule.ModSaveData.SavedFlags.Add(SceneAs<Level>().Session.Area.LevelSet + "_" + SceneAs<Level>().Session.Level + "_" + eid.ID);
            yield return note.EaseIn();
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            Audio.Play("event:/ui/main/button_lowkey");
            yield return note.EaseOut();
            note = null;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            if (note != null)
            {
                note.RemoveSelf();
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(SceneAs<Level>().Session.Area.LevelSet + "_" + SceneAs<Level>().Session.Level + "_" + eid.ID))
            {
                sprite.Render();
            }
        }
    }
}
