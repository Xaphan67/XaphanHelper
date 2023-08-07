using System;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class TimerIndicator : Entity
    {
        private TimeManager manager;
        public TimerIndicator(TimeManager manager)
        {
            Depth = -20000;
            AddTag(Tags.Persistent);
            this.manager = manager;
        }

        public override void Render()
        {
            base.Render();
            Player player = Scene.Tracker.GetEntity<Player>();
            ScrewAttackManager manager = SceneAs<Level>().Tracker.GetEntity<ScrewAttackManager>();
            bool startedScrewAttack = false;
            if (manager != null)
            {
                startedScrewAttack = manager.StartedScrewAttack;
            }
            if (player != null && (player.Sprite.Visible || !player.Sprite.Visible && (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || startedScrewAttack)) && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                string TimeString = Math.Truncate((double)this.manager.currentTime + 1).ToString();
                if (TimeString != "0")
                {
                    if (TimeString.Length == 3)
                    {
                        MTexture FirstFigure = GFX.Gui["timer/" + TimeString[0]];
                        MTexture SecondFigure = GFX.Gui["timer/" + TimeString[1]];
                        MTexture ThirdFigure = GFX.Gui["timer/" + TimeString[2]];
                        FirstFigure.Draw(player.Center + new Vector2(-11, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f));
                        SecondFigure.Draw(player.Center + new Vector2(-3, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f));
                        ThirdFigure.Draw(player.Center + new Vector2(5, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f));
                    }
                    else if (TimeString.Length == 2)
                    {
                        MTexture FirstFigure = GFX.Gui["timer/" + TimeString[0]];
                        MTexture SecondFigure = GFX.Gui["timer/" + TimeString[1]];
                        FirstFigure.Draw(player.Center + new Vector2(-7, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f));
                        SecondFigure.Draw(player.Center + new Vector2(1, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f));
                    }
                    else
                    {
                        MTexture Figure = GFX.Gui["timer/" + TimeString[0]];
                        Figure.Draw(player.Center + new Vector2(-3, SpaceJump.GetJumpBuffer() > 0 ? -27f : -20f), Vector2.Zero, TimeString[0] == '3' ? Color.Yellow : TimeString[0] == '2' ? Color.Orange : TimeString[0] == '1' ? Color.Red : Color.White);
                    }
                }
            }
        }
    }
}
