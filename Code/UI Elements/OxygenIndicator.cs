using System;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class OxygenIndicator : Entity
    {
        private Coroutine TimerRoutine = new();

        public OxygenIndicator()
        {
            Depth = -20000;
            AddTag(Tags.Persistent);
            Visible = false;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            BreathManager manager = SceneAs<Level>().Tracker.GetEntity<BreathManager>();
            if (player != null && manager != null)
            {
                Visible = manager.isVisible;
            }
        }

        public override void Render()
        {
            base.Render();
            if (Visible && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMOxygenIndicator : XaphanModule.ModSettings.OxygenIndicator) != 0
                && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMOxygenIndicator : XaphanModule.ModSettings.OxygenIndicator) != 3)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                ScrewAttackManager SAmanager = SceneAs<Level>().Tracker.GetEntity<ScrewAttackManager>();
                BreathManager Bmanager = SceneAs<Level>().Tracker.GetEntity<BreathManager>();
                bool startedScrewAttack = false;
                if (SAmanager != null)
                {
                    startedScrewAttack = SAmanager.StartedScrewAttack;
                }
                if (player != null && Bmanager != null && (player.Sprite.Visible || !player.Sprite.Visible && (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || startedScrewAttack)) && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    MTexture oxygenIndicator = GFX.Gui["upgrades/staminaindicator"];
                    Vector2 Position = player.Center + new Vector2(player.Facing == Facings.Right ? (player.StateMachine.State != Player.StClimb ? -14f : -12f) : (player.StateMachine.State != Player.StClimb ? 7f : 5f), -10f);
                    StaminaIndicator staminaIndicator = SceneAs<Level>().Tracker.GetEntity<StaminaIndicator>();
                    if (staminaIndicator != null && staminaIndicator.Visible)
                    {
                        Position += player.Facing == Facings.Right ? Vector2.UnitX * -4f : Vector2.UnitX * 4f;
                    }
                    oxygenIndicator.Draw(Position);
                    Position += new Vector2(3f, 3f);
                    Draw.Line(Position + Vector2.UnitY * 10f, Position + Vector2.UnitY * 10f - Vector2.UnitY * (float)Math.Ceiling(10f * Bmanager.GetAirPercent() / 100f), Bmanager.GetAirPercent() > 26.66f ? Calc.HexToColor("00AAFF") : Calc.HexToColor("002BFE"));
                }
            }
        }
    }
}
