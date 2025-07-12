using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/UpgradeTrigger")]
    class UpgradeTrigger : Trigger
    {
        private bool Disable;

        private string Upgrade;

        private bool OnlyOnce;

        public UpgradeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Disable = data.Bool("disable", false);
            Upgrade = data.Attr("upgrade", "All");
            OnlyOnce = data.Bool("onlyOnce", false);
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
                if (Upgrade == "All")
                {
                    foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                    {
                        if (upgrade.ToString() == "SpaceJump")
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
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                if (Upgrade == "All")
                {
                    foreach (XaphanModule.Upgrades upgrade in XaphanModule.Instance.UpgradeHandlers.Keys)
                    {
                        if (upgrade.ToString() == "SpaceJump")
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
                            }
                            break;
                        }
                    }
                }
            }
            if (OnlyOnce)
            {
                RemoveSelf();
            }
        }
    }
}
