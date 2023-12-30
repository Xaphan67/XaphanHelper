using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/UnlockWarpTrigger")]
    class UnlockWarpTrigger : Trigger
    {
        private int Chapter;

        private string Room;

        private int Index;

        public UnlockWarpTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Chapter = data.Int("chapter");
            Room = data.Attr("room");
            Index = data.Int("index", 0);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            string warpSuffix = Index != 0 ? "_" + Index : "";
            if (Chapter < 0)
            {
                Chapter = 0;
            }
            string warpId = $"{player.SceneAs<Level>().Session.Area.LevelSet}_Ch{Chapter}_{Room}{warpSuffix}";
            WarpManager.ActivateWarp(warpId);
            RemoveSelf();
        }
    }
}
