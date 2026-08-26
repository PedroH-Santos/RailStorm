using System.IO;
using UnityEditor;
using UnityEngine;

public static class ChooseWayArtGenerator
{
    const string OutputFolder = "Assets/UI/ChooseWay";

    static readonly Color32 WoodLight = new Color32(0xDE, 0xAD, 0x8B, 0xFF);
    static readonly Color32 WoodFace = new Color32(0xC6, 0x9F, 0x7A, 0xFF);
    static readonly Color32 WoodMid = new Color32(0xA0, 0x75, 0x4B, 0xFF);
    static readonly Color32 WoodDark = new Color32(0x72, 0x4E, 0x21, 0xFF);
    static readonly Color32 WoodOutline = new Color32(0x2A, 0x19, 0x0B, 0xFF);
    static readonly Color32 ShadowColor = new Color32(0x00, 0x00, 0x00, 0x8C);

    const int SignSize = 192;
    const int SignSpriteBorder = 52;
    const float SignChamfer = 30f;
    const float SignOutline = 5f;
    const float SignLipHeight = 26f;

    const int ShieldSize = 192;
    const float ShieldChamfer = 28f;
    const float ShieldOutline = 6f;
    const float ShieldRimWidth = 10f;

    const int RibbonSize = 128;
    const int RibbonSpriteBorder = 40;
    const float RibbonChamfer = 18f;
    const float RibbonOutline = 5f;
    const float RibbonNotch = 16f;

    const int PostWidth = 64;
    const int PostHeight = 160;
    const float PostChamfer = 8f;
    const float PostOutline = 4f;

    const int NailSize = 32;

    const int PipSize = 48;

    const int ChevronSize = 128;
    const float ChevronChamfer = 22f;
    const float ChevronOutline = 5f;

    const int TrailSparkSize = 32;
    const float TrailSparkChamfer = 8f;

    [MenuItem("Tools/RailStorm/Gerar arte da UI de escolha de caminho")]
    public static void Generate()
    {
        Directory.CreateDirectory(OutputFolder);

        SavePng(PathOf("SignPlate9Slice"), SignSize, SignSize, BuildSignPlate(), SignSpriteBorder);
        SavePng(PathOf("SignShadow9Slice"), SignSize, SignSize, BuildSignShadow(), SignSpriteBorder);
        SavePng(PathOf("Shield"), ShieldSize, ShieldSize, BuildShield(), 0);
        SavePng(PathOf("RibbonTag9Slice"), RibbonSize, RibbonSize, BuildRibbon(), RibbonSpriteBorder);
        SavePng(PathOf("PostPlank"), PostWidth, PostHeight, BuildPost(), 0);
        SavePng(PathOf("NailHead"), NailSize, NailSize, BuildNailHead(), 0);
        SavePng(PathOf("PipEmpty"), PipSize, PipSize, BuildPip(false), 0);
        SavePng(PathOf("PipFull"), PipSize, PipSize, BuildPip(true), 0);
        SavePng(PathOf("ChevronPlate"), ChevronSize, ChevronSize, BuildChevronPlate(), 0);
        SavePng(PathOf("TrailSpark"), TrailSparkSize, TrailSparkSize, BuildTrailSpark(), 0);

        AssetDatabase.Refresh();
        Debug.Log("[ChooseWayArt] Arte cartoon da UI de escolha de caminho regenerada em " + OutputFolder + ".");
    }

    static string PathOf(string fileName) => OutputFolder + "/" + fileName + ".png";

