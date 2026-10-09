using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerHandFlame : MonoBehaviour
{
    [SerializeField] private LowPolyFlame handFlamePrefab;
    [SerializeField] private Transform handFlameBone;
    [Tooltip("Centro da mão no espaço do osso. A base da chama nasce aqui e acompanha a mão quando ela gira.")]
    [SerializeField] private Vector3 palmCenterInBoneSpace = new(0f, 0.0085f, 0.0015f);
    [Tooltip("Ajuste fino em unidades do mundo, somado depois do centro da mão.")]
    [SerializeField] private Vector3 offsetFromPalm = Vector3.zero;
    [SerializeField] private float handFlameScale = 0.7f;
    [Tooltip("Trecho (tempo normalizado) da Celebrate com fogo na mão: frames 13–34 de 58.")]
    [SerializeField] private Vector2 celebrateFlameWindow = new(13f / 58f, 34f / 58f);
    [Tooltip("Trecho (tempo normalizado) da Celebrate_Quick com fogo na mão: frames 8–20 de 30.")]
    [SerializeField] private Vector2 quickCelebrateFlameWindow = new(8f / 30f, 20f / 30f);

    PlayerAnimationController _animation;
    LowPolyFlame _flame;
    bool _burning;

    void Awake()
    {
        _animation = GetComponent<PlayerAnimationController>();
    }

    void Start()
    {
        if (handFlamePrefab == null) return;

        _flame = Instantiate(handFlamePrefab);
        _flame.transform.localScale = Vector3.one * handFlameScale;
    }

    void OnDestroy()
    {
        if (_flame != null) Destroy(_flame.gameObject);
    }

    void LateUpdate()
    {
        if (_flame == null || handFlameBone == null) return;

        _flame.transform.position = handFlameBone.TransformPoint(palmCenterInBoneSpace) + offsetFromPalm;
        SetBurning(IsCelebratingWithFire());
    }

    bool IsCelebratingWithFire()
    {
        bool inCelebrateFireWindow = _animation.TryGetCelebrateProgress(out float celebrateTime)
                                     && IsInside(celebrateFlameWindow, celebrateTime);
        bool inQuickCelebrateFireWindow = _animation.TryGetQuickCelebrateProgress(out float quickTime)
                                          && IsInside(quickCelebrateFlameWindow, quickTime);
        return inCelebrateFireWindow || inQuickCelebrateFireWindow;
    }

    static bool IsInside(Vector2 window, float time) => time >= window.x && time <= window.y;

    void SetBurning(bool burning)
    {
        if (burning == _burning) return;
        _burning = burning;

        if (burning) _flame.Ignite();
        else _flame.Extinguish();
    }
}
