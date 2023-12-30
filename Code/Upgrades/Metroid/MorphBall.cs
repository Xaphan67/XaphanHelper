namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class MorphingBall : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.MorphingBall ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.MorphingBall = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.MorphingBall && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).MorphingBallInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
