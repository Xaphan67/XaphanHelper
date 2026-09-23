using System;
using System.Collections;
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

        public enum States { Deploy, Attached, WallGrab, Breaked, Failed }

        // --- Réglages exposés pour pouvoir équilibrer le feeling sans replonger dans la physique ---
        public float Gravity = 900f;                // "pesanteur" appliquée au pendule
        public float ReelSpeed = 150f;              // vitesse à laquelle Haut/Bas change la longueur de corde (px/s)
        public float PumpForce = 200f;              // poussée tangentielle donnée par Gauche/Droite
        public float MinLength = 24f;               // portée minimale (1 tile = 8px)
        public float MaxLength = 64f;               // portée maximale (1 tile = 8px)
        public float AngularDamping = 0.05f;        // légère friction pour éviter une accumulation infinie d'énergie
        public float MaxAngularSpeed = 6f;          // limite dure de vitesse angulaire (rad/s)
        public float MaxLaunchSpeed = 320f;         // vitesse max autorisée au relâchement (0 ou moins = pas de limite)
        public float CeilingBounceFactor = 0.8f;    // rebond contre un plafond (1 = rebond parfaitement élastique, <1 = perd un peu d'énergie)

        private float length;  // longueur actuelle de la corde
        private float theta;   // angle par rapport à la verticale (0 = joueur directement sous l'ancre)
        private float omega;   // vitesse angulaire (rad/s)

        private Vector2 direction;

        private float distance;

        private Player player;

        public States State;

        private Vector2 anchorPoint;

        private GrapplePoint attachedTarget;

        private Sprite hook;

        private MTexture head;

        private float timer = 0f;

        private SoundSource sfx;

        private ConditionalGrabNode grabNode;

        public Grapple(Player player)
        {
            this.player = player;
            Position = player.Center;
            direction = Input.GetAimVector(player.Facing);
            Collider = new Hitbox(4, 4, -2, -2);
            Add(hook = new Sprite(GFX.Game, "upgrades/GrappleHook/"));
            hook.AddLoop("hook", "hook", 0.08f);
            hook.Play("hook");
            hook.Visible = false;
            head = GFX.Game["upgrades/GrappleHook/head00"];
            head.ScaleFix = 1f;
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
                    UpdateSwing();
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
            if (distance >= MaxLength || CollideCheck<Solid, GrapplePoint>())
            {
                Add(new Coroutine(Fail()));
            }
        }

        private void Attach(GrapplePoint target)
        {
            sfx.Play("event:/game/xaphan/grapple_attached");
            State = States.Attached;
            attachedTarget = target;
            anchorPoint = target.Center;
            Position = anchorPoint;
            Vector2 offset = player.Center - anchorPoint;
            length = Calc.Clamp(offset.Length(), MinLength, MaxLength);
            theta = (float)Math.Atan2(offset.X, offset.Y);
            Vector2 tangent = new Vector2((float)Math.Cos(theta), -(float)Math.Sin(theta));
            omega = Vector2.Dot(player.Speed, tangent) / length;
            player.StateMachine.State = XaphanModule.StGrapple;
        }

        private void UpdateSwing()
        {
            if (!XaphanModule.ModSettings.UseBagItemSlot.Check)
            {
                Detach();
                return;
            }
            float dt = Engine.DeltaTime;
            float previousLength = length;
            length = Calc.Clamp(length + Input.MoveY.Value * ReelSpeed * dt, MinLength, MaxLength);
            if (length != previousLength && length > 0f)
            {
                omega *= (previousLength * previousLength) / (length * length);
            }
            float alpha = -(Gravity / length) * (float)Math.Sin(theta);
            alpha += Input.MoveX.Value * PumpForce / length;
            omega += alpha * dt;
            omega *= 1f - Calc.Clamp(AngularDamping * dt, 0f, 1f);
            omega = Calc.Clamp(omega, -MaxAngularSpeed, MaxAngularSpeed);
            theta += omega * dt;
            Vector2 offset = new Vector2(length * (float)Math.Sin(theta), length * (float)Math.Cos(theta));
            Vector2 targetPosition = anchorPoint + offset;
            Vector2 radialDir = new Vector2((float)Math.Sin(theta), (float)Math.Cos(theta));
            Vector2 tangentDir = new Vector2((float)Math.Cos(theta), -(float)Math.Sin(theta));
            float lengthRate = (length - previousLength) / dt;
            Vector2 velocity = tangentDir * (length * omega) + radialDir * lengthRate;

            if (MaxLaunchSpeed > 0f && velocity.Length() > MaxLaunchSpeed)
            {
                velocity.Normalize();
                velocity *= MaxLaunchSpeed;
            }
            player.Speed = velocity;
            player.MoveH(targetPosition.X - player.Position.X, OnCollideH);
            if (State != States.Attached)
            {
                return;
            }
            player.MoveV(targetPosition.Y - player.Position.Y, OnCollideV);
            if (player.OnGround())
            {
                Detach();
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (State != States.Attached)
            {
                return;
            }
            player.Facing = data.Direction.X > 0f ? Facings.Right : Facings.Left;
            Add(new Coroutine(AttachToWall()));
        }

        private void OnCollideV(CollisionData data)
        {
            if (State != States.Attached)
            {
                return;
            }
            if (data.Direction.Y < 0f)
            {
                omega = Calc.Clamp(-omega * CeilingBounceFactor, -MaxAngularSpeed, MaxAngularSpeed);
                Vector2 tangentDir = new Vector2((float)Math.Cos(theta), -(float)Math.Sin(theta));
                player.Speed = tangentDir * (length * omega);
            }
        }

        private IEnumerator AttachToWall()
        {
            State = States.WallGrab;
            float stamina = player.Stamina;
            if (attachedTarget.Collidable)
            {
                grabNode = new ConditionalGrabNode { Condition = () => State == States.WallGrab && XaphanModule.ModSettings.UseBagItemSlot.Check };
                Input.Grab.Nodes.Add(grabNode);
                player.StateMachine.State = Player.StClimb;
                yield return null;
                Vector2 position = player.Position;
                while (State == States.WallGrab && XaphanModule.ModSettings.UseBagItemSlot.Check && player.Position == position)
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

        private void Detach()
        {
            State = States.Breaked;
            sfx.Stop();
            attachedTarget?.SetSparks(false);
            player.StateMachine.State = Player.StNormal;
            RemoveSelf();
        }

        private IEnumerator Fail()
        {
            State = States.Failed;
            player.StateMachine.State = Player.StNormal;
            while (sfx.Playing)
            {
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            if (State != States.Breaked && State != States.Failed)
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
                bool rotateSrpite = State == States.Attached || State == States.WallGrab;
                Vector2 aim = rotateSrpite ? player.Center - Position : direction;
                float headRotation = (float)Math.Atan2(aim.Y, aim.X) + (rotateSrpite ? (float)Math.PI : 0f);
                head.DrawCentered(Position, Color.White, Vector2.One, headRotation);
            }
            base.Render();
        }
    }
}