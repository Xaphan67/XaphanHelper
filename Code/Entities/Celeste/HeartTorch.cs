using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/HeartTorch")]
    class HeartTorch : Entity
    {
        public string flag;

        public string activeFlag;

        public string sound;

        private Sprite sprite;

        public HeartTorch(EntityData data, Vector2 position) : base(data.Position + position)
        {
            flag = data.Attr("flag");
            activeFlag = data.Attr("activeFlag");
            sound = data.Attr("sound");
            Depth = 8999;
            Add(sprite = new Sprite(GFX.Game, "objects/XaphanHelper/heartTorch/torch"));
            sprite.AddLoop("idle", "", 0f, default(int));
            sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
            sprite.Play("idle");
            sprite.Origin = new Vector2(32f, 64f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                if (SceneAs<Level>().Session.GetFlag(activeFlag))
                {
                    PlayLit();
                }
                else
                {
                    Add(new Coroutine(Activate()));
                }
            }
        }

        private void PlayLit()
        {
            sprite.Play("lit");
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            Add(new VertexLight(Color.White, 1f, 24, 48));
            Add(new BloomPoint(0.6f, 16f));
        }

        private IEnumerator Activate()
        {
            yield return 1;
            Audio.Play(sound, Position);
            PlayLit();
            SceneAs<Level>().Session.SetFlag(activeFlag, true);
            string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + activeFlag))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + activeFlag);
            }
            if (XaphanModule.PlayerHasGolden)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + activeFlag + "_GoldenStrawberry"))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + activeFlag + "_GoldenStrawberry");
                }
            }
        }
    }
}
