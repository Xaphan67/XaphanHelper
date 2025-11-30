using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    public abstract class Upgrade
    {
        public abstract int GetDefaultValue();

        public abstract void Load();

        public abstract void Unload();

        public abstract int GetValue();

        public abstract void SetValue(int value);

        public static void LoadComponent()
        {
            On.Celeste.Player.ctor += onPlayerCtor;
        }

        public static void UnloadComponent()
        {
            On.Celeste.Player.ctor -= onPlayerCtor;
        }

        private static void onPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            if (XaphanModule.useUpgrades && self.Get<UpgradesComponent>() == null)
            {
                self.Add(new UpgradesComponent());
            }
        }

        public static BagDisplay GetDisplay(Level level, string type)
        {
            List<Entity> displays = level.Tracker.GetEntities<BagDisplay>();
            foreach (Entity entity in displays)
            {
                BagDisplay display = entity as BagDisplay;
                if (display.type == type)
                {
                    return display;
                }
            }
            return null;
        }
    }
}