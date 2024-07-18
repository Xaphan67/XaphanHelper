using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/LightModeTrigger")]
    class LightModeTrigger : Trigger
    {
        private enum Modes
        {
            Light,
            Dark,
            None
        }

        private readonly Modes mode;

        public LightModeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            mode = data.Enum("mode", Modes.None);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Scene as Level;
            foreach (LightManager manager in level.Tracker.GetEntities<LightManager>())
            {
                manager.MainMode = manager.RespawnMode = (XaphanModuleSession.LightModes)mode;
            }
            XaphanModuleSession.LightModes lightMode = XaphanModuleSession.LightModes.None;
            lightMode = (XaphanModuleSession.LightModes)mode;
        }
    }
}
