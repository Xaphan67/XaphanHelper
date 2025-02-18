using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.XaphanHelper
{
    public static class Utils
    {
        public static EntityData GetEntityData(this MapData mapData, string entityName)
        {
            foreach (LevelData levelData in mapData.Levels)
            {
                if (levelData.GetEntityData(entityName) is EntityData entityData)
                {
                    return entityData;
                }
            }

            return null;
        }

        public static List<EntityData> GetEntityDatas(this MapData mapData, string entityName)
        {
            List<EntityData> entityDatas = new();
            foreach (LevelData levelData in mapData.Levels)
            {
                if (levelData.GetEntityDatas(entityName) is List<EntityData> entityDataList)
                {
                    entityDatas.AddRange(entityDataList);
                }
            }

            return entityDatas;
        }

        public static bool HasEntity(this MapData mapData, string entityName)
        {
            return mapData.GetEntityData(entityName) != null;
        }

        public static EntityData GetEntityData(this LevelData levelData, string entityName)
        {
            foreach (EntityData entity in levelData.Entities)
            {
                if (entity.Name == entityName)
                {
                    return entity;
                }
            }

            return null;
        }

        public static List<EntityData> GetEntityDatas(this LevelData levelData, string entityName)
        {
            List<EntityData> entityDatas = new();
            foreach (EntityData entity in levelData.Entities)
            {
                if (entity.Name == entityName)
                {
                    entityDatas.Add(entity);
                }
            }

            return entityDatas;
        }

        public static bool HasEntity(this LevelData levelData, string entityName)
        {
            return levelData.GetEntityData(entityName) != null;
        }

        public static Color GetGradientColor(Color firstColor, Color lastColor, float percent)
        {
            float interval_R = (lastColor.R - firstColor.R) / 100f;
            float interval_G = (lastColor.G - firstColor.G) / 100f;
            float interval_B = (lastColor.B - firstColor.B) / 100f;

            float current_R = firstColor.R;
            float current_G = firstColor.G;
            float current_B = firstColor.B;

            for (int i = 1; i <= percent; i++)
            {
                current_R += interval_R;
                current_G += interval_G;
                current_B += interval_B;
            }
            Color color = new((int)current_R, (int)current_G, (int)current_B);
            return color;
        }
    }
}
