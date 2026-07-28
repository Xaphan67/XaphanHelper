using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/TeleportTrigger")]
    class TeleportTrigger : Trigger
    {
        private bool triggered;

        private readonly string room;

        private readonly int spawnPointX;

        private readonly int spawnPointY;

        private float timer;

        private string wipeType;

        public string flag;

        private bool inverted;

        private bool cameraOnPlayer;

        private int cameraX;

        private int cameraY;

        private Level level;

        public TeleportTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            room = data.Attr("room");
            spawnPointX = data.Int("spawnPointX");
            spawnPointY = data.Int("spawnPointY");
            timer = data.Float("timer");
            wipeType = data.Attr("wipeType");
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            cameraOnPlayer = data.Bool("cameraOnPlayer");
            cameraX = data.Int("cameraX");
            cameraY = data.Int("cameraY");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                RemoveSelf();
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (inverted ? (flag == "" || !SceneAs<Level>().Session.GetFlag(flag)) : (flag == "" || SceneAs<Level>().Session.GetFlag(flag)))
            {
                if (triggered)
                {
                    return;
                }
                triggered = true;
                if ((level = (Engine.Scene as Level)) != null)
                {
                    if (string.IsNullOrEmpty(room))
                    {
                        level.Add(new MiniTextbox("XaphanHelper_room_name_empty"));
                        return;
                    }

                    if (level.Session.MapData.Get(room) == null)
                    {
                        level.Add(new MiniTextbox("XaphanHelper_room_not_exist"));
                        return;
                    }
                }
                Scene.Add(new TeleportCutscene(player, room, new Vector2(spawnPointX, spawnPointY), cameraX, cameraY, cameraOnPlayer, timer, wipeType));
            }
        }
    }
}
