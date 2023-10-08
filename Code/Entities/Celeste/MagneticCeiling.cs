using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/MagneticCeiling")]
    class MagneticCeiling : Entity
    {
        private Sprite spriteA;

        private Sprite spriteB;

        private bool PlayerCollideSolid;

        private bool Flashing;

        public bool CanJump;

        private string Directory;

        private float AnimationSpeed;

        private bool playSfx;

        private PlayerCollider pc;

        private EntityID ID;

        private StaticMover staticMover;

        private Vector2 imageOffset;

        private bool NoStaminaDrain;

        public bool JumpGracePeriod;

        public bool playerWasAttached;
        public string PlayerPose = "";

        public static Player player;

        private Coroutine JumpGraceTimerRoutine = new();

        public CustomMoveBlock attachedMoveBlock;

        public Color EnabledColor = Color.White;

        public Color DisabledColor = Color.White;

        public bool VisibleWhenDisabled;

        public MagneticCeiling(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            ID = eid;
            Collider = new Hitbox(data.Width, 4f);
            Directory = data.Attr("directory");
            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/MagneticCeiling";
            }
            AnimationSpeed = data.Float("animationSpeed");
            CanJump = data.Bool("canJump");
            NoStaminaDrain = data.Bool("noStaminaDrain");
            Add(spriteA = new Sprite(GFX.Game, Directory + "/"));
            spriteA.Position = Vector2.Zero;
            spriteA.AddLoop("idle", "idle_a", AnimationSpeed);
            spriteA.Play("idle");
            Add(spriteB = new Sprite(GFX.Game, Directory + "/"));
            spriteB.Position = new Vector2(8f, 0f);
            spriteB.AddLoop("idle", "idle_b", AnimationSpeed);
            spriteB.Play("idle");
            Add(pc = new PlayerCollider(OnCollide, new Hitbox(data.Width, 5f)));
            Add(staticMover = new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
                OnEnable = OnEnable,
                OnDisable = OnDisable,
                OnMove = OnMove,
            });
            Depth = -9999;
            playSfx = true;
        }

        public static void Load()
        {
            Everest.Events.Player.OnSpawn += onPlayerSpawn;
            On.Celeste.Player.IsRiding_Solid += OnPlayerIsRiding;
            On.Celeste.Solid.GetPlayerOnTop += OnSolidGetPlayerOnTop;

            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
        }

        public static void Unload()
        {
            Everest.Events.Player.OnSpawn -= onPlayerSpawn;
            On.Celeste.Player.IsRiding_Solid -= OnPlayerIsRiding;
            On.Celeste.Solid.GetPlayerOnTop -= OnSolidGetPlayerOnTop;

            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
        }
        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false) {

            if (self.Entity is Player player && player != null && player.StateMachine.State != Player.StDash) {
                foreach (MagneticCeiling ceiling in self.SceneAs<Level>().Tracker.GetEntities<MagneticCeiling>()) {
                    if (ceiling.PlayerPose != "" && ceiling.Active) {
                        id = ceiling.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
        }

        private static void onPlayerSpawn(Player player)
        {
            if (player.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
            {
                player.SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Ceiling", false);
            }
        }

        private static bool OnPlayerIsRiding(On.Celeste.Player.orig_IsRiding_Solid orig, Player self, Solid solid)
        {
            if (self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
            {
                if (solid is CustomMoveBlock)
                {
                    CustomMoveBlock block = solid as CustomMoveBlock;
                    return Collide.Check(self, solid, self.Position - new Vector2(0, 6 + block.magneticCeilingOffset));
                }
                return Collide.Check(self, solid, self.Position - new Vector2(0, 6));
            }
            return orig(self, solid);
        }

        private static Player OnSolidGetPlayerOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self)
        {
            if (self.SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") && player != null)
            {
                return (Collide.Check(self, player, self.Position + new Vector2(0, self.Height) + Vector2.UnitY)) ? player : null;
            }
            return orig(self);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (CustomMoveBlock block in SceneAs<Level>().Tracker.GetEntities<CustomMoveBlock>())
            {
                if (block.Left <= Left && block.Right >= Right && block.Bottom == Top)
                {
                    attachedMoveBlock = block;
                    break;
                }
            }
        }

        private IEnumerator JumpGraceTimer(Player player)
        {
            float timer = 0.1f;
            JumpGracePeriod = true;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                yield return null;
                if (Input.Jump.Pressed && CanJump)
                {
                    timer = 0;
                }
            }
            if (!NoStaminaDrain && !player.DashAttacking)
            {
                player.Stamina -= 27.5f;
            }
            JumpGracePeriod = false;
        }

        private void DetachPlayer(Player player)
        {
            SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Ceiling", false);
            SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Ceiling_Can_Jump", false);
            playSfx = true;
            playerWasAttached = false;
        }
        private void OnCollide(Player player)
        {
            PlayerPose = "";
            if (player.Holding == null && !player.DashAttacking)
            {
                if (player.CollideCheck(this, player.Position - Vector2.UnitY) && (SpiderMagnet.Active(SceneAs<Level>()) || !XaphanModule.useUpgrades) && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    if (Input.GrabCheck && (!CanJump || player.Speed.Y >= 0) && player.Left > Left - 6 && player.Right < Right + 6 && player.Top > Top && player.Stamina > 0 && !player.Dead && !player.CollideCheck<Spikes>())
                    {
                        playerWasAttached = true;
                        if (playSfx)
                        {
                            Audio.Play("event:/char/madeline/climb_ledge");
                            playSfx = false;
                        }
                        SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Ceiling", true);
                        if (CanJump)
                        {
                            SceneAs<Level>().Session.SetFlag("Xaphan_Helper_Ceiling_Can_Jump", true);
                        }
                        player.Speed.Y = 0;
                        player.MoveToY(Bottom + 11);
                        staticMover.TriggerPlatform();
                        if (!NoStaminaDrain)
                        {
                            if ((Input.Aim.Value.SafeNormalize() == -Vector2.UnitX || Input.Aim.Value.SafeNormalize() == Vector2.UnitX) && !player.CollideCheck<Solid>(player.Position + Vector2.UnitX) && !player.CollideCheck<Solid>(player.Position - Vector2.UnitX))
                            {
                                player.Stamina -= 27.5f * Engine.DeltaTime;
                            }
                            else
                            {
                                player.Stamina -= 10f * Engine.DeltaTime;
                            }
                        }
                        if (Input.Aim.Value.SafeNormalize().X != 0 && !PlayerCollideSolid) {
                            PlayerPose = "XaphanHelper_ceilingMove";
                        } else {
                            PlayerPose = "XaphanHelper_ceilingIdle";
                        }
                        player.Sprite.Play(PlayerPose);
                    }
                    else if (player.CollideCheck<Spikes>())
                    {
                        DetachPlayer(player);
                        player.Die(new Vector2(0f, 1f));
                    }
                    else if (playerWasAttached)
                    {
                        if (CanJump && !JumpGraceTimerRoutine.Active)
                        {
                            Add(JumpGraceTimerRoutine = new Coroutine(JumpGraceTimer(player)));
                            playerWasAttached = false;
                        }
                        DetachPlayer(player);
                    }
                }
                else if (playerWasAttached)
                {
                    DetachPlayer(player);
                }
            }
        }

        public override void Render()
        {
            float num = Width / 8f;
            int i = 0;
            while (i < num)
            {
                spriteA.RenderPosition = Position + imageOffset + new Vector2(i * 8, 0f);
                spriteA.Render();
                i = i + 2;
            }
            spriteA.Visible = false;
            i = 0;
            while (i < num - 1)
            {
                spriteB.RenderPosition = Position + imageOffset + new Vector2(8f, 0f) + new Vector2(i * 8, 0f);
                spriteB.Render();
                i = i + 2;
            }
            spriteB.Visible = false;
        }

        private void OnShake(Vector2 amount)
        {
            imageOffset += amount;
        }

        private bool IsRiding(Solid solid)
        {
            if (attachedMoveBlock != null)
            {
                return CollideCheckOutside(solid, Position - Vector2.UnitY * (1 + attachedMoveBlock.magneticCeilingOffset));
            }
            return CollideCheckOutside(solid, Position - Vector2.UnitY);
        }

        private bool IsRiding(JumpThru jumpThru)
        {
            return CollideCheck(jumpThru, Position - Vector2.UnitY);
        }

        public void SetCeilingColor(Color color)
        {
            foreach (Component component in Components)
            {
                if (component is Sprite sprite)
                {
                    sprite.Color = color;
                }
            }
        }

        private void OnEnable()
        {
            Visible = (Collidable = true);
            SetCeilingColor(EnabledColor);
        }

        private void OnDisable()
        {
            Collidable = false;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling"))
            {
                DetachPlayer(player);
            }
            JumpGracePeriod = false;
            if (VisibleWhenDisabled)
            {
                SetCeilingColor(DisabledColor);
                return;
            }
            Visible = false;
        }

        private void OnMove(Vector2 amount)
        {
            Position += amount;
        }
    }
}
