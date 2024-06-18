using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AuxiliaryGenerator")]
    public class AuxiliaryGenerator : Entity
    {
        public class GeneratorMessage : Entity
        {
            private string Text;

            private Vector2 TextSize;

            private float height;

            public bool drawText;

            public GeneratorMessage(Vector2 position, string text) : base(position)
            {
                Tag = (Tags.HUD | Tags.Persistent);
                Text = text;
                TextSize = ActiveFont.Measure(Dialog.Clean(text));
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(new Coroutine(OpenRoutine()));
            }

            public IEnumerator OpenRoutine()
            {
                while (height < TextSize.Y + 75)
                {
                    height += Engine.DeltaTime * 1200;
                    yield return null;
                }
                drawText = true;
            }

            public void Close()
            {
                Add(new Coroutine(CloseRoutine()));
            }

            public IEnumerator CloseRoutine()
            {
                drawText = false;
                while (height > 1)
                {
                    height -= Engine.DeltaTime * 1200;
                    yield return null;
                }
                RemoveSelf();
            }

            public override void Render()
            {
                Draw.Rect(Engine.Width / 2 - TextSize.X / 2 - 50, (Engine.Height / 2 - TextSize.Y / 2 - 125) + ((TextSize.Y + 200) / 2) - height / 2, TextSize.X + 100, height, Color.Black);
                if (drawText)
                {
                    ActiveFont.Draw(Dialog.Clean(Text), new Vector2(Engine.Width / 2 - TextSize.X / 2, Engine.Height / 2 - TextSize.Y / 2), new Vector2(0f, 0.5f), Vector2.One * 1f, Calc.HexToColor("AA00AA"));
                }
            }
        }

        Sprite Sprite;

        private GeneratorMessage message;

        private TalkComponent talk;

        private Coroutine ActivationRoutine = new();

        public string PlayerPose = "";

        public AuxiliaryGenerator(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/AuxiliaryGenerator/"));
            Sprite.Add("off", "off", 0);
            Sprite.Add("turnOn", "turnOn", 0.08f);
            Sprite.AddLoop("on", "on", 0.08f);
            Sprite.Play("off");
            Depth = 999;
        }

        public static void Load()
        {
            On.Monocle.Sprite.Play += PlayerSpritePlayHook;
        }

        public static void Unload()
        {
            On.Monocle.Sprite.Play -= PlayerSpritePlayHook;
        }

        private static void PlayerSpritePlayHook(On.Monocle.Sprite.orig_Play orig, Sprite self, string id, bool restart = false, bool randomizeFrame = false)
        {
            if (self.Entity is Player && self.Scene is Level level && !XaphanModule.PlayerIsControllingRemoteDrone())
            {
                foreach (AuxiliaryGenerator generator in level.Tracker.GetEntities<AuxiliaryGenerator>())
                {
                    if (!string.IsNullOrEmpty(generator.PlayerPose) && generator.Active)
                    {
                        id = generator.PlayerPose;
                        break;
                    }
                }
            }
            orig(self, id, restart, randomizeFrame);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(talk = new TalkComponent(new Rectangle(0, 64, 24, 16), new Vector2(12f, 56f), Interact));
            talk.PlayerMustBeFacing = false;
            talk.Enabled = false;
            if (!SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
            {
                Sprite.Play("on");
            }
        }

        public override void Update()
        {
            base.Update();
            if (talk != null)
            {
                if (SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
                {
                    talk.Enabled = true;
                    if (!ActivationRoutine.Active)
                    {
                        Sprite.Play("off");
                    }
                }
                else
                {
                    talk.Enabled = false;
                    Sprite.Play("on");
                }
            }
        }

        private void Interact(Player player)
        {
            Add(ActivationRoutine = new Coroutine(Activate(player)));
        }

        private IEnumerator Activate(Player player)
        {
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X + 12, false, 1f, true);
            PlayerPose = "XaphanHelper_turnAround";
            player.Sprite.Play(PlayerPose);
            player.Sprite.OnLastFrame = stopSprite;
            yield return 0.5f;
            Sprite.Play("turnOn");
            yield return 0.3f;
            Audio.Play("event:/game/xaphan/cell_unlock", Position);
            SceneAs<Level>().Add(message = new GeneratorMessage(Vector2.Zero, "Xaphan_Ch5_Generator"));
            while (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_5_geothermal_active");
            SceneAs<Level>().Session.Audio.Apply(forceSixteenthNoteHack: false);
            Sprite.Play("on");
            message.Close();
            PlayerPose = "XaphanHelper_turnAround_reverse";
            player.Sprite.Play(PlayerPose);
            if (XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Main_Power_Off"))
            {
                XaphanModule.ModSaveData.GlobalFlags.Remove("Xaphan/0_Ch4_Main_Power_Off");
            }
            XaphanModule.ModSaveData.GlobalFlags.Add("Xaphan/0_Ch5_Auxiliary_Power");
            if (SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
            {
                SceneAs<Level>().Session.SetFlag("Ch4_Main_Power_Off", false);
            }
            SceneAs<Level>().Session.SetFlag("Ch5_Auxiliary_Power", true);
            player.Sprite.OnLastFrame = resumeSprite;
        }

        public void stopSprite(string s)
        {
            PlayerPose = "XaphanHelper_turnAround_end";
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.Sprite.Play(PlayerPose);
            }
        }

        public void resumeSprite(string s)
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = 0;
            }
            PlayerPose = "";
            player.Sprite.OnLastFrame = resetSprite;
        }

        public void resetSprite(string s)
        {

        }
    }
}
