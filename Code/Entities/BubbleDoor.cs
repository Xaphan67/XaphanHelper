using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/BubbleDoor")]
    class BubbleDoor : Solid
    {
        private EntityID ID;

        private string side;

        private string directory;

        public string color;

        private string origColor;

        private string openSound;

        private string closeSound;

        private string unlockSound;

        private string lockSound;

        private bool open;

        public bool isActive;

        public bool locked;

        private bool StateChanged = true;

        public string[] flags;

        public string forceLockedFlag;

        public string forceInactiveFlag;

        public bool OnlyNeedOneFlag;

        private Sprite doorCap;

        private Sprite doorStruct;

        public bool keepOpen;

        public BubbleDoor(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            Tag = Tags.TransitionUpdate;
            ID = eid;
            side = data.Attr("side", "Left");
            directory = data.Attr("directory", "objects/XaphanHelper/BubbleDoor");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/BubbleDoor";
            }
            color = origColor = data.Attr("color", "Blue");
            if (!string.IsNullOrEmpty(data.Attr("flag")))
            {
                flags = data.Attr("flag").Split(',');
            }
            else
            {
                flags = data.Attr("flags").Split(',');
            }
            forceLockedFlag = data.Attr("forceLockedFlag");
            forceLockedFlag = string.IsNullOrEmpty(forceLockedFlag) ? null : forceLockedFlag;
            forceInactiveFlag = data.Attr("forceInactiveFlag");
            OnlyNeedOneFlag = data.Bool("onlyNeedOneFlag", false);
            openSound = data.Attr("openSound", "event:/game/05_mirror_temple/gate_main_open");
            closeSound = data.Attr("closeSound", "event:/game/05_mirror_temple/gate_main_close");
            unlockSound = data.Attr("unlockSound", "");
            lockSound = data.Attr("lockSound", "");
            Add(doorStruct = new Sprite(GFX.Game, directory + "/"));
            doorStruct.Add("struct", "struct", 0.08f, 0);
            if (side == "Left" || side == "Right")
            {
                Collider.Width = 6f;
                Collider.Height = 40f;
            }
            else
            {
                Collider.Width = 40f;
                Collider.Height = 6f;
            }
            doorStruct.Justify = new Vector2(1f, 0f);
            doorStruct.Play("struct", restart: true);
            Depth = -90000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (color != "Blue" && color != "Grey")
            {
                if (XaphanModule.ModSaveData.DoorsOpened.Contains(SceneAs<Level>().Session.Area.LevelSet + "_" + ID))
                {
                    color = "Blue";
                }
            }
            Add(doorCap = new Sprite(GFX.Game, directory + "/"));
            doorCap.Add("closed", color.ToLower() + "/closed", 0.8f, 0);
            doorCap.AddLoop("closedActive", color.ToLower() + "/closed", color.ToLower() == "grey" ? 0.075f : 0.2f);
            doorCap.Add("opened", "opened", 0.8f);
            doorCap.Add("close", color.ToLower() + "/close", 0.05f);
            doorCap.Add("open", color.ToLower() + "/open", 0.05f);
            doorCap.Add("closedForced", "grey/closed", 0.8f, 0);
            doorCap.Add("openedForced", "opened", 0.8f);
            doorCap.Add("closeForced", "grey/close", 0.05f);
            doorCap.Add("openForced", "grey/open", 0.05f);
            switch (side)
            {
                case "Right":
                    doorCap.FlipX = true;
                    doorCap.Position.X = doorCap.Position.X + 2;
                    doorStruct.FlipX = true;
                    doorStruct.Position.X = doorStruct.Position.X + 16;
                    Collider.Left = Collider.Left + 2;
                    break;
                case "Top":
                    doorCap.FlipY = true;
                    doorCap.Rotation = (float)Math.PI / 2f;
                    doorCap.Position.X = doorCap.Position.X + 40;
                    doorStruct.FlipY = true;
                    doorStruct.Rotation = (float)Math.PI / 2f;
                    doorStruct.Position.X = doorStruct.Position.X + 40;
                    break;
                case "Bottom":
                    doorCap.Rotation = -(float)Math.PI / 2f;
                    doorCap.Position.Y = doorCap.Position.Y + 8;
                    doorStruct.Rotation = -(float)Math.PI / 2f;
                    doorStruct.Position.Y = doorStruct.Position.Y + 8;
                    Collider.Top = Collider.Top + 2;
                    break;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = Scene.Tracker.GetEntity<Player>();
            FakePlayer fakePlayer = Scene.Tracker.GetEntity<FakePlayer>();
            switch (side)
            {
                case "Left":
                    if ((player != null && player.Left < Right && player.Bottom > Top && player.Top < Bottom) || !FakePlayerNotCloseToDoor())
                    {
                        StartOpened();
                    }
                    else
                    {
                        StartClosed();
                    }
                    break;
                case "Right":
                    if ((player != null && player.Right > Left && player.Bottom > Top && player.Top < Bottom) || !FakePlayerNotCloseToDoor())
                    {
                        StartOpened();
                    }
                    else
                    {
                        StartClosed();
                    }
                    break;
                case "Top":
                    if ((player != null && player.Top < Bottom && player.Left < Right && player.Right > Left) || !FakePlayerNotCloseToDoor())
                    {
                        StartOpened();
                    }
                    else
                    {
                        StartClosed();
                    }
                    break;
                case "Bottom":
                    if ((player != null && player.Bottom > Top && player.Left < Right && player.Right > Left) || !FakePlayerNotCloseToDoor())
                    {
                        StartOpened();
                    }
                    else
                    {
                        StartClosed();
                    }
                    break;
            }
        }

        public override void Update()
        {
            base.Update();
            bool allFlagsTrue = true;
            bool oneFlagTrue = false;
            foreach (string flag in flags)
            {
                if (!OnlyNeedOneFlag)
                {
                    if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag))
                    {
                        allFlagsTrue = false;
                        break;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag))
                    {
                        oneFlagTrue = true;
                        break;
                    }
                }
            }
            bool shouldBeActive = !OnlyNeedOneFlag ? allFlagsTrue : oneFlagTrue;

            if (shouldBeActive && 
                ((!string.IsNullOrEmpty(forceLockedFlag) && !SceneAs<Level>().Session.GetFlag(forceLockedFlag)) || string.IsNullOrEmpty(forceLockedFlag)) &&
                ((!string.IsNullOrEmpty(forceInactiveFlag) && !SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || string.IsNullOrEmpty(forceInactiveFlag))
                )
            {
                isActive = true;
            }
            else
            {
                isActive = false;
                StateChanged = true;
            }
            if (!open)
            {
                if (!isActive && StateChanged)
                {
                    doorCap.Play(SceneAs<Level>().Session.GetFlag(forceLockedFlag) ? "closedForced" : "closed", restart: true);
                    StateChanged = false;
                    locked = true;
                }
                else
                {
                    if (isActive && !StateChanged && locked)
                    {
                        doorCap.Play("closedActive", restart: true);
                        if (unlockSound != "")
                        {
                            Audio.Play(unlockSound, Position);
                        }
                        StateChanged = true;
                        locked = false;
                    }
                    if (isActive && StateChanged)
                    {
                        doorCap.Play("closedActive", restart: true);
                        StateChanged = false;
                    }
                }
            }
        }

        public void StartOpened()
        {
            doorCap.Play(SceneAs<Level>().Session.GetFlag(forceLockedFlag) ? "openedForced" : "opened", restart: true);
            Collidable = false;
            open = true;
            Add(new Coroutine(CloseBehindPlayer()));
        }

        public void StartClosed()
        {
            if (!isActive)
            {
                doorCap.Play(SceneAs<Level>().Session.GetFlag(forceLockedFlag) ? "closedForced" : "closed", restart: true);
            }
            else
            {
                doorCap.Play("closedActive", restart: true);
            }
            Collidable = true;
            open = false;
            Add(new Coroutine(OpenInFrontPlayer()));
        }

        public void Close()
        {
            Audio.Play(closeSound == "" ? "event:/game/05_mirror_temple/gate_main_close" : closeSound, Position);
            doorCap.Play(SceneAs<Level>().Session.GetFlag(forceLockedFlag) ? "closeForced" : "close", restart: true);
            Collidable = true;
            Add(new Coroutine(Wait()));
        }

        public void Open()
        {
            if (isActive && !locked)
            {
                Audio.Play(openSound == "" ? "event:/game/05_mirror_temple/gate_main_open" : openSound, Position);
                doorCap.Play(SceneAs<Level>().Session.GetFlag(forceLockedFlag) ? "openForced" : "open", restart: true);
                Collidable = false;
                if (XaphanModule.useMetroidGameplay && color != "Blue" && color != "Grey")
                {
                    XaphanModule.ModSaveData.DoorsOpened.Add(SceneAs<Level>().Session.Area.LevelSet + "_" + ID);
                }
                Add(new Coroutine(Wait()));
            }
        }

        private IEnumerator Wait()
        {
            float WaitTimer = 0.25f;
            while (WaitTimer > 0f)
            {
                yield return null;
                WaitTimer -= Engine.DeltaTime;
            }
            if (Collidable)
            {
                if (!isActive)
                {
                    if (lockSound != "")
                    {
                        Audio.Play(lockSound, Position);
                    }
                }
                StartClosed();
            }
            else
            {
                StartOpened();
            }
        }

        private IEnumerator CloseBehindPlayer()
        {
            if (!keepOpen)
            {
                bool isNotCloseToDoor = false;
                while (!isNotCloseToDoor)
                {
                    if (FakePlayerNotCloseToDoor())
                    {
                        isNotCloseToDoor = PlayerNotCloseToDoor();
                    }
                    yield return null;
                }
                Close();
            }
        }

        private bool FakePlayerNotCloseToDoor()
        {
            bool isNotCloseToDoor = false;
            FakePlayer player = Scene.Tracker.GetEntity<FakePlayer>();
            if (player != null && SceneAs<Level>().Bounds.Contains((int)player.Position.X, (int)player.Position.Y))
            {
                if (side == "Right")
                {
                    if ((player.Right < Left - 16 || player.Bottom < Top || player.Top > Bottom))
                    {
                        isNotCloseToDoor = true;
                    }
                }
                else if (side == "Left")
                {
                    if ((player.Left > Right + 16 || player.Bottom < Top || player.Top > Bottom))
                    {
                        isNotCloseToDoor = true;
                    }
                }
                else if (side == "Top")
                {
                    if ((player.Right < Left || player.Left > Right || player.Top > Bottom + 10))
                    {
                        isNotCloseToDoor = true;
                    }
                }
                else if (side == "Bottom")
                {
                    if ((player.Right < Left || player.Left > Right || player.Bottom < Top - 10))
                    {
                        isNotCloseToDoor = true;
                    }
                }
            }
            else
            {
                isNotCloseToDoor = true;
            }
            return isNotCloseToDoor;
        }

        private bool PlayerNotCloseToDoor()
        {
            bool isNotCloseToDoor = false;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (side == "Right")
            {
                if (player != null && (player.Right < Left - 16 || player.Bottom < Top || player.Top > Bottom))
                {
                    isNotCloseToDoor = true;
                }
            }
            else if (side == "Left")
            {
                if (player != null && (player.Left > Right + 16 || player.Bottom < Top || player.Top > Bottom))
                {
                    isNotCloseToDoor = true;
                }
            }
            else if (side == "Top")
            {
                if (player != null && (player.Right < Left || player.Left > Right || player.Top > Bottom + 10))
                {
                    isNotCloseToDoor = true;
                }
            }
            else if (side == "Bottom")
            {
                if (player != null && (player.Right < Left || player.Left > Right || player.Bottom < Top - 10))
                {
                    isNotCloseToDoor = true;
                }
            }
            return isNotCloseToDoor;
        }

        private IEnumerator OpenInFrontPlayer()
        {
            if (!XaphanModule.useMetroidGameplay)
            {
                while (!open)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (side == "Right")
                    {
                        if (player != null && player.Left > Left - 16 && player.Bottom > Top && player.Top < Bottom)
                        {
                            break;
                        }
                        yield return null;
                    }
                    else if (side == "Left")
                    {
                        if (player != null && player.Right < Right + 16 && player.Bottom > Top && player.Top < Bottom)
                        {
                            break;
                        }
                        yield return null;
                    }
                    else if (side == "Top")
                    {
                        if (player != null && player.Right > Left && player.Left < Right && player.Top < Bottom + 10)
                        {
                            break;
                        }
                        yield return null;
                    }
                    else if (side == "Bottom")
                    {
                        if (player != null && player.Right > Left && player.Left < Right && player.Bottom > Top - 10)
                        {
                            break;
                        }
                        yield return null;
                    }
                }
                if (isActive)
                {
                    Open();
                }
                else
                {
                    Add(new Coroutine(CheckFlag()));
                    StartClosed();
                }
            }
        }

        private IEnumerator CheckFlag()
        {
            while (true)
            {
                if (isActive)
                {
                    break;
                }
                yield return null;
            }
        }

        public override void Render()
        {
            if (doorCap != null)
            {
                doorCap.Render();
            }
            doorStruct.Render();
        }
    }
}
