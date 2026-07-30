using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/EndBlock")]
    class EndBlock : Solid
    {
        public Sprite sprite;

        private bool playBreakSound;

        public int index;

        private EntityID eid;

        public EndBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            eid = id;
            playBreakSound = data.Bool("playBreakSound");
            index = data.Int("index");
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
        }

        public void Break()
        {
            if (playBreakSound)
            {
                Audio.Play("event:/game/general/wall_break_stone", Position);
            }
            SceneAs<Level>().Session.DoNotLoad.Add(eid);
            foreach (BreakBlock breakblock in SceneAs<Level>().Entities.FindAll<BreakBlock>())
            {
                if (breakblock.index == index)
                {
                    breakblock.Break(false, true);
                    SceneAs<Level>().Session.DoNotLoad.Add(breakblock.eid);
                }
            }
            RemoveSelf();
        }
    }
}
