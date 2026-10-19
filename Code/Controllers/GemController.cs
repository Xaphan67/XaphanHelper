using System.Collections;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/GemController")]
    class GemController : Entity
    {
        public Sprite PlayerSprite;

        public Sprite PlayerHairSprite;

        public bool AllGemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected") &&
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected") &&
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected") &&
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected") &&
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected") &&
                XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Gem_Collected");
        }

        public GemController(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Add(PlayerSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerSprite.CenterOrigin();
            PlayerSprite.Position += new Vector2(0f, 62f);
            PlayerSprite.Visible = false;
            Add(PlayerHairSprite = GFX.SpriteBank.Create("XaphanHelper_player_turnAround"));
            PlayerHairSprite.CenterOrigin();
            PlayerHairSprite.Position += new Vector2(0f, 62f);
            PlayerHairSprite.Visible = false;
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
            foreach (GemController controller in self.SceneAs<Level>().Tracker.GetEntities<GemController>())
            {
                if (controller.PlayerSprite.Visible)
                {
                    controller.PlayerSprite.Visible = false;
                    controller.PlayerHairSprite.Visible = false;
                    self.Sprite.Visible = true;
                }
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public IEnumerator ActivateGems()
        {
            foreach (GemSlot gem in Scene.Entities.FindAll<GemSlot>())
            {
                if (!gem.Activated && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + gem.Chapter + "_Gem" + ((gem.Index != 1 ? gem.Index : "")) + "_Collected"))
                {
                    yield return 0.5f;
                    gem.Activated = true;
                    yield return gem.Activate();
                }
            }
        }

        public IEnumerator OpenEndArea()
        {
            SceneAs<Level>().Session.SetFlag("Open_End_Area", true);
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_End_Area_Open");
            for (int i = 0; i <= 2; i ++)
            {
                yield return 0.1f;
                foreach (EndBlock block in SceneAs<Level>().Tracker.GetEntities<EndBlock>())
                {
                    if (block.index == i)
                    {
                        block.Break();
                    }
                }
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
