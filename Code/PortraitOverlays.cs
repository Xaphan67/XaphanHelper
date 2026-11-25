using System;
using System.Linq;
using System.Reflection;
using Celeste;
using Celeste.Mod.XaphanHelper;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

public static class PortraitOverlays
{
    private static readonly FieldInfo portraitSpriteField = typeof(Textbox).GetField("portraitSprite", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly FieldInfo miniPortraitField = typeof(MiniTextbox).GetField("portrait", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static MTexture Hair;
    
    private static MTexture Jacket;

    public static void Load()
    {
        IL.Celeste.Textbox.Render += modTextboxRender;
        IL.Celeste.MiniTextbox.Render += modMiniTextboxRender;
    }

    public static void Unload()
    {
        IL.Celeste.Textbox.Render -= modTextboxRender;
        IL.Celeste.MiniTextbox.Render -= modMiniTextboxRender;
    }

    private static void modTextboxRender(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<Textbox>("portraitSprite"),
                instr => instr.MatchCallOrCallvirt(out var value) && value.Name == "Render"))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, portraitSpriteField);
            cursor.EmitDelegate<Action<Image>>(DrawOverlays);
        }
    }

    private static void modMiniTextboxRender(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<MiniTextbox>("portrait"),
                instr => instr.MatchCallOrCallvirt(out var value) && value.Name == "Render"))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, miniPortraitField);
            cursor.EmitDelegate<Action<Image>>(DrawOverlays);
        }
    }

    public static void DrawOverlays(Image sprite)
    {
        static Color? GetHairTint(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();

            /* Get the current color of the player hair if its not the default one, then lighten it */
            if (player.Hair.Color != Player.NormalHairColor)
            {
                return Color.Lerp(player.Hair.Color, Color.White, 0.3f);
            }

            /* Default hair color */
            return null;
        }

        static Color? GetJacketTint(Level level)
        {
            /* If the player has one or both jacket upgrades active, return the color of the one with higher priority */
            if (XaphanModule.useUpgrades && (VariaJacket.Active(level) || GravityJacket.Active(level)))
            {
                string id = "";
                foreach (string name in XaphanModule.JacketPriorityNames)
                {
                    JacketPriorityData jacketData = XaphanModule.JacketPriority.FirstOrDefault(data => data.Name == name);
                    if (jacketData.Test != null && jacketData.Test(level))
                    {
                        id = jacketData.Id;
                        break;
                    }
                }
                return Calc.HexToColor(id == "varia" ? "C97044" : "9345CE");
            }

            /* No jacket upgrade active */
            return null;
        }

        if (Engine.Scene is Level level)
        {
            /* Only in SoCM */
            if (level.Session.Area.LevelSet != "Xaphan/0")
            {
                return;
            }

            MTexture texture = sprite.Texture;
            if (texture.AtlasPath == null)
            {
                return;
            }
            Hair = GFX.Portraits.Has(texture.AtlasPath + "_hair") ? GFX.Portraits[texture.AtlasPath + "_hair"] : null;
            Jacket = GFX.Portraits.Has(texture.AtlasPath + "_jacket") ? GFX.Portraits[texture.AtlasPath + "_jacket"] : null;
            if (Hair == null && Jacket == null)
            {
                return;
            }

            /* Draw the overlays only if necessary */
            if (Hair != null && GetHairTint(level) is Color hairTint)
            {
                Hair.Draw(sprite.RenderPosition, sprite.Origin, hairTint, sprite.Scale, sprite.Rotation, sprite.Effects);
            }
            if (Jacket != null && GetJacketTint(level) is Color jacketTint)
            {
                Jacket.Draw(sprite.RenderPosition, sprite.Origin, jacketTint, sprite.Scale, sprite.Rotation, sprite.Effects);
            }
        }
    }
}
