namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class MergeChaptersBCSideHeartCompleteArea
    {
        public static void Load()
        {
            On.Celeste.HeartGem.IsCompleteArea += onHeartGemIsCompleteArea;
        }

        public static void Unload()
        {
            On.Celeste.HeartGem.IsCompleteArea -= onHeartGemIsCompleteArea;
        }

        private static bool onHeartGemIsCompleteArea(On.Celeste.HeartGem.orig_IsCompleteArea orig, HeartGem self, bool value)
        {
            if (XaphanModule.useMergeChaptersController && self.SceneAs<Level>().Session.Area.Mode != AreaMode.Normal)
            {
                return false;
            }
            else
            {
                return orig(self, value);
            }
        }
    }
}
