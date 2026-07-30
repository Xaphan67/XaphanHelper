using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/GemSlot")]
    class GemSlot : Entity
    {
        public Sprite Sprite;

        public int Chapter;

        public int Index;

        public bool Activated;

        public string ParticleColor;

        public List<Entity> absorbs = new();

        public GemSlot(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Depth = 2000;
            Chapter = data.Int("chapter");
            Index = data.Int("index", 1);
            Add(Sprite = new Sprite(GFX.Game, "collectables/Xaphan/CustomCollectable/gems/Ch" + Chapter + (Index != 1 ? "-" + Index : "") + "/gem"));
            ParticleColor = data.Attr("particleColor");
            Sprite.AddLoop("idle", "", 0.05f, 0);
            Sprite.Add("spin", "", 0.05f, "idle");
            Sprite.Play("idle");
            Sprite.CenterOrigin();
            Sprite.Position.Y -= 6;
            Visible = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + Chapter + "_Gem" + (Index != 1 ? Index : "") + "_Sloted"))
            {
                Activated = true;
                Sprite.Position.Y += 6;
                Visible = true;
            }
        }

        public IEnumerator Activate()
        {
            Level level = Scene as Level;
            level.Displacement.AddBurst(Position, 0.5f, 8f, 32f, 0.5f);
            Visible = true;
            int soundIndex = 0;
            switch (Chapter)
            {
                case 1:
                    soundIndex = Index == 1 ? 1 : 3;
                    break;
                case 2:
                    soundIndex = 2;
                    break;
                case 3:
                    soundIndex = 6;
                    break;
                case 4:
                    soundIndex = 4;
                    break;
                case 5:
                    soundIndex = 5;
                    break;
            }                
            Audio.Play("event:/game/07_summit/gem_unlock_" + soundIndex);
            Sprite.Play("spin");
            while (Sprite.CurrentAnimationID == "spin")
            {
                Sprite.Y += Engine.DeltaTime * 8f;
                yield return null;
            }
            yield return 0.2f;
            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            for (int i = 0; i < 20; i++)
            {
                level.ParticlesFG.Emit(SummitGem.P_Shatter, Position + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-8, 8)), Calc.HexToColor(ParticleColor), Calc.Random.NextFloat((float)Math.PI * 2f));
            }
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + Chapter + "_Gem" + (Index != 1 ? Index : "") + "_Sloted");
            yield return 0.25f;
        }

        public void ReleaseOrbs(Entity gemController)
        {
            Add(new Coroutine(ReleaseOrbsSequence(gemController)));
        }

        public IEnumerator ReleaseOrbsSequence(Entity gemController)
        {
            for (int i = 0; i < 20; i++)
            {
                CustomAbsorbOrb orb = new(Position, gemController, color: ParticleColor);
                Scene.Add(orb);
                absorbs.Add(orb);
                yield return null;
            }
        }

        public void ActivateNoAnim()
        {
            Sprite.Play("spin");
            Sprite.SetAnimationFrame(13);
            Sprite.Position.Y += 6;
            Visible = true;
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + Chapter + "_Gem" + (Index != 1 ? Index : "") + "_Sloted");
        }

        public override void Render()
        {
            Vector2 position = Sprite.Position;
            base.Render();
            Sprite.Position = position;
        }
    }
}
