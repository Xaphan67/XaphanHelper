using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [CustomEntity("XaphanHelper/GemController")]
    class GemController : Entity
    {
        public bool Ch1GemCollected() { return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected"); }

        public bool Ch2GemCollected() { return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected"); }

        public bool Ch3GemCollected() { return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected"); }

        public bool Ch4GemCollected() { return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected"); }

        public bool EndAreaOpened;

        private bool triggered;

        public GemController(EntityData data, Vector2 position) : base(data.Position + position)
        {

        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Session.GetFlag("CS_Ch0_Gem_Room_Activeate_Gems") && !triggered)
            {
                triggered = true;
                Add(new Coroutine(ActivateGems()));
            }
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_End_Area_Open"))
            {
                SceneAs<Level>().Session.SetFlag("Open_End_Area", true);
            }
            else if (Ch1GemCollected() && Ch2GemCollected() && Ch3GemCollected() && Ch4GemCollected())
            {
                if (!EndAreaOpened)
                {
                    EndAreaOpened = true;
                    Add(new Coroutine(OpenEndArea()));
                }
            }
        }

        public IEnumerator ActivateGems()
        {
            foreach (GemSlot gem in Scene.Entities.FindAll<GemSlot>())
            {
                if (!gem.Activated && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + gem.Chapter + "_Gem_Collected"))
                {
                    yield return 0.5f;
                    gem.Activated = true;
                    yield return gem.Activate();
                }
            }
        }

        public IEnumerator OpenEndArea()
        {
            float timer = 1.5f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            SceneAs<Level>().Session.SetFlag("Open_End_Area", true);
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_End_Area_Open");
        }
    }
}
