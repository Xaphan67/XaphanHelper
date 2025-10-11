using System.Collections;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/TrackFireflea")]
    public class TrackFireflea : TrackEnemy
    {
        private Wiggler scaleWiggler;

        private Sprite Body;

        public VertexLight Light;

        private bool PlayerBounced;

        private bool MoveAfterBounce;

        private bool DieOnBounceAtLastNode;

        private bool ForcePause;

        private Coroutine StopRoutine = new();

        private int MaxBounces;

        private int Bounced;

        private int Group;

        public TrackFireflea(EntityData data, Vector2 offset) : base(data, offset)
        {
            Group = data.Int("group", -1);
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            pc.Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(new PlayerCollider(OnBounce, new Hitbox(12f, 3f, -6f, -4f)));
            MoveAfterBounce = data.Bool("moveAfterBounce", false);
            DieOnBounceAtLastNode = data.Bool("dieOnBounceAtLastNode", false);
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
            Add(Light = new VertexLight(Vector2.Zero, Color.White, 1f, 16, 24));
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                Body.Scale = Vector2.One * (1f + f * 0.3f);
            }));
        }

        public override void Update()
        {
            base.Update();
            if (MoveAfterBounce && ForcePause)
            {
                WaitTime = 1f;
                PauseTimer = 1f;
            }
            if (Group != -1)
            {
                foreach (TrackFireflea fireflea in SceneAs<Level>().Tracker.GetEntities<TrackFireflea>())
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
            if (MoveAfterBounce)
            {
                PauseTimer = 0f;
                ForcePause = false;
            }
            if ((DieOnBounceAtLastNode && !Up) || (Bounced == MaxBounces && MaxBounces > 0))
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
            if (Percent > 0 && Percent < 1)
            {
                Moving = false;
                yield return 0.35f;
                Moving = true;
            }
        }

        public override void OnTrackStart()
        {
            if (MoveAfterBounce)
            {
                ForcePause = true;
            }
        }

        public override void OnTrackEnd()
        {
            
        }
    }
}
