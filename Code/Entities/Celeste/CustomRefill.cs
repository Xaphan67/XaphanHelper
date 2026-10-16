using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomRefill")]
    public class CustomRefill : Entity
    {
        private class CooldownMask
        {
            public int Width, Height;

            public Vector2 Offset;

            public Color[] Colors;

            public bool[] IsEdge;

            public float[] Angle;
        }

        private Sprite sprite;

        private Sprite flash;

        public Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        public SineWave sine;

        public string type;

        public bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        public float respawnTimer;

        public float respawnTime;

        private string path;

        public CustomRefill(Vector2 position, string type, bool oneUse, float respawnTime, string directory = "objects/XaphanHelper/CustomRefill") : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            this.type = type;
            this.oneUse = type.Contains("Oxygen") ? true : oneUse;
            this.respawnTime = respawnTime;
            if (this.oneUse)
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
                    case "Oxygen":
                        spriteStr = "Oxygen";
                        break;
                    case "Oxygen Small":
                        spriteStr = "OxygenSmall";
                        break;
                }
                path = directory + "/refill" + spriteStr + (type.Contains("Oxygen") ? "/" : "Once/");
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
                path = directory + "/refill" + spriteStr + "/";
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

        public CustomRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("type", "One Dash"), data.Bool("oneUse"), data.Float("respawnTime"), data.Attr("directory", "objects/XaphanHelper/CustomRefill/"))
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

        private static readonly Dictionary<string, CooldownMask> masks = new Dictionary<string, CooldownMask>();

        private static CooldownMask GetMask(MTexture texture, string key)
        {
            if (masks.TryGetValue(key, out CooldownMask mask))
            {
                return mask;
            }
            Rectangle clip = texture.ClipRect;
            Color[] sourceColors = new Color[clip.Width * clip.Height];
            texture.Texture.Texture_Safe.GetData(0, clip, sourceColors, 0, sourceColors.Length);
            int width = clip.Width + 2;
            int height = clip.Height + 2;
            mask = new CooldownMask
            {
                Width = width,
                Height = height,
                Colors = new Color[width * height],
                IsEdge = new bool[width * height],
                Angle = new float[width * height],
                Offset = new Vector2(texture.DrawOffset.X - 1 - texture.Width / 2f, texture.DrawOffset.Y - 1 - texture.Height / 2f)
            };
            for (int y = 0; y < clip.Height; y++)
            {
                for (int x = 0; x < clip.Width; x++)
                {
                    mask.Colors[(y + 1) * width + (x + 1)] = sourceColors[y * clip.Width + x];
                }
            }

            bool Opaque(int x, int y)
            {
                return x >= 0 && y >= 0 && x < width && y < height && mask.Colors[y * width + x].A > 0;
            }

            bool HasOpaqueNeighbor(int x, int y)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if ((dx != 0 || dy != 0) && Opaque(x + dx, y + dy))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    bool opaque = mask.Colors[index].A > 0;
                    mask.IsEdge[index] = !opaque && HasOpaqueNeighbor(x, y);
                    if (opaque || mask.IsEdge[index])
                    {
                        float dx = x + 0.5f + mask.Offset.X;
                        float dy = y + 0.5f + mask.Offset.Y;
                        float angle = (float)Math.Atan2(dx, -dy);
                        if (angle < 0) angle += MathHelper.TwoPi;
                        mask.Angle[index] = angle / MathHelper.TwoPi;
                    }
                    else
                    {
                        mask.Angle[index] = -1f;
                    }
                }
            }
            masks[key] = mask;
            return mask;
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
            if (respawnTimer > 0f && !oneUse)
            {
                RenderCooldown();
            }
        }

        private void RenderCooldown()
        {
            CooldownMask mask = GetMask(GFX.Game.GetAtlasSubtextures(path + "idle")[0], path);
            float progress = Calc.Clamp(1f - respawnTimer / respawnTime, 0f, 1f);
            Vector2 origin = (Position + mask.Offset).Floor();
            for (int i = 0; i < mask.Angle.Length; i++)
            {
                float angle = mask.Angle[i];
                if (angle < 0f)
                {
                    continue;
                }
                bool filled = angle <= progress;
                Color color;
                if (mask.IsEdge[i])
                {
                    color = filled ? Color.Black : Color.Transparent;
                }
                else
                {
                    color = filled ? mask.Colors[i] * 0.6f : Color.Transparent;
                }
                Draw.Rect(origin.X + i % mask.Width, origin.Y + i / mask.Width + sine.Value * 2f, 1f, 1f, color);
            }
        }

        public void OnPlayer(Player player)
        {
            Drone drone = SceneAs<Level>().Tracker.GetEntity<Drone>();
            int maxMissileCount = 10;
            int maxSuperMissileCount = 5;
            if (type.Contains("Missiles"))
            {
                string Prefix = SceneAs<Level>().Session.Area.LevelSet;
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
            BreathManager manager = SceneAs<Level>().Tracker.GetEntity<BreathManager>();
            float airRegen = 0;
            if (manager != null)
            {
                int airSections = (type == "Oxygen" ? 15 : 5);
                airRegen = manager.air + (airSections * manager.currentMaxAir / 15);
                if (airRegen > manager.currentMaxAir)
                {
                    airRegen = manager.currentMaxAir;
                }
            }
            if ((((type == "One Dash" || type == "Two Dashes") && player.Dashes < (type == "Two Dashes" ? 2 : player.MaxDashes)) || (type.Contains("Jumps") && SpaceJump.GetJumpBuffer() == 0)) && drone == null || (type.Contains("Missiles") && drone != null) || (type.Contains("Oxygen") && manager != null && manager.air < manager.currentMaxAir && manager.air != -1) || (!type.Contains("Oxygen") && player.Stamina <= 20))
            {
                Audio.Play(type == "Two Dashes" ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player, drone, manager, maxMissileCount, maxSuperMissileCount, airRegen)));
                respawnTimer = respawnTime;
            }
        }

        private IEnumerator RefillRoutine(Player player, Drone drone, BreathManager manager, int maxMissileCount, int maxSuperMissileCount, float airRegen)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            Depth = 8999;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            if (type == "One Dash" || type == "Two Dashes")
            {
                if (type == "One Dash")
                {

                    player.Dashes+= 1;
                }
                else
                {
                    player.Dashes = 2;
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
            else if (type.Contains("Oxygen"))
            {
                manager.air = airRegen;
            }
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }
    }
}
