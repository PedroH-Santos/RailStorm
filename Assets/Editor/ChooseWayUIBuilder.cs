using System.Linq;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ChooseWayUIBuilder
{
    const string ArtFolder = "Assets/UI/ChooseWay";
    const string ThemeFolder = "Assets/UI/Theme";
    const string CoinIconPath = "Assets/SkymonIconPackFree/skymon-icons-white/coin2.png";
    const string CaretLeftPath = "Assets/SkymonIconPackFree/skymon-icons-white/caret-left.png";
    const string CaretRightPath = "Assets/SkymonIconPackFree/skymon-icons-white/caret-right.png";
    const string LockPath = "Assets/SkymonIconPackFree/skymon-icons-white/padlock-locked.png";

    const float BarWidth = 1240f;
    const float BarHeight = 260f;
    const float BarBottomOffset = 48f;

    const float CrestColumnWidth = 220f;
    const float InfoColumnWidth = 660f;
    const float CostColumnWidth = 320f;
    const float ColumnPadding = 30f;
    const float ColumnSpacing = 16f;

    const float TitleFontSize = 52f;
    const float DescriptionFontSize = 28f;
    const float PipSize = 22f;
    const float PipSpacing = 14f;

    const float ShieldSize = 150f;
    const float ShieldIconSize = 80f;

    const float CostTagWidth = 220f;
    const float CostTagHeight = 76f;
    const float CostFontSize = 46f;
    const float WalletFontSize = 22f;
    const float UnlockButtonWidth = 260f;
    const float UnlockButtonHeight = 74f;
    const float UnlockLabelFontSize = 32f;

    const float ArrowSize = 84f;
    const float ArrowOffsetX = 700f;

    const float HintsFontSize = 22f;

    const float BadgeCanvasWidth = 260f;
    const float BadgeCanvasHeight = 320f;
    const float BadgeCanvasHeightOverTotem = 3.4f;
    const float BadgeShieldSize = 140f;
    const float BadgeIconSize = 74f;
    const float BadgeLockSize = 42f;
    const float BadgeCostTagWidth = 150f;
    const float BadgeCostTagHeight = 54f;
    const float BadgeCostFontSize = 30f;
    const float BadgePostWidth = 26f;
    const float BadgePostHeight = 90f;

    [MenuItem("Tools/RailStorm/Reconstruir UI dos totens de caminho")]
    public static void Rebuild()
    {
        var barCanvas = FindCanvas("CanvasChooseSpline");
        if (barCanvas == null)
        {
            Debug.LogWarning("[ChooseWayUI] GameObject 'CanvasChooseSpline' nao encontrado na cena aberta.");
            return;
        }

        BuildBottomBar(barCanvas);

        BuildPathParticles();

        var views = Object.FindObjectsByType<TotemView>(FindObjectsInactive.Include);
        foreach (var view in views)
            BuildBadge(view);

        EditorSceneManager.MarkSceneDirty(barCanvas.gameObject.scene);
        Debug.Log($"[ChooseWayUI] Barra inferior, {views.Length} selo(s) de totem e o sistema de particulas do trilho foram reconstruidos.");
    }

    static Canvas FindCanvas(string name)
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var canvas in canvases)
            if (canvas.gameObject.name == name) return canvas;
        return null;
    }

    static void BuildBottomBar(Canvas canvas)
    {
        var canvasTransform = (RectTransform)canvas.transform;
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvasTransform.GetChild(i).gameObject);

        foreach (var stale in canvas.GetComponents<ChooseWayScreenUI>())
            Object.DestroyImmediate(stale);

        var scaler = Ensure<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        Ensure<GraphicRaycaster>(canvas.gameObject);

        var screenUI = canvas.gameObject.AddComponent<ChooseWayScreenUI>();

        var bar = Create("BottomBar", canvasTransform);
        bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.sizeDelta = new Vector2(BarWidth, BarHeight);
        bar.anchoredPosition = new Vector2(0f, BarBottomOffset);
        bar.gameObject.SetActive(false);

        var shadow = Create("Shadow", bar);
        Stretch(shadow, 0f);
        shadow.anchoredPosition = new Vector2(6f, -8f);
        var shadowImage = AddImage(shadow, Sprite(ArtFolder, "SignShadow9Slice"), Image.Type.Sliced, false);

        var fill = Create("Fill", bar);
        Stretch(fill, 0f);
        var fillImage = AddImage(fill, Sprite(ArtFolder, "SignPlate9Slice"), Image.Type.Sliced, true);

        var content = Create("Content", bar);
        Stretch(content, 0f);
        content.offsetMin = new Vector2(ColumnPadding, ColumnPadding);
        content.offsetMax = new Vector2(-ColumnPadding, -ColumnPadding);
        var contentLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        contentLayout.spacing = ColumnSpacing;
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = true;

        var crestColumn = Create("CrestColumn", content);
        SetLayoutSize(crestColumn, CrestColumnWidth, -1f);
        var crestLayout = crestColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        crestLayout.childAlignment = TextAnchor.MiddleCenter;
        crestLayout.childControlWidth = true;
        crestLayout.childControlHeight = true;
        crestLayout.childForceExpandWidth = false;
        crestLayout.childForceExpandHeight = false;

        var shield = Create("Shield", crestColumn);
        SetLayoutSize(shield, ShieldSize, ShieldSize);
        var shieldImage = AddImage(shield, Sprite(ArtFolder, "Shield"), Image.Type.Simple, false);

        var icon = Create("Icon", shield);
        Center(icon, Vector2.zero, new Vector2(ShieldIconSize, ShieldIconSize));
        var iconImage = AddImage(icon, null, Image.Type.Simple, false);
        iconImage.preserveAspect = true;

        var infoColumn = Create("InfoColumn", content);
        SetLayoutSize(infoColumn, InfoColumnWidth, -1f);
        var infoLayout = infoColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 6f;
        infoLayout.childAlignment = TextAnchor.UpperLeft;
        infoLayout.childControlWidth = true;
        infoLayout.childControlHeight = true;
        infoLayout.childForceExpandWidth = true;
        infoLayout.childForceExpandHeight = false;

        var title = CreateText("Title", infoColumn, TitleFontSize, 60f, TextAlignmentOptions.Left);
        title.fontStyle = FontStyles.UpperCase;
        title.text = "CAMINHO DAS MONTANHAS";
        AddShadow(title.gameObject, new Vector2(0f, -2.5f));

        var description = CreateText("Description", infoColumn, DescriptionFontSize, 76f, TextAlignmentOptions.TopLeft);
        description.text = "Terras altas e perigosas.";

        var pipsRow = Create("Pips", infoColumn);
        SetLayoutSize(pipsRow, -1f, PipSize);
        var pipsLayout = pipsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        pipsLayout.spacing = PipSpacing;
        pipsLayout.childAlignment = TextAnchor.MiddleLeft;
        pipsLayout.childControlWidth = false;
        pipsLayout.childControlHeight = false;
        pipsLayout.childForceExpandWidth = false;
        pipsLayout.childForceExpandHeight = false;

        var pipEmptyPrefab = BuildPipPrefab("PipEmptyTemplate", false);
        var pipFullPrefab = BuildPipPrefab("PipFullTemplate", true);

        var costColumn = Create("CostColumn", content);
        SetLayoutSize(costColumn, CostColumnWidth, -1f);
        var costLayout = costColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        costLayout.spacing = 8f;
        costLayout.childAlignment = TextAnchor.MiddleCenter;
        costLayout.childControlWidth = true;
        costLayout.childControlHeight = true;
        costLayout.childForceExpandWidth = false;
        costLayout.childForceExpandHeight = false;

        var costTag = Create("CostTag", costColumn);
        SetLayoutSize(costTag, CostTagWidth, CostTagHeight);
        var costTagImage = AddImage(costTag, Sprite(ArtFolder, "RibbonTag9Slice"), Image.Type.Sliced, false);

        var costRow = Create("CostRow", costTag);
        Stretch(costRow, 0f);
        var costRowLayout = costRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        costRowLayout.spacing = 8f;
        costRowLayout.childAlignment = TextAnchor.MiddleCenter;
        costRowLayout.childControlWidth = true;
        costRowLayout.childControlHeight = true;
        costRowLayout.childForceExpandWidth = false;
        costRowLayout.childForceExpandHeight = false;

        var costIcon = Create("CostIcon", costRow);
        SetLayoutSize(costIcon, 40f, 40f);
        var costIconImage = AddImage(costIcon, LoadSprite(CoinIconPath), Image.Type.Simple, false);
        costIconImage.preserveAspect = true;

        var costText = CreateText("CostText", costRow, CostFontSize, 50f, TextAlignmentOptions.Left);
        SetLayoutSize(costText.rectTransform, 110f, 50f);
        costText.text = "75";
        costText.textWrappingMode = TextWrappingModes.NoWrap;
        AddShadow(costText.gameObject, new Vector2(0f, -2.5f));

        var wallet = CreateText("Wallet", costColumn, WalletFontSize, 28f, TextAlignmentOptions.Center);
        wallet.text = "Você tem 50";

        var unlock = Create("UnlockButton", costColumn);
        SetLayoutSize(unlock, UnlockButtonWidth, UnlockButtonHeight);
        var unlockImage = AddImage(unlock, Sprite(ThemeFolder, "ButtonPlate9Slice"), Image.Type.Sliced, true);
        unlockImage.pixelsPerUnitMultiplier = 1.5f;
        var unlockButton = unlock.gameObject.AddComponent<Button>();
        unlockButton.image = unlockImage;
        unlockButton.transition = Selectable.Transition.ColorTint;
        unlockButton.colors = ActionColors();

        var unlockLabel = CreateText("Label", unlock, UnlockLabelFontSize, 0f, TextAlignmentOptions.Center);
        Stretch(unlockLabel.rectTransform, 0f);
        unlockLabel.rectTransform.offsetMin = new Vector2(0f, 12f);
        unlockLabel.rectTransform.offsetMax = new Vector2(0f, -4f);
        unlockLabel.text = "Desbloquear";
        unlockLabel.textWrappingMode = TextWrappingModes.NoWrap;
        unlockLabel.raycastTarget = false;
        AddShadow(unlockLabel.gameObject, new Vector2(0f, -2.5f));

        var previous = CreateArrowButton("PreviousButton", bar, -1f, LoadSprite(CaretLeftPath));
        var next = CreateArrowButton("NextButton", bar, 1f, LoadSprite(CaretRightPath));

        var hints = CreateText("Hints", bar, HintsFontSize, 0f, TextAlignmentOptions.Center);
        hints.rectTransform.anchorMin = hints.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        hints.rectTransform.pivot = new Vector2(0.5f, 0f);
        hints.rectTransform.sizeDelta = new Vector2(700f, 30f);
        hints.rectTransform.anchoredPosition = new Vector2(0f, -34f);
        hints.text = "[A/D] Trocar   ·   [Enter] Desbloquear   ·   [E] Sair";
        hints.textWrappingMode = TextWrappingModes.NoWrap;

        var animator = bar.gameObject.AddComponent<ChooseWayScreenAnimator>();
        var animatorData = new SerializedObject(animator);
        animatorData.FindProperty("bar").objectReferenceValue = bar;
        animatorData.ApplyModifiedPropertiesWithoutUndo();

        var screenData = new SerializedObject(screenUI);
        screenData.FindProperty("canvasRoot").objectReferenceValue = canvas.gameObject;
        screenData.FindProperty("barRoot").objectReferenceValue = bar.gameObject;
        screenData.FindProperty("plaqueFill").objectReferenceValue = fillImage;
        screenData.FindProperty("plaqueShadow").objectReferenceValue = shadowImage;
        screenData.FindProperty("shield").objectReferenceValue = shieldImage;
        screenData.FindProperty("icon").objectReferenceValue = iconImage;
        screenData.FindProperty("titleText").objectReferenceValue = title;
        screenData.FindProperty("descriptionText").objectReferenceValue = description;
        screenData.FindProperty("costText").objectReferenceValue = costText;
        screenData.FindProperty("costIcon").objectReferenceValue = costIconImage;
        screenData.FindProperty("walletText").objectReferenceValue = wallet;
        screenData.FindProperty("pipsContainer").objectReferenceValue = pipsRow;
        screenData.FindProperty("pipEmptyPrefab").objectReferenceValue = pipEmptyPrefab;
        screenData.FindProperty("pipFullPrefab").objectReferenceValue = pipFullPrefab;
        screenData.FindProperty("unlockButton").objectReferenceValue = unlockButton;
        screenData.FindProperty("unlockLabel").objectReferenceValue = unlockLabel;
        screenData.FindProperty("previousButton").objectReferenceValue = previous;
        screenData.FindProperty("nextButton").objectReferenceValue = next;
        screenData.FindProperty("hintsText").objectReferenceValue = hints;
        screenData.ApplyModifiedPropertiesWithoutUndo();

        screenUI.ApplyTheme();
    }

    static GameObject BuildPipPrefab(string name, bool full)
    {
        var rect = new GameObject(name, typeof(RectTransform));
        rect.hideFlags = HideFlags.HideInHierarchy;
        var rt = (RectTransform)rect.transform;
        rt.sizeDelta = new Vector2(PipSize, PipSize);
        AddImage(rt, Sprite(ArtFolder, full ? "PipFull" : "PipEmpty"), Image.Type.Simple, false);
        return rect;
    }

    static Button CreateArrowButton(string name, RectTransform parent, float side, Sprite glyph)
    {
        var rect = Create(name, parent);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(ArrowSize, ArrowSize);
        rect.anchoredPosition = new Vector2(side * ArrowOffsetX, 10f);

        var image = AddImage(rect, Sprite(ArtFolder, "ChevronPlate"), Image.Type.Simple, true);

        var button = rect.gameObject.AddComponent<Button>();
        button.image = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = ActionColors();

        var arrow = Create("Glyph", rect);
        Center(arrow, Vector2.zero, new Vector2(36f, 36f));
        var arrowImage = AddImage(arrow, glyph, Image.Type.Simple, false);
        arrowImage.preserveAspect = true;

        var theme = UIThemeConfig.Instance;
        if (theme != null) arrowImage.color = theme.textTitle;

        return button;
    }

    static void BuildPathParticles()
    {
        var existing = Object.FindObjectsByType<SplinePathParticles>(FindObjectsInactive.Include).FirstOrDefault();
        GameObject root = existing != null ? existing.gameObject : new GameObject("SplinePathParticles");

        foreach (Transform child in root.transform)
            Object.DestroyImmediate(child.gameObject);

        foreach (var stale in root.GetComponents<SplinePathParticles>())
            Object.DestroyImmediate(stale);

        var psObject = new GameObject("Particles");
        psObject.transform.SetParent(root.transform, false);
        var ps = psObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        main.startLifetime = 0.85f;
        main.startSize = 0.35f;
        main.startSpeed = 0f;
        main.gravityModifier = -0.05f;
        main.loop = true;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var renderer = psObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");

        var material = new Material(shader);
        var sparkTexture = LoadSprite(ArtFolder + "/TrailSpark.png");
        if (sparkTexture != null && material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", sparkTexture.texture);
        else if (sparkTexture != null && material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", sparkTexture.texture);

        string materialPath = ArtFolder + "/TrailSparkMaterial.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
            AssetDatabase.DeleteAsset(materialPath);
        AssetDatabase.CreateAsset(material, materialPath);
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        var script = root.AddComponent<SplinePathParticles>();
        var data = new SerializedObject(script);
        data.FindProperty("particles").objectReferenceValue = ps;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildBadge(TotemView view)
    {
        var canvasTransform = view.transform.Find("BadgeCanvas");
        GameObject canvasObject;

        if (canvasTransform != null)
        {
            canvasObject = canvasTransform.gameObject;
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(canvasTransform.GetChild(i).gameObject);
        }
        else
        {
            canvasObject = new GameObject("BadgeCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(view.transform, false);
        }

        foreach (var stale in canvasObject.GetComponents<ChooseWayTotemBadge>())
            Object.DestroyImmediate(stale);
        foreach (var stale in canvasObject.GetComponents<ChooseWayBadgeAnimator>())
            Object.DestroyImmediate(stale);

        var canvas = Ensure<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.WorldSpace;

        var scaler = Ensure<CanvasScaler>(canvasObject);
        scaler.dynamicPixelsPerUnit = 3f;

        var canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(BadgeCanvasWidth, BadgeCanvasHeight);
        canvasRect.localScale = Vector3.one * 0.01f;
        canvasRect.localPosition = new Vector3(0f, BadgeCanvasHeightOverTotem, 0f);
        canvasRect.localEulerAngles = new Vector3(30f, 45f, 0f);

        var root = Create("Root", canvasRect);
        Stretch(root, 0f);

        var post = Create("Post", root);
        post.anchorMin = post.anchorMax = new Vector2(0.5f, 0f);
        post.pivot = new Vector2(0.5f, 0f);
        post.sizeDelta = new Vector2(BadgePostWidth, BadgePostHeight);
        post.anchoredPosition = Vector2.zero;
        AddImage(post, Sprite(ArtFolder, "PostPlank"), Image.Type.Simple, false);

        var plate = Create("Plate", root);
        plate.anchorMin = plate.anchorMax = new Vector2(0.5f, 0f);
        plate.pivot = new Vector2(0.5f, 0f);
        plate.sizeDelta = new Vector2(BadgeCanvasWidth, BadgeCanvasHeight - BadgePostHeight);
        plate.anchoredPosition = new Vector2(0f, BadgePostHeight - 6f);

        var shield = Create("Shield", plate);
        shield.anchorMin = shield.anchorMax = new Vector2(0.5f, 1f);
        shield.pivot = new Vector2(0.5f, 1f);
        shield.sizeDelta = new Vector2(BadgeShieldSize, BadgeShieldSize);
        shield.anchoredPosition = Vector2.zero;
        var shieldImage = AddImage(shield, Sprite(ArtFolder, "Shield"), Image.Type.Simple, false);

        var icon = Create("Icon", shield);
        Center(icon, Vector2.zero, new Vector2(BadgeIconSize, BadgeIconSize));
        var iconImage = AddImage(icon, null, Image.Type.Simple, false);
        iconImage.preserveAspect = true;

        var lockIcon = Create("Lock", shield);
        lockIcon.anchorMin = lockIcon.anchorMax = new Vector2(1f, 0f);
        lockIcon.pivot = new Vector2(1f, 0f);
        lockIcon.sizeDelta = new Vector2(BadgeLockSize, BadgeLockSize);
        lockIcon.anchoredPosition = new Vector2(6f, -6f);
        var lockImage = AddImage(lockIcon, LoadSprite(LockPath), Image.Type.Simple, false);
        var theme = UIThemeConfig.Instance;
        if (theme != null) lockImage.color = theme.woodOutline;

        var costTag = Create("CostTag", plate);
        costTag.anchorMin = costTag.anchorMax = new Vector2(0.5f, 1f);
        costTag.pivot = new Vector2(0.5f, 1f);
        costTag.sizeDelta = new Vector2(BadgeCostTagWidth, BadgeCostTagHeight);
        costTag.anchoredPosition = new Vector2(0f, -BadgeShieldSize - 10f);
        AddImage(costTag, Sprite(ArtFolder, "RibbonTag9Slice"), Image.Type.Sliced, false);

        var costRow = Create("CostRow", costTag);
        Stretch(costRow, 0f);
        var costRowLayout = costRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        costRowLayout.spacing = 6f;
        costRowLayout.childAlignment = TextAnchor.MiddleCenter;
        costRowLayout.childControlWidth = true;
        costRowLayout.childControlHeight = true;
        costRowLayout.childForceExpandWidth = false;
        costRowLayout.childForceExpandHeight = false;

        var costIcon = Create("CostIcon", costRow);
        SetLayoutSize(costIcon, 28f, 28f);
        var costIconImage = AddImage(costIcon, LoadSprite(CoinIconPath), Image.Type.Simple, false);
        costIconImage.preserveAspect = true;

        var costText = CreateText("CostText", costRow, BadgeCostFontSize, 40f, TextAlignmentOptions.Left);
        SetLayoutSize(costText.rectTransform, 80f, 40f);
        costText.text = "75";
        costText.textWrappingMode = TextWrappingModes.NoWrap;

        var animator = root.gameObject.AddComponent<ChooseWayBadgeAnimator>();
        var animatorData = new SerializedObject(animator);
        animatorData.FindProperty("plate").objectReferenceValue = plate;
        animatorData.ApplyModifiedPropertiesWithoutUndo();

        var badge = canvasObject.AddComponent<ChooseWayTotemBadge>();
        var badgeData = new SerializedObject(badge);
        badgeData.FindProperty("root").objectReferenceValue = root.gameObject;
        badgeData.FindProperty("shield").objectReferenceValue = shieldImage;
        badgeData.FindProperty("icon").objectReferenceValue = iconImage;
        badgeData.FindProperty("lock_").objectReferenceValue = lockImage;
        badgeData.FindProperty("costText").objectReferenceValue = costText;
        badgeData.FindProperty("costIcon").objectReferenceValue = costIconImage;
        badgeData.ApplyModifiedPropertiesWithoutUndo();

        var viewData = new SerializedObject(view);
        viewData.FindProperty("badge").objectReferenceValue = badge;
        viewData.ApplyModifiedPropertiesWithoutUndo();

        badge.ApplyTheme();
        root.gameObject.SetActive(false);
    }

    static ColorBlock ActionColors()
    {
        var colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.16f, 1.16f, 1.16f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        colors.fadeDuration = 0.06f;
        return colors;
    }

    static TextMeshProUGUI CreateText(string name, RectTransform parent, float fontSize, float preferredHeight, TextAlignmentOptions alignment)
    {
        var rect = Create(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.color = Color.white;

        if (preferredHeight > 0f) SetLayoutSize(rect, -1f, preferredHeight);

        return text;
    }

    static void AddShadow(GameObject target, Vector2 distance)
    {
        var shadow = target.AddComponent<Shadow>();
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    static void SetLayoutSize(RectTransform rect, float width, float height)
    {
        var element = rect.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
        if (width > 0f)
        {
            element.preferredWidth = width;
            element.flexibleWidth = 0f;
        }
        if (height > 0f)
        {
            element.preferredHeight = height;
            element.flexibleHeight = 0f;
        }
    }

    static RectTransform Create(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }

    static Image AddImage(RectTransform rect, Sprite sprite, Image.Type type, bool raycastTarget)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.raycastTarget = raycastTarget;
        return image;
    }

    static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    static void Center(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    static T Ensure<T>(GameObject target) where T : Component
    {
        var component = target.GetComponent<T>();
        if (component == null) component = target.AddComponent<T>();
        return component;
    }

    static Sprite Sprite(string folder, string fileName) => LoadSprite($"{folder}/{fileName}.png");

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogWarning($"[ChooseWayUI] Sprite nao encontrado: {path}");
        return sprite;
    }
}
