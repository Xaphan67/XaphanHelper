using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class LevelShake
    {
        private static FieldInfo Level_shakeDirection = typeof(Level).GetField("shakeDirection", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo Level_lastDirectionalShake = typeof(Level).GetField("lastDirectionalShake", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo Level_shakeTimer = typeof(Level).GetField("shakeTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo Level_cameraPreShake = typeof(Level).GetField("cameraPreShake", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load()
        {
            On.Celeste.Level.Shake += onLevelShake;
            On.Celeste.Level.DirectionalShake += onLevelDirectionalShake;
            On.Celeste.Level.BeforeRender += onLevelBeforeRender;
        }

        public static void Unload()
        {
            On.Celeste.Level.Shake -= onLevelShake;
            On.Celeste.Level.DirectionalShake -= onLevelDirectionalShake;
            On.Celeste.Level.BeforeRender -= onLevelBeforeRender;
        }

        private static void onLevelShake(On.Celeste.Level.orig_Shake orig, Level self, float time)
        {
            orig(self, time);
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Off && XaphanModule.IgnoreShakeSettings)
            {
                Level_shakeDirection.SetValue(self, Vector2.Zero);
                Level_shakeTimer.SetValue(self, Math.Max((float)Level_shakeTimer.GetValue(self), time));
            }
        }

        private static void onLevelDirectionalShake(On.Celeste.Level.orig_DirectionalShake orig, Level self, Vector2 dir, float time)
        {
            orig(self, dir, time);
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Off && XaphanModule.IgnoreShakeSettings)
            {
                Level_shakeDirection.SetValue(self, dir.SafeNormalize());
                Level_lastDirectionalShake.SetValue(self, 0);
                Level_shakeTimer.SetValue(self, Math.Max((float)Level_shakeTimer.GetValue(self), time));
            }
        }

        private static void onLevelBeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self)
        {
            orig(self);
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Off && XaphanModule.IgnoreShakeSettings)
            {
                self.Camera.Position = (Vector2)Level_cameraPreShake.GetValue(self);
                float x = self.ShakeVector.X / 2;
                float y = self.ShakeVector.Y / 2;
                self.Camera.Position += new Vector2(x, y);
                self.Camera.Position = self.Camera.Position.Floor();
            }
        }
    }
}
