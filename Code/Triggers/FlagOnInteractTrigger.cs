using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/FlagOnInteractTrigger")]
    class FlagOnInteractTrigger : Trigger
    {
        private string reqFlags;

        private string setFlag;

        private bool state;

        private TalkComponent talk;

        public FlagOnInteractTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            reqFlags = data.Attr("reqFlags");
            setFlag = data.Attr("setFlag");
            state = data.Bool("state");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), new Vector2(Width / 2, 0f), Interact));
            talk.PlayerMustBeFacing = false;
            talk.Enabled = false;
        }

        public override void Update()
        {
            base.Update();
            if (talk != null && !string.IsNullOrEmpty(setFlag))
            {
                if (!SceneAs<Level>().Session.GetFlag(setFlag))
                {
                    CheckFlags();
                }
                else
                {
                    talk.Enabled = false;
                }
            }
        }

        private void CheckFlags()
        {
            bool showTalker = true;
            if (!string.IsNullOrEmpty(reqFlags))
            {
                foreach (string flag in reqFlags.Split(','))
                {
                    if (!SceneAs<Level>().Session.GetFlag(flag))
                    {
                        showTalker = false;
                        break;
                    }
                }
            }
            else
            {
                showTalker = true;
            }
            talk.Enabled = showTalker;
        }

        private void Interact(Player player)
        {
            if (!string.IsNullOrEmpty(setFlag))
            {
                SceneAs<Level>().Session.SetFlag(setFlag, state);
            }
        }
    }
}
