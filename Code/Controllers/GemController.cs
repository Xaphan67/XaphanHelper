using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/GemController")]
    class GemController : Entity
    {
        public string PlayerPose = "";

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
                foreach (GemController controller in level.Tracker.GetEntities<GemController>())
                {
                    if (!string.IsNullOrEmpty(controller.PlayerPose))
                    {
                        id = controller.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
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
    }
}
