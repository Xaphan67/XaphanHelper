using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/GlobalFlagTrigger")]
    class GlobalFlagTrigger : Trigger
    {
        public string flag;

        public bool state;

        public bool switchFlag;

        public string levelSet;

        public GlobalFlagTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            state = data.Bool("state", true);
            switchFlag = data.Bool("switchFlag");
            levelSet = data.Attr("levelSet");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (string.IsNullOrEmpty(levelSet))
            {
                levelSet = Prefix;
            }
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex == -1 ? 0 : SceneAs<Level>().Session.Area.ChapterIndex;
            if ((!switchFlag && state) || (switchFlag && !XaphanModule.ModSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""))))
            {
                if (!XaphanModule.ModSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.GlobalFlags.Add(levelSet + "_Ch" + chapterIndex + "_" + flag);
                    if (levelSet == Prefix)
                    {
                        SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag, true);
                    }
                }
                if (XaphanModule.PlayerHasGolden)
                {
                    if (!XaphanModule.ModSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                    {
                        XaphanModule.ModSaveData.GlobalFlags.Add(levelSet + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                        if (levelSet == Prefix)
                        {
                            SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag, true);
                        }
                    }
                }
            }
            else
            {
                if (XaphanModule.ModSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.GlobalFlags.Remove(levelSet + "_Ch" + chapterIndex + "_" + flag);
                    if (levelSet == Prefix)
                    {
                        SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag, false);
                    }
                }
                if (XaphanModule.PlayerHasGolden)
                {
                    if (XaphanModule.ModSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                    {
                        XaphanModule.ModSaveData.GlobalFlags.Remove(levelSet + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                        if (levelSet == Prefix)
                        {
                            SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag, false);
                        }
                    }
                }
            }
        }
    }
}