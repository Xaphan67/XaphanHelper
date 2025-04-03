using System;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/WaterWheel")]
    public class WaterWheel : Entity
    {
        Sprite wheelSprite;

        MTexture middle;

        string flag;

        string directory;

        bool turnLeft;

        float rotationDegree;

        float acceleration;

        public WaterWheel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Circle(12f);
            flag = data.Attr("flag");
            directory = data.Attr("directory", "objects/XaphanHelper/WaterWheel");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/WaterWheel";
            }
            Add(wheelSprite = new Sprite(GFX.Game, directory + "/"));
            wheelSprite.Origin = Vector2.One * 12f;
            wheelSprite.AddLoop("wheel", "wheel", 0f, 0);
            wheelSprite.Play("wheel");
            middle = GFX.Game[directory + "/middle"];
        }

        public override void Update()
        {
            base.Update();
            bool CollideWaterfallRight = false;
            bool CollideWaterfallLeft = false;
            foreach (Waterfall.WaterfallSection section in SceneAs<Level>().Tracker.GetEntities<Waterfall.WaterfallSection>())
            {
                if (CollideCheck(section, Position + Vector2.UnitY * -1))
                {
                    if (section.Right < (Left + Collider.Width / 2))
                    {
                        CollideWaterfallLeft = true;
                    }
                    else if(section.Left > (Left + Collider.Width / 2))
                    {
                        CollideWaterfallRight = true;
                    }
                }
            }
            if ((CollideWaterfallRight && !CollideWaterfallLeft) || (!CollideWaterfallRight && CollideWaterfallLeft))
            {
                wheelSprite.Rotation = (float)(Math.PI / 180) * rotationDegree;
                acceleration = Calc.Approach(acceleration, 175f, Engine.DeltaTime * 50f);
                if (CollideWaterfallRight)
                {
                    rotationDegree += Engine.DeltaTime * acceleration;
                    if (rotationDegree > 360)
                    {
                        rotationDegree = 0;
                    }
                    turnLeft = false;
                }
                else
                {
                    rotationDegree -= Engine.DeltaTime * acceleration;
                    if (rotationDegree < -360)
                    {
                        rotationDegree = 0;
                    }
                    turnLeft = true;
                }
                if (!string.IsNullOrEmpty(flag) && acceleration > 100f)
                {
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
            }
            else
            {
                wheelSprite.Rotation = (float)(Math.PI / 180) * rotationDegree;
                acceleration = Calc.Approach(acceleration, 0f, Engine.DeltaTime * 75f);
                if (turnLeft)
                {
                    rotationDegree -= Engine.DeltaTime * acceleration;
                    if (rotationDegree < -360)
                    {
                        rotationDegree = 0;
                    }
                }
                else
                {
                    rotationDegree += Engine.DeltaTime * acceleration;
                    if (rotationDegree > 360)
                    {
                        rotationDegree = 0;
                    }
                }
                if (!string.IsNullOrEmpty(flag) && acceleration < 100f)
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
            }
        }

        public override void Render()
        {
            base.Render();
            wheelSprite.DrawOutline();
            wheelSprite.Render();
            middle.DrawCentered(Position);
        }
    }
}
