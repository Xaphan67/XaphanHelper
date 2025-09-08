using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/PoemTrigger")]
    public class PoemTrigger : Trigger
    {
        public EntityID ID;

        private string triggerSound;

        private string newMusic;

        public string flag;

        private bool changeMusic;

        private bool endChapter;

        private bool registerInSaveData;

        private bool onlyOnce;

        public static ParticleType shineParticle;

        private SoundEmitter sfx;

        private CustomPoem poem;

        private string poemName;

        private string poemColor;

        private string poemParticlesColor;

        private string poemSprite;

        private float poemSpriteAnimationSpeed;

        private float poemSpriteAnimationPause;

        public PoemTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            changeMusic = data.Bool("changeMusic");
            triggerSound = data.Attr("triggerSound");
            newMusic = data.Attr("newMusic");
            flag = data.Attr("flag");
            endChapter = data.Bool("completeArea") || data.Bool("endChapter");
            registerInSaveData = data.Bool("registerInSaveData");
            onlyOnce = data.Bool("onlyOnce");
            poemName = Dialog.Clean(data.Attr("poemName"));
            poemColor = data.Attr("poemColor", "FFFFFF");
            poemParticlesColor = data.Attr("poemParticlesColor", "FFFFFF");
            poemSprite = data.Attr("poemSprite", "collectables/heartgem/0/spin");
            poemSpriteAnimationSpeed = data.Float("poemSpriteAnimationSpeed", 0.08f);
            poemSpriteAnimationPause = data.Float("poemSpriteAnimationPause", 0f);
        }

        public override void OnEnter(Player player)
        {
            Level level = Scene as Level;
            base.OnEnter(player);
            Add(new Coroutine(DisplayPoem(player, level)));
        }

        private IEnumerator DisplayPoem(Player player, Level level)
        {
            Collidable = false;
            Session session = level.Session;
            level.CanRetry = false;
            if (endChapter)
            {
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
            }
            sfx = SoundEmitter.Play(triggerSound, this);
            Add(new LevelEndingHook(delegate
            {
                sfx.Source.Stop();
            }));
            Depth = -2000000;
            yield return null;
            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            Visible = false;
            for (float t3 = 0f; t3 < 2f; t3 += Engine.RawDeltaTime)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }
            yield return null;
            if (player.Dead)
            {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            if (endChapter)
            {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            poem = new CustomPoem("", poemName, poemColorA: poemColor, sprite: poemSprite, spriteSpeed: poemSpriteAnimationSpeed, spriteWait: poemSpriteAnimationPause, poemParticleColor: poemParticlesColor);
            poem.Alpha = 0f;
            Scene.Add(poem);
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
            {
                poem.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            sfx.Source.Param("end", 1f);
            if (!endChapter)
            {
                for (float t3 = 0f; t3 < 1f; t3 += Engine.RawDeltaTime * 2f)
                {
                    poem.Alpha = Ease.CubeIn(1f - t3);
                    yield return null;
                }
                player.Depth = 0;
                level.FormationBackdrop.Display = false;
                level.Frozen = false;
                level.CanRetry = true;
                Engine.TimeRate = 1f;
                if (poem != null)
                {
                    poem.RemoveSelf();
                }
            }
            RegisterFlag();
            List<Strawberry> strawbs = new();
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Strawberry)
                {
                    strawbs.Add(follower.Entity as Strawberry);
                }
            }
            if (changeMusic)
            {
                if (!string.IsNullOrEmpty(newMusic))
                {
                    session.Audio.Music.Event = SFX.EventnameByHandle(newMusic);
                    session.Audio.Apply(forceSixteenthNoteHack: false);
                }
            }
            if (!endChapter)
            {
                Engine.TimeRate = 0.5f;
                while (Engine.TimeRate < 1f)
                {
                    Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                    yield return null;
                }
            }
            else
            {
                foreach (Strawberry strawb in strawbs)
                {
                    strawb.OnCollect();
                }
                Visible = false;
                if (player.Dead)
                {
                    yield return 100f;
                }
                Engine.TimeRate = 1f;
                Tag = Tags.FrozenUpdate;
                level.Frozen = true;
                level.TimerStopped = true;
                yield return new FadeWipe(level, wipeIn: false)
                {
                    Duration = 1.25f
                }.Duration;
                level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: false);
            }
            if (onlyOnce)
            {
                session.DoNotLoad.Add(ID);
                RemoveSelf();
            }
            while (player != null && CollideCheck(player) && !player.Dead)
            {
                yield return null;
            }
            Collidable = true;
        }

        private void RegisterFlag()
        {
            Session session = SceneAs<Level>().Session;
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (flag != "")
            {
                session.SetFlag(flag, true);
            }
            if (registerInSaveData)
            {
                string Prefix = session.Area.LevelSet;
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    session.SetFlag("Ch" + chapterIndex + "_" + flag, true);
                }
            }
        }
    }
}
