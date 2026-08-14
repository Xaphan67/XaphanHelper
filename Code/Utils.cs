using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

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

        public static bool HasEntity(this LevelData levelData, string entityName)
        {
            return levelData.GetEntityData(entityName) != null;
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

        public static char GetLetter(char c)
        {
            char result = ' ';
            if (char.IsLetter(c))
            {
                bool upper = char.IsUpper(c);
                char baseChar = upper ? 'A' : 'a';
                int position = (c - baseChar - 10) % 26;
                if (position < 0)
                {
                    position += 26;
                }
                result = (char)(baseChar + position);
            }
            else
            {
                result = c;
            }
            return result;
        }

        public static void RecalculateTextPositions(FancyText.Text text)
        {
            PixelFontSize size = text.Font.Get(text.BaseSize);
            float currentPosition = 0f;
            List<FancyText.Char> currentLineChars = new List<FancyText.Char>();

            void FlushLine()
            {
                float lineWidth = currentPosition;
                foreach (FancyText.Char c in currentLineChars)
                {
                    c.LineWidth = lineWidth;
                }
                currentLineChars.Clear();
                currentPosition = 0f;
            }

            for (int i = 0; i < text.Nodes.Count; i++)
            {
                FancyText.Node node = text.Nodes[i];

                if (node is FancyText.NewLine || node is FancyText.NewPage)
                {
                    FlushLine();
                    continue;
                }

                if (node is FancyText.Char c)
                {
                    var fontChar = size.Get(c.Character);
                    if (fontChar == null)
                    {
                        continue;
                    }
                    c.Position = currentPosition;
                    currentLineChars.Add(c);
                    currentPosition += fontChar.XAdvance * c.Scale;
                    if (i + 1 < text.Nodes.Count && text.Nodes[i + 1] is FancyText.Char next)
                    {
                        if (fontChar.Kerning.TryGetValue(next.Character, out int kerning))
                        {
                            currentPosition += kerning * c.Scale;
                        }
                    }
                }
            }
            FlushLine();
        }
    }
}
