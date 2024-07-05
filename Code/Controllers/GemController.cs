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
        public bool Ch1GemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool Ch1Gem2Collected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool Ch2GemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool Ch3GemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool Ch4GemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public bool Ch5GemCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

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
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_End_Area_Open" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                SceneAs<Level>().Session.SetFlag("Open_End_Area", true);
            }
            else if (Ch1GemCollected() && Ch1Gem2Collected() && Ch2GemCollected() && Ch3GemCollected() && Ch4GemCollected() && Ch5GemCollected())
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
                if (!gem.Activated && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + gem.Chapter + "_Gem" + ((gem.Index != 1 ? gem.Index : "")) + "_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
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
            if (XaphanModule.PlayerHasGolden)
            {
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_End_Area_Open_GoldenStrawberry");
            }
        }
    }
}
