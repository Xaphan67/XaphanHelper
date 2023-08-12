using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{

    [CustomEntity("XaphanHelper/StopCountdownTrigger")]
    class StopCountdownTrigger : Trigger
    {
        public StopCountdownTrigger(EntityData data, Vector2 offset, EntityID ID) : base(data, offset)
        {

        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            CountdownDisplay timer = SceneAs<Level>().Tracker.GetEntity<CountdownDisplay>();
            if (timer != null)
            {
                SceneAs<Level>().Session.SetFlag(timer.activeFlag, false);
                XaphanModule.ModSaveData.CountdownCurrentTime = -1;
                XaphanModule.ModSaveData.CountdownShake = false;
                XaphanModule.ModSaveData.CountdownExplode = false;
                XaphanModule.ModSaveData.CountdownActiveFlag = "";
                XaphanModule.ModSaveData.CountdownStartChapter = -1;
                XaphanModule.ModSaveData.CountdownStartRoom = "";
                XaphanModule.ModSaveData.CountdownSpawn = new Vector2();
                XaphanModule.ModSaveData.CountdownHideFlag = "";
                XaphanModule.ModSaveData.CountdownEventsFlags = "";
                SceneAs<Level>().SaveQuitDisabled = false;
                timer.RemoveSelf();
                StartCountdownTrigger startTrigger = SceneAs<Level>().Tracker.GetEntity<StartCountdownTrigger>();
                if (startTrigger != null)
                {
                    startTrigger.CancelTimer();
                }
            }
        }
    }
}
