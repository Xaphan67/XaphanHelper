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

        public bool entered;

        public LightModeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tag = Tags.TransitionUpdate;
            mode = data.Enum("mode", LightModes.None);
        }

        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && !entered)
            {
                entered = true;
                foreach (LightManager manager in level.Tracker.GetEntities<LightManager>())
                {
                    manager.MainMode = manager.RespawnMode = mode;
                }
                XaphanModule.ModSession.LightMode = mode;
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            entered = false;
        }
    }
}
