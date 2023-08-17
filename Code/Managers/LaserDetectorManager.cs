using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Entities;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class LaserDetectorManager : Entity
    {
        private string Prefix;

        private int chapterIndex;

        public List<LaserDetector> activeDetectors = new();

        public List<LaserDetector> inactiveDetectors = new();

        public List<string> activeFlags = new();

        private List<string> inactiveFlags = new();

        public LaserDetectorManager()
        {
            Tag = Tags.TransitionUpdate;
        }

        public static void Load()
        {
            Everest.Events.Level.OnLoadLevel += modOnLevelLoad;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= modOnLevelLoad;
        }

        private static void modOnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new LaserDetectorManager());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
        }

        public void GetDetectorsFlags()
        {
            activeFlags.Clear();
            inactiveFlags.Clear();
            foreach (LaserDetector activeDetector in activeDetectors)
            {
                activeFlags.Add(activeDetector.flag);
            }
            foreach (LaserDetector inactiveDetector in inactiveDetectors)
            {
                inactiveFlags.Add(inactiveDetector.flag);
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (string flag in activeFlags)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
                {
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
            }
            foreach (string flag in inactiveFlags)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")) && !activeFlags.Contains(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
            }
        }
    }
}
