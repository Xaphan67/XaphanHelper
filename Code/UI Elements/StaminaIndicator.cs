using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Managers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class StaminaIndicator : Entity
    {
        public static float BaseStamina;

        public static string Prefix;

        private Coroutine TimerRoutine = new();

        private bool Hide;

        public StaminaIndicator()
        {
            Depth = -20000;
            AddTag(Tags.Persistent);
            Visible = false;
        }

        public static void getStaminaData(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            string entity = "XaphanHelper/UpgradeController";
            if (MapData.HasEntity(entity))
            {
                BaseStamina = MapData.GetEntityData(entity).Float("baseStamina", 110);
                Prefix = area.LevelSet;
            }
            int ExtraStamina = 0;
            foreach (string upgrade in XaphanModule.PlayerHasGolden ? XaphanModule.ModSaveData.GoldenStrawberryStaminaUpgrades : XaphanModule.ModSaveData.StaminaUpgrades)
            {
                if (upgrade.Contains(Prefix))
                {
                    ExtraStamina += 5;
                }
            }
            BaseStamina += ExtraStamina;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (player.Stamina < BaseStamina)
                {
                    Visible = true;
                    Hide = false;
                }
                else if (player.Stamina == BaseStamina && !Hide)
                {
                    if (!TimerRoutine.Active)
                    {
                        Add(TimerRoutine = new Coroutine(DisplayTimer()));
                    }
                }
            }
        }

        public IEnumerator DisplayTimer()
        {
            Hide = true;
            yield return 1.5f;
            Visible = false;
        }

        public override void Render()
        {
            base.Render();
            if (Visible && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMStaminaIndicator : XaphanModule.ModSettings.StaminaIndicator) != 0
                && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMStaminaIndicator : XaphanModule.ModSettings.StaminaIndicator) != 3)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                ScrewAttackManager manager = SceneAs<Level>().Tracker.GetEntity<ScrewAttackManager>();
                bool startedScrewAttack = false;
                if (manager != null)
                {
                    startedScrewAttack = manager.StartedScrewAttack;
                }
                if (player != null && (player.Sprite.Visible || !player.Sprite.Visible && (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || startedScrewAttack)) && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    MTexture staminaIndicator = GFX.Gui["upgrades/staminaindicator"];
                    Vector2 Position = player.Center + new Vector2(player.Facing == Facings.Right ? (player.StateMachine.State != Player.StClimb ? -14f : - 12f) : (player.StateMachine.State != Player.StClimb ? 7f : 5f), -10f);
                    staminaIndicator.Draw(Position);
                    Position += new Vector2(3f, 3f);
                    float Percent = player.Stamina * 100 / BaseStamina;
                    Draw.Line(Position + Vector2.UnitY * 10f, Position + Vector2.UnitY * 10f - Vector2.UnitY * (float)Math.Ceiling(10f * Percent / 100f), player.Stamina > 20 ? Calc.HexToColor("2DFF00") : Calc.HexToColor("FE0000"));
                }
            }
        }
    }
}
