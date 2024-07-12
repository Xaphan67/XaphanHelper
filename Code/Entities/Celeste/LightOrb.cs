using System;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/LightOrb")]
    class LightOrb : Entity
    {
        private Sprite Sprite;

        private bool PlayerOnTop;

        private bool Temporary;

        private float Timer;

        private LightManager Manager;

        public LightOrb(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            Temporary = data.Bool("temporary", false);
            Timer = data.Float("time", 3f);
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/LightOrb/"));
            Add(new PlayerCollider(onPlayer, Collider));
            Sprite.AddLoop("light", "light", 0.08f);
            Sprite.AddLoop("dark", "dark", 0.08f);
            Sprite.AddLoop("light-small", "light-small", 0.08f);
            Sprite.AddLoop("dark-small", "dark-small", 0.08f);
            Sprite.CenterOrigin();
            Collider = new Circle(Temporary ? 5f : 9f);
            if (Temporary)
            {
                Sprite.Position += Vector2.One * 4f;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            LightManager manager = SceneAs<Level>().Tracker.GetEntity<LightManager>();
            if(manager != null)
            {
                Manager = manager;
            }
        }

        private void onPlayer(Player player)
        {
            if (!PlayerOnTop)
            {
                PlayerOnTop = true;
                if (Manager != null)
                {
                    if (Temporary)
                    {
                        if (Manager.ForceFlagRoutine.Active)
                        {
                            Manager.ForceTemporaryFlag(Manager.ForcedFlagState, Timer);
                        }
                        else
                        {
                            Manager.ForceTemporaryFlag(!SceneAs<Level>().Session.GetFlag("XaphanHelper_LightMode"), Timer);
                        }
                    }
                    else
                    {
                        Manager.SetMainFlag(!Manager.MainFlagState);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Manager == null)
            {
                Manager = SceneAs<Level>().Tracker.GetEntity<LightManager>();
            }
            if (Temporary)
            {
                if (Manager.ForceFlagRoutine.Active)
                {
                    Sprite.Play((Manager.ForcedFlagState ? "light-small" : "dark-small"));
                }
                else
                {
                    Sprite.Play((Manager.MainFlagState ? "dark-small" : "light-small"));
                }
            }
            else
            {
                Sprite.Play((Manager.MainFlagState ? "light" : "dark"));
            }
            
            if (CollideFirst<Player>() == null && PlayerOnTop)
            {
                PlayerOnTop = false;
            }
        }
    }
}
