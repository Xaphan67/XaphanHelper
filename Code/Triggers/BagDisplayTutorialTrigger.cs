using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/BagDisplayTutorialTrigger")]
    class BagDisplayTutorialTrigger : Trigger
    {
        EntityID ID;

        string slot;

        bool keepUIOpenOnLeave;

        bool onlyOnce;

        bool shownTutorial;

        public BagDisplayTutorialTrigger(EntityData data, Vector2 offset, EntityID eid) : base(data, offset)
        {
            ID = eid;
            slot = data.Attr("slot", "Bag");
            keepUIOpenOnLeave = data.Bool("keepUIOpenOnLeave", false);
            onlyOnce = data.Bool("onlyOnce", false);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((onlyOnce && SceneAs<Level>().Session.DoNotLoad.Contains(ID)) || (!CollideCheck<UpgradeCollectable>()))
            {
                RemoveSelf();
            }
            else if (SceneAs<Level>().Session.DoNotLoad.Contains(ID))
            {
                SceneAs<Level>().Session.DoNotLoad.Remove(ID);
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            foreach (BagDisplay display in SceneAs<Level>().Tracker.GetEntities<BagDisplay>())
            {
                if (display.type == slot.ToLower() && !display.preventTutorialDisplay)
                {
                    display.ShowTutorial(true);
                }
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            foreach (BagDisplay display in SceneAs<Level>().Tracker.GetEntities<BagDisplay>())
            {
                if (display.shownTutorial)
                {
                    shownTutorial = true;
                    if (!keepUIOpenOnLeave)
                    {
                        if (display.type == slot.ToLower())
                        {
                            display.ShowTutorial(false);
                        }
                    }
                    if (display.type == slot.ToLower())
                    {
                        display.preventTutorialDisplay = false;
                        if (onlyOnce)
                        {
                            RemoveSelf();
                        }
                    }
                }
            }
            if (shownTutorial && onlyOnce && !SceneAs<Level>().Session.DoNotLoad.Contains(ID))
            {
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
                RemoveSelf();
            }
        }
    }
}
