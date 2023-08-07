using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/TimerRefill")]
    public class TimerRefill : Entity
    {
        public static ParticleType P_Shatter;

        public static ParticleType P_Regen;

        public static ParticleType P_Glow;

        public static ParticleType P_ShatterTwo;

        public static ParticleType P_RegenTwo;

        public static ParticleType P_GlowTwo;

        private Sprite sprite;

        private Sprite flash;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        private bool Appeared;

        private int timer;

        private string mode;

        private float respawnTime;

        public TimerRefill(EntityData data, Vector2 position) : base(data.Position + position)
        {
            oneUse = data.Bool("oneUse", false);
            timer = data.Int("timer", 10);
            mode = data.Attr("mode").ToLower();
            respawnTime = data.Float("respawnTime", 2.5f);
            if (timer < 3)
            {
                timer = 3;
            }
            if (string.IsNullOrEmpty(mode))
            {
                mode = "add";
            }
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            string str;
            str = "objects/XaphanHelper/TimerRefill/";
            p_shatter = Refill.P_Shatter;
            p_regen = Refill.P_Regen;
            p_glow = Refill.P_Glow;
            Add(outline = new Image(GFX.Game[str + "outline"]));
            outline.CenterOrigin();
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.2f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0, 16f));
            Add(light = new VertexLight(Color.White, 0, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            sprite.Visible = false;
            flash.Visible = false;
            Collidable = false;
            Appeared = false;
        }

        public override void Update()
        {
            base.Update();
            if (Appeared)
            {
                if (respawnTimer > 0f)
                {
                    respawnTimer -= Engine.DeltaTime;
                    if (respawnTimer <= 0f)
                    {
                        Respawn(true);
                    }
                }
                else if (Scene.OnInterval(0.1f))
                {
                    level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
                }
                UpdateY();
                light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
                bloom.Alpha = light.Alpha * 0.4f;
                if (Scene.OnInterval(2f) && sprite.Visible)
                {
                    flash.Play("flash", restart: true);
                    flash.Visible = true;
                }
            }
        }

        private void Respawn(bool NotEndTimer)
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                if (NotEndTimer)
                {
                    Audio.Play("event:/game/general/diamond_return", Position);
                    level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
                }
            }
        }

        private void UpdateY()
        {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = bloom.Y = sine.Value * 2f;
            float num5 = obj.Y = (obj2.Y = num2);
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Scene.Add(new CustomFollower.CustomFollowerCollectText(player.TopCenter + new Vector2(0f, -15f), 0, 2, "+ " + timer + " " + Dialog.Clean("XaphanHelper_Collect_Time" + (timer == 1 ? "_One" : "")), 0.75f));
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = respawnTime;
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            TimeManager manager = SceneAs<Level>().Tracker.GetEntity<TimeManager>();
            if (manager != null)
            {
                if (mode == "add")
                {
                    manager.AddTime(timer);
                }
                else
                {
                    manager.SetTime(timer);
                }
            }
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }

        public void Appear()
        {
            for (int i = 0; i < 6; i++)
            {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                SceneAs<Level>().ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
            }
            Appeared = true;
            outline.Visible = false;
            sprite.Visible = true;
            flash.Visible = true;
            bloom.Visible = true;
            Collidable = true;
        }

        public void Hide()
        {
            for (int i = 0; i < 6; i++)
            {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                SceneAs<Level>().ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
            }
            Appeared = false;
            outline.Visible = true;
            sprite.Visible = false;
            flash.Visible = false;
            bloom.Visible = false;
            Respawn(false);
            Collidable = false;
            Depth = 8999;
        }
    }
}
