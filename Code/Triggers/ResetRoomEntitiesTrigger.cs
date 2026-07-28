using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/ResetRoomEntitiesTrigger")]
    class ResetRoomEntitiesTrigger : Trigger
    {
        public ResetRoomEntitiesTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Tag = Tags.TransitionUpdate;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            HashSet<EntityID> ToRemove = new();
            foreach (EntityID entityID in SceneAs<Level>().Session.DoNotLoad)
            {
                if (entityID.Level == SceneAs<Level>().Session.Level)
                {
                    ToRemove.Add(entityID);
                }
            }
            foreach (EntityID entityID in ToRemove)
            {
                SceneAs<Level>().Session.DoNotLoad.Remove(entityID);
            }
        }
    }
}
