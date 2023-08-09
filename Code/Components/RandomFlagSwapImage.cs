using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Components
{
    public class RandomFlagSwapImage : Component
    {
        private string flag;
        private List<MTexture> off;
        private List<MTexture> on;
        private MTexture currentTexture;
        private bool prevState;
        public RandomFlagSwapImage(string flag, List<MTexture> off, List<MTexture> on) : base(active: true, visible: true)
        {
            this.flag = flag;
            this.off = off;
            this.on = on;
            SelectTexture(prevState);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            prevState = (Scene as Level).Session.GetFlag(flag);
            SelectTexture(prevState);
        }

        public override void Update()
        {
            base.Update();
            bool state = (Scene as Level).Session.GetFlag(flag);
            if (state != prevState)
            {
                SelectTexture(state);
            }
            prevState = state;
        }

        public override void Render()
        {
            Decal decal = Entity as Decal;
            currentTexture.DrawCentered(decal.Position, decal.Color, decal.Scale, decal.Rotation);
        }

        private void SelectTexture(bool state)
        {
            var list = state ? on : off;
            currentTexture = list[Calc.Random.Next(list.Count)];
        }
    }
}
