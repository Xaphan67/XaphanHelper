using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/Slope")]
    class Slope : Solid
    {
        public class LightOccludeBlock : Entity
        {
            public LightOccludeBlock(Vector2 position, float width, float height) : base(position)
            {
                Collider = new Hitbox(width, height);
                Add(new LightOcclude());
                Depth = -10003;
            }

            public override void DebugRender(Camera camera)
            {

            }
        }

        public string Side;

        public string TilesTop;

        public string TilesBottom;

        public string Directory;

        public string FlagDirectory;

        public string Texture;

        public string FlagTexture;

        public bool Gentle;

        public bool CanSlide;

        public bool UpsideDown;

        public bool NoRender;

        public bool StickyDash;

        public int SoundIndex;

        public int SlopeHeight;

        public int[] variation = new int[35];

        public int[] variationInner = new int[35];

        private MTexture[,] BaseTextures;

        private MTexture[] SlopeTextures;

        public bool Rainbow;

        public bool CanJumpThrough;

        public bool VisualOnly;

        private bool AffectPlayerSpeed;

        public string Flag;

        public ColliderList colliderList;

        private List<LightOccludeBlock> lightOccludeBlocks = new();

        private string RenderMethod;

        public Slope(Vector2 position, Vector2 offset, bool gentle, string side, int soundIndex, int slopeHeight, string tilesTop, string tilesBottom, string texture, string flagTexture, bool canSlide, string directory, string flagDirectory, bool upsideDown, bool noRender, bool stickyDash, bool rainbow, bool canJumpThrough, string flag, bool affectPlayerSpeed, string renderMethod = "Type A", bool visualOnly = false) : base(position + offset, 0, 0, true)
        {

            Tag = Tags.TransitionUpdate;
            AllowStaticMovers = false;
            Collidable = false;
            Gentle = gentle;
            Side = side;
            SoundIndex = soundIndex;
            SlopeHeight = slopeHeight;
            TilesTop = tilesTop;
            TilesBottom = tilesBottom;
            Texture = texture;
            FlagTexture = flagTexture;
            CanSlide = canSlide;
            Directory = directory;
            FlagDirectory = flagDirectory;
            UpsideDown = upsideDown;
            NoRender = noRender;
            StickyDash = stickyDash;
            Rainbow = rainbow;
            CanJumpThrough = canJumpThrough;
            VisualOnly = visualOnly;
            Flag = flag;
            AffectPlayerSpeed = affectPlayerSpeed;
            RenderMethod = renderMethod;
            if (!VisualOnly)
            {
                if (!upsideDown)
                {
                    if (side == "Left")
                    {
                        colliderList = new ColliderList(new Hitbox(4, 1, 0, 0));
                        if (!CanJumpThrough)
                        {
                            lightOccludeBlocks.Add(new LightOccludeBlock(Position, 8, 2));
                        }
                        for (int i = 0; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(4, 1, (Gentle ? i * 16 : i * 8), i * 8));
                            }
                            colliderList.Add(new Hitbox(Gentle ? 6 : 5, 1, (Gentle ? i * 16 : i * 8), 1 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 8 : 6, 1, (Gentle ? i * 16 : i * 8), 2 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 10 : 7, 1, (Gentle ? i * 16 : i * 8), 3 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 12 : 8, 1, (Gentle ? i * 16 : i * 8), 4 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 14 : 9, 1, (Gentle ? i * 16 : i * 8), 5 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 16 : 10, 1, (Gentle ? i * 16 : i * 8), 6 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 18 : 11, 1, (Gentle ? i * 16 : i * 8), 7 + i * 8));
                            if (!CanJumpThrough)
                            {
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 1 + i * 8), 4 + (Gentle ? 6 : 5), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 3 + i * 8), 4 + (Gentle ? 10 : 7), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 5 + i * 8), 4 + (Gentle ? 14 : 9), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 7 + i * 8), 4 + (Gentle ? 18 : 11), 2));
                            }
                        }
                        Collider = colliderList;
                    }
                    else
                    {
                        colliderList = new ColliderList(new Hitbox(4, 1, 20, 0));
                        if (!CanJumpThrough)
                        {
                            lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2(16, 0), 8, 2));
                        }
                        for (int i = 0; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(4, 1, 20 + (Gentle ? i * -16 : i * -8), i * 8));
                            }
                            colliderList.Add(new Hitbox(Gentle ? 6 : 5, 1, (Gentle ? 18f - i * 16 : 19f - i * 8), 1 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 8 : 6, 1, (Gentle ? 16f - i * 16 : 18f - i * 8), 2 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 10 : 7, 1, (Gentle ? 14f - i * 16 : 17f - i * 8), 3 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 12 : 8, 1, (Gentle ? 12f - i * 16 : 16f - i * 8), 4 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 14 : 9, 1, (Gentle ? 10f - i * 16 : 15f - i * 8), 5 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 16 : 10, 1, (Gentle ? 8f - i * 16 : 14f - i * 8), 6 + i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 18 : 11, 1, (Gentle ? 6f - i * 16 : 13f - i * 8), 7 + i * 8));
                            if (!CanJumpThrough)
                            {
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 14f - i * 16 : 15f - i * 8), 1 + i * 8), 4 + (Gentle ? 6 : 5), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 10f - i * 16 : 13f - i * 8), 3 + i * 8), 4 + (Gentle ? 10 : 7), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 6f - i * 16 : 11f - i * 8), 5 + i * 8), 4 + (Gentle ? 14 : 9), 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 2f - i * 16 : 9f - i * 8), 7 + i * 8), 4 + (Gentle ? 18 : 11), 2));
                            }
                        }
                        Collider = colliderList;
                    }
                }
                else
                {
                    if (side == "Left")
                    {
                        colliderList = new ColliderList(new Hitbox(8, 1, 0, 15));
                        if (!CanJumpThrough)
                        {
                            lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2(0, 14), 8, 2));
                        }
                        for (int i = 0; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(8, 1, (Gentle ? i * 16 : i * 8), 15 + i * -8));
                            }
                            colliderList.Add(new Hitbox(Gentle ? 10 : 9, 1, (Gentle ? i * 16 : i * 8), 7 + 7 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 12 : 10, 1, (Gentle ? i * 16 : i * 8), 7 + 6 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 14 : 11, 1, (Gentle ? i * 16 : i * 8), 7 + 5 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 16 : 12, 1, (Gentle ? i * 16 : i * 8), 7 + 4 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 18 : 13, 1, (Gentle ? i * 16 : i * 8), 7 + 3 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 20 : 14, 1, (Gentle ? i * 16 : i * 8), 7 + 2 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 22 : 15, 1, (Gentle ? i * 16 : i * 8), 7 + 1 - i * 8));
                            if (!CanJumpThrough)
                            {
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 7 + 6 - i * 8), Gentle ? 10 : 9, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 7 + 4 - i * 8), Gentle ? 14 : 11, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 7 + 2 - i * 8), Gentle ? 18 : 13, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? i * 16 : i * 8), 7 + 0 - i * 8), Gentle ? 22 : 15, 2));
                            }
                        }
                        Collider = colliderList;
                    }
                    else
                    {
                        colliderList = new ColliderList(new Hitbox(8, 1, 16, 15));
                        if (!CanJumpThrough)
                        {
                            lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2(16, 14), 8, 2));
                        }
                        for (int i = 0; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(8, 1, 16 + (Gentle ? i * -16 : i * -8), 15 + i * -8));
                            }
                            colliderList.Add(new Hitbox(Gentle ? 10 : 9, 1, (Gentle ? 14f - i * 16 : 15f - i * 8), 7 + 7 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 12 : 10, 1, (Gentle ? 12f - i * 16 : 14f - i * 8), 7 + 6 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 14 : 11, 1, (Gentle ? 10f - i * 16 : 13f - i * 8), 7 + 5 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 16 : 12, 1, (Gentle ? 8f - i * 16 : 12f - i * 8), 7 + 4 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 18 : 13, 1, (Gentle ? 6f - i * 16 : 11f - i * 8), 7 + 3 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 20 : 14, 1, (Gentle ? 4f - i * 16 : 10f - i * 8), 7 + 2 - i * 8));
                            colliderList.Add(new Hitbox(Gentle ? 22 : 15, 1, (Gentle ? 2f - i * 16 : 9f - i * 8), 7 + 1 - i * 8));
                            if (!CanJumpThrough)
                            {
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 14f - i * 16 : 15f - i * 8), 7 + 6 - i * 8), Gentle ? 10 : 9, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 10f - i * 16 : 13f - i * 8), 7 + 4 - i * 8), Gentle ? 14 : 11, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 6f - i * 16 : 11f - i * 8), 7 + 2 - i * 8), Gentle ? 18 : 13, 2));
                                lightOccludeBlocks.Add(new LightOccludeBlock(Position + new Vector2((Gentle ? 2f - i * 16 : 9f - i * 8), 7 + 0 - i * 8), Gentle ? 22 : 15, 2));
                            }
                        }
                        Collider = colliderList;
                    }
                }
            }
            if (SlopeHeight < 1)
            {
                SlopeHeight = 1;
            }
            else if (SlopeHeight > 30)
            {
                SlopeHeight = 30;
            }
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/Slope";
            }
            if (string.IsNullOrEmpty(FlagDirectory))
            {
                FlagDirectory = "objects/XaphanHelper/Slope";
            }
            Depth = -10003;
        }

        public Slope(EntityData data, Vector2 offset) : this(data.Position, offset, data.Bool("gentle"), data.Attr("side"), data.Int("soundIndex"), data.Int("slopeHeight", 1), data.Attr("tilesTop"), data.Attr("tilesBottom"),
            data.Attr("texture", "cement"), data.Attr("flagTexture", ""), data.Bool("canSlide", false), data.Attr("customDirectory", ""), data.Attr("flagCustomDirectory", ""), data.Bool("upsideDown", false), data.Bool("noRender", false), data.Bool("stickyDash", false), data.Bool("rainbow", false),
            data.Bool("canJumpThrough", false), data.Attr("flag", ""), data.Bool("affectPlayerSpeed", false), data.Attr("renderMethod", "Type A"))
        {

        }

        public static void Load()
        {
            On.Celeste.Actor.MoveH += onActorMoveH;
            On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int += onActorTrySquishWiggle;
            On.Celeste.TheoCrystal.Update += TheoCrystalOnUpdate;
            On.Celeste.TheoCrystal.OnCollideH += TheoCrystalOnOnCollideH;
            On.Celeste.Glider.Update += GliderOnUpdate;
            On.Celeste.Glider.OnCollideH += GliderOnOnCollideH;
            On.Celeste.Puffer.Update += PufferOnUpdate;
            On.Celeste.Seeker.Update += SeekerOnUpdate;
            On.Celeste.Debris.Update += DebrisOnUpdate;
            On.Celeste.MoveBlock.Update += MoveBlockOnUpdate;
            On.Celeste.Player.Update += modPlayerUpdate;
            IL.Celeste.Player.NormalUpdate += ilPlayerNormalUpdate;
        }

        private static bool onActorTrySquishWiggle(On.Celeste.Actor.orig_TrySquishWiggle_CollisionData_int_int orig, Actor self, CollisionData data, int wiggleX, int wiggleY)
        {
            if (data.Pusher.GetType() == typeof(PlayerPlatform))
            {
                PlayerPlatform platform = (PlayerPlatform)data.Pusher;
                if (platform.UpsideDown)
                {
                    self.Top = platform.Bottom;
                    return true;
                }
            }
            return orig(self, data, wiggleX, wiggleY);
        }

        public static void Unload()
        {
            On.Celeste.Actor.MoveH -= onActorMoveH;
            On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int -= onActorTrySquishWiggle;
            On.Celeste.TheoCrystal.Update -= TheoCrystalOnUpdate;
            On.Celeste.TheoCrystal.OnCollideH -= TheoCrystalOnOnCollideH;
            On.Celeste.Glider.Update -= GliderOnUpdate;
            On.Celeste.Glider.OnCollideH -= GliderOnOnCollideH;
            On.Celeste.Puffer.Update -= PufferOnUpdate;
            On.Celeste.Seeker.Update -= SeekerOnUpdate;
            On.Celeste.Debris.Update -= DebrisOnUpdate;
            On.Celeste.MoveBlock.Update -= MoveBlockOnUpdate;
            On.Celeste.Player.Update -= modPlayerUpdate;
            IL.Celeste.Player.NormalUpdate -= ilPlayerNormalUpdate;

        }

        private static bool onActorMoveH(On.Celeste.Actor.orig_MoveH orig, Actor self, float moveH, Collision onCollide, Solid pusher)
        {
            if (self.CollideCheck<Slope>(self.Position + Vector2.UnitY) && self.GetType() != typeof(Player))
            {
                moveH = 0;
            }
            return orig(self, moveH, onCollide, pusher);
        }

        private static void TheoCrystalOnUpdate(On.Celeste.TheoCrystal.orig_Update orig, TheoCrystal self)
        {
            if (self.GetType() != typeof(TheoCrystal))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            if (!self.Hold.IsHeld)
            {
                foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
                {
                    if (slope.UpsideDown && self.CollideCheck(slope))
                    {
                        self.Position.Y += 1;
                    }
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void TheoCrystalOnOnCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            if (data.Hit is Slope)
            {
                self.Speed.X *= -0.4f;
            }
            else
            {
                orig(self, data);
            }
        }

        private static void GliderOnUpdate(On.Celeste.Glider.orig_Update orig, Glider self)
        {
            if (self.GetType() != typeof(Glider))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            if (!self.Hold.IsHeld)
            {
                foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
                {
                    if (slope.UpsideDown && self.CollideCheck(slope))
                    {
                        self.Position.Y += 1;
                    }
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void GliderOnOnCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
        {
            if (data.Hit is Slope)
            {
                if (self.Speed.X < -50f)
                {
                    Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", self.Position);
                }
                else if (self.Speed.X > 50f)
                {
                    Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", self.Position);
                }
                self.Speed.X *= -1f;
            }
            else
            {
                orig(self, data);
            }
        }

        private static void PufferOnUpdate(On.Celeste.Puffer.orig_Update orig, Puffer self)
        {
            if (self.GetType() != typeof(Puffer))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
            {
                if (slope.UpsideDown && self.CollideCheck(slope))
                {
                    self.Position.Y += 1;
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void SeekerOnUpdate(On.Celeste.Seeker.orig_Update orig, Seeker self)
        {
            if (self.GetType() != typeof(Seeker))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
            {
                if (slope.UpsideDown && self.CollideCheck(slope))
                {
                    self.Position.Y += 1;
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void DebrisOnUpdate(On.Celeste.Debris.orig_Update orig, Debris self)
        {
            if (self.GetType() != typeof(Debris))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
            {
                if (slope.UpsideDown && self.CollideCheck(slope))
                {
                    self.Position.Y += 1;
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void modPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (XaphanModule.onSlope && self.Bottom != XaphanModule.onSlopeTop && self.Speed.X != 0 && XaphanModule.onSlopeAffectPlayerSpeed)
            {
                XaphanModule.MaxRunSpeed += 0.025f;
            }
            else
            {
                if (self.Speed.X == 0)
                {
                    XaphanModule.MaxRunSpeed = 0f;
                }
                if (XaphanModule.MaxRunSpeed != 0)
                {
                    XaphanModule.MaxRunSpeed -= 0.025f;
                }
            }
            if (XaphanModule.MaxRunSpeed > 0.4f)
            {
                XaphanModule.MaxRunSpeed = 0.4f;
            }
            if (XaphanModule.MaxRunSpeed < 0)
            {
                XaphanModule.MaxRunSpeed = 0;
            }
            orig(self);
        }

        private static void MoveBlockOnUpdate(On.Celeste.MoveBlock.orig_Update orig, MoveBlock self)
        {
            if (self.GetType() != typeof(MoveBlock))
            {
                orig(self);
                return;
            }
            SetCollisionBeforeUpdate(self);
            orig(self);
            foreach (Slope slope in self.SceneAs<Level>().Tracker.GetEntities<Slope>())
            {
                if (slope.UpsideDown && self.CollideCheck(slope))
                {
                    self.Position.Y += 1;
                }
            }
            SetCollisionAfterUpdate(self);
        }

        private static void ilPlayerNormalUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            // Increase X speed based on MaxRunSpeed value

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(90f))
                && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S
                    && (((VariableDefinition)instr.Operand).Index == 6 || ((VariableDefinition)instr.Operand).Index == 31)))
            {
                VariableDefinition variable = (VariableDefinition)cursor.Next.Operand;
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldflda))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.Emit(OpCodes.Stloc_S, variable);
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }
        }

        private static float determineSpeedXFactor()
        {
            float speedFactor = 0f;
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                Player player = level.Tracker.GetEntity<Player>();
                bool wasAccelerating = false;
                if (XaphanModule.onSlope && player.Bottom != XaphanModule.onSlopeTop)
                {
                    if ((XaphanModule.onSlopeDir == -1 && player.Speed.X < 0) || (XaphanModule.onSlopeDir == 1 && player.Speed.X > 0))
                    {
                        speedFactor = -(XaphanModule.onSlopeGentle ? XaphanModule.MaxRunSpeed / 2 : XaphanModule.MaxRunSpeed);
                    }
                    else if ((XaphanModule.onSlopeDir == -1 && player.Speed.X > 0) || (XaphanModule.onSlopeDir == 1 && player.Speed.X < 0))
                    {
                        wasAccelerating = true;
                        speedFactor = (XaphanModule.onSlopeGentle ? XaphanModule.MaxRunSpeed / 2 : XaphanModule.MaxRunSpeed);
                    }
                }
                else if (XaphanModule.MaxRunSpeed > 0)
                {
                    speedFactor = wasAccelerating ? XaphanModule.MaxRunSpeed : -XaphanModule.MaxRunSpeed;
                }
            }
            return 1f + speedFactor;
        }

        public static void SetCollisionBeforeUpdate(Actor actor)
        {
            List<Entity> playerPlatforms = actor.Scene.Tracker.GetEntities<PlayerPlatform>().ToList();
            List<Entity> slopes = actor.Scene.Tracker.GetEntities<Slope>().ToList();
            foreach (Slope slope in slopes)
            {
                if (slope.CollideCheck(actor))
                {
                    slope.Collidable = true;
                }
                else
                {
                    if (slope.CanJumpThrough)
                    {
                        if (!slope.UpsideDown)
                        {
                            if (slope.Side == "Right")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (actor.BottomCenter.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (actor.BottomCenter.X - slope.SlopeTop.X) >= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                            else if (slope.Side == "Left")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (actor.BottomCenter.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (actor.BottomCenter.X - slope.SlopeTop.X) <= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                        }
                        else
                        {
                            if (slope.Side == "Right")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (actor.TopRight.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (actor.TopRight.X - slope.SlopeTop.X) >= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                            else if (slope.Side == "Left")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (actor.TopLeft.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (actor.TopLeft.X - slope.SlopeTop.X) <= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                        }
                    }
                    else if (!slope.UpsideDown || (slope.UpsideDown && ((slope.Side == "Right" && actor.Right < slope.Right) || (slope.Side == "Left" && actor.Left > slope.Left))))
                    {
                        slope.Collidable = true;
                    }
                }
            }
            foreach (PlayerPlatform platform in playerPlatforms)
            {
                platform.Collidable = false;
            }
        }

        public static void SetCollisionBeforeUpdate(Solid solid)
        {
            List<Entity> playerPlatforms = solid.Scene.Tracker.GetEntities<PlayerPlatform>().ToList();
            List<Entity> slopes = solid.Scene.Tracker.GetEntities<Slope>().ToList();
            foreach (Slope slope in slopes)
            {
                if (slope.CollideCheck(solid))
                {
                    slope.Collidable = true;
                }
                else
                {
                    if (slope.CanJumpThrough)
                    {
                        if (!slope.UpsideDown)
                        {
                            if (slope.Side == "Right")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (solid.BottomCenter.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (solid.BottomCenter.X - slope.SlopeTop.X) >= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                            else if (slope.Side == "Left")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (solid.BottomCenter.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (solid.BottomCenter.X - slope.SlopeTop.X) <= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                        }
                        else
                        {
                            if (slope.Side == "Right")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (solid.TopRight.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (solid.TopRight.X - slope.SlopeTop.X) >= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                            else if (slope.Side == "Left")
                            {
                                if ((slope.SlopeBottom.X - slope.SlopeTop.X) * (solid.TopLeft.Y - slope.SlopeTop.Y) - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (solid.TopLeft.X - slope.SlopeTop.X) <= 0)
                                {
                                    slope.Collidable = true;
                                }
                            }
                        }
                    }
                    else if (!slope.UpsideDown || (slope.UpsideDown && ((slope.Side == "Right" && solid.Right < slope.Right) || (slope.Side == "Left" && solid.Left > slope.Left))))
                    {
                        slope.Collidable = true;
                    }
                }
            }
            foreach (PlayerPlatform platform in playerPlatforms)
            {
                platform.Collidable = false;
            }
        }


        public static void SetCollisionAfterUpdate(Entity entity)
        {
            List<Entity> slopes = entity.Scene.Tracker.GetEntities<Slope>().ToList();
            Player player = entity.Scene.Tracker.GetEntity<Player>();
            slopes.ForEach(entity => entity.Collidable = false);
            foreach (PlayerPlatform platform in entity.Scene.Tracker.GetEntities<PlayerPlatform>())
            {
                platform.SetCollision(player);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (LightOccludeBlock lightOccludeBlock in lightOccludeBlocks)
            {
                SceneAs<Level>().Add(lightOccludeBlock);
            }
            MTexture mtexture = GFX.Game[((!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag)) ? FlagDirectory : Directory) + "/" + ((!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag)) ? FlagTexture : Texture)];
            BaseTextures = new MTexture[6, 15];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    BaseTextures[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            SlopeTextures = new MTexture[96];
            if (!UpsideDown)
            {
                for (int k = 0; k < 32; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k, 0, 1, 8));
                }
                for (int k = 32; k < 64; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 32, 88, 1, 8));
                }
                for (int k = 64; k < 96; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 64, 96, 1, 8));
                }
            }
            else
            {
                for (int k = 0; k < 32; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k, 8, 1, 8));
                }
                for (int k = 32; k < 64; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 32, 104, 1, 8));
                }
                for (int k = 64; k < 96; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 64, 112, 1, 8));
                }
            }
            int Variation;
            Math.DivRem((int)Position.X / 8, 4, out Variation);
            Variation = Math.Abs(Variation);
            variation[0] = Variation;

            for (int l = 1; l < 35; l++)
            {
                Variation++;
                if (Variation >= 4)
                {
                    Variation = 0;
                }
                variation[l] = Variation;
            }
            Math.DivRem((int)Position.X / 8, 12, out Variation);
            Variation = Math.Abs(Variation);
            variationInner[0] = Variation;

            for (int m = 1; m < 35; m++)
            {
                Variation++;
                if (Variation >= 12)
                {
                    Variation = 0;
                }
                variationInner[m] = Variation;
            }
            if (!VisualOnly)
            {
                SceneAs<Level>().Add(new PlayerPlatform(Position + new Vector2(Side == "Right" ? ((Gentle ? -(SlopeHeight - 1) * 16 : -(SlopeHeight - 1) * 8) + 8) * (UpsideDown ? -1 : 1) : 0 + 0, (8 * (SlopeHeight - 1) + 4)) * (UpsideDown ? -1 : 1), Gentle ? 8 + 16 * SlopeHeight : 8 + 8 * SlopeHeight, Gentle, Side, SoundIndex, SlopeHeight, CanSlide, Top, AffectPlayerSpeed, UpsideDown, StickyDash, CanJumpThrough));
                if (!UpsideDown)
                {
                    SceneAs<Level>().Add(new FakePlayerPlatform(Position + new Vector2(Side == "Right" ? ((Gentle ? -(SlopeHeight - 1) * 16 : -(SlopeHeight - 1) * 8) + 8) * (UpsideDown ? -1 : 1) : 0 + 0, (8 * (SlopeHeight - 1) + 4)) * (UpsideDown ? -1 : 1), Gentle ? 8 + 16 * SlopeHeight : 8 + 8 * SlopeHeight, Gentle, Side, SoundIndex, SlopeHeight, Top, UpsideDown, StickyDash, CanJumpThrough));
                }
            }

            if (!UpsideDown)
            {
                if (Side == "Left")
                {
                    SlopeTop = Position + new Vector2(7, 0);
                    SlopeBottom = Position + new Vector2(7 + (Gentle ? 16 : 8) * SlopeHeight, 8 * SlopeHeight);
                }
                else if (Side == "Right")
                {
                    SlopeTop = Position + new Vector2(17, 0);
                    SlopeBottom = Position + new Vector2(17 + (Gentle ? 16 : 8) * -SlopeHeight, 8 * SlopeHeight);
                }
            }
            else
            {
                if (Side == "Left")
                {
                    SlopeTop = Position + new Vector2(7 + (Gentle ? 16 : 8) * SlopeHeight, -16);
                    SlopeBottom = Position + new Vector2(7, -16 + 8 * SlopeHeight);
                }
                else if (Side == "Right")
                {
                    SlopeTop = Position + new Vector2(17 + (Gentle ? 16 : 8) * -SlopeHeight, -16);
                    SlopeBottom = Position + new Vector2(17, -16 + 8 * SlopeHeight);
                }
            }
        }

        public Vector2 SlopeTop;

        public Vector2 SlopeBottom;

        private Color GetHue(Vector2 position)
        {
            float num = 280f;
            float value = (position.Length() + Scene.TimeActive * 50f) % num / num;
            return Calc.HsvToColor(0.4f + Calc.YoYo(value) * 0.4f, 0.4f, 0.9f);
        }

        public override void Update()
        {
            if (SceneAs<Level>().Tracker.GetEntities<DroneDebris>().Count > 0 && !CanJumpThrough)
            {
                Collidable = true;
            }
            else
            {
                Collidable = false;
            }
        }

        public void DrawSlopesTiles(Vector2 Pos)
        {
            if (!UpsideDown)
            {
                if (Side == "Left")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                BaseTextures[5, 0 + variationInner[i]].Draw(Pos = Position + new Vector2(-24f + i * 16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[i + 1]].Draw(Pos = Position + new Vector2(-16f + i * 16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[i + 2]].Draw(Pos = Position + new Vector2(-8f + i * 16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[i + 3]].Draw(Pos = Position + new Vector2(i * 16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            SlopeTextures[0 + variation[j] * 8].Draw(Pos = Position + new Vector2(8f + j * 16f, j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[1 + variation[j] * 8].Draw(Pos = Position + new Vector2(9f + j * 16f, j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[2 + variation[j] * 8].Draw(Pos = Position + new Vector2(10f + j * 16f, 1f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[3 + variation[j] * 8].Draw(Pos = Position + new Vector2(11f + j * 16f, 1f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[4 + variation[j] * 8].Draw(Pos = Position + new Vector2(12f + j * 16f, 2f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[5 + variation[j] * 8].Draw(Pos = Position + new Vector2(13f + j * 16f, 2f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[6 + variation[j] * 8].Draw(Pos = Position + new Vector2(14f + j * 16f, 3f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[7 + variation[j] * 8].Draw(Pos = Position + new Vector2(15f + j * 16f, 3f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[0 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(16f + j * 16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(17f + j * 16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(18f + j * 16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(19f + j * 16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(20f + j * 16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(21f + j * 16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(22f + j * 16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[7 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(23f + j * 16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(16f + j * 16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(17f + j * 16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(18f + j * 16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(19f + j * 16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(20f + j * 16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(21f + j * 16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(22f + j * 16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(23f + j * 16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);

                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    BaseTextures[5, 0 + variationInner[k]].Draw(Pos = Position + new Vector2(-16 + k * 8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[k + 1]].Draw(Pos = Position + new Vector2(-8 + k * 8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[k + 2]].Draw(Pos = Position + new Vector2(k * 8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[0 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * 8f, l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * 8f, 1f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * 8f, 2f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * 8f, 3f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * 8f, 4f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * 8f, 5f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * 8f, 6f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[7 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * 8f, 7f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * 8f, l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * 8f, 1f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * 8f, 2f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * 8f, 3f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * 8f, 4f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * 8f, 5f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * 8f, 6f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * 8f, 7f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);

                            }
                        }
                    }
                }
                else if (Side == "Right")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                BaseTextures[5, 0 + variationInner[i]].Draw(Pos = Position + new Vector2(40f + i * -16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[i + 1]].Draw(Pos = Position + new Vector2(32f + i * -16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[i + 2]].Draw(Pos = Position + new Vector2(24f + i * -16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[i + 3]].Draw(Pos = Position + new Vector2(16f + i * -16f, i * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            SlopeTextures[7 + variation[j] * 8].Draw(Pos = Position + new Vector2(15f + j * -16f, j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[6 + variation[j] * 8].Draw(Pos = Position + new Vector2(14f + j * -16f, j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[5 + variation[j] * 8].Draw(Pos = Position + new Vector2(13f + j * -16f, 1f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[4 + variation[j] * 8].Draw(Pos = Position + new Vector2(12f + j * -16f, 1f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[3 + variation[j] * 8].Draw(Pos = Position + new Vector2(11f + j * -16f, 2f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[2 + variation[j] * 8].Draw(Pos = Position + new Vector2(10f + j * -16f, 2f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[1 + variation[j] * 8].Draw(Pos = Position + new Vector2(9f + j * -16f, 3f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[0 + variation[j] * 8].Draw(Pos = Position + new Vector2(8f + j * -16f, 3f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[7 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(7f + j * -16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(6f + j * -16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(5f + j * -16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(4f + j * -16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(3f + j * -16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(2f + j * -16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(1f + j * -16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[0 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(j * -16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(7f + j * -16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(6f + j * -16f, 4f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(5f + j * -16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(4f + j * -16f, 5f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(3f + j * -16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(2f + j * -16f, 6f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(1f + j * -16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(j * -16f, 7f + j * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    BaseTextures[5, 0 + variationInner[k]].Draw(Pos = Position + new Vector2(32f + k * -8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[k + 1]].Draw(Pos = Position + new Vector2(24f + k * -8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[k + 2]].Draw(Pos = Position + new Vector2(16f + k * -8f, k * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[7 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * -8f, l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * -8f, 1f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * -8f, 2f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * -8f, 3f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * -8f, 4f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * -8f, 5f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * -8f, 6f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[0 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * -8f, 7f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * -8f, l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * -8f, 1f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * -8f, 2f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * -8f, 3f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * -8f, 4f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * -8f, 5f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * -8f, 6f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * -8f, 7f + l * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Side == "Left")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                BaseTextures[5, 0 + variationInner[i]].Draw(Pos = Position + new Vector2(-24f + i * 16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[i + 1]].Draw(Pos = Position + new Vector2(-16f + i * 16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[i + 2]].Draw(Pos = Position + new Vector2(-8f + i * 16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[i + 3]].Draw(Pos = Position + new Vector2(i * 16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            SlopeTextures[0 + variation[j] * 8].Draw(Pos = Position + new Vector2(8f + j * 16f, j * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[1 + variation[j] * 8].Draw(Pos = Position + new Vector2(9f + j * 16f, j * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[2 + variation[j] * 8].Draw(Pos = Position + new Vector2(10f + j * 16f, 1f + j * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[3 + variation[j] * 8].Draw(Pos = Position + new Vector2(11f + j * 16f, 1f + j * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[4 + variation[j] * 8].Draw(Pos = Position + new Vector2(12f + j * 16f, 2f + j * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[5 + variation[j] * 8].Draw(Pos = Position + new Vector2(13f + j * 16f, 2f + j * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[6 + variation[j] * 8].Draw(Pos = Position + new Vector2(14f + j * 16f, 3f + j * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[7 + variation[j] * 8].Draw(Pos = Position + new Vector2(15f + j * 16f, 3f + j * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[0 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(16f + j * 16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(17f + j * 16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(18f + j * 16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(19f + j * 16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(20f + j * 16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(21f + j * 16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(22f + j * 16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[7 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(23f + j * 16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(16f + j * 16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(17f + j * 16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(18f + j * 16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(19f + j * 16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(20f + j * 16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(21f + j * 16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(22f + j * 16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(23f + j * 16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    BaseTextures[5, 0 + variationInner[k]].Draw(Pos = Position + new Vector2(-16 + k * 8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[k + 1]].Draw(Pos = Position + new Vector2(-8 + k * 8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[k + 2]].Draw(Pos = Position + new Vector2(k * 8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[0 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * 8f, l * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * 8f, 1f + l * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * 8f, 2f + l * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * 8f, 3f + l * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * 8f, 4f + l * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * 8f, 5f + l * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * 8f, 6f + l * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[7 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * 8f, 7f + l * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * 8f, l * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * 8f, 1f + l * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * 8f, 2f + l * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * 8f, 3f + l * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * 8f, 4f + l * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * 8f, 5f + l * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * 8f, 6f + l * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * 8f, 7f + l * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);

                            }
                        }
                    }
                }
                else if (Side == "Right")
                {
                    if (Gentle)
                    {
                        for (int i = 1; i <= SlopeHeight - 1; i++)
                        {
                            if (i > 1)
                            {
                                BaseTextures[5, 0 + variationInner[i]].Draw(Pos = Position + new Vector2(40f + i * -16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[i + 1]].Draw(Pos = Position + new Vector2(32f + i * -16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[i + 2]].Draw(Pos = Position + new Vector2(24f + i * -16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[i + 3]].Draw(Pos = Position + new Vector2(16f + i * -16f, i * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int j = 0; j <= SlopeHeight - 1; j++)
                        {
                            SlopeTextures[7 + variation[j] * 8].Draw(Pos = Position + new Vector2(15f + j * -16f, j * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[6 + variation[j] * 8].Draw(Pos = Position + new Vector2(14f + j * -16f, j * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[5 + variation[j] * 8].Draw(Pos = Position + new Vector2(13f + j * -16f, 1f + j * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[4 + variation[j] * 8].Draw(Pos = Position + new Vector2(12f + j * -16f, 1f + j * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[3 + variation[j] * 8].Draw(Pos = Position + new Vector2(11f + j * -16f, 2f + j * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[2 + variation[j] * 8].Draw(Pos = Position + new Vector2(10f + j * -16f, 2f + j * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[1 + variation[j] * 8].Draw(Pos = Position + new Vector2(9f + j * -16f, 3f + j * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            SlopeTextures[0 + variation[j] * 8].Draw(Pos = Position + new Vector2(8f + j * -16f, 3f + j * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (j + 1 != SlopeHeight || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[7 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(7f + j * -16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(6f + j * -16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(5f + j * -16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(4f + j * -16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(3f + j * -16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(2f + j * -16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(1f + j * -16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[0 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(j * -16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(7f + j * -16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(6f + j * -16f, 4f + j * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(5f + j * -16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(4f + j * -16f, 5f + j * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(3f + j * -16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(2f + j * -16f, 6f + j * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(1f + j * -16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[j + 1] * 8].Draw(Pos = Position + new Vector2(j * -16f, 7f + j * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k <= SlopeHeight - 1; k++)
                        {
                            if (k > 1)
                            {
                                if (TilesTop != "Small Edge" || k > 2)
                                {
                                    BaseTextures[5, 0 + variationInner[k]].Draw(Pos = Position + new Vector2(32f + k * -8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[k + 1]].Draw(Pos = Position + new Vector2(24f + k * -8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            BaseTextures[5, 0 + variationInner[k + 2]].Draw(Pos = Position + new Vector2(16f + k * -8f, k * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        for (int l = 0; l <= SlopeHeight - 1; l++)
                        {
                            if (l != SlopeHeight - 1 || TilesBottom == "Horizontal" || RenderMethod == "Type A")
                            {
                                SlopeTextures[7 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * -8f, l * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[6 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * -8f, 1f + l * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[5 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * -8f, 2f + l * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[4 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * -8f, 3f + l * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[3 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * -8f, 4f + l * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[2 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * -8f, 5f + l * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[1 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * -8f, 6f + l * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[0 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * -8f, 7f + l * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                SlopeTextures[64 + variation[l] * 8].Draw(Pos = Position + new Vector2(15f + l * -8f, l * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[65 + variation[l] * 8].Draw(Pos = Position + new Vector2(14f + l * -8f, 1f + l * -8f + 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[66 + variation[l] * 8].Draw(Pos = Position + new Vector2(13f + l * -8f, 2f + l * -8f + 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[67 + variation[l] * 8].Draw(Pos = Position + new Vector2(12f + l * -8f, 3f + l * -8f + 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[68 + variation[l] * 8].Draw(Pos = Position + new Vector2(11f + l * -8f, 4f + l * -8f + 0), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[69 + variation[l] * 8].Draw(Pos = Position + new Vector2(10f + l * -8f, 5f + l * -8f - 2), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[70 + variation[l] * 8].Draw(Pos = Position + new Vector2(9f + l * -8f, 6f + l * -8f - 4), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                SlopeTextures[71 + variation[l] * 8].Draw(Pos = Position + new Vector2(8f + l * -8f, 7f + l * -8f - 6), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (!NoRender)
            {
                Vector2 Pos = Vector2.Zero;

                if (!UpsideDown)
                {
                    if (Side == "Left")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            BaseTextures[0 + variation[0], 0].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[1], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[0]].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[1]].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[1], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            BaseTextures[4, 1].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[2], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[2]].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[3]].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[2], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            BaseTextures[4, 1].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[4]].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            BaseTextures[0 + variation[3], 11].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[4], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[5], 2].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[5]].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            BaseTextures[0 + variation[6], 11].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[7], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 3].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[6]].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            BaseTextures[0 + variation[8], 11].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[9], 2].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            BaseTextures[0 + variation[10], 11].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 3].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            BaseTextures[5, 0 + variationInner[7]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[8]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[9]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[10]].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[11]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[12]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[13]].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[14]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[15]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[11], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[16]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[17]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[12], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[18]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[13], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[14], 1].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[15], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[19]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[20]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[16], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[17], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[18], 1].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[19], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[21]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[22]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[20], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[23]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[24]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[21], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[25]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[26]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[22], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[27]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[23], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[24], 1].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[25], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[28]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[26], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[27], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[28], 1].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[29], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[29]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[30], 14].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(Pos);
                        }

                    }
                    else if (Side == "Right")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            BaseTextures[0 + variation[0], 0].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[1], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[0]].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[1]].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[1], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            BaseTextures[4, 3].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[2], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[2]].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[3]].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[2], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            BaseTextures[4, 3].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[4]].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            BaseTextures[0 + variation[3], 12].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[4], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[5], 3].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[5]].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            BaseTextures[0 + variation[6], 12].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[7], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 1].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[6]].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            BaseTextures[0 + variation[8], 12].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[9], 3].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            BaseTextures[0 + variation[10], 12].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            BaseTextures[5, 0 + variationInner[7]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[8]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[9]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[10]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);

                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[11]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[12]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[13]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[14]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[15]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[11], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[16]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[17]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[12], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[18]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[13], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[14], 1].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[15], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[19]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[20]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[16], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[17], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[18], 1].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[19], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[21]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[22]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[20], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[23]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[24]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[21], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[25]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[26]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[22], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[27]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[23], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[24], 1].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[25], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[28]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[26], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[27], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[28], 1].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[29], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[29]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[30], 13].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(Pos);
                        }

                    }
                }
                else
                {
                    if (Side == "Left")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            BaseTextures[0 + variation[0], 1].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[1], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[0]].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[1]].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[1], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            BaseTextures[4, 0].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[2], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[2]].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[3]].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[2], 0].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            BaseTextures[4, 0].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[4]].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            BaseTextures[0 + variation[3], 13].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[4], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[5], 2].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[5]].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            BaseTextures[0 + variation[6], 13].Draw(Pos = Position + new Vector2(-8f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[7], 1].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 2].Draw(Pos = Position + new Vector2(-8f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[6]].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            BaseTextures[0 + variation[8], 13].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[9], 2].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            BaseTextures[0 + variation[10], 13].Draw(Pos = Position + new Vector2(0f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 2].Draw(Pos = Position + new Vector2(0f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            BaseTextures[5, 0 + variationInner[7]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[8]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[9]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[10]].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[11]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[12]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[13]].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[14]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[15]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[11], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[16]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[17]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[12], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[18]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[13], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[14], 0].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[15], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[19]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[20]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[16], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[17], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[18], 0].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[19], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[21]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[22]].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 16 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[20], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 16, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[23]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[24]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[21], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[25]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[26]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[22], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[27]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 0].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[23], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[24], 0].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[25], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[28]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[26], 3].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[27], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[28], 0].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[29], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[29]].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[4, 1].Draw(Pos = Position + new Vector2(SlopeHeight * 8 - 8f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[30], 12].Draw(Pos = Position + new Vector2(SlopeHeight * 8, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(Pos);
                        }

                    }
                    else if (Side == "Right")
                    {
                        // Top tiles

                        if (TilesTop == "Horizontal")
                        {
                            BaseTextures[0 + variation[0], 1].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[1], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[0]].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[1]].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            /*if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[1], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }*/
                        }
                        else if (TilesTop == "Horizontal Corner")
                        {
                            BaseTextures[4, 2].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[2], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            if (SlopeHeight != 1 || TilesBottom != "Small Edge")
                            {
                                BaseTextures[5, 0 + variationInner[2]].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[3]].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            if (SlopeHeight == 1 && TilesBottom == "Small Edge" && Gentle)
                            {
                                BaseTextures[0 + variation[2], 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else if (TilesTop == "Vertical")
                        {
                            BaseTextures[4, 2].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[4]].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge")
                        {
                            BaseTextures[0 + variation[3], 14].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[4], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[5], 3].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[5]].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Edge Corner")
                        {
                            BaseTextures[0 + variation[6], 14].Draw(Pos = Position + new Vector2(24f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[7], 1].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 0].Draw(Pos = Position + new Vector2(24f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[5, 0 + variationInner[6]].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge")
                        {
                            BaseTextures[0 + variation[8], 14].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[0 + variation[9], 3].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }
                        else if (TilesTop == "Small Edge Corner")
                        {
                            BaseTextures[0 + variation[10], 14].Draw(Pos = Position + new Vector2(16f, 8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            BaseTextures[4, 0].Draw(Pos = Position + new Vector2(16f, 0f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        // Special case for bottom tiles

                        if (Gentle && TilesBottom != "Small Edge" && TilesBottom != "Small Edge Corner")
                        {
                            BaseTextures[5, 0 + variationInner[7]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                        }

                        if (TilesBottom == "Horizontal")
                        {
                            if (Gentle)
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[8]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[9]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[10]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);

                            }
                            else
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[11]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[12]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[13]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type A")
                        {
                            DrawSlopesTiles(Pos);
                        }

                        // Bottom Tiles

                        if (Gentle)
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[14]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[15]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[11], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[16]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[17]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[12], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[18]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[13], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[14], 0].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[15], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[19]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[20]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[16], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[17], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[18], 0].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[19], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[21]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 40f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[5, 0 + variationInner[22]].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[20], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -16 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }
                        else
                        {
                            if (TilesBottom == "Vertical")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[23]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[24]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[21], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Vertical Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[25]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                    BaseTextures[5, 0 + variationInner[26]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[22], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[5, 0 + variationInner[27]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge")
                            {
                                BaseTextures[0 + variation[23], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[24], 0].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[25], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[28]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[0 + variation[26], 2].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[27], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge")
                            {
                                BaseTextures[0 + variation[28], 0].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[29], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                            else if (TilesBottom == "Small Edge Corner")
                            {
                                if (SlopeHeight > 1)
                                {
                                    BaseTextures[5, 0 + variationInner[29]].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 32f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                }
                                BaseTextures[4, 3].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 24f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                                BaseTextures[0 + variation[30], 11].Draw(Pos = Position + new Vector2(SlopeHeight * -8 + 16f, SlopeHeight * -8f + 8), Vector2.Zero, Rainbow ? GetHue(Pos) : Color.White);
                            }
                        }

                        if (RenderMethod == "Type B")
                        {
                            DrawSlopesTiles(Pos);
                        }

                    }
                }
            }
        }
    }
}