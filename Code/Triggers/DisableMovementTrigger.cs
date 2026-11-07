using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/DisableMovementTrigger")]
    class DisableMovementTrigger : Trigger
    {
        private enum EndFacings
        {
            Left,
            Right
        }

        private string Flag;

        private float Time;

        private bool WalkCenter;

        private EndFacings EndFacing;

        private Coroutine WaitRoutine = new();

        public DisableMovementTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag");
            Time = data.Float("time");
            WalkCenter = data.Bool("walkCenter");
            EndFacing = data.Enum<EndFacings>("endFacing");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag))
            {
                RemoveSelf();
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!WaitRoutine.Active)
            {
                Add(WaitRoutine = new Coroutine(Wait(player)));
            }
        }

        private IEnumerator Wait(Player player)
        {
            while (!SceneAs<Level>().Session.GetFlag(Flag))
            {
                yield return null;
            }
            player.StateMachine.State = Player.StDummy;
            XaphanModule.IgnoreShakeSettings = true;
            SceneAs<Level>().CanRetry = false;
            Facings currentFacing = player.Facing;
            if (WalkCenter)
            {
                yield return player.DummyWalkTo(currentFacing == Facings.Left ? Center.X - 4f : Center.X + 4f);
            }
            if (EndFacing == EndFacings.Left)
            {
                player.Facing = Facings.Left;
            }
            else
            {
                player.Facing = Facings.Right;
            }
            while (Time > 0)
            {
                Time -= Engine.DeltaTime;
                yield return null;
            }
            XaphanModule.IgnoreShakeSettings = false;
            player.StateMachine.State = Player.StNormal;
            MapDisplay.UpdateTiles(this);
            SceneAs<Level>().CanRetry = true;
            RemoveSelf();
        }
    }
}
