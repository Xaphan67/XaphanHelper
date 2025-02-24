using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    public class CustomParticleSystem : Entity
    {
        private Particle[] particles;

        private int nextSlot;

        public CustomParticleSystem(int depth, int maxParticles)
        {
            Tag = Tags.TransitionUpdate;
            particles = new Particle[maxParticles];
            Depth = depth;
        }

        public void Clear()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Active = false;
            }
        }

        public void ClearRect(Rectangle rect, bool inside)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 position = particles[i].Position;
                if ((position.X > (float)rect.Left && position.Y > (float)rect.Top && position.X < (float)rect.Right && position.Y < (float)rect.Bottom) == inside)
                {
                    particles[i].Active = false;
                }
            }
        }

        public override void Update()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].Active)
                {
                    particles[i].Update();
                }
            }
        }

        public override void Render()
        {
            Particle[] array = particles;
            for (int i = 0; i < array.Length; i++)
            {
                Particle particle = array[i];
                if (particle.Active)
                {
                    particle.Render();
                }
            }
        }

        public void Render(float alpha)
        {
            Particle[] array = particles;
            for (int i = 0; i < array.Length; i++)
            {
                Particle particle = array[i];
                if (particle.Active)
                {
                    particle.Render(alpha);
                }
            }
        }

        public void Simulate(float duration, float interval, Action<CustomParticleSystem> emitter)
        {
            float num = 0.016f;
            for (float num2 = 0f; num2 < duration; num2 += num)
            {
                if ((int)((num2 - num) / interval) < (int)(num2 / interval))
                {
                    emitter(this);
                }

                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].Active)
                    {
                        particles[i].Update(num);
                    }
                }
            }
        }

        public void Add(Particle particle)
        {
            particles[nextSlot] = particle;
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, Vector2 position, float direction)
        {
            type.Create(ref particles[nextSlot], position, direction);
            nextSlot = (nextSlot + 1) % particles.Length;
        }

        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            for (int i = 0; i < amount; i++)
            {
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), direction);
            }
        }
    }
}
