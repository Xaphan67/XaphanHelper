using System;
using System.Linq;
using System.Reflection;
using Celeste;
using Celeste.Mod.XaphanHelper;
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
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Textbox>("portraitSprite"),
                i => i.MatchCallOrCallvirt(out var m) && m.Name == "Render"))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, portraitSpriteField);
            c.EmitDelegate<Action<Image>>(drawOverlays);
        }
    }

    private static void modMiniTextboxRender(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<MiniTextbox>("portrait"),
                i => i.MatchCallOrCallvirt(out var m) && m.Name == "Render"))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, miniPortraitField);
            c.EmitDelegate<Action<Image>>(drawOverlays);
        }
    }

    public static void drawOverlays(Image sprite)
    {
        MTexture tex = sprite?.Texture;
        if (tex?.AtlasPath == null) return;

        if (!ReferenceEquals(tex, lastBase))
        {
            lastBase = tex;
            lastHair = GFX.Portraits.Has(tex.AtlasPath + "_hair") ? GFX.Portraits[tex.AtlasPath + "_hair"] : null;
            lastJacket = GFX.Portraits.Has(tex.AtlasPath + "_jacket") ? GFX.Portraits[tex.AtlasPath + "_jacket"] : null;
        }
        if (lastHair == null && lastJacket == null) return;

        float alpha = sprite.Color.A / 255f;

        if (lastJacket != null && JacketTintProvider() is Color jt)
            lastJacket.Draw(sprite.RenderPosition, sprite.Origin, jt * alpha, sprite.Scale, sprite.Rotation, sprite.Effects);

        if (lastHair != null && HairTintProvider() is Color ht)
            lastHair.Draw(sprite.RenderPosition, sprite.Origin, ht * alpha, sprite.Scale, sprite.Rotation, sprite.Effects);
    }

    private static Color? DefaultHairTint()
    {
        Player player = (Engine.Scene as Level)?.Tracker.GetEntity<Player>();
        Color c = player?.Hair?.Color ?? Calc.HexToColor("AC3232");
        return NormalizeBrightness(c);
    }

    private static Color? DefaultJacketTint()
    {
        Level level = Engine.Scene as Level;
        if (XaphanModule.useUpgrades && (VariaJacket.Active(level) || GravityJacket.Active(level)))
        {
            string id = "";
            foreach (string name in XaphanModule.JacketPriorityNames)
            {
                var data = XaphanModule.JacketPriority.FirstOrDefault(n => n.Name == name);
                if (data.Test != null && data.Test(level))
                {
                    id = data.Id;
                    break;
                }
            }
            Color c = Calc.HexToColor(id == "varia" ? "C97044" : "9345CE");
            return NormalizeBrightness(c);
        }
        return null;
    }

    public static Color NormalizeBrightness(Color c)
    {
        int m = Math.Max(c.R, Math.Max(c.G, c.B));
        if (m <= 0) return Color.White;
        float s = 255f / m;
        return new Color((byte)Math.Min(255f, c.R * s), (byte)Math.Min(255f, c.G * s), (byte)Math.Min(255f, c.B * s));
    }
}
