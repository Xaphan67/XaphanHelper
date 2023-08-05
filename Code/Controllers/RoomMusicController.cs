using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Controllers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/RoomMusicController")]
    class RoomMusicController : Entity
    {
        public RoomMusicController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
    }
}
