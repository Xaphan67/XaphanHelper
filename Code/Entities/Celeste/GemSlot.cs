using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/GemSlot")]
    class GemSlot : Entity
    {
        public Sprite Sprite;

        public int Chapter;

        public bool Activated;

        public string ParticleColor;

        public GemSlot(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Depth = 2000;
            Add(Sprite = new Sprite(GFX.Game, "collectables/Xaphan/CustomCollectable/gems/Ch" + data.Int("chapter") + "/gem"));
            Chapter = data.Int("chapter");
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
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + Chapter + "_Gem_Sloted" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
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
            Audio.Play("event:/game/07_summit/gem_unlock_" + Chapter);
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
            XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + Chapter + "_Gem_Sloted");
            if (XaphanModule.PlayerHasGolden)
            {
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch" + Chapter + "_Gem_Sloted_GoldenStrawberry");
            }
            yield return 0.25f;
        }

        public override void Render()
        {
            Vector2 position = Sprite.Position;
            base.Render();
            Sprite.Position = position;
        }
    }
}
