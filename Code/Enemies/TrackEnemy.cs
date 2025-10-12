using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    public class TrackEnemy : Enemy
    {
        public bool Up = true;

        public float PauseTimer;

        public bool Moving = true;

        public float Angle;

        public Vector2[] Nodes;

        public int CurrentEndNode;

        public Vector2 Offset;

        private Vector2 StartPosition;

        private float MoveTime;

        public float WaitTime;

        public Vector2 Start { get; private set; }

        public Vector2 End { get; private set; }

        public float Percent { get; private set; }

        public TrackEnemy(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tag = Tags.TransitionUpdate;
            StartPosition = Start = data.Position + offset;
            End = data.Nodes[0] + offset;
            Offset = offset;
            Nodes = data.Nodes;
            CurrentEndNode = 0;
            Angle = (Start - End).Angle();
            MoveTime = data.Float("moveTime", 0.5f);
            WaitTime = data.Float("waitTime", 1f);
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Position = Vector2.Lerp(Start, End, Ease.SineInOut(Percent));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            OnTrackStart();
        }

        public override void Update()
        {
            base.Update();
            if (!Moving || Freezed)
            {
                return;
            }
            if (PauseTimer > 0f)
            {
                PauseTimer -= Engine.DeltaTime;
                if (PauseTimer <= 0f)
                {
                    OnTrackStart();
                }
                return;
            }
            Percent = Calc.Approach(Percent, Up ? 1 : 0, Engine.DeltaTime / MoveTime);
            UpdatePosition();
            if ((Up && Percent == 1f) || (!Up && Percent == 0f))
            {
                if (Up)
                {
                    if (Nodes.Length > CurrentEndNode + 1)
                    {
                        Percent = 0f;
                        Start = Nodes[CurrentEndNode] + Offset;
                        End = Nodes[CurrentEndNode + 1] + Offset;
                        OnTrackNode();
                        CurrentEndNode++;
                    }
                    else
                    {
                        Up = !Up;
                        CurrentEndNode--;
                    }
                }
                else
                {
                    if (CurrentEndNode - 1 >= 0)
                    {
                        Percent = 1f;
                        Start = Nodes[CurrentEndNode - 1] + Offset;
                        End = Nodes[CurrentEndNode] + Offset;
                        OnTrackNode();
                        CurrentEndNode--;
                    }
                    else if (CurrentEndNode == 0)
                    {
                        Percent = 1f;
                        Start = StartPosition;
                        End = Nodes[0] + Offset;
                        OnTrackNode();
                        CurrentEndNode--;
                    }
                    else
                    {
                        Up = !Up;
                        CurrentEndNode++;
                    }
                }
                PauseTimer = WaitTime;
                if (CurrentEndNode == Nodes.Length - 1)
                {
                    OnTrackEnd();
                }
            }
        }

        public virtual void OnTrackStart()
        {
        }

        public virtual void OnTrackEnd()
        {
        }

        public virtual void OnTrackNode()
        {
        }
    }
}
