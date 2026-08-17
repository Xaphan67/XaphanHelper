using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using static Celeste.Mod.XaphanHelper.XaphanModuleSession;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/LightModeTrigger")]
    class LightModeTrigger : Trigger
    {
        private readonly LightModes mode;

        public LightModeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            mode = data.Enum("mode", LightModes.None);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Scene as Level;
            foreach (LightManager manager in level.Tracker.GetEntities<LightManager>())
            {
                manager.MainMode = manager.RespawnMode = mode;
            }
            XaphanModule.ModSession.LightMode = mode;
        }
    }
}
