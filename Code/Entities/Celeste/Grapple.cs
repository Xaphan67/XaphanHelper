using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked]
    public class Grapple : Entity
    {
        public class ConditionalGrabNode : VirtualButton.Node
        {
            public Func<bool> Condition;
            private bool wasPressed;
            public override bool Check => Condition?.Invoke() ?? false;
            public override bool Pressed => Check && !wasPressed;
            public override bool Released => !Check && wasPressed;
            public override void Update() { wasPressed = Check; }
        }

        public enum States { Deploy, Attached, Reached, Breaked }

        private Vector2 direction;

        private bool vertical;

        private float distance;

        private float pullSpeed = 300f;

        private Player player;

        public States State;

        private Vector2 anchorPoint;

        private GrapplePoint attachedTarget;

        private Sprite hook;

        private float timer = 0f;

        private ConditionalGrabNode grabNode;

        private SoundSource sfx;

        public Grapple(Player player, bool vertical = false)
        {
            this.player = player;
            this.vertical = vertical;
            Position = player.Center;

            direction = vertical
                ? new Vector2(0f, -1f)
                : new Vector2(player.Facing == Facings.Left ? -1f : 1f, 0f);

            Collider = new Hitbox(4, 4, -2, -2);
            Add(hook = new Sprite(GFX.Game, "upgrades/GrappleHook/"));
            hook.AddLoop("hook", "hook", 0.08f);
            hook.Play("hook");
            hook.Visible = false;
            timer = Calc.Random.NextFloat();
            Depth = -1000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            State = States.Deploy;
            Add(sfx = new SoundSource());
            sfx.Play("event:/game/xaphan/grapple");
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime * 4f;

            switch (State)
            {
                case States.Deploy:
                    UpdateDeploy();
                    break;
                case States.Attached:
                    UpdateAttached();
                    break;
            }
        }

        private void UpdateDeploy()
        {
            float step = 480f * Engine.DeltaTime;
            Position += direction * step;
            distance += step;

            foreach (GrapplePoint target in Scene.Tracker.GetEntities<GrapplePoint>())
            {
                if (Collider.Collide(target.Collider))
                {
                    Attach(target);
                    return;
                }
            }

            if (distance >= 80f || CollideCheck<Solid, GrapplePoint>())
            {
                Add(new Coroutine(Fail()));
            }
        }

        private void Attach(GrapplePoint target)
        {
            sfx.Play("event:/game/xaphan/grapple_attached");
            State = States.Attached;
            attachedTarget = target;

            if (vertical)
            {
                player.Position += new Vector2((float)Math.Round(target.Center.X - player.Center.X), 0f);
                Position.X = target.Center.X;

                if (player.Y < target.Center.Y)
                {
                    Bottom = target.Top + (target.Collidable ? 2 : 1);
                }
                else
                {
                    Top = target.Bottom - (target.Collidable ? 2 : 1);
                }
            }
            else
            {
                player.Position += new Vector2(0f, (float)Math.Round(target.Center.Y - player.Center.Y));
                Position.Y = target.Center.Y;

                if (player.X < target.Center.X)
                {
                    Right = target.Left + (target.Collidable ? 2 : 1);
                }
                else
                {
                    Left = target.Right - (target.Collidable ? 2 : 1);
                }
            }

            anchorPoint = Position;
        }

        private void UpdateAttached()
        {
            if (Input.Jump.Pressed && SpaceJump.Active(player.SceneAs<Level>()) || !XaphanModule.ModSettings.UseBagItemSlot.Check)
            {
                Detach();
                return;
            }

            if (vertical)
            {
                player.MoveTowardsY(anchorPoint.Y, pullSpeed * Engine.DeltaTime);

                if (player.CollideCheck(this))
                {
                    Detach();
                }
            }
            else
            {
                player.MoveTowardsX(anchorPoint.X, pullSpeed * Engine.DeltaTime);

                if (Math.Abs(player.X - anchorPoint.X) <= 4f || (attachedTarget != null && player.CollideCheck(attachedTarget)))
                {
                    Add(new Coroutine(Reach()));
                }
            }
        }

        private IEnumerator Reach()
        {
            State = States.Reached;
            float stamina = player.Stamina;
            Vector2 position = player.Position;

            if (attachedTarget.Collidable)
            {
                grabNode = new ConditionalGrabNode { Condition = () => State == States.Reached && XaphanModule.ModSettings.UseBagItemSlot.Check };
                Input.Grab.Nodes.Add(grabNode);
                player.StateMachine.State = Player.StClimb;
                while (State == States.Reached && XaphanModule.ModSettings.UseBagItemSlot.Check && CollideCheck<Player>() && player.Position == position)
                {
                    player.Stamina = stamina;
                    yield return null;
                }
                State = States.Breaked;

                RemoveGrabNode();
                RemoveSelf();
            }
            else
            {
                Detach();
            }
        }

        private void Detach()
        {
            State = States.Breaked;

            sfx.Stop();
            attachedTarget?.SetSparks(false);

            if (vertical)
            {
                player.Speed.Y = direction.Y * pullSpeed * Engine.DeltaTime * 50f;
            }
            else
            {
                player.Speed.X = direction.X * pullSpeed * Engine.DeltaTime * 50f;
            }

            player.StateMachine.State = Player.StNormal;
            RemoveSelf();
        }

        private IEnumerator Fail()
        {
            State = States.Breaked;
            player.StateMachine.State = Player.StNormal;
            while (sfx.Playing)
            {
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            if (State != States.Breaked)
            {
                Vector2 origin = new Vector2(0f, hook.Height / 2f);
                Vector2 tip = Position + direction;
                MTexture currentFrame = hook.Texture;

                Draw.SineTextureH(
                    currentFrame,
                    player.Center,
                    origin,
                    new Vector2(Vector2.Distance(tip, player.Center) / 80f, 1f),
                    Calc.Angle(tip, player.Center) + (float)Math.PI,
                    Color.White * 1f,
                    SpriteEffects.None,
                    timer,
                    0.5f,
                    1,
                    0.08f
                );
            }
            base.Render();
        }

        private void RemoveGrabNode()
        {
            if (grabNode != null)
            {
                Input.Grab.Nodes.Remove(grabNode);
                grabNode = null;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RemoveGrabNode();
        }
    }
}