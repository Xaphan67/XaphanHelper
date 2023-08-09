using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class FlagDecalVisibilityToggle : Component
    {
        public string[] Flags;

        public bool Inverted;

        public FlagDecalVisibilityToggle(string[] flags, bool inverted) : base(true, false)
        {
            Flags = flags;
            Inverted = inverted;
        }

        public override void Update()
        {
            base.Update();
            foreach (string flag in Flags)
            {
                Entity.Visible = Inverted ? Entity.SceneAs<Level>().Session.GetFlag(flag) : !Entity.SceneAs<Level>().Session.GetFlag(flag);
                if (Entity.Visible)
                {
                    break;
                }
            }
        }
    }
}
