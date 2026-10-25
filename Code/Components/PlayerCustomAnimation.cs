using System.Linq;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class PlayerCustomAnimation : Component
    {
        public Sprite PlayerSprite;

        public Sprite PlayerHairSprite;

        private Player player;

        public PlayerCustomAnimation(Player player) : base(active: true, visible: true)
        {
            this.player = player;
        }

        public static void Load()
        {
            On.Celeste.Player.ctor += OnPlayerCtor;
        }

        public static void Unload()
        {
            On.Celeste.Player.ctor -= OnPlayerCtor;
        }

        public void SetAnim(string anim)
        {
            PlayerSprite = GFX.SpriteBank.Create(anim);
            PlayerSprite.Origin = new Vector2(PlayerSprite.Width / 2f, PlayerSprite.Height);
            PlayerSprite.Visible = false;
            PlayerHairSprite = GFX.SpriteBank.Create(anim);
            PlayerHairSprite.Origin = new Vector2(PlayerHairSprite.Width / 2f, PlayerHairSprite.Height);
            PlayerHairSprite.Visible = false;
        }

        public void PlaySprite(string spriteId, string hairId, bool reverse, bool resetStateToNormal = false)
        {
            if (!reverse)
            {
                PlayerSprite.OnLastFrame = null;
                PlayerSprite.FlipX = player.Facing == Facings.Left;
                PlayerHairSprite.FlipX = player.Facing == Facings.Left;
                player.Sprite.Visible = false;
                player.Hair.Visible = false;
                PlayerSprite.Visible = true;
                PlayerHairSprite.Visible = true;
                PlayerSprite.Play(spriteId);
                PlayerHairSprite.Play(hairId);
            }
            else
            {
                PlayerSprite.Play(spriteId + "_reverse");
                PlayerHairSprite.Play(hairId + "_reverse");
                PlayerSprite.OnLastFrame = delegate
                {
                    PlayerSprite.Visible = false;
                    PlayerHairSprite.Visible = false;
                    player.Sprite.Visible = true;
                    player.Hair.Visible = true;
                    if (resetStateToNormal)
                    {
                        player.StateMachine.State = 0;
                    }
                };
            }
        }

        private static void OnPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            self.Add(new PlayerCustomAnimation(self));
        }

        public override void Update()
        {
            base.Update();
            if (PlayerSprite != null && PlayerHairSprite != null)
            {
                PlayerSprite.Update();
                PlayerHairSprite.Update();

                PlayerSprite.Position = player.Position;
                PlayerHairSprite.Position = player.Position;
                PlayerHairSprite.Color = player.Hair.Color;

                if (PlayerSprite.Visible)
                {
                    player.Sprite.Visible = false;
                    player.Hair.Visible = false;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (PlayerSprite == null || PlayerHairSprite == null)
            {
                return;
            }
            Level level = SceneAs<Level>();
            bool useGrading = !XaphanModule.useMetroidGameplay && XaphanModule.useUpgrades && (VariaJacket.Active(level) || GravityJacket.Active(level));
            if (useGrading && PlayerSprite.Visible)
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
                PlayerSprite.Render();
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.GameplayRenderer.Camera.Matrix);
            }
            else if (PlayerSprite.Visible)
            {
                PlayerSprite.Render();
            }
            if (PlayerHairSprite.Visible)
            {
                PlayerHairSprite.Render();
            }
        }
    }
}
