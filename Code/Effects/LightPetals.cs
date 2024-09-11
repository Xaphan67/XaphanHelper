using Microsoft.Xna.Framework;
using System;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Effects
{
    public class LightPetals : Backdrop
    {
        private struct Particle
        {
            public Vector2 Position;

            public float Speed;

            public float Spin;

            public float MaxRotate;

            public int Color;

            public float RotationCounter;
        }

        private static Color[] colors = new Color[2];

        private Particle[] particles = new Particle[40];

        private float fade;

        private XaphanModuleSession.LightModes previousLightMode;

        public LightPetals(string lightColor, string darkColor)
        {
            colors[0] = Calc.HexToColor(lightColor);
            colors[1] = Calc.HexToColor(darkColor);
            for (int i = 0; i < particles.Length; i++)
            {
                Reset(i);
            }
        }

        private void Reset(int i)
        {
            particles[i].Position = new Vector2(Calc.Random.Range(0, 352), Calc.Random.Range(0, 212));
            particles[i].Speed = Calc.Random.Range(6f, 16f);
            particles[i].Spin = Calc.Random.Range(8f, 12f) * 0.2f;
            if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light)
            {
                particles[i].Color = 0;
                previousLightMode = XaphanModuleSession.LightModes.Light;
            }
            else
            {
                particles[i].Color = 1;
                previousLightMode = XaphanModuleSession.LightModes.Dark;
            }
            particles[i].RotationCounter = Calc.Random.NextAngle();
            particles[i].MaxRotate = Calc.Random.Range(0.3f, 0.6f) * ((float)Math.PI / 2f);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            int color = -1;
            if (UpdateColor(scene) != -1)
            {
                color = UpdateColor(scene);
            }
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
                particles[i].RotationCounter += particles[i].Spin * Engine.DeltaTime;
                if (color != -1)
                {
                    particles[i].Color = color;
                    previousLightMode = color == 0 ? XaphanModuleSession.LightModes.Light : XaphanModuleSession.LightModes.Dark;
                }
            }
            fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
        }

        private int UpdateColor(Scene scene)
        {
            Player player = scene.Tracker.GetEntity<Player>();
            if (player != null && !player.Dead)
            {
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Light && previousLightMode == XaphanModuleSession.LightModes.Dark)
                {
                    return 0;
                }
                if (XaphanModule.ModSession.LightMode == XaphanModuleSession.LightModes.Dark && previousLightMode == XaphanModuleSession.LightModes.Light)
                {
                    return 1;
                }
            }
            return -1;
        }

        public override void Render(Scene level)
        {
            if (!(fade <= 0f))
            {
                Camera camera = (level as Level).Camera;
                MTexture mTexture = GFX.Game["particles/petal"];
                for (int i = 0; i < particles.Length; i++)
                {
                    Vector2 position = default(Vector2);
                    position.X = -16f + Mod(particles[i].Position.X - camera.X, 352f);
                    position.Y = -16f + Mod(particles[i].Position.Y - camera.Y, 212f);
                    float num = (float)(1.5707963705062866 + Math.Sin(particles[i].RotationCounter * particles[i].MaxRotate) * 1.0);
                    position += Calc.AngleToVector(num, 4f);
                    mTexture.DrawCentered(position, colors[particles[i].Color] * fade, 1f, num - 0.8f);
                }
            }
        }

        private float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
