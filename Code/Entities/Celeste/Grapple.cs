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

            public override void Update()
            {
                wasPressed = Check;
            }
        }

        public enum States
        {
            Deploy,
            Attached,
            Reached
        }

        private int direction;

        private float distance;

        private float pullSpeed = 300f;

        private Player player;

        private States State;

        private Vector2 anchorPoint;

        private GrapplePoint attachedTarget;

        private MTexture lineSprite;

        private float timer = 0f;

        private ConditionalGrabNode grabNode;

        private SoundSource sfx;

        public Grapple(Player player)
        {
            this.player = player;
            Position = player.Center;
            direction = player.Facing == Facings.Left ? -1 : 1;
            Collider = new Hitbox(4, 4, -2, -2);
            lineSprite = GFX.Game["util/XaphanHelper/grappleBeam"];
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
            Position.X += direction * step;
            distance += step;

            foreach (GrapplePoint target in Scene.Tracker.GetEntities<GrapplePoint>())
            {
                if (Collider.Collide(target.Collider))
                {
                    sfx.Play("event:/game/xaphan/grapple_attached");
                    State = States.Attached;
                    attachedTarget = target;
                    attachedTarget.SetSparks(true);
                    player.Position += new Vector2(0f, (float)Math.Round(target.Center.Y - player.Center.Y));
                    Position.Y = target.Center.Y;
                    if (player.X < attachedTarget.Center.X)
                    {
                        Right = attachedTarget.Left + (attachedTarget.Collidable ? 0 : 1);
                    }
                    else
                    {
                        Left = attachedTarget.Right - (attachedTarget.Collidable ? 0 : 1);
                    }
                    anchorPoint = Position;
                    return;
                }
            }

            if (distance >= 80f || CollideCheck<Solid, GrapplePoint>())
            {
                Fail();
            }
        }

        private void UpdateAttached()
        {
            
            if (Input.Jump.Pressed && SpaceJump.Active(player.SceneAs<Level>()) || !XaphanModule.ModSettings.UseBagItemSlot.Check)
            {
                Detach();
                return;
            }

            player.MoveTowardsX(anchorPoint.X, pullSpeed * Engine.DeltaTime);

            if (Math.Abs(player.X - anchorPoint.X) <= 2f || (attachedTarget != null && player.CollideCheck(attachedTarget)))
            {
                Add(new Coroutine(Reach()));
            }
        }

        private IEnumerator Reach()
        {
            sfx.Stop();
            State = States.Reached;
            attachedTarget.SetSparks(false);

            if (attachedTarget.Collidable)
            {
                grabNode = new ConditionalGrabNode { Condition = () => State == States.Reached && XaphanModule.ModSettings.UseBagItemSlot.Check };
                Input.Grab.Nodes.Add(grabNode);
                player.StateMachine.State = Player.StClimb;
                while (State == States.Reached && XaphanModule.ModSettings.UseBagItemSlot.Check)
                {
                    yield return null;
                }

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
            sfx.Stop();
            attachedTarget.SetSparks(false);
            player.Speed.X = direction * pullSpeed * Engine.DeltaTime * 50f;
            player.StateMachine.State = Player.StNormal;
            RemoveSelf();
        }

        private void Fail()
        {
            sfx.Stop();
            player.StateMachine.State = Player.StNormal;
            RemoveSelf();
        }

        public override void Render()
        {
            if (State != States.Reached)
            {
                Vector2 origin = new Vector2(0f, lineSprite.Height / 2f);
                Vector2 tip = Position + Vector2.UnitX * (player.Facing == Facings.Left ? -1 : 1);

                Draw.SineTextureH(
                    lineSprite,
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
