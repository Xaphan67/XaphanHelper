using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/UpgradeTrigger")]
    class UpgradeTrigger : Trigger
    {
        private bool Disable;

        private string Upgrade;

        public UpgradeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Disable = data.Bool("disable", false);
            Upgrade = data.Attr("upgrade", "BlazingShard");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            string prefix = SceneAs<Level>().Session.Area.LevelSet;
            AreaKey area = SceneAs<Level>().Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            bool temporary = false;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/UpgradeController")
                    {
                        temporary = entity.Bool("upgradesAreTemporary", false);
                        break;
                    }
                }
            }
            if (!Disable)
            {
                foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                {
                    if (Upgrade == upgrade.ToString())
                    {
                        if (Upgrade == "SpaceJump")
                        {
                            XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(2);
                        }
                        else
                        {
                            XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                        }
                        SceneAs<Level>().Session.SetFlag("Upgrade_" + upgrade.ToString(), true);
                        Commands.ReActivateUpgrade(Upgrade);
                        if (!temporary)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade.ToString()))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(prefix + "_Upgrade_" + upgrade.ToString());
                            }
                            if (XaphanModule.PlayerHasGolden)
                            {
                                if (!XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Add(prefix + "_Upgrade_" + upgrade + "_GoldenStrawberry");
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                {
                    if (Upgrade == upgrade.ToString())
                    {
                        if (Upgrade == "SpaceJump")
                        {
                            XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(1);
                        }
                        else
                        {
                            XaphanModule.Instance.UpgradeHandlers[upgrade].SetValue(0);
                        }
                        SceneAs<Level>().Session.SetFlag("Upgrade_" + upgrade.ToString(), false);
                        if (!temporary)
                        {
                            if (XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade.ToString()))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(prefix + "_Upgrade_" + upgrade.ToString());
                            }
                            if (XaphanModule.PlayerHasGolden)
                            {
                                if (!XaphanModule.ModSaveData.SavedFlags.Contains(prefix + "_Upgrade_" + upgrade.ToString() + "_GoldenStrawberry"))
                                {
                                    XaphanModule.ModSaveData.SavedFlags.Remove(prefix + "_Upgrade_" + upgrade.ToString() + "_GoldenStrawberry");
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}
