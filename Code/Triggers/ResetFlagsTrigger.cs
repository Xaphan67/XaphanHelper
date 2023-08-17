using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/ResetFlagsTrigger")]
    class ResetFlagsTrigger : Trigger
    {
        public string setTrueFlags;

        public string setFalseFlags;

        public bool transitionUpdate;

        public bool removeWhenOutside;

        public bool registerInSaveData;

        public bool entered;

        private string conditionFlags;

        private bool inverted;

        public ResetFlagsTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            setTrueFlags = data.Attr("setTrueFlags");
            setFalseFlags = data.Attr("setFalseFlags");
            transitionUpdate = data.Bool("transitionUpdate");
            removeWhenOutside = data.Bool("removeWhenOutside");
            registerInSaveData = data.Bool("registerInSaveData");
            conditionFlags = data.Attr("conditionFlags");
            inverted = data.Bool("inverted");
            if (transitionUpdate)
            {
                Tag = Tags.TransitionUpdate;
            }
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            bool allConditionFlagsSet = true;
            if (!string.IsNullOrEmpty(conditionFlags))
            {
                string[] flags = conditionFlags.Split(',');

                foreach (string flag in flags)
                {
                    if (!inverted)
                    {
                        if (!SceneAs<Level>().Session.GetFlag(flag))
                        {
                            allConditionFlagsSet = false;
                            break;
                        }
                    }
                    else
                    {
                        if (SceneAs<Level>().Session.GetFlag(flag))
                        {
                            allConditionFlagsSet = false;
                            break;
                        }
                    }
                }
            }
            if (player != null && CollideCheck(player) && !entered && allConditionFlagsSet)
            {
                entered = true;
                string[] trueFlags = setTrueFlags.Split(',');
                string[] falseFlags = setFalseFlags.Split(',');
                foreach (string flag in trueFlags)
                {
                    SceneAs<Level>().Session.SetFlag(flag, true);
                    if (registerInSaveData)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                            }
                        }
                    }
                    foreach (FlagDashSwitch dashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                    {
                        if (dashSwitch.flag == flag && !dashSwitch.registerInSaveData)
                        {
                            dashSwitch.flagState = true;
                            if (dashSwitch.mode == "SetFalse" || dashSwitch.mode == "SetInverted")
                            {
                                dashSwitch.ResetSwitch(true);
                            }
                        }
                    }
                }
                foreach (string flag in falseFlags)
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + SceneAs<Level>().Session.Area.ChapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
                    {
                        SceneAs<Level>().Session.SetFlag(flag, false);
                    }
                    if (registerInSaveData)
                    {
                        if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag+ "_GoldenStrawberry");
                            }
                        }
                    }
                    foreach (FlagDashSwitch dashSwitch in SceneAs<Level>().Tracker.GetEntities<FlagDashSwitch>())
                    {
                        if (dashSwitch.flag == flag && !dashSwitch.registerInSaveData)
                        {
                            dashSwitch.flagState = false;
                            if (dashSwitch.mode == "SetTrue" || dashSwitch.mode == "SetInverted")
                            {
                                dashSwitch.ResetSwitch(true);
                            }
                        }
                    }
                }
            }
            else if (player != null && !CollideCheck(player) && removeWhenOutside)
            {
                RemoveSelf();
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            entered = false;
        }
    }
}