    static Color32[] BuildSignPlate()
    {
        var pixels = new Color32[SignSize * SignSize];
        float half = SignSize * 0.5f - 6f;
        float lipTop = -half + SignLipHeight;

        for (int y = 0; y < SignSize; y++)
        {
            for (int x = 0; x < SignSize; x++)
            {
                var p = Centered(x, y, SignSize);
                float distance = ChamferedRectDistance(p, half, half, SignChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * SignSize + x] = Transparent(); continue; }

                bool inLip = p.y < lipTop;
                Color32 body = inLip ? WoodMid : (p.y > half - 22f ? WoodLight : WoodFace);

                float toOutline = -distance;
                float outline = Mathf.Clamp01(SignOutline + 0.5f - toOutline);
                Color32 final = Color32.Lerp(body, WoodOutline, outline);

                pixels[y * SignSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildSignShadow()
    {
        var pixels = new Color32[SignSize * SignSize];
        float half = SignSize * 0.5f - 6f;

        for (int y = 0; y < SignSize; y++)
        {
            for (int x = 0; x < SignSize; x++)
            {
                var p = Centered(x, y, SignSize);
                float distance = ChamferedRectDistance(p, half, half, SignChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                pixels[y * SignSize + x] = WithAlpha(ShadowColor, coverage * (ShadowColor.a / 255f));
            }
        }

        return pixels;
    }

    static Color32[] BuildShield()
    {
        var pixels = new Color32[ShieldSize * ShieldSize];
        float half = ShieldSize * 0.5f - 8f;

        for (int y = 0; y < ShieldSize; y++)
        {
            for (int x = 0; x < ShieldSize; x++)
            {
                var p = Centered(x, y, ShieldSize);
                float distance = ChamferedRectDistance(p, half, half, ShieldChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * ShieldSize + x] = Transparent(); continue; }

                float toOutline = -distance;
                float outline = Mathf.Clamp01(ShieldOutline + 0.5f - toOutline);
                float toRim = Mathf.Abs(distance + ShieldRimWidth * 0.5f + ShieldOutline);
                float rim = Mathf.Clamp01(ShieldRimWidth * 0.5f + 0.5f - toRim);

                float bodyValue = p.y > 0f ? 0.62f : 0.5f;
                Color32 body = Gray(bodyValue);
                Color32 final = Color32.Lerp(body, Gray(1f), rim);
                final = Color32.Lerp(final, Gray(0.08f), outline);

                pixels[y * ShieldSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildRibbon()
    {
        var pixels = new Color32[RibbonSize * RibbonSize];
        float half = RibbonSize * 0.5f - 6f;

        for (int y = 0; y < RibbonSize; y++)
        {
            for (int x = 0; x < RibbonSize; x++)
            {
                var p = Centered(x, y, RibbonSize);
                float distance = ChamferedRectDistance(p, half, half, RibbonChamfer);

                float notchL = new Vector2(p.x + half - RibbonNotch * 0.6f, p.y).magnitude - RibbonNotch;
                float notchR = new Vector2(p.x - half + RibbonNotch * 0.6f, p.y).magnitude - RibbonNotch;
                float notchCut = Mathf.Min(notchL, notchR);
                float shapeDistance = Mathf.Max(distance, -notchCut);

                float coverage = Mathf.Clamp01(0.5f - shapeDistance);
                if (coverage <= 0f) { pixels[y * RibbonSize + x] = Transparent(); continue; }

                float toOutline = -shapeDistance;
                float outline = Mathf.Clamp01(RibbonOutline + 0.5f - toOutline);
                Color32 body = p.y > 6f ? WoodLight : WoodFace;
                Color32 final = Color32.Lerp(body, WoodOutline, outline);

                pixels[y * RibbonSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildPost()
    {
        var pixels = new Color32[PostWidth * PostHeight];
        float halfW = PostWidth * 0.5f - 3f;
        float halfH = PostHeight * 0.5f - 3f;

        for (int y = 0; y < PostHeight; y++)
        {
            for (int x = 0; x < PostWidth; x++)
            {
                float px = x + 0.5f - PostWidth * 0.5f;
                float py = y + 0.5f - PostHeight * 0.5f;
                var p = new Vector2(px, py);

                float distance = ChamferedRectDistance(p, halfW, halfH, PostChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * PostWidth + x] = Transparent(); continue; }

                float toOutline = -distance;
                float outline = Mathf.Clamp01(PostOutline + 0.5f - toOutline);
                Color32 body = px < 0f ? WoodMid : WoodFace;
                Color32 final = Color32.Lerp(body, WoodOutline, outline);

                pixels[y * PostWidth + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildNailHead()
    {
        var pixels = new Color32[NailSize * NailSize];
        float radius = NailSize * 0.5f - 3f;

        for (int y = 0; y < NailSize; y++)
        {
            for (int x = 0; x < NailSize; x++)
            {
                var p = Centered(x, y, NailSize);
                float distance = p.magnitude - radius;
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * NailSize + x] = Transparent(); continue; }

                float toOutline = -distance;
                float outline = Mathf.Clamp01(2.5f + 0.5f - toOutline);
                float highlight = Mathf.Clamp01(1f - (p - new Vector2(-radius * 0.35f, radius * 0.35f)).magnitude / (radius * 0.7f));

                Color32 body = Color32.Lerp(Gray(0.45f), Gray(0.7f), highlight);
                Color32 final = Color32.Lerp(body, Gray(0.08f), outline);

                pixels[y * NailSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildPip(bool full)
    {
        var pixels = new Color32[PipSize * PipSize];
        float radius = PipSize * 0.5f - 4f;

        for (int y = 0; y < PipSize; y++)
        {
            for (int x = 0; x < PipSize; x++)
            {
                var p = Centered(x, y, PipSize);
                float distance = p.magnitude - radius;
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * PipSize + x] = Transparent(); continue; }

                float toOutline = -distance;
                float outline = Mathf.Clamp01(3f + 0.5f - toOutline);

                Color32 body = full ? Gray(1f) : Gray(0.24f);
                Color32 final = Color32.Lerp(body, Gray(0.06f), outline);

                pixels[y * PipSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildChevronPlate()
    {
        var pixels = new Color32[ChevronSize * ChevronSize];
        float half = ChevronSize * 0.5f - 6f;

        for (int y = 0; y < ChevronSize; y++)
        {
            for (int x = 0; x < ChevronSize; x++)
            {
                var p = Centered(x, y, ChevronSize);
                float distance = ChamferedRectDistance(p, half, half, ChevronChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                if (coverage <= 0f) { pixels[y * ChevronSize + x] = Transparent(); continue; }

                float toOutline = -distance;
                float outline = Mathf.Clamp01(ChevronOutline + 0.5f - toOutline);
                Color32 body = p.y > 0f ? WoodLight : WoodFace;
                Color32 final = Color32.Lerp(body, WoodOutline, outline);

                pixels[y * ChevronSize + x] = WithAlpha(final, coverage);
            }
        }

        return pixels;
    }

    static Color32[] BuildTrailSpark()
    {
        var pixels = new Color32[TrailSparkSize * TrailSparkSize];
        float half = TrailSparkSize * 0.5f - 2f;

        for (int y = 0; y < TrailSparkSize; y++)
        {
            for (int x = 0; x < TrailSparkSize; x++)
            {
                var p = Centered(x, y, TrailSparkSize);
                float distance = ChamferedRectDistance(p, half, half, TrailSparkChamfer);
                float coverage = Mathf.Clamp01(0.5f - distance);
                pixels[y * TrailSparkSize + x] = WithAlpha(Gray(1f), coverage);
            }
        }

        return pixels;
    }

    static Vector2 Centered(int x, int y, int size) =>
        new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f);

    static float ChamferedRectDistance(Vector2 point, float halfWidth, float halfHeight, float chamfer)
    {
        float ax = Mathf.Abs(point.x);
        float ay = Mathf.Abs(point.y);

        float rectDistance = Mathf.Max(ax - halfWidth, ay - halfHeight);
        float diagDistance = (ax - halfWidth + chamfer + ay - halfHeight + chamfer) * 0.70710678f;

        return Mathf.Max(rectDistance, diagDistance);
    }

    static Color32 Gray(float value)
    {
        byte channel = (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        return new Color32(channel, channel, channel, 255);
    }

    static Color32 Transparent() => new Color32(0, 0, 0, 0);

    static Color32 WithAlpha(Color32 color, float alpha)
    {
        color.a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f);
        return color;
    }

    static void SavePng(string path, int width, int height, Color32[] pixels, int spriteBorder)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();

        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(spriteBorder, spriteBorder, spriteBorder, spriteBorder);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
}
