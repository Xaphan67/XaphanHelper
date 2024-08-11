using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class FlagDecalLightToogle : Component
    {
        public string Flag;

        public bool Inverted;

        public Vector2 Position;

        public Color Color;

        public float Alpha;

        public int StartFade;

        public int EndFade;

        VertexLight Light;

        private bool AddedLight;

        public FlagDecalLightToogle(string flag, bool inverted, Vector2 psition, Color color, float alpha, int startFade, int endFade) : base(true, false)
        {
            Flag = flag;
            Inverted = inverted;
            Position = psition;
            Color = color;
            Alpha = alpha;
            StartFade = startFade;
            EndFade = endFade;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Tag = Tags.TransitionUpdate;
            Light = new VertexLight(Position, Color, Alpha, StartFade, EndFade);
        }

        public override void Update()
        {
            base.Update();
            if ((Inverted ? !Entity.SceneAs<Level>().Session.GetFlag(Flag) : Entity.SceneAs<Level>().Session.GetFlag(Flag)) && !AddedLight)
            {
                AddedLight = true;
                if (!Entity.Components.Contains(Light))
                {
                    Entity.Add(Light);
                }
                else
                {
                    foreach (Component component in Entity.Components)
                    {
                        if (component.GetType() == typeof(VertexLight))
                        {
                            component.Visible = true;
                            break;
                        }
                    }
                }
            }
            else if ((Inverted ? Entity.SceneAs<Level>().Session.GetFlag(Flag) : !Entity.SceneAs<Level>().Session.GetFlag(Flag)) && AddedLight)
            {
                foreach (Component component in Entity.Components)
                {
                    if (component.GetType() == typeof(VertexLight)) {
                        component.Visible = false;
                        break;
                    }
                }
                AddedLight = false;
            }
        }
    }
}
