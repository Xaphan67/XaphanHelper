using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomTorch")]
    class CustomTorch : Entity
    {
        public ParticleType P_OnLight;

        public Color Color;

        private EntityID eid = new();

        private bool lit;

        private bool playLitSound;

        private VertexLight light;

        private BloomPoint bloom;

        private bool startLit;

        private Sprite TorchSprite;

        private string sprite;

        private string flag;

        private string FlagName => "torch_" + eid.Key;

        private float alpha;

        private int startFade;

        private int endFade;

        private string sound;

        private bool noParticles;

        private int[] offFrames;

        private int[] turnOnFrames;

        private int[] onFrames;

        public CustomTorch(Vector2 position, Vector2 offset, string color, bool playLitSound, bool startLit, string sprite, string flag, float alpha, int startFade, int endFade, string sound, bool noParticles, string offFrames = null, string turnOnFrames = null, string onFrames = null) : base(position + offset)
        {
            this.playLitSound = playLitSound;
            this.startLit = startLit;
            this.flag = flag;
            this.sprite = sprite;
            if (offFrames != null)
            {
                this.offFrames = Array.ConvertAll(offFrames.Split(','), int.Parse);
            }
            else
            {
                this.offFrames = [0];
            }
            if (turnOnFrames != null)
            {
                this.turnOnFrames = Array.ConvertAll(turnOnFrames.Split(','), int.Parse);
            }
            else
            {
                this.turnOnFrames = [1, 2];
            }
            if (onFrames != null)
            {
                this.onFrames = Array.ConvertAll(onFrames.Split(','), int.Parse);
            }
            else
            {
                this.onFrames = [3, 4, 5, 6, 7];
            }
            this.alpha = alpha;
            this.noParticles = noParticles;
            if (alpha < 0)
            {
                alpha = 0;
            }
            else if (alpha > 1)
            {
                alpha = 1;
            }
            this.startFade = startFade;
            this.endFade = endFade;
            if (endFade < startFade)
            {
                endFade = startFade;
            }
            this.sound = sound;
            if (string.IsNullOrEmpty(sound))
            {
                sound = "event:/game/05_mirror_temple/torch_activate";
            }
            Color = Color.Lerp(Color.White, Calc.HexToColor(color), 0.5f);
            P_OnLight = new ParticleType
            {
                Color = Calc.HexToColor(color),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 0.8f,
                Size = 1f,
                SpeedMin = 80f,
                SpeedMax = 90f,
                SpeedMultiplier = 0.03f,
                DirectionRange = (float)Math.PI * 2f
            };
            Collider = new Hitbox(32f, 32f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Color, alpha, startFade, endFade));
            Add(bloom = new BloomPoint(0.5f, 8f));
            bloom.Visible = false;
            light.Visible = false;
            bloom.Position += new Vector2(8, 8);
            light.Position += new Vector2(8, 8);
            light.Color = Color;
            Depth = 2000;
            if (!string.IsNullOrEmpty(sprite))
            {
                Add(TorchSprite = new Sprite(GFX.Game, sprite));
                TorchSprite.AddLoop("off", "", 0.1f, this.offFrames);
                TorchSprite.AddLoop("turnOn", "", 0.08f, this.turnOnFrames);
                TorchSprite.AddLoop("on", "", 0.08f, this.onFrames);
                TorchSprite.CenterOrigin();
                TorchSprite.Position = TorchSprite.Position + new Vector2(8, 8);
                TorchSprite.Play("off");
            }
            else
            {
                Add(TorchSprite = GFX.SpriteBank.Create("torch"));
            }
        }

        public CustomTorch(EntityData data, Vector2 position, EntityID ID) : this(data.Position, position, data.Attr("color", "ffa500"), data.Bool("playLitSound"), data.Bool("startLit"), data.Attr("sprite"),
            data.Attr("flag"), data.Float("alpha", 1f), data.Int("startFade", 48), data.Int("endFade", 64), data.Attr("sound", "event:/game/05_mirror_temple/torch_activate"), data.Bool("noParticles"),
            data.Attr("offFrames", null), data.Attr("turnOnFrames", null), data.Attr("onFrames", null))
        {
            eid = ID;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(eid.Key))
            {
                eid = new EntityID(SceneAs<Level>().Session.Level, 0);
            }
            if (startLit || SceneAs<Level>().Session.GetFlag(FlagName) || SceneAs<Level>().Session.GetFlag(flag))
            {
                bloom.Visible = (light.Visible = true);
                lit = true;
                Collidable = false;
                TorchSprite.Play("on");
            }
            if (!string.IsNullOrEmpty(flag))
            {
                if (!SceneAs<Level>().Session.GetFlag(flag))
                {
                    lit = false;
                    bloom.Visible = false;
                    light.Visible = false;
                    SceneAs<Level>().Session.SetFlag(FlagName, false);
                    TorchSprite.Play("off");
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag))
            {
                if (!lit)
                {
                    if (playLitSound)
                    {
                        Audio.Play(sound, Position);
                    }
                    lit = true;
                    bloom.Visible = true;
                    light.Visible = true;
                    Collidable = false;
                    TorchSprite.Play("turnOn");
                    TorchSprite.OnLastFrame = onLastFrame;
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                    tween.OnUpdate = delegate (Tween t)
                    {
                        light.Color = Color.Lerp(Color.White, Color, t.Eased);
                        light.StartRadius = startFade + (1f - t.Eased) * 32f;
                        light.EndRadius = endFade + (1f - t.Eased) * 32f;
                        bloom.Alpha = alpha + 0.5f * (1f - t.Eased);
                    };
                    Add(tween);
                    SceneAs<Level>().Session.SetFlag(FlagName);
                    if (!noParticles)
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8), new Vector2(3f, 3f));
                    }
                }
            }
            else if (lit && !string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag))
            {
                lit = false;
                bloom.Visible = false;
                light.Visible = false;
                SceneAs<Level>().Session.SetFlag(FlagName, false);
                if (!noParticles)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8), new Vector2(3f, 3f));
                }
                TorchSprite.Play("off");
            }
        }

        private void onLastFrame(string s)
        {
            if (SceneAs<Level>().Session.GetFlag(FlagName))
            {
                TorchSprite.Play("on");
            }
        }

        private void OnPlayer(Player player)
        {
            if (flag == "" && !lit)
            {
                if (playLitSound)
                {
                    Audio.Play(sound, Position);
                }
                lit = true;
                bloom.Visible = true;
                light.Visible = true;
                Collidable = false;
                TorchSprite.Play("turnOn");
                TorchSprite.OnLastFrame = onLastFrame;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = delegate (Tween t)
                {
                    light.Color = Color.Lerp(Color.White, Color, t.Eased);
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    bloom.Alpha = alpha + 0.5f * (1f - t.Eased);
                };
                Add(tween);
                SceneAs<Level>().Session.SetFlag(FlagName);
                if (!noParticles)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8), new Vector2(3f, 3f));
                }
            }
        }
    }
}
