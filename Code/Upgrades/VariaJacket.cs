using System.Collections;
using System.Reflection;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class VariaJacket : Upgrade
    {
        private FieldInfo LookoutAnimPrefix = typeof(Lookout).GetField("animPrefix", BindingFlags.Instance | BindingFlags.NonPublic);

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.VariaJacket ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.VariaJacket = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Lookout.LookRoutine += modLookoutLookRoutine;
        }

        public override void Unload()
        {
            On.Celeste.Lookout.LookRoutine -= modLookoutLookRoutine;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.VariaJacket && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).VariaJacketInactive.Contains(level.Session.Area.LevelSet);
        }

        private IEnumerator modLookoutLookRoutine(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player)
        {
            if (Active(player.SceneAs<Level>()) && !GravityJacket.Active(player.SceneAs<Level>()))
            {
                LookoutAnimPrefix.SetValue(self, "varia_");
            }
            IEnumerator origEnum = orig(self, player);
            while (origEnum.MoveNext()) yield return origEnum.Current;
        }
    }
}
