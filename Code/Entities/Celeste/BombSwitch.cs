using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/BombSwitch")]
    public class BombSwitch : Entity
    {
        private bool bombInside;

        private bool triggered;

        private string flag;

        private bool registerInSaveData;

        private string sprite;

        private Sprite switchSprite;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : ""));
        }

        public BombSwitch(EntityData data, Vector2 position) : base(data.Position + position)
        {
            flag = data.Attr("flag");
            registerInSaveData = data.Bool("registerInSaveData");
            sprite = data.Attr("sprite", "objects/XaphanHelper/BombSwitch");
            Add(new BombCollider(OnBomb, new Circle(8f, 8f, 8f)));
            Add(switchSprite = new Sprite(GFX.Game, sprite + "/"));
            switchSprite.AddLoop("idle", "idle", 0);
            switchSprite.Play("idle");
            Depth = 8999;
        }


        private void OnBomb(Bomb bomb)
        {
            if (!bomb.Hold.IsHeld && !triggered && !bombInside && bomb.AllowPushing)
            {
                triggered = true;
                Add(new Coroutine(MoveBomb(bomb)));
            }
        }

        private IEnumerator MoveBomb(Bomb bomb)
        {
            float timer = 0.15f;
            bomb.sloted = true;
            bomb.Depth = Depth - 1;
            while ((Vector2.Distance(Center + new Vector2(8, 12), bomb.Position) > 3f) && bomb != null && !bombInside && timer > 0f)
            {
                Vector2 vector = Calc.Approach(bomb.Position, Center + new Vector2(8, 12), 250f * Engine.DeltaTime);
                bomb.MoveToX(vector.X);
                bomb.MoveToY(vector.Y);
                timer -= Engine.DeltaTime;
                yield return null;
            }
            if (!bombInside)
            {
                Add(new Coroutine(SlotBomb(bomb)));
            }
        }

        private IEnumerator SlotBomb(Bomb bomb = null)
        {
            bombInside = true;
            if (bomb != null)
            {
                bomb.Position = Center + new Vector2(8, 12);
                while (!bomb.explode && !bomb.Hold.IsHeld)
                {
                    yield return null;
                }
                if (!bomb.Hold.IsHeld)
                {
                    SceneAs<Level>().Session.SetFlag(flag, !SceneAs<Level>().Session.GetFlag(flag));
                    if (registerInSaveData)
                    {
                        string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                        int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                        else
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                        if (XaphanModule.PlayerHasGolden)
                        {
                            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry"))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                            }
                            else
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag + "_GoldenStrawberry");
                            }
                        }
                    }
                }
                else
                {
                    bomb.sloted = false;
                    bomb.Depth = 0;
                }
            }
            bombInside = triggered = false;
        }
    }
}
