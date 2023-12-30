using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [CustomEntity("XaphanHelper/InGameMapController")]
    class InGameMapController : Entity
    {
        public bool RequireMapUpgradeToOpen;

        public InGameMapController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            RequireMapUpgradeToOpen = data.Bool("requireMapUpgradeToOpen");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!RequireMapUpgradeToOpen)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(SceneAs<Level>().Session.Area.LevelSet + "_Can_Open_Map"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(SceneAs<Level>().Session.Area.LevelSet + "_Can_Open_Map");
                }
                if (XaphanModule.PlayerHasGolden)
                {
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(SceneAs<Level>().Session.Area.LevelSet + "_Can_Open_Map_GoldenStrawberry"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(SceneAs<Level>().Session.Area.LevelSet + "_Can_Open_Map_GoldenStrawberry");
                    }
                }
            }
        }
    }
}
