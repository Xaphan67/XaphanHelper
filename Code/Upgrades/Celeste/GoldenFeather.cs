using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class GoldenFeather : Upgrade
    {
        Coroutine UseFeatherCoroutine = new();

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.GoldenFeather ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.GoldenFeather = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
        }

        public bool Active(Level level)
        {
            return XaphanModule.ModSettings.GoldenFeather && !XaphanModule.ModSaveData.GoldenFeatherInactive.Contains(level.Session.Area.LevelSet);
        }

        public static bool isActive;

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
            {
                if (Active(self))
                {
                    isActive = true;
                    Player player = self.Tracker.GetEntity<Player>();
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && !player.OnGround() && player.Stamina > 20 && player.Speed.Y > 0 && Input.Grab.Pressed && player.StateMachine.State == 0 && player.Holding == null)
                    {
                        if (!inLiquid(self) && !UseFeatherCoroutine.Active)
                        {
                            UseFeatherCoroutine = new Coroutine(routine(player, self));
                        }
                    }
                    if (UseFeatherCoroutine != null && UseFeatherCoroutine.Active)
                    {
                        UseFeatherCoroutine.Update();
                    }
                }
                else
                {
                    isActive = false;
                }
            }
        }

        public static bool inLiquid(Level level)
        {
            foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
            {
                if (liquid.PlayerInside())
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator routine(Player player, Level level)
        {
            while ((player.Speed.Y < 100f || ClimbCheck(player, player.Facing == Facings.Left ? -1 : 1)) && Input.Grab.Check)
            {
                yield return null;
            }
            if (player.Speed.Y >= 100f && Input.Grab.Check)
            {
                int totalFeathers = level.Tracker.CountEntities<Feather>();
                if (totalFeathers == 0 && !ClimbCheck(player, player.Facing == Facings.Left ? -1 : 1))
                {
                    level.Add(new Feather(player.Position));
                }
            }
        }

        public bool ClimbCheck(Player player, int dir)
        {
            if (player.ClimbBoundsCheck(dir) && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX * 2f * (float)player.Facing))
            {
                if (player.CollideCheck<Solid>(player.Position + new Vector2(dir * 8, 0)))
                {
                    if (dir == -1 ? player.Speed.X < 0 : player.Speed.X > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
