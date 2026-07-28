using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class RandomSwapImage : Component
    {
        private List<MTexture> list;
        private MTexture currentTexture;
        public RandomSwapImage(List<MTexture> list) : base(active: true, visible: true)
        {
            SelectTexture();
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            SelectTexture();
        }

        public override void Render()
        {
            Decal decal = Entity as Decal;
            currentTexture.DrawCentered(decal.Position, decal.Color, decal.Scale, decal.Rotation);
        }

        private void SelectTexture()
        {
            currentTexture = list[Calc.Random.Next(list.Count)];
        }
    }
}
