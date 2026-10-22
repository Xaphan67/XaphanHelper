using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/CustomLightFadeTrigger", "XaphanHelper/MultiLightFadeTrigger")]
    public class CustomLightFadeTrigger : Trigger
    {
        public float LightAddFrom;

        public float LightAddTo;

        public float FlagLightAddFrom;

        public float FlagLightAddTo;

        public PositionModes PositionMode;

        public string flag;

        public CustomLightFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tag = Tags.TransitionUpdate;
            LightAddFrom = data.Float("lightAddFrom");
            LightAddTo = data.Float("lightAddTo");
            FlagLightAddFrom = data.Float("flagLightAddFrom");
            FlagLightAddTo = data.Float("flagLightAddTo");
            PositionMode = data.Enum("positionMode", PositionModes.NoEffect);
            flag = data.Attr("flag");
        }

        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                float light = 0f;
                if (!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag))
                {
                    light = level.Session.LightingAlphaAdd = FlagLightAddFrom + (FlagLightAddTo - FlagLightAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f);
                }
                else
                {
                    light = (level.Session.LightingAlphaAdd = LightAddFrom + (LightAddTo - LightAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f));
                }
                level.Lighting.Alpha = level.BaseLightingAlpha + light;
            }
        }
    }
}
