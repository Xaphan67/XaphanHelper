using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class BreakBlockIndicator : Entity
    {
        public Sprite blockType;

        public BreakBlock block;

        private bool broken;

        private bool breakBlockAlreadyBroken = true;

        private bool startRevealed;

        public int index;

        public string mode;

        private string color;

        private string directory;

        private EntityID eid;

        private bool autoAdded;

        public BreakBlockIndicator(BreakBlock block, bool autoAdded, Vector2 position)
        {
            eid = block.eid;
            this.block = block;
            this.autoAdded = autoAdded;
            Position = position;
            mode = block.type;
            color = block.color;
            startRevealed = block.startRevealed;
            directory = block.directory;
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/BreakBlock";
            }
            Collider = new Hitbox(8f, 8f);
            Add(new PlayerCollider(OnPlayerBooster, new Hitbox(16f, 16f, -4f, -4f)));
            Add(blockType = new Sprite(GFX.Game, directory + "/"));
            blockType.AddLoop("bomb", "Bomb", 1f);
            blockType.AddLoop("megaBomb", "MegaBomb", 1f);
            blockType.AddLoop("lightningDash", "LightningDash", 1f);
            blockType.AddLoop("redBooster", "RedBooster", 1f);
            blockType.AddLoop("drone", "Drone", 1f);
            blockType.AddLoop("screwAttack", "ScrewAttack", 1f);
            blockType.AddLoop("missile", "Missile", 1f);
            blockType.AddLoop("superMissile", "SuperMissile", 1f);
            Depth = -13001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CollideCheck<SolidTiles>())
            {
                RemoveSelf();
            }
            if (CollideCheck<Player>())
            {
                foreach (BreakBlockIndicator breakblock in Scene.Entities.FindAll<BreakBlockIndicator>())
                {
                    if ((!autoAdded && breakblock.index == index) || (breakblock.eid.ID == eid.ID && breakblock.eid.Level == eid.Level))
                    {
                        breakblock.RemoveSelf();
                    }
                }
            }
            else
            {
                Collidable = (Visible = true);
            }
            if (startRevealed)
            {
                RevealSequence();
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (BreakBlock breakblock in Scene.Entities.FindAll<BreakBlock>())
            {
                if ((!autoAdded && breakblock.index == index) || (breakblock.eid.ID == eid.ID && breakblock.eid.Level == eid.Level))
                {
                    breakBlockAlreadyBroken = false;
                }
            }
            if (broken || breakBlockAlreadyBroken)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            blockType.Render();
        }

        public void OnPlayer(Player player)
        {
            if (player.StateMachine.State == 2 || player.DashAttacking)
            {
                RevealSequence();
            }
            if (mode == "LightningDash" && SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Shinesparking"))
            {
                BreakSequence();
            }
        }

        public void OnPlayerBooster(Player player)
        {
            if (mode == "RedBooster" && player.StateMachine.State == 5)
            {
                BreakSequence();
            }
        }

        public void BreakSequence()
        {
            foreach (BreakBlockIndicator indicator in SceneAs<Level>().Entities.FindAll<BreakBlockIndicator>())
            {
                if ((!autoAdded && indicator.index == index) || (indicator.eid.ID == eid.ID && indicator.eid.Level == eid.Level))
                {
                    indicator.blockType.RemoveSelf();
                    indicator.broken = true;
                    indicator.Collidable = false;
                    if (!autoAdded)
                    {
                        SceneAs<Level>().Session.DoNotLoad.Add(eid);
                    }
                }
            }
            foreach (BreakBlock breakblock in SceneAs<Level>().Entities.FindAll<BreakBlock>())
            {
                if ((!autoAdded && breakblock.index == index) || (breakblock.eid.ID == eid.ID && breakblock.eid.Level == eid.Level))
                {
                    breakblock.Break();
                }
            }
        }

        public void RevealSequence()
        {
            if (mode == "LightningDash")
            {
                blockType.Play("lightningDash");
            }
            else if (mode == "Bomb")
            {
                blockType.Play("bomb");
            }
            else if (mode == "MegaBomb")
            {
                blockType.Play("megaBomb");
            }
            else if (mode == "RedBooster")
            {
                blockType.Play("redBooster");
            }
            else if (mode == "Drone")
            {
                blockType.Play("drone");
            }
            else if (mode == "ScrewAttack")
            {
                blockType.Play("screwAttack");
            }
            else if (mode == "Missile")
            {
                blockType.Play("missile");
            }
            else if (mode == "SuperMissile")
            {
                blockType.Play("superMissile");
            }
        }
    }
}
