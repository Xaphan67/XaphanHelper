using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/EndBlock")]
    class EndBlock : Solid
    {
        public Sprite sprite;

        private bool broken;

        private bool playBreakSound;

        public int index;

        public float timer;

        private EntityID eid;

        public EndBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            eid = id;
            playBreakSound = data.Bool("playBreakSound");
            index = data.Int("index");
            timer = data.Float("timer");
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/EndBlock/"));
            sprite.AddLoop("idle", "idle", 1f);
            Depth = -13001;
            Add(new LightOcclude(0.5f));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_End_Area_Open"))
            {
                sprite.Play("idle");
                Collidable = true;
            }
            else
            {
                foreach (BreakBlock breakblock in SceneAs<Level>().Entities.FindAll<BreakBlock>())
                {
                    if (breakblock.index == index)
                    {
                        breakblock.RemoveSelf();
                    }
                }
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            if (broken)
            {
                RemoveSelf();
            }
            else if (SceneAs<Level>().Session.GetFlag("Open_End_Area"))
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                Add(new Coroutine(BreakSequence(player)));
            }
        }

        public IEnumerator BreakSequence(Player player)
        {
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            sprite.RemoveSelf();
            broken = true;
            Collidable = false;
            SceneAs<Level>().Session.DoNotLoad.Add(eid);
            foreach (BreakBlock breakblock in SceneAs<Level>().Entities.FindAll<BreakBlock>())
            {
                if (breakblock.index == index)
                {
                    breakblock.Break(playBreakSound, true);
                }
            }
        }
    }
}
