using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [CustomEntity("XaphanHelper/Ch3WaterWheelController")]
    class Ch3WaterWheelController : Entity
    {
        private string flag;

        public Ch3WaterWheelController(EntityData data, Vector2 position) : base(data.Position + position)
        {
            flag = data.Attr("flag");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = SceneAs<Level>().Session;
            if (session.Level == "M-06" && session.GetFlag(flag) && !session.GetFlag("M-06_Trigwall_1") && !session.GetFlag("Ch3_M-06_Trigwall_1"))
            {
                Add(new Coroutine(OpenWallsRoutine()));
            }
        }

        public IEnumerator OpenWallsRoutine()
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                yield return 1f;
                SceneAs<Level>().Session.SetFlag("M-06_Trigwall_1", true);
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch3_M-06_Trigwall_1");
            }
            SceneAs<Level>().CanRetry = true;
        }
    }
}
