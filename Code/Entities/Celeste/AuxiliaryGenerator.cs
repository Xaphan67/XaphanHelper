using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AuxiliaryGenerator")]
    public class AuxiliaryGenerator : Entity
    {
        Sprite Sprite;

        private Message message;

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
            if (self.Entity is Player player && player.Sprite == self && self.Scene is Level level && !XaphanModule.PlayerIsControllingRemoteDrone())
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
                    talk.Enabled = XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator");
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
            SceneAs<Level>().Add(message = new Message(Vector2.Zero, "event:/game/xaphan/cell_unlock", "Xaphan_Ch5_Generator"));
            while (!Input.ESC.Pressed && !Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            player.Facing = Facings.Right;
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
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Main_Power_Off"))
            {
                XaphanModule.ModSaveData.SavedFlags.Remove("Xaphan/0_Ch4_Main_Power_Off");
            }
            XaphanModule.ModSaveData.GlobalFlags.Add("Xaphan/0_Ch5_Auxiliary_Power");
            if (SceneAs<Level>().Session.GetFlag("Ch4_Main_Power_Off"))
            {
                SceneAs<Level>().Session.SetFlag("Ch4_Main_Power_Off", false);
            }
            SceneAs<Level>().Session.SetFlag("Ch5_Auxiliary_Power", true);
            player.Sprite.OnLastFrame = resumeSprite;
            if (XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator2") || XaphanModule.PlayerHasGolden)
            {
                yield return 0.2f;
                player.StateMachine.State = 0;
            }
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
            PlayerPose = "";
            player.Sprite.OnLastFrame = resetSprite;
        }

        public void resetSprite(string s)
        {

        }
    }
}
