using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/FlagCameraOffsetTrigger")]
    class FlagCameraOffsetTrigger : Trigger
    {
        private bool onlyOnce;

        private Vector2 offsetFrom;

        private Vector2 offsetTo;

        private PositionModes positionMode;

        private bool xOnly;

        private bool yOnly;

        private string Flag;

        public FlagCameraOffsetTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            offsetFrom = new Vector2(data.Float("offsetXFrom") * 48f, data.Float("offsetYFrom") * 32f);
            offsetTo = new Vector2(data.Float("offsetXTo") * 48f, data.Float("offsetYTo") * 32f);
            positionMode = data.Enum("positionMode", PositionModes.NoEffect);
            onlyOnce = data.Bool("onlyOnce");
            xOnly = data.Bool("xOnly");
            yOnly = data.Bool("yOnly");
            Flag = data.Attr("flag");
            onlyOnce = data.Bool("onlyOnce", false);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag))
            {
                if (!yOnly)
                {
                    SceneAs<Level>().CameraOffset.X = MathHelper.Lerp(offsetFrom.X, offsetTo.X, GetPositionLerp(player, positionMode));
                }
                if (!xOnly)
                {
                    SceneAs<Level>().CameraOffset.Y = MathHelper.Lerp(offsetFrom.Y, offsetTo.Y, GetPositionLerp(player, positionMode));
                }
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag) && onlyOnce)
            {
                RemoveSelf();
            }
        }
    }
}
