using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomRefill")]
    public class CustomRefill : Entity
    {
        private Sprite sprite;

        private Sprite flash;

        private Sprite outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private string type;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        private float respawnTime;

        public CustomRefill(Vector2 position, string type, bool oneUse, float respawnTime) : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            this.type = type;
            this.oneUse = oneUse;
            this.respawnTime = respawnTime;
            string path;
            if (oneUse)
            {
                string spriteStr = "";
                switch (type)
                {
                    case "Max Jumps":
                        spriteStr = "Jumps";
                        break;
                    case "Two Dashes":
                        spriteStr = "Two";
                        break;
                    case "Missiles":
                        spriteStr = "Missile";
                        break;
                    case "Super Missiles":
                        spriteStr = "SMissile";
                        break;
                }
                path = "objects/XaphanHelper/CustomRefill/refill" + spriteStr + "Once/";
                p_shatter = new ParticleType(Refill.P_Shatter)
                {
                    Color = Calc.HexToColor("DAECFA"),
                    Color2 = Calc.HexToColor("8FC7EF")
                };
                p_glow = new ParticleType(Refill.P_Glow)
                {
                    Color = Calc.HexToColor("BED6E9"),
                    Color2 = Calc.HexToColor("73A5CE")
                };
            }
            else
            {
                string spriteStr = "";
                p_shatter = Refill.P_Shatter;
                p_regen = Refill.P_Regen;
                p_glow = Refill.P_Glow;
                switch (type)
                {
                    case "Max Jumps":
                        spriteStr = "Jumps";
                        break;
                    case "Two Dashes":
                        spriteStr = "Two";
                        p_shatter = Refill.P_ShatterTwo;
                        p_regen = Refill.P_RegenTwo;
                        p_glow = Refill.P_GlowTwo;
                        break;
                    case "Missiles":
                        spriteStr = "Missile";
                        p_shatter = new ParticleType(Refill.P_Shatter)
                        {
                            Color = Calc.HexToColor("FADBDB"),
                            Color2 = Calc.HexToColor("EF9090")
                        };
                        p_regen = new ParticleType(Refill.P_Regen)
                        {
                            Color = Calc.HexToColor("E9BFBF"),
                            Color2 = Calc.HexToColor("CE7474")
                        };
                        p_glow = new ParticleType(Refill.P_Glow)
                        {
                            Color = Calc.HexToColor("E9BFBF"),
                            Color2 = Calc.HexToColor("CE7474")
                        };
                        break;
                    case "Super Missiles":
                        spriteStr = "SMissile";
                        p_shatter = new ParticleType(Refill.P_Shatter)
                        {
                            Color = Calc.HexToColor("DBFADB"),
                            Color2 = Calc.HexToColor("90EF90")
                        };
                        p_regen = new ParticleType(Refill.P_Regen)
                        {
                            Color = Calc.HexToColor("BFE9BF"),
                            Color2 = Calc.HexToColor("74CE74")
                        };
                        p_glow = new ParticleType(Refill.P_Glow)
                        {
                            Color = Calc.HexToColor("BFE9BF"),
                            Color2 = Calc.HexToColor("74CE74")
                        };
                        break;
                }
                path = "objects/XaphanHelper/CustomRefill/refill" + spriteStr + "/";
            }
            if (!oneUse)
            {
                Add(outline = new Sprite(GFX.Game, path + "outline"));
                outline.AddLoop("idle", "", respawnTime / (type == "Missiles" ? 37f : (type == "Super Missiles" ? 35f : (type == "Two Dashes" ? 25f : 21f))));
                outline.Visible = false;
                outline.CenterOrigin();
            }
            Add(sprite = new Sprite(GFX.Game, path + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, path + "flash"));
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
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            Depth = -100;
        }

        public CustomRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("type", "Max Dashes"), data.Bool("oneUse"), data.Float("respawnTime"))
        {

        }

        public static void Load()
        {
            On.Celeste.Player.UseRefill += modUseRefill;
        }

        public static void Unload()
        {
            On.Celeste.Player.UseRefill -= modUseRefill;
        }

        private static bool modUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes)
        {
            if (self.CollideCheck<CustomRefill>())
            {
                return false;
            }
            return orig(self, twoDashes);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                outline.Stop();
                Depth = -100;
                wiggler.Start();
                Audio.Play(type == "Two Dashes" ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = (bloom.Y = sine.Value * 2f);
            float num5 = (obj.Y = (obj2.Y = num2));
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
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            int maxMissileCount = 10;
            int maxSuperMissileCount = 5;
            if (type.Contains("Missiles"))
            {
                string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                foreach (string missileUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryDroneMissilesUpgrades : XaphanModule.ModSaveData.DroneMissilesUpgrades)
                {
                    if (missileUpgrade.Contains(Prefix))
                    {
                        maxMissileCount += 2;
                    }
                }
                foreach (string superMissileUpgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades : XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
                {
                    if (superMissileUpgrade.Contains(Prefix))
                    {
                        maxSuperMissileCount++;
                    }
                }
            }
            if (((type.Contains("Dashes") && player.Dashes < (type == "Two Dashes" ? 2 : player.MaxDashes)) || (type.Contains("Jumps") && SpaceJump.GetJumpBuffer() == 0)) && drone == null || (type.Contains("Missiles") && drone != null) || player.Stamina <= 20)
            {
                Audio.Play(type == "Two Dashes" ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player, drone, maxMissileCount, maxSuperMissileCount)));
                respawnTimer = respawnTime;
            }
        }

        private IEnumerator RefillRoutine(Player player, Drone drone, int maxMissileCount, int maxSuperMissileCount)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (!oneUse)
            {
                outline.Visible = true;
                outline.Play("idle");
            }
            Depth = 8999;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            if (type.Contains("Dashes"))
            {
                if (type == "Two Dashes")
                {
                    player.Dashes = 2;
                }
                else
                {
                    player.Dashes = player.MaxDashes;
                }
                SpaceJump.SetJumpBuffer(XaphanModule.ModSettings.SpaceJump - 1);
                player.RefillStamina();
            }
            else if (type.Contains("Jumps"))
            {
                SpaceJump.SetJumpBuffer(XaphanModule.ModSettings.SpaceJump - 1);
                player.RefillStamina();
            }
            else if (type.Contains("Missiles"))
            {
                if (type == "Missiles")
                {
                    drone.CurrentMissiles = maxMissileCount;
                }
                else
                {
                    drone.CurrentSuperMissiles = maxSuperMissileCount;
                }
            }
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }
    }
}
