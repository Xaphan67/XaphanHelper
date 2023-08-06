using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using Celeste.Mod.XaphanHelper.Data;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal static class DecalsRegisteryUpdate
    {
        public static List<DecalsFlagsHideData> flagsHideData = new();

        public static List<DecalsFlagsSwapData> flagsSwapData = new();

        public static List<Decal> SwapedDecals = new();

        public static void Load()
        {
            On.Celeste.Decal.Added += onDecalAdded;
            On.Celeste.Decal.Update += onDecalUpdate;
            Everest.Events.Level.OnLoadLevel += onLevelLoad;
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public static void Unload()
        {
            On.Celeste.Decal.Added -= onDecalAdded;
            On.Celeste.Decal.Update -= onDecalUpdate;
            Everest.Events.Level.OnLoadLevel -= onLevelLoad;
            Everest.Events.Level.OnExit -= onLevelExit;
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            flagsHideData.Clear();
            flagsSwapData.Clear();
        }

        private static void onDecalAdded(On.Celeste.Decal.orig_Added orig, Decal self, Scene scene)
        {
            orig(self, scene);
            UpdateDecal(self);
        }

        private static void onDecalUpdate(On.Celeste.Decal.orig_Update orig, Decal self)
        {
            UpdateDecal(self);
            orig(self);
        }

        private static void onLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (flagsHideData.Count == 0 && flagsSwapData.Count == 0)
            {
                foreach (string decal in DecalRegistry.RegisteredDecals.Keys)
                {
                    DecalRegistry.DecalInfo decalInfo = DecalRegistry.RegisteredDecals[decal];
                    foreach (KeyValuePair<string, XmlAttributeCollection> property in decalInfo.CustomProperties)
                    {
                        if (property.Key == "XaphanHelper_flagsHide")
                        {
                            string flags = "";
                            string room = "";
                            bool inverted = false;
                            foreach (XmlAttribute attribute in property.Value)
                            {
                                if (attribute.Name == "flags")
                                {
                                    flags = attribute.Value;
                                }
                                if (attribute.Name == "inverted")
                                {
                                    inverted = bool.Parse(attribute.Value);
                                }
                                if (attribute.Name == "room")
                                {
                                    room = attribute.Value;
                                }
                            }
                            flagsHideData.Add(new DecalsFlagsHideData(decal, flags, room, inverted));
                        }
                        if (property.Key == "XaphanHelper_flagSwap")
                        {
                            string flag = "";
                            string offPath = "";
                            string onPath = "";
                            string room = "";
                            foreach (XmlAttribute attribute in property.Value)
                            {
                                if (attribute.Name == "flag")
                                {
                                    flag = attribute.Value;
                                }
                                if (attribute.Name == "offPath")
                                {
                                    offPath = attribute.Value;
                                }
                                if (attribute.Name == "onPath")
                                {
                                    onPath = attribute.Value;
                                }
                                if (attribute.Name == "room")
                                {
                                    room = attribute.Value;
                                }
                            }
                             flagsSwapData.Add(new DecalsFlagsSwapData(decal, flag, offPath, onPath, room));
                        }
                    }
                }
            }
        }

        private static void UpdateDecal(Decal self)
        {
            string text = self.Name.ToLower();
            if (text.StartsWith("decals/"))
            {
                text = text.Substring(7);
            }
            if (DecalRegistry.RegisteredDecals.ContainsKey(text))
            {
                foreach (DecalsFlagsHideData data in flagsHideData)
                {
                    if (data.Decal == text)
                    {
                        if (!string.IsNullOrEmpty(data.Room) ? self.SceneAs<Level>().Session.Level == data.Room : true)
                        {
                            foreach (string flag in data.Flags.Split(','))
                            {
                                self.Visible = data.Inverted ? self.SceneAs<Level>().Session.GetFlag(flag) : !self.SceneAs<Level>().Session.GetFlag(flag);
                            }
                        }
                    }
                }

                foreach (DecalsFlagsSwapData data in flagsSwapData)
                {
                    if (data.Decal == text)
                    {
                        string[] onPaths = data.OnPath.Split(',');
                        if (!string.IsNullOrEmpty(data.Flag) && !string.IsNullOrEmpty(data.OffPath) && !string.IsNullOrEmpty(data.OnPath) && (!string.IsNullOrEmpty(data.Room) ? self.SceneAs<Level>().Session.Level == data.Room : true))
                        {
                            if (self.SceneAs<Level>().Session.GetFlag(data.Flag) && !SwapedDecals.Contains(self))
                            {
                                SwapedDecals.Add(self);
                                self.Remove(self.Image);
                                int textures = onPaths.Length;
                                if (textures > 1)
                                {
                                    int texture = Calc.Random.Next(textures);
                                    self.MakeFlagSwap(data.Flag, data.OffPath, onPaths[texture]);
                                }
                                else
                                {
                                    self.MakeFlagSwap(data.Flag, data.OffPath, data.OnPath);
                                }
                            }
                            else if (!self.SceneAs<Level>().Session.GetFlag(data.Flag) && SwapedDecals.Contains(self))
                            {
                                SwapedDecals.Remove(self);
                                self.Remove(self.Image);
                                self.MakeFlagSwap(data.Flag, data.OffPath, data.OnPath);
                            }
                        }
                    }
                }
            }
        }
    }
}
