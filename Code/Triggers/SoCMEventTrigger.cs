using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Events;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/SoCMEventTrigger")]
    class SoCMEventTrigger : Trigger
    {
        private bool triggered;

        public string Event;

        public SoCMEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tag = Tags.TransitionUpdate;
            Event = data.Attr("event");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered)
            {
                return;
            }
            triggered = true;
            Level level = Scene as Level;
            switch (Event)
            {
                case "Ch1 - Bombs":
                    Scene.Add(new E01_Bombs(player, level));
                    break;
                case "Ch1 - Boss":
                    Scene.Add(new E01_Boss(player, level));
                    break;
                case "Ch2 - Boss":
                    Scene.Add(new E02_Boss(player, level));
                    break;
                case "Ch4 - Boss":
                    Scene.Add(new E04_Boss(player, level));
                    break;
                case "Ch5 - Boss":
                    Scene.Add(new E05_Boss(player, level));
                    break;
                case "Ch5 - Escape Start":
                    Scene.Add(new E05_EscapeStart(player, level));
                    break;
                case "Ch5 - Escape End":
                    Scene.Add(new E05_EscapeEnd(player, level));
                    break;
            }
        }
    }
}