using UnityEngine.Serialization;
using DG.Tweening;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    [Header("Fontes de dados")]
    [SerializeField] private PlayerStatsAggregator stats;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private PlayerCarWeaponHandler weaponHandler;
    [FormerlySerializedAs("skillHandler")][SerializeField] private PlayerPerkHandler perkHandler;
    [SerializeField] private PlayerItemHandler itemHandler;
    [SerializeField] private PlayerSkillHandler playerSkillHandler;

    [Header("Contadores")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text waveText;

    [Header("Vida")]
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [Range(0f, 1f)]
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float healthFillDuration = 0.25f;

    [Header("Wave")]
    [SerializeField] private string waveFormat = "WAVE {0}";
    [SerializeField] private string waveClearedFormat = "WAVE {0} LIMPA";

    [Header("Mochila")]
    [SerializeField] private Button backpackButton;
    [SerializeField] private TMP_Text backpackKeyText;
    [SerializeField] private string keyboardKeyLabel = "I";
    [SerializeField] private string gamepadKeyLabel = "Y";
    [SerializeField] private float backpackHoverScale = 1.08f;

    [Header("Animação")]
    [SerializeField] private float valuePunch = 0.25f;
    [SerializeField] private float hideFadeDuration = 0.15f;

    CanvasGroup _group;
    Color _healthColor;
    Tween _lowHealthPulse;
    Tween _damageShake;
    Tween _visibilityTween;
    bool _hidden;

    int _coins = int.MinValue;
    int _kills = int.MinValue;
    int _hp = int.MinValue;
    int _maxHp = int.MinValue;
    int _seconds = int.MinValue;
    int _waveNumber = int.MinValue;
    bool _waveCleared;
    bool _usingGamepad;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();

        if (stats == null) stats = FindFirstObjectByType<PlayerStatsAggregator>();
        if (spawner == null) spawner = FindFirstObjectByType<EnemySpawner>();
        if (weaponHandler == null) weaponHandler = FindFirstObjectByType<PlayerCarWeaponHandler>();
        if (perkHandler == null) perkHandler = FindFirstObjectByType<PlayerPerkHandler>();
        if (itemHandler == null) itemHandler = FindFirstObjectByType<PlayerItemHandler>();
        if (playerSkillHandler == null) playerSkillHandler = FindFirstObjectByType<PlayerSkillHandler>();

        if (healthFill != null) _healthColor = healthFill.color;

        if (backpackButton != null)
        {
            backpackButton.onClick.AddListener(OpenInventory);
            AddHover(backpackButton.gameObject);
        }

        if (backpackKeyText != null) backpackKeyText.text = keyboardKeyLabel;
    }

    void OnEnable()
    {
        EnemySpawner.OnWaveStarted += HandleWaveChanged;
        EnemySpawner.OnWaveCleared += HandleWaveChanged;
        if (RunTracker.Instance != null) RunTracker.Instance.OnKillsChanged += HandleKillsChanged;
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged += PunchBackpack;
        if (perkHandler != null) perkHandler.OnPerksChanged += PunchBackpack;
        if (itemHandler != null) itemHandler.OnItemsChanged += PunchBackpack;
        if (playerSkillHandler != null) playerSkillHandler.OnSkillsChanged += PunchBackpack;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveStarted -= HandleWaveChanged;
        EnemySpawner.OnWaveCleared -= HandleWaveChanged;
        if (RunTracker.Instance != null) RunTracker.Instance.OnKillsChanged -= HandleKillsChanged;
        if (weaponHandler != null) weaponHandler.OnWeaponsChanged -= PunchBackpack;
        if (perkHandler != null) perkHandler.OnPerksChanged -= PunchBackpack;
        if (itemHandler != null) itemHandler.OnItemsChanged -= PunchBackpack;
        if (playerSkillHandler != null) playerSkillHandler.OnSkillsChanged -= PunchBackpack;
        _lowHealthPulse = null;
    }

    void LateUpdate()
    {
        UpdateVisibility();
        UpdateHealth();
        UpdateCoins();
        UpdateKills(false);
        UpdateTimer();
        UpdateWave(false);
        UpdateKeyLabel();
    }

    void UpdateVisibility()
    {
        bool shouldHide = Time.timeScale <= 0f;
        if (shouldHide == _hidden) return;

        _hidden = shouldHide;
        _visibilityTween?.Kill();
        _visibilityTween = _group.DOFade(shouldHide ? 0f : 1f, hideFadeDuration).AsUI(gameObject);
        _group.blocksRaycasts = !shouldHide;
    }

    void UpdateHealth()
    {
        if (stats == null) return;

        int hp = stats.HP;
        int maxHp = stats.MaxHP;
        if (hp == _hp && maxHp == _maxHp) return;

        bool tookDamage = _hp != int.MinValue && hp < _hp;
        bool firstFrame = _hp == int.MinValue;
        _hp = hp;
        _maxHp = maxHp;

        float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;

        if (healthText != null) healthText.text = $"{hp} / {maxHp}";

        if (healthFill != null)
        {
            var fillRect = healthFill.rectTransform;
            healthFill.DOKill();
            fillRect.DOKill();
            if (firstFrame) SetFillRatio(fillRect, ratio);
            else DOTween.To(() => fillRect.anchorMax.x, value => SetFillRatio(fillRect, value), ratio, healthFillDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(fillRect)
                .AsUI(healthFill.gameObject);

            if (tookDamage)
            {
                healthFill.color = Color.white;
                healthFill.DOColor(_healthColor, 0.2f).AsUI(healthFill.gameObject);
            }
        }

        if (tookDamage && healthBar != null)
        {
            _damageShake?.Complete();
            _damageShake = healthBar.DOShakeAnchorPos(0.25f, 6f, 18, 90f, false, true).AsUI(healthBar.gameObject);
        }

        SetLowHealthPulse(ratio <= lowHealthThreshold && hp > 0);
    }

    static void SetFillRatio(RectTransform fillRect, float ratio)
    {
        fillRect.anchorMax = new Vector2(ratio, fillRect.anchorMax.y);
        fillRect.gameObject.SetActive(ratio > 0.001f);
    }

    void SetLowHealthPulse(bool active)
    {
        if (healthBar == null) return;

        if (active && _lowHealthPulse == null)
        {
            _lowHealthPulse = healthBar.DOScale(1.05f, 0.35f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .AsUI(healthBar.gameObject);
        }
        else if (!active && _lowHealthPulse != null)
        {
            _lowHealthPulse.Kill();
            _lowHealthPulse = null;
            healthBar.localScale = Vector3.one;
        }
    }

    void UpdateCoins()
    {
        if (stats == null || coinsText == null || stats.Coins == _coins) return;

        bool first = _coins == int.MinValue;
        _coins = stats.Coins;
        coinsText.text = _coins.ToString();
        if (!first) Punch(coinsText.transform);
    }

    void HandleKillsChanged() => UpdateKills(true);

    void UpdateKills(bool animate)
    {
        var tracker = RunTracker.Instance;
        if (tracker == null || killsText == null || tracker.EnemiesKilled == _kills) return;

        bool first = _kills == int.MinValue;
        _kills = tracker.EnemiesKilled;
        killsText.text = _kills.ToString();
        if (animate && !first) Punch(killsText.transform);
    }

    void UpdateTimer()
    {
        var tracker = RunTracker.Instance;
        if (tracker == null || timerText == null) return;

        int seconds = Mathf.FloorToInt(tracker.ElapsedSeconds);
        if (seconds == _seconds) return;

        _seconds = seconds;
        int hours = seconds / 3600;
        int minutes = seconds / 60 % 60;
        int secs = seconds % 60;

        timerText.text = hours > 0
            ? $"{hours}:{minutes:00}:{secs:00}"
            : $"{minutes:00}:{secs:00}";
    }

    void HandleWaveChanged() => UpdateWave(true);

    void UpdateWave(bool animate)
    {
        if (spawner == null || waveText == null) return;

        bool cleared = !spawner.WaveInProgress && spawner.CurrentWave > 0;
        int number = cleared ? spawner.CurrentWave : spawner.CurrentWave + 1;
        if (number == _waveNumber && cleared == _waveCleared) return;

        bool first = _waveNumber == int.MinValue;
        _waveNumber = number;
        _waveCleared = cleared;
        waveText.text = string.Format(cleared ? waveClearedFormat : waveFormat, number);
        if (animate && !first) Punch(waveText.transform);
    }

    void UpdateKeyLabel()
    {
        var input = InventoryScreenInput.Instance;
        if (input == null || backpackKeyText == null || input.UsingGamepad == _usingGamepad) return;

        _usingGamepad = input.UsingGamepad;
        backpackKeyText.text = _usingGamepad ? gamepadKeyLabel : keyboardKeyLabel;
    }

    void OpenInventory()
    {
        if (InventoryScreenInput.Instance != null) InventoryScreenInput.Instance.TryOpen();
    }

    void PunchBackpack()
    {
        if (backpackButton == null) return;
        Punch(backpackButton.transform);
    }

    void Punch(Transform target)
    {
        target.DOKill(true);
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * valuePunch, 0.3f, 6, 0.5f).AsUI(target.gameObject);
    }

    void AddHover(GameObject target)
    {
        var trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        AddEntry(trigger, EventTriggerType.PointerEnter, () => ScaleTo(target.transform, backpackHoverScale));
        AddEntry(trigger, EventTriggerType.PointerExit, () => ScaleTo(target.transform, 1f));
    }

    static void AddEntry(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    static void ScaleTo(Transform target, float scale)
    {
        target.DOKill(true);
        target.DOScale(scale, 0.12f).SetEase(Ease.OutBack).AsUI(target.gameObject);
    }
}
