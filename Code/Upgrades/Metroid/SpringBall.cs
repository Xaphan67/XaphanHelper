namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class SpringBall : Upgrade
    {
        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.SpringBall ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.SpringBall = (value != 0);
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.SpringBall && !(XaphanModule.Instance._SaveData as XaphanModuleSaveData).SpringBallInactive.Contains(level.Session.Area.LevelSet);
        }
    }
}
