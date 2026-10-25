using System.Collections;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public AuxiliaryGenerator(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/AuxiliaryGenerator/"));
            Sprite.Add("off", "off", 0);
            Sprite.Add("turnOn", "turnOn", 0.08f);
            Sprite.AddLoop("on", "on", 0.08f);
            Sprite.Play("off");
            Depth = 999;
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
            PlayerCustomAnimation component = player.Components.Get<PlayerCustomAnimation>();
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X + 12, false, 1f, true);
            if (component != null)
            {
                component.SetAnim("XaphanHelper_player_turnAround");
                component.PlaySprite("turnBackpack", "hairBackpack", false);
            }
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
            if (component != null)
            {
                component.PlaySprite("turnBackpack", "hairBackpack", true, false);
            }
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
            if (XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch5_Generator2") || XaphanModule.PlayerHasGolden)
            {
                yield return 0.2f;
                player.StateMachine.State = 0;
            }
        }
    }
}
