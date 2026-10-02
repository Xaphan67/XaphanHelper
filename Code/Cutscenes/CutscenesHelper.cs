using System;
using System.Collections;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    internal class CutscenesHelper
    {
        public static void Load()
        {
            On.Celeste.CutsceneEntity.EndCutscene += onCutsceneEntityEndCutscene;
        }

        public static void Unload()
        {
            On.Celeste.CutsceneEntity.EndCutscene -= onCutsceneEntityEndCutscene;
        }

        private static void onCutsceneEntityEndCutscene(On.Celeste.CutsceneEntity.orig_EndCutscene orig, CutsceneEntity self, Level level, bool removeSelf)
        {
            Player player = level.Tracker.GetEntity<Player>();
            RestaurePlayerDashes(level, player);
            orig(self, level, removeSelf);
        }

        // Badeline movements

        public static BadelineDummy BadelineAppears(Level level, Vector2 position)
        {
            Player player = level.Tracker.GetEntity<Player>();
            BadelineDummy badeline = new BadelineDummy(position);
            level.Add(badeline);
            badeline.Appear(level);
            level.Session.Inventory = new PlayerInventory(1);
            player.Dashes = 1;
            return badeline;
        }

        public static BadelineDummy BadelineSplit(Level level, Player player)
        {
            BadelineDummy badeline = new BadelineDummy(player.Position);
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            level.Add(badeline);
            level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            level.Session.Inventory = new PlayerInventory(1);
            player.Dashes = 1;
            return badeline;
        }

        public static IEnumerator BadelineFloat(CutsceneEntity cutscene, int x, int y, BadelineDummy badeline, int? turnAtEndTo, bool faceDirection, bool fadeLight, bool quickEnd)
        {
            Vector2 badelineEndPosition = new Vector2(badeline.Position.X + x, badeline.Position.Y + y);
            yield return badeline.FloatTo(badelineEndPosition, turnAtEndTo, faceDirection, fadeLight, quickEnd);
            while (badeline.Position != badelineEndPosition)
            {
                yield return null;
            }
        }

        public static IEnumerator BadelineMerge(Level level, Player player, BadelineDummy badeline)
        {
            yield return badeline.FloatTo(player.Position, null, true, false, true);
            while (badeline.Position != player.Position)
            {
                yield return null;
            }
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", badeline.Position);
            level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            RestaurePlayerDashes(level, player);
            badeline.RemoveSelf();
        }

        private static void RestaurePlayerDashes(Level level, Player player)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            EntityData UpgradeController = XaphanModule.GetUpgradeController(MapData);
            string doubleDashFlag = UpgradeController.Attr("doubleDashFlag");
            if (!string.IsNullOrEmpty(doubleDashFlag) && (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_" + doubleDashFlag) || level.Session.GetFlag(doubleDashFlag)))
            {
                level.Session.Inventory = new PlayerInventory(2);
                player.Dashes = 2;
            }
        }
    }
}
