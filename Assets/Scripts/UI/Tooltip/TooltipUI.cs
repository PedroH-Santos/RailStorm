using System.Collections.Generic;
using Assets.Scripts.Systems.UITheme;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    static TooltipUI _instance;

    public static bool HasInstance => _instance != null;

    public static TooltipUI Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<TooltipUI>(FindObjectsInactive.Include);
                if (found != null && !found.gameObject.activeSelf)
                    found.gameObject.SetActive(true);
                if (_instance == null && found != null)
                    _instance = found;
            }
            return _instance;
        }
    }

    [Header("Painel")]
    public GameObject panel;
    public RectTransform panelRect;

    [Header("Cabeçalho")]
    public TMP_Text headerText;
    public Image iconImage;
    public Image iconBackground;
    public TMP_Text titleText;
    public TMP_Text rarityText;

    [Header("Corpo")]
    public TMP_Text descriptionText;
    public Transform statsContainer;
    public GameObject statRowPrefab;

    [Header("Habilidade Especial")]
    public GameObject abilitySection;
    public TMP_Text abilityNameText;
    public TMP_Text abilityDescriptionText;

    [Header("Melhorias")]
    public GameObject upgradesSection;
    public Transform upgradesContainer;
    public GameObject upgradeRowPrefab;
    public TMP_Text upgradesOverflowText;

    [Header("Posicionamento")]
    public Vector2 cursorOffset = new Vector2(22f, 22f);

    readonly List<TooltipStatRowUI> _rows = new();
    readonly List<TooltipUpgradeRowUI> _upgradeRows = new();
    Canvas _canvas;
    RectTransform _canvasRect;

    void Awake()
    {
        _instance = this;
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
        {
            _canvas = _canvas.rootCanvas;
            _canvasRect = _canvas.transform as RectTransform;
        }

        if (statRowPrefab != null) statRowPrefab.SetActive(false);
        if (upgradeRowPrefab != null) upgradeRowPrefab.SetActive(false);
        if (abilitySection != null) abilitySection.SetActive(false);
        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public void Show(TooltipData data)
    {
        if (data == null || panel == null) return;

        var theme = UIThemeConfig.Instance;

        if (headerText != null)
        {
            headerText.text = data.HeaderLabel;
            theme?.ApplyTitle(headerText);
        }

        if (titleText != null)
        {
            titleText.text = data.Title;
            theme?.ApplyTitle(titleText);
        }

        if (rarityText != null)
        {
            rarityText.text = data.RarityLabel;
            rarityText.color = data.RarityColor;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = data.Icon != null;
        }

        if (iconBackground != null)
            iconBackground.color = data.RarityColor;

        if (descriptionText != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(data.Description);
            descriptionText.gameObject.SetActive(hasDescription);
            descriptionText.text = data.Description;
            theme?.ApplyBody(descriptionText);
        }

        BuildAbility(data);
        BuildRows(data.Stats);
        BuildUpgradeRows(data.Upgrades, data.HiddenUpgrades);

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        FollowCursor();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void LateUpdate()
    {
        if (panel != null && panel.activeSelf) FollowCursor();
    }

    void BuildRows(List<TooltipStatLine> lines)
    {
        if (statsContainer == null || statRowPrefab == null) return;

        while (_rows.Count < lines.Count)
        {
            var row = Instantiate(statRowPrefab, statsContainer).GetComponent<TooltipStatRowUI>();
            _rows.Add(row);
        }

        for (int i = 0; i < _rows.Count; i++)
        {
            bool used = i < lines.Count;
            _rows[i].gameObject.SetActive(used);
            if (used) _rows[i].Setup(lines[i]);
        }
    }

    void BuildAbility(TooltipData data)
    {
        if (abilitySection != null)
            abilitySection.SetActive(data.HasAbility);

        if (!data.HasAbility) return;

        var theme = UIThemeConfig.Instance;

        if (abilityNameText != null)
        {
            bool hasName = !string.IsNullOrWhiteSpace(data.AbilityName);
            abilityNameText.gameObject.SetActive(hasName);
            abilityNameText.text = data.AbilityName;
            theme?.ApplyTitle(abilityNameText);
        }

        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.text = data.AbilityDescription;
            theme?.ApplyBody(abilityDescriptionText);
        }
    }

    void BuildUpgradeRows(List<TooltipUpgradeLine> lines, int hidden)
    {
        if (upgradesSection != null)
            upgradesSection.SetActive(lines.Count > 0);

        if (upgradesOverflowText != null)
        {
            upgradesOverflowText.gameObject.SetActive(hidden > 0);
            if (hidden > 0)
            {
                upgradesOverflowText.text = hidden == 1 ? "+1 melhoria" : $"+{hidden} melhorias";
                UIThemeConfig.Instance?.ApplyBody(upgradesOverflowText);
            }
        }

        if (upgradesContainer == null || upgradeRowPrefab == null) return;

        while (_upgradeRows.Count < lines.Count)
        {
            var row = Instantiate(upgradeRowPrefab, upgradesContainer).GetComponent<TooltipUpgradeRowUI>();
            _upgradeRows.Add(row);
        }

        for (int i = 0; i < _upgradeRows.Count; i++)
        {
            bool used = i < lines.Count;
            _upgradeRows[i].gameObject.SetActive(used);
            if (used) _upgradeRows[i].Setup(lines[i]);
        }
    }

    void FollowCursor()
    {
        if (panelRect == null || _canvasRect == null || Mouse.current == null) return;

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Camera camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPosition, camera, out var local))
            return;

        float pivotX = screenPosition.x > Screen.width * 0.5f ? 1f : 0f;
        float pivotY = screenPosition.y > Screen.height * 0.5f ? 1f : 0f;

        panelRect.pivot = new Vector2(pivotX, pivotY);
        panelRect.anchoredPosition = local + new Vector2(
            pivotX > 0.5f ? -cursorOffset.x : cursorOffset.x,
            pivotY > 0.5f ? -cursorOffset.y : cursorOffset.y);
    }
}
