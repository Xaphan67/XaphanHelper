using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [CustomEntity("XaphanHelper/Ch3WaterWheelController")]
    class Ch3WaterWheelController : Entity
    {
        public Ch3WaterWheelController(EntityData data, Vector2 position) : base(data.Position + position)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = SceneAs<Level>().Session;
            if (
                (session.Level == "M-06" && session.GetFlag("M-05_Trigwall_2") && !session.GetFlag("M-06_Trigwall_1") && !session.GetFlag("Ch3_M-06_Trigwall_1")) ||
                (session.Level == "M-53-B" && session.GetFlag("M-40_Trigwall_1") && session.GetFlag("M-48_Trigwall_1") && !session.GetFlag("M-53_Trigwall_1") && !session.GetFlag("Ch3_M-53_Trigwall_1"))
                )
            {
                Add(new Coroutine(OpenWallsRoutine(session.Level)));
            }
        }

        public IEnumerator OpenWallsRoutine(string room)
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                yield return 1f;
                if (room.Contains("-B"))
                {
                    room = room.Substring(0, 4);
                }
                SceneAs<Level>().Session.SetFlag(room + "_Trigwall_1", true);
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch3_" + room + "_Trigwall_1");
            }
            SceneAs<Level>().CanRetry = true;
        }
    }
}
