using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/RotateFireflea")]
    public class RotateFireflea : RotateEnemy
    {
        private Wiggler scaleWiggler;

        private Sprite Body;

        public VertexLight Light;

        private bool PlayerBounced;

        private Coroutine StopRoutine = new ();

        private int MaxBounces;

        private int Bounced;

        private int Group;

        public RotateFireflea(EntityData data, Vector2 offset) : base(data, offset)
        {
            Group = data.Int("group", -1);
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            pc.Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(new PlayerCollider(OnBounce, new Hitbox(12f, 3f, -6f, -4f)));
            MaxBounces = data.Int("maxBounces", 0);
            HitSound = "event:/game/xaphan/fireflea_hit";
            Health = 20;
            Body = new Sprite(GFX.Game, "enemies/Xaphan/Fireflea/");
            Body.AddLoop("body", "body", 0.04f);
            Body.Play("body");
            Body.CenterOrigin();
            sprites.Add(Body);
            foreach (Sprite sprite in sprites)
            {
                Add(sprite);
            }
            Add(Light = new VertexLight(Vector2.Zero, Color.White, 1f, 24, 32));
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                Body.Scale = Vector2.One * (1f + f * 0.3f);
            }));
        }

        public override void Update()
        {
            base.Update();
            if (Group != -1)
            {
                foreach (RotateFireflea fireflea in SceneAs<Level>().Tracker.GetEntities<RotateFireflea>())
                {
                    if (fireflea.Group == Group && fireflea != this)
                    {
                        fireflea.Moving = Moving;
                    }
                }
            }
        }

        public override void onHitPlayer(Player player)
        {
            if (player.Bottom >= Y - 4f && player.Speed.Y <= 0f)
            {
                player.Die(new Vector2(0f, -1f));
            }
        }

        private void OnBounce(Player player)
        {
            if (player.Bottom < Y + 1 && player.Speed.Y >= 0f && !Freezed && !PlayerBounced)
            {
                PlayerBounced = true;
                Add(new Coroutine(Bounce(player)));
            }
        }

        private IEnumerator Bounce(Player player)
        {
            scaleWiggler.Start();
            XaphanModule.refillJumps = false;
            player.Bounce((int)(Y - 4f));
            Bounced++;
            if (Bounced == MaxBounces && MaxBounces > 0)
            {
                Die();
            }
            else
            {
                Audio.Play(HitSound, Center);
            }
            if (StopRoutine.Active)
            {
                StopRoutine.Cancel();
            }
            Add(StopRoutine = new Coroutine(Stop()));
            yield return 0.05f;
            PlayerBounced = false;
        }

        private IEnumerator Stop()
        {
            Moving = false;
            yield return 0.35f;
            Moving = true;
        }
    }
}
