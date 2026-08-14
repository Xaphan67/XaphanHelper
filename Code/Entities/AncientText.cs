using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AncientText")]
    public class AncientText : Entity
    {
        private class TextDisplay : Entity
        {
            private AncientText text;

            private string DialogID;

            private List<Sprite> Text = new();

            private List<Sprite> RevealText = new();

            private List<int> LinesLength = new();

            private char previousLetter;

            private bool skipLetter;

            private List<Entity> absorbs = new();

            public ParticleType P_OnReveal;

            private float alpha;

            public bool Revealed;

            private Color Tint;

            public TextDisplay(AncientText text)
            {
                Tag = Tags.TransitionUpdate;
                this.text = text;
                DialogID = text.DialogID;
                Tint = text.Tint;
                Position = text.Position - Vector2.UnitY * text.TextVerticalOffset;
                P_OnReveal = new ParticleType
                {
                    Source = GFX.Game["particles/fire"],
                    Color = Calc.HexToColor("3FD6CA"),
                    Color2 = Color.White,
                    ColorMode = ParticleType.ColorModes.Fade,
                    FadeMode = ParticleType.FadeModes.Late,
                    Acceleration = new Vector2(0f, -40f),
                    LifeMin = 0.8f,
                    LifeMax = 1.2f,
                    Size = 0.5f,
                    SizeRange = 0.4f,
                    Direction = -(float)Math.PI / 2f,
                    DirectionRange = (float)Math.PI / 6f,
                    SpeedMin = 12f,
                    SpeedMax = 10f,
                    SpeedMultiplier = 0.2f,
                    ScaleOut = true
                };
                Depth = 1;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                int row = 0;
                int col = 0;
                foreach (char letter in Dialog.Get(DialogID))
                {
                    if (previousLetter != '{' && !skipLetter)
                    {
                        if (letter == ' ')
                        {
                            col += 4;

                        }
                        else if (letter != '{')
                        {
                            Sprite sprite = new Sprite(GFX.Game, "objects/Xaphan/AncientText/symbols/" + letter);
                            sprite.Add("letter", "", 0);
                            sprite.Color = Tint;
                            sprite.Position += new Vector2(col, row);
                            sprite.Play("letter");
                            Text.Add(sprite);
                            col += 8;
                        }
                    }
                    else
                    {
                        skipLetter = true;
                        if (letter == 'n')
                        {
                            LinesLength.Add(col);
                            row += 8;
                            col = 0;
                        }
                        else if (letter == '}')
                        {
                            skipLetter = false;
                        }
                    }
                    previousLetter = letter;
                }
                LinesLength.Add(col);

                foreach (Sprite sprite in Text)
                {
                    sprite.Position -= new Vector2(LinesLength[(int)sprite.Position.Y / 8] / 2, 0);
                    Add(sprite);

                    Sprite reveal = new Sprite(GFX.Game, "objects/Xaphan/AncientText/letters/" + Utils.GetLetter(sprite.Path[sprite.Path.Length - 1]));
                    reveal.Add("letter", "", 0);
                    reveal.Position = sprite.Position;
                    reveal.Visible = false;
                    reveal.Play("letter");

                    RevealText.Add(reveal);
                    Add(reveal);
                }

                if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + text.chapterIndex + "_AncientText_" + SceneAs<Level>().Session.Level))
                {
                    for (int i = 0; i < Text.Count; i++)
                    {
                        Text[i].Visible = false;
                        RevealText[i].Visible = true;
                    }
                }
            }

            

            public override void Update()
            {
                base.Update();
                alpha += Engine.DeltaTime * 4f;
                if (RevealText.Count > 0)
                {
                    foreach (Sprite sprite in RevealText)
                    {
                        if (sprite.Visible)
                        {
                            sprite.Color = Calc.HexToColor("3FD6CA") * (0.9f * (0.9f + ((float)Math.Sin(alpha) + 1f) * 0.125f));
                        }
                    }
                }
            }

            public IEnumerator RevealRoutine()
            {
                Revealed = true;
                Add(new Coroutine(SwitchLetters()));
                for (int i = 0; i < Text.Count; i++)
                {
                    Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", Position + Text[i].Position + Vector2.One * 4);
                    for (int j = 0; j < 3; j++)
                    {
                        CustomAbsorbOrb orb = new(text.Position, absorbTarget: new Vector2?(Position + Text[i].Position) + Vector2.One * 4, color: "3FD6CA", consumeDelay: 0, randomiseConsumeDelay: false, direction: "Top", fade: false);
                        Scene.Add(orb);
                        absorbs.Add(orb);
                        yield return null;
                    }
                    yield return 0.01f;
                }
                yield return 0.5f;
                foreach (Entity orb in absorbs)
                {
                    orb.RemoveSelf();
                }
            }

            private IEnumerator SwitchLetters()
            {
                yield return 0.5f;
                for (int i = 0; i < Text.Count; i++)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_OnReveal, 12, Position + Text[i].Position + Vector2.One * 4, new Vector2(3f, 3f));
                    Text[i].Visible = false;
                    RevealText[i].Visible = true;
                    yield return 0.06f;
                }
            }
        }

        private class SemantographDisplay : Entity
        {
            private Sprite semantograph;

            private float alpha = 0f;

            private SoundSource sfx;

            public SemantographDisplay(AncientText text)
            {
                Position = text.Position;
                Visible = false;
                Add(semantograph = new Sprite(GFX.Game, "collectables/Xaphan/CustomCollectable/items/semantograph"));
                semantograph.Add("idle", "", 0f);
                semantograph.AddLoop("spin", "", 0.08f);
                semantograph.CenterOrigin();
                Depth = -2000001;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(sfx = new SoundSource());
            }

            public IEnumerator Appear()
            {
                semantograph.Play("idle");
                while (alpha < 1f)
                {
                    alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
                    semantograph.Color = Color.White * alpha;
                    yield return null;
                }
            }

            public IEnumerator StartSpinning()
            {
                sfx.Play("event:/game/04_cliffside/arrowblock_move");
                sfx.Param("arrow_stop", 0f);
                sfx.Param("arrow_influence", 1.6f);
                semantograph.Play("spin");
                semantograph.Rate = 0.5f;
                float timer = 2f;
                while (timer > 0)
                {
                    sfx.Param("arrow_stop", ConvertRateToParaam());
                    timer -= Engine.DeltaTime;
                    if (timer <= 0.25f)
                    {
                        semantograph.Rate = 4f;
                    }
                    else if (timer <= 0.5f)
                    {
                        semantograph.Rate = 2f;
                    }
                    else if (timer <= 1f)
                    {
                        semantograph.Rate = 1f;
                    }
                    yield return null;
                }
            }

            public IEnumerator StopSpinning()
            {
                float timer = 2f;
                while (timer > 0)
                {
                    sfx.Param("arrow_stop", ConvertRateToParaam());
                    timer -= Engine.DeltaTime;
                    if (timer <= 1f)
                    {
                        semantograph.Rate = 0.5f;
                        if (sfx.InstancePlaying)
                        {
                            sfx.Stop(true);
                        }
                    }
                    else if (timer <= 1.5f)
                    {
                        semantograph.Rate = 1f;
                    }
                    else if (timer <= 1.75f)
                    {
                        semantograph.Rate = 2f;
                    }
                    yield return null;
                }
                while (semantograph.CurrentAnimationFrame != 6)
                {
                    yield return null;
                }
            }

            public IEnumerator Disappear()
            {
                semantograph.Play("idle");
                yield return 0.5f;
                while (alpha > 0f)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 2f);
                    semantograph.Color = Color.White * alpha;
                    yield return null;
                }
            }

            private float ConvertRateToParaam()
            {
                switch (semantograph.Rate)
                {
                    case 4f:
                        return 0.4f;
                    case 2f:
                        return 0.3f;
                    case 1f:
                        return 0.2f;
                    case 0.5f:
                        return 0.1f;
                    default:
                        return 0f;
                }
            }
        }

        private TextDisplay display;

        private SemantographDisplay semantograph;

        private string DialogID;

        private float TextVerticalOffset;

        private TalkComponent talk;

        private Player player;

        public string PlayerPose = "";

        private Image rune;

        private SoundSource sfx;

        private Color Tint;

        private int chapterIndex;

        public AncientText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            DialogID = data.Attr("dialogId");
            Tint = Calc.HexToColor(data.Attr("color", "FFFFFF"));
            TextVerticalOffset = data.Float("textOffset", 64f);
            Collider = new Hitbox(24f, 8f, -12f, 24f);
            rune = new(GFX.Game["objects/Xaphan/AncientText/rune00"]);
            rune.Color = Tint;
            rune.CenterOrigin();
            Add(rune);
        }

        public static void Load()
        {
            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
        }

        public static void Unload()
        {
            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
        }

        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false)
        {
            if (self.Entity is Player player && player.Sprite == self && self.Scene is Level level && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                foreach (AncientText text in level.Tracker.GetEntities<AncientText>())
                {
                    if (!string.IsNullOrEmpty(text.PlayerPose))
                    {
                        id = text.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex == -1 ? 0 : SceneAs<Level>().Session.Area.ChapterIndex;
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch0_Semantograph") && !XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + chapterIndex + "_AncientText_" + SceneAs<Level>().Session.Level))
            {
                Add(talk = new TalkComponent(new Rectangle(-12, 24, 24, 8), new Vector2(0, -12f), Interact));
                talk.PlayerMustBeFacing = false;
                Add(sfx = new SoundSource());
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(display = new TextDisplay(this));
            scene.Add(semantograph = new SemantographDisplay(this));
            player = scene.Tracker.GetEntity<Player>();
        }

        public override void Update()
        {
            base.Update();
            if (talk != null)
            {
                talk.Enabled = display != null && !display.Revealed && player != null && CollideCheck(player);
            }
        }

        private void Interact(Player player)
        {
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkToExact((int)BottomCenter.X);
            PlayerPose = "XaphanHelper_turnAround";
            player.Sprite.Play(PlayerPose);
            player.Sprite.OnLastFrame = delegate
            {
                PlayerPose = "XaphanHelper_turnAround_end";
                player.Sprite.Play(PlayerPose);
            };
            yield return 0.5f;

            semantograph.Visible = true;
            sfx.Play("event:/game/04_cliffside/arrowblock_reappear");
            yield return semantograph.Appear();
            yield return 0.3f;
            yield return semantograph.StartSpinning();
            yield return 0.25f;
            yield return display.RevealRoutine();
            yield return 0.25f;
            yield return semantograph.StopSpinning();
            yield return 0.3f;
            yield return semantograph.Disappear();
            semantograph.Visible = false;
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + chapterIndex + "_AncientText_" + SceneAs<Level>().Session.Level);

            yield return 0.5f;
            PlayerPose = "XaphanHelper_turnAround_reverse";
            player.Sprite.Play(PlayerPose);
            player.Sprite.OnLastFrame = delegate
            {
                PlayerPose = "";
            };
            yield return 0.2f;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }
    }
}
