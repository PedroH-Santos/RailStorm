using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Systems.Rarity;
using UnityEditor;
using UnityEngine;

public static class RarityIconArtGenerator
{
    const string OutputFolder = "Assets/UI/Theme/Rarity";
    const int PlateSize = 64;
    const int PlateSpriteBorder = 26;
    const int GlowSize = 256;

    const float PlateCornerRadius = 12f;
    const float PlateMargin = 2f;
    const float ContourDepth = 2f;
    const float RimDepth = 5.6f;
    const float ContourValue = 0.10f;
    const float RimValue = 1f;
    const float BodyTopValue = 0.30f;
    const float BodyBottomValue = 0.15f;
    const float DetailValue = 0.58f;
    const float GemValue = 0.94f;
    const float DetailInset = 21f;

    static readonly string[] RarityIds = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };

    [MenuItem("Tools/RailStorm/Gerar placas e brilhos de raridade")]
    public static void Generate()
    {
        Directory.CreateDirectory(OutputFolder);

        for (int i = 0; i < RarityIds.Length; i++)
        {
            SavePng(PlatePath(i), BuildPlate(i), PlateSpriteBorder);
            SavePng(GlowPath(i), BuildGlow(i), 0);
        }

        AssetDatabase.Refresh();
        AssignToRarityConfig();
        Debug.Log("[RarityIconArt] Placas e brilhos de raridade regenerados.");
    }

    static string PlatePath(int i) => $"{OutputFolder}/IconPlate{RarityIds[i]}.png";

    static string GlowPath(int i) => $"{OutputFolder}/IconGlow{RarityIds[i]}.png";

    static void AssignToRarityConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<RarityConfig>("Assets/Resources/RarityConfig.asset");
        if (config == null)
        {
            Debug.LogWarning("[RarityIconArt] Resources/RarityConfig.asset nao encontrado, sprites nao foram atribuidos.");
            return;
        }

        for (int i = 0; i < RarityIds.Length && i < config.rarities.Count; i++)
        {
            config.rarities[i].iconPlate = AssetDatabase.LoadAssetAtPath<Sprite>(PlatePath(i));
            config.rarities[i].iconGlow = AssetDatabase.LoadAssetAtPath<Sprite>(GlowPath(i));
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }

    static void SavePng(string path, Color32[] pixels, int spriteBorder)
    {
        int size = Mathf.RoundToInt(Mathf.Sqrt(pixels.Length));
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
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

    static Color32[] BuildPlate(int rarityIndex)
    {
        int size = PlateSize;
        var pixels = new Color32[size * size];
        float half = size * 0.5f;
        float extent = half - PlateMargin;

        for (int y = 0; y < size; y++)
        {
            float tDown = 1f - (y + 0.5f) / size;

            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - half;
                float py = y + 0.5f - half;
                float sd = RoundedBoxDistance(px, py, extent, extent, PlateCornerRadius);

                float value = Mathf.Lerp(BodyTopValue, BodyBottomValue, tDown);
                value = Mathf.Lerp(value, RimValue, Step(sd, -RimDepth, 1f));
                value = Mathf.Lerp(value, ContourValue, Step(sd, -ContourDepth, 1f));

                float insideBody = 1f - Step(sd, -RimDepth - 1f, 1f);
                PlateDetail(rarityIndex, Mathf.Abs(px), Mathf.Abs(py), sd, out float detailCoverage, out float detailValue);
                value = Mathf.Lerp(value, detailValue, detailCoverage * insideBody);

                float alpha = 1f - Step(sd, 0f, 1f);
                pixels[y * size + x] = ToColor32(value, alpha);
            }
        }

        return pixels;
    }

    static void PlateDetail(int rarityIndex, float ax, float ay, float sd, out float coverage, out float value)
    {
        coverage = 0f;
        value = DetailValue;
        if (rarityIndex <= 0) return;

        coverage = LineCoverage(Mathf.Abs(sd + 9f), 0.7f);

        if (rarityIndex == 2)
            coverage = Mathf.Max(coverage, CornerTriangle(ax, ay, 7f));

        if (rarityIndex == 3)
            coverage = Mathf.Max(coverage, CornerBracket(ax, ay, 11f, 1.5f));

        if (rarityIndex == 4)
        {
            coverage = Mathf.Max(coverage, CornerBracket(ax, ay, 13f, 1.6f));

            float gem = CornerDiamond(ax, ay, 3.2f);
            if (gem > 0f)
            {
                value = Mathf.Lerp(DetailValue, GemValue, gem);
                coverage = Mathf.Max(coverage, gem);
            }
        }
    }

    static float CornerTriangle(float ax, float ay, float leg)
        => Saturate(0.5f - (ax - DetailInset)) * Saturate(0.5f - (ay - DetailInset)) * Saturate((ax - DetailInset) + (ay - DetailInset) + leg + 0.5f);

    static float CornerBracket(float ax, float ay, float arm, float halfThickness)
    {
        float horizontal = SegmentCoverage(ax, DetailInset - arm, DetailInset, ay - DetailInset, halfThickness);
        float vertical = SegmentCoverage(ay, DetailInset - arm, DetailInset, ax - DetailInset, halfThickness);
        return Mathf.Max(horizontal, vertical);
    }

    static float CornerDiamond(float ax, float ay, float size)
        => Saturate(size + 0.5f - (Mathf.Abs(ax - DetailInset) + Mathf.Abs(ay - DetailInset)));

    static float SegmentCoverage(float along, float from, float to, float across, float halfThickness)
        => Saturate(along - from + 0.5f) * Saturate(to - along + halfThickness + 0.5f) * Saturate(halfThickness + 0.5f - Mathf.Abs(across));

    static float LineCoverage(float distance, float halfThickness)
        => Saturate(halfThickness + 0.5f - distance);

    struct GlowCircle
    {
        public Vector2 center;
        public float radius;
        public float alpha;
        public bool ring;
    }

    struct GlowSparkle
    {
        public Vector2 center;
        public float length;
        public float alpha;
    }

    struct GlowRayBundle
    {
        public int count;
        public float phase;
        public float sharpness;
        public float alpha;
        public float innerRadius;
        public float outerRadius;
    }

    static Color32[] BuildGlow(int rarityIndex)
    {
        int size = GlowSize;
        var pixels = new Color32[size * size];
        var random = new System.Random(4300 + rarityIndex * 91);

        float coreRadius = new[] { 0.46f, 0.47f, 0.48f, 0.50f, 0.52f }[rarityIndex];
        float corePeak = new[] { 0.55f, 0.62f, 0.68f, 0.76f, 0.88f }[rarityIndex];
        float coreExponent = new[] { 2.0f, 2.0f, 1.9f, 1.8f, 1.7f }[rarityIndex];
        float haloRadius = new[] { 0f, 0f, 0f, 0.36f, 0.33f }[rarityIndex];
        float haloAlpha = new[] { 0f, 0f, 0f, 0.28f, 0.30f }[rarityIndex];

        var circles = BuildGlowCircles(rarityIndex, random);
        var sparkles = BuildGlowSparkles(rarityIndex, random);
        var rayBundles = BuildGlowRayBundles(rarityIndex);

        for (int y = 0; y < size; y++)
        {
            float py = (y + 0.5f) / size - 0.5f;

            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f) / size - 0.5f;
                float distance = Mathf.Sqrt(px * px + py * py);
                float angle = Mathf.Atan2(py, px);

                float alpha = corePeak * Mathf.Pow(Saturate(1f - distance / coreRadius), coreExponent);
                alpha *= 1f - 0.16f * (1f - Smooth(distance / 0.10f));

                foreach (var bundle in rayBundles)
                {
                    float lobe = Mathf.Pow(0.5f + 0.5f * Mathf.Cos(angle * bundle.count + bundle.phase), bundle.sharpness);
                    float radial = Smooth(distance / bundle.innerRadius) * (1f - Smooth((distance - bundle.innerRadius) / (bundle.outerRadius - bundle.innerRadius)));
                    alpha += bundle.alpha * lobe * radial;
                }

                if (haloAlpha > 0f)
                    alpha += haloAlpha * Saturate(1f - Mathf.Abs(distance - haloRadius) / 0.014f);

                foreach (var circle in circles)
                {
                    float d = Vector2.Distance(new Vector2(px, py), circle.center);
                    alpha += circle.ring
                        ? circle.alpha * Saturate(1f - Mathf.Abs(d - circle.radius) / (circle.radius * 0.4f))
                        : circle.alpha * Mathf.Pow(Saturate(1f - d / circle.radius), 1.6f);
                }

                foreach (var sparkle in sparkles)
                {
                    float dx = Mathf.Abs(px - sparkle.center.x);
                    float dy = Mathf.Abs(py - sparkle.center.y);
                    float thin = sparkle.length * 0.14f;
                    float horizontal = Saturate(1f - dx / sparkle.length) * Saturate(1f - dy / thin);
                    float vertical = Saturate(1f - dy / sparkle.length) * Saturate(1f - dx / thin);
                    alpha += sparkle.alpha * Mathf.Max(horizontal * horizontal, vertical * vertical);
                }

                alpha *= 1f - Smooth((distance - 0.36f) / 0.12f);
                pixels[y * size + x] = ToColor32(1f, Saturate(alpha));
            }
        }

        return pixels;
    }

    static List<GlowCircle> BuildGlowCircles(int rarityIndex, System.Random random)
    {
        var circles = new List<GlowCircle>();

        int count = new[] { 0, 18, 9, 7, 6 }[rarityIndex];
        float minRadius = new[] { 0f, 0.008f, 0.026f, 0.045f, 0.030f }[rarityIndex];
        float maxRadius = new[] { 0f, 0.015f, 0.042f, 0.072f, 0.055f }[rarityIndex];
        float ringChance = new[] { 0f, 0f, 0.45f, 0.55f, 0.5f }[rarityIndex];
        float alpha = new[] { 0f, 0.55f, 0.45f, 0.38f, 0.42f }[rarityIndex];

        for (int i = 0; i < count; i++)
        {
            float angle = (i + (float)random.NextDouble() * 0.7f) / count * Mathf.PI * 2f;
            float orbit = Mathf.Lerp(0.20f, 0.33f, (float)random.NextDouble());

            circles.Add(new GlowCircle
            {
                center = new Vector2(Mathf.Cos(angle) * orbit, Mathf.Sin(angle) * orbit),
                radius = Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble()),
                alpha = alpha,
                ring = random.NextDouble() < ringChance
            });
        }

        return circles;
    }

    static List<GlowSparkle> BuildGlowSparkles(int rarityIndex, System.Random random)
    {
        var sparkles = new List<GlowSparkle>();

        int count = new[] { 0, 0, 0, 4, 6 }[rarityIndex];
        float length = new[] { 0f, 0f, 0f, 0.075f, 0.100f }[rarityIndex];
        float alpha = new[] { 0f, 0f, 0f, 0.55f, 0.75f }[rarityIndex];

        for (int i = 0; i < count; i++)
        {
            float angle = (i + 0.35f) / count * Mathf.PI * 2f;
            float orbit = Mathf.Lerp(0.19f, 0.28f, (float)random.NextDouble());

            sparkles.Add(new GlowSparkle
            {
                center = new Vector2(Mathf.Cos(angle) * orbit, Mathf.Sin(angle) * orbit),
                length = length * Mathf.Lerp(0.75f, 1.15f, (float)random.NextDouble()),
                alpha = alpha
            });
        }

        return sparkles;
    }

    static List<GlowRayBundle> BuildGlowRayBundles(int rarityIndex)
    {
        var bundles = new List<GlowRayBundle>();

        if (rarityIndex == 4)
        {
            bundles.Add(new GlowRayBundle { count = 10, phase = 0f, sharpness = 8f, alpha = 0.35f, innerRadius = 0.07f, outerRadius = 0.38f });
            bundles.Add(new GlowRayBundle { count = 10, phase = Mathf.PI, sharpness = 14f, alpha = 0.22f, innerRadius = 0.06f, outerRadius = 0.24f });
        }

        return bundles;
    }

    static float RoundedBoxDistance(float px, float py, float extentX, float extentY, float radius)
    {
        float qx = Mathf.Abs(px) - (extentX - radius);
        float qy = Mathf.Abs(py) - (extentY - radius);
        float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
        return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
    }

    static Color32 ToColor32(float value, float alpha)
    {
        byte channel = (byte)Mathf.RoundToInt(Saturate(value) * 255f);
        return new Color32(channel, channel, channel, (byte)Mathf.RoundToInt(Saturate(alpha) * 255f));
    }

    static float Step(float x, float edge, float width)
        => Smooth((x - (edge - width * 0.5f)) / width);

    static float Smooth(float t)
    {
        t = Saturate(t);
        return t * t * (3f - 2f * t);
    }

    static float Saturate(float v) => Mathf.Clamp01(v);
}
