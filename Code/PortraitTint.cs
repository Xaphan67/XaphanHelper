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

public static class PortraitTint
{
    public static Func<Color?> HairTintProvider = DefaultHairTint;

    public static Func<Color?> JacketTintProvider = DefaultJacketTint;

    private static readonly FieldInfo portraitSpriteField = typeof(Textbox).GetField("portraitSprite", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly FieldInfo miniPortraitField = typeof(MiniTextbox).GetField("portrait", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    private static MTexture lastBase, lastHair, lastJacket;

    public static void Load()
    {
        IL.Celeste.Textbox.Render += modTextboxRender;
        IL.Celeste.MiniTextbox.Render += modMiniTextboxRender;
    }

    public static void Unload()
    {
        IL.Celeste.Textbox.Render -= modTextboxRender;
        IL.Celeste.MiniTextbox.Render -= modMiniTextboxRender;
        lastBase = lastHair = lastJacket = null;
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
        if ((Engine.Scene as Level)?.Session.Area.LevelSet != "Xaphan/0")
        {
            return;
        }

        MTexture texture = sprite?.Texture;

        if (texture?.AtlasPath == null)
        {
            return;
        }
        if (!ReferenceEquals(texture, lastBase))
        {
            lastBase = texture;
            lastHair = GFX.Portraits.Has(texture.AtlasPath + "_hair") ? GFX.Portraits[texture.AtlasPath + "_hair"] : null;
            lastJacket = GFX.Portraits.Has(texture.AtlasPath + "_jacket") ? GFX.Portraits[texture.AtlasPath + "_jacket"] : null;
        }
        if (lastHair == null && lastJacket == null)
        {
            return;
        }
        float alpha = sprite.Color.A / 255f;
        if (lastJacket != null && JacketTintProvider() is Color jacketTint)
        {
            lastJacket.Draw(sprite.RenderPosition, sprite.Origin, jacketTint * alpha, sprite.Scale, sprite.Rotation, sprite.Effects);
        }
        if (lastHair != null && HairTintProvider() is Color hairTint)
        {
            lastHair.Draw(sprite.RenderPosition, sprite.Origin, hairTint * alpha, sprite.Scale, sprite.Rotation, sprite.Effects);
        }
    }

    private static Color? DefaultHairTint()
    {
        Player player = (Engine.Scene as Level)?.Tracker.GetEntity<Player>();
        Color color = player?.Hair?.Color ?? Calc.HexToColor("AC3232");
        return NormalizeBrightness(color);
    }

    private static Color? DefaultJacketTint()
    {
        Level level = Engine.Scene as Level;
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
            Color color = Calc.HexToColor(id == "varia" ? "C97044" : "9345CE");
            return NormalizeBrightness(color);
        }
        return null;
    }

    public static Color NormalizeBrightness(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        if (max <= 0)
        {
            return Color.White;
        }
        float scale = 255f / max;
        return new Color((byte)Math.Min(255f, color.R * scale), (byte)Math.Min(255f, color.G * scale), (byte)Math.Min(255f, color.B * scale));
    }
}
