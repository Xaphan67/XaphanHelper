using System.Collections;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AuxiliaryGenerator")]
    public class AuxiliaryGenerator : Entity
    {
        Sprite Sprite;

        private Message message;

        private TalkComponent talk;

        private Coroutine ActivationRoutine = new();

        public Sprite PlayerSprite;

        public Sprite PlayerHairSprite;

        public AuxiliaryGenerator(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/AuxiliaryGenerator/"));
            Sprite.Add("off", "off", 0);
            Sprite.Add("turnOn", "turnOn", 0.08f);
            Sprite.AddLoop("on", "on", 0.08f);
            Sprite.Play("off");
            Add(PlayerSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerSprite.CenterOrigin();
            PlayerSprite.Position += new Vector2(12f, 64f);
            PlayerSprite.Visible = false;
            Add(PlayerHairSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerHairSprite.CenterOrigin();
            PlayerHairSprite.Position += new Vector2(12f, 64f);
            PlayerHairSprite.Visible = false;
            Depth = 999;
        }

        public static void Load()
        {
            On.Celeste.Player.Die += OnPlayerDie;
        }

        public static void Unload()
        {
            On.Celeste.Player.Die -= OnPlayerDie;
        }

        private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            foreach (AuxiliaryGenerator generator in self.SceneAs<Level>().Tracker.GetEntities<AuxiliaryGenerator>())
            {
                if (generator.PlayerSprite.Visible)
                {
                    generator.PlayerSprite.Visible = false;
                    generator.PlayerHairSprite.Visible = false;
                    self.Sprite.Visible = true;
                }
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(talk = new TalkComponent(new Rectangle(0, 64, 24, 16), new Vector2(12f, 56f), Interact));
            talk.PlayerMustBeFacing = false;
            talk.Enabled = false;
            if (!SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
            {
                Sprite.Play("on");
            }
        }

        public override void Update()
        {
            base.Update();
            if (talk != null)
            {
                if (SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
                {
                    talk.Enabled = XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator");
                    if (!ActivationRoutine.Active)
                    {
                        Sprite.Play("off");
                    }
                }
                else
                {
                    talk.Enabled = false;
                    Sprite.Play("on");
                }
            }
        }

        private void Interact(Player player)
        {
            Add(ActivationRoutine = new Coroutine(Activate(player)));
        }

        private IEnumerator Activate(Player player)
        {
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X + 12, false, 1f, true);
            PlayerSprite.FlipX = player.Facing == Facings.Left;
            PlayerHairSprite.FlipX = player.Facing == Facings.Left;
            player.Sprite.Visible = false;
            player.Hair.Visible = false;
            PlayerSprite.Visible = true;
            PlayerHairSprite.Visible = true;
            PlayerSprite.Play("turnBackpack");
            PlayerHairSprite.Play("hairBackpack");
            yield return 0.5f;
            Sprite.Play("turnOn");
            yield return 0.3f;
            SceneAs<Level>().Add(message = new Message(Vector2.Zero, "event:/game/xaphan/cell_unlock", "Xaphan_Ch5_Generator"));
            while (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            player.Facing = Facings.Right;
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_5_geothermal_active");
            SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
            Sprite.Play("on");
            message.Close();
            PlayerSprite.Play("turnBackpack_reverse");
            PlayerHairSprite.Play("hairBackpack_reverse");
            PlayerSprite.OnLastFrame = delegate
            {
                PlayerSprite.Visible = false;
                PlayerHairSprite.Visible = false;
                player.Sprite.Visible = true;
                player.Hair.Visible = true;
            };
            if (XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Main_Power_Off"))
            {
                XaphanModule.ModSaveData.GlobalFlags.Remove("Xaphan/0_Ch4_Main_Power_Off");
            }
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Main_Power_Off"))
            {
                XaphanModule.ModSaveData.SavedFlags.Remove("Xaphan/0_Ch4_Main_Power_Off");
            }
            XaphanModule.ModSaveData.GlobalFlags.Add("Xaphan/0_Ch5_Auxiliary_Power");
            if (SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
            {
                SceneAs<Level>().Session.SetFlag("Ch4_Main_Power_Off", false);
            }
            SceneAs<Level>().Session.SetFlag("Ch5_Auxiliary_Power", true);
            if (XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator2") || XaphanModule.PlayerHasGolden)
            {
                yield return 0.2f;
                player.StateMachine.State = 0;
            }
        }

        public override void Render()
        {
            base.Render();
            Level level = SceneAs<Level>();
            if (!XaphanModule.useMetroidGameplay && XaphanModule.useUpgrades && (VariaJacket.Active(level) || GravityJacket.Active(level)))
            {
                string id = "";
                foreach (string name in XaphanModule.JacketPriorityNames)
                {
                    var data = XaphanModule.JacketPriority.FirstOrDefault(n => n.Name == name);
                    if (data.Test != null && data.Test(level))
                    {
                        id = data.Id;
                        break;
                    }
                }
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, level.GameplayRenderer.Camera.Matrix);
                if (PlayerSprite != null && PlayerSprite.Visible)
                {

                    PlayerSprite.Render();
                }
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.GameplayRenderer.Camera.Matrix);
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (PlayerHairSprite != null && PlayerHairSprite.Visible && player != null)
            {
                PlayerHairSprite.Color = player.Hair.Color;
                PlayerHairSprite.Render();
            }
        }
    }
}
