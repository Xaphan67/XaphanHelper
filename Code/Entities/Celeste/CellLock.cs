using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CellLock")]
    class CellLock : Entity
    {
        private TalkComponent talk;

        private string sprite;

        private string sound;

        private string slotSound;

        private string color;

        private string flag;

        private string type;

        private bool instant;

        private bool registerInSaveData;

        private bool cellInside;

        private bool keepCell;

        private bool onlyCellVisible;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.LevelSet;
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        private bool triggered;

        private Sprite bgSprite;

        private Sprite colorSprite;

        private Sprite cellSprite;

        private Sprite leverSprite;

        private Sprite lightningSprite;

        public CellLock(EntityData data, Vector2 position) : base(data.Position + position)
        {
            sprite = data.Attr("sprite", "objects/XaphanHelper/CellLock");
            color = data.Attr("color", "blue");
            type = data.Attr("type", "normal");
            flag = data.Attr("flag");
            instant = data.Bool("instant");
            sound = data.Attr("sound");
            slotSound = data.Attr("slotSound");
            registerInSaveData = data.Bool("registerInSaveData");
            cellInside = data.Bool("cellInside");
            keepCell = data.Bool("keepCell");
            onlyCellVisible = data.Bool("onlyCellVisible");
            if (string.IsNullOrEmpty(sprite))
            {
                sprite = "objects/XaphanHelper/CellLock";
            }
            if (!onlyCellVisible)
            {
                Add(bgSprite = new Sprite(GFX.Game, sprite + "/"));
                bgSprite.AddLoop("bgSprite", type, 0.08f);
                bgSprite.CenterOrigin();
                bgSprite.Play("bgSprite");
                Add(colorSprite = new Sprite(GFX.Game, sprite + "/"));
                colorSprite.AddLoop("colorSprite", color, 0.08f);
                colorSprite.CenterOrigin();
                colorSprite.Play("colorSprite");
            }
            Add(new CellCollider(OnCell, new Circle(17f, 0f, 5f)));
            Add(new CellCollider(OnSlot, new Hitbox(8f, 8f, -4f, 1f)));
            Add(cellSprite = new Sprite(GFX.Game, sprite + "/"));
            cellSprite.Justify = new Vector2(0.5f, 0.2f);
            cellSprite.AddLoop("cellSprite", "bgCell", 0.08f);
            cellSprite.CenterOrigin();
            Add(lightningSprite = new Sprite(GFX.Game, sprite + "/"));
            lightningSprite.Justify = new Vector2(0.5f, 0.2f);
            lightningSprite.AddLoop("lightningSprite", "lightning", 0.06f);
            lightningSprite.CenterOrigin();
            Add(leverSprite = new Sprite(GFX.Game, sprite + "/"));
            leverSprite.Justify = new Vector2(0.5f, 0.2f);
            leverSprite.AddLoop("leverIdle", "lever", 0.08f, 0);
            leverSprite.AddLoop("leverActive", "lever", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0);
            leverSprite.CenterOrigin();
            Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (keepCell ? (cellInside || SceneAs<Level>().Session.GetFlag(flag + "_sloted")) : cellInside)
            {
                SceneAs<Level>().Session.SetFlag(flag + "_sloted", true);
                cellSprite.Play("cellSprite");
                leverSprite.Play("leverIdle");
                Add(new VertexLight(cellSprite.Center - Vector2.UnitY, Color.White, 1f, 32, 64));
                if (!instant)
                {
                    Add(talk = new TalkComponent(new Rectangle(-20, 12, 40, 8), new Vector2(-0.5f, -20f), Interact));
                    talk.PlayerMustBeFacing = false;
                }
                else
                {
                    SlotCell();
                }
            }
            bool haveGolden = false;
            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null)
                {
                    haveGolden = true;
                    break;
                }
            }
            if ((keepCell || cellInside) && ((!haveGolden && (SceneAs<Level>().Session.GetFlag(flag) || FlagRegiseredInSaveData())) || (haveGolden && SceneAs<Level>().Session.GetFlag(flag))))
            {
                if (!instant)
                {
                    Remove(talk);
                }
                cellSprite.Play("cellSprite");
                lightningSprite.Play("lightningSprite");
                leverSprite.Play("leverIdle");
                Add(new VertexLight(lightningSprite.Center - Vector2.UnitY, Color.White, 1f, 32, 64));
            }
            else if (!cellInside && !FlagRegiseredInSaveData())
            {
                if (SceneAs<Level>().Session.GetFlag(flag))
                {
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
                if (SceneAs<Level>().Session.GetFlag(flag + "_sloted"))
                {
                    SceneAs<Level>().Session.SetFlag(flag + "_sloted", false);
                }
            }
        }

        private void onLastFrame(string s)
        {
            leverSprite.Stop();
            SceneAs<Level>().Session.SetFlag(flag, true);
            lightningSprite.Play("lightningSprite");
            Add(new VertexLight(lightningSprite.Center, Color.White, 1f, 32, 64));
            if (registerInSaveData)
            {
                string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                }
                if (XaphanModule.PlayerHasGolden)
                {
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                    }
                }
            }
        }

        private void Interact(Player player)
        {
            Add(new Coroutine(InteractRoutine(player)));
        }

        private IEnumerator InteractRoutine(Player player)
        {
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X, false, 1f, true);
            if (Math.Abs(X - player.X) > 4f || player.Dead || !player.OnGround())
            {
                if (!player.Dead)
                {
                    player.StateMachine.State = 0;
                }
                yield break;
            }
            Audio.Play((sound == "" ? "event:/game/05_mirror_temple/button_activate" : sound));
            leverSprite.Play("leverActive", restart: true);
            leverSprite.OnLastFrame = onLastFrame;
            while (SceneAs<Level>().Session.GetFlag(flag) == false)
            {
                yield return null;
            }
            player.StateMachine.State = 0;
            Remove(talk);
        }

        private void OnCell(Cell cell)
        {
            if (!cell.Hold.IsHeld && !SceneAs<Level>().Session.GetFlag(flag + "_sloted") && !triggered && !cellInside)
            {
                triggered = true;
                Add(new Coroutine(MoveCell(cell)));
            }
        }

        private void OnSlot(Cell cell)
        {
            if (!SceneAs<Level>().Session.GetFlag(flag + "_sloted") && !cellInside)
            {
                SlotCell(cell);
            }
        }

        private IEnumerator MoveCell(Cell cell)
        {
            float timer = 0.15f;
            while ((Vector2.Distance(Center + new Vector2(0, 12), cell.Position) > 3f) && cell != null && !cellInside && timer > 0f)
            {
                Vector2 vector = Calc.Approach(cell.Position, Center + new Vector2(0, 12), 250f * Engine.DeltaTime);
                cell.MoveToX(vector.X);
                cell.MoveToY(vector.Y);
                timer -= Engine.DeltaTime;
                yield return null;
            }
            cell.Position = Center + new Vector2(0, 12);
            if (!cellInside)
            {
                SlotCell(cell);
            }
        }

        private void SlotCell(Cell cell = null)
        {
            cellInside = true;
            if (keepCell)
            {
                SceneAs<Level>().Session.SetFlag(flag + "_sloted", true);
            }
            if (cell != null)
            {
                cell.RemoveSelf();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play(string.IsNullOrEmpty(slotSound) ? "event:/game/05_mirror_temple/button_activate" : slotSound, Position);
            }
            cellSprite.Play("cellSprite");
            Add(new VertexLight(cellSprite.Center - Vector2.UnitY, Color.White, 1f, 32, 64));
            leverSprite.Play("leverIdle");
            if (!instant)
            {
                Add(talk = new TalkComponent(new Rectangle(-20, 12, 40, 8), new Vector2(-0.5f, -20f), Interact));
                talk.PlayerMustBeFacing = false;
            }
            else
            {
                SceneAs<Level>().Session.SetFlag(flag, true);
                lightningSprite.Play("lightningSprite");
                Add(new VertexLight(lightningSprite.Center - Vector2.UnitY, Color.White, 1f, 32, 64));
                if (registerInSaveData)
                {
                    string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                    int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                    if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                    if (XaphanModule.PlayerHasGolden)
                    {
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                        }
                    }
                }
            }
        }
    }
}
