using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerHandFlame : MonoBehaviour
{
    [SerializeField] private ParticleSystem handFlamePrefab;
    [SerializeField] private Transform handFlameBone;
    [SerializeField] private Vector3 handFlameOffset = new(0f, 0.25f, 0f);
    [SerializeField] private float handFlameScale = 0.7f;
    [Tooltip("Trecho (tempo normalizado) da Celebrate com fogo na mão: frames 13–34 de 58.")]
    [SerializeField] private Vector2 celebrateFlameWindow = new(13f / 58f, 34f / 58f);
    [Tooltip("Trecho (tempo normalizado) da Celebrate_Quick com fogo na mão: frames 8–20 de 30.")]
    [SerializeField] private Vector2 quickCelebrateFlameWindow = new(8f / 30f, 20f / 30f);

    PlayerAnimationController _animation;
    ParticleSystem _flame;
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
        _flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDestroy()
    {
        if (_flame != null) Destroy(_flame.gameObject);
    }

    void LateUpdate()
    {
        if (_flame == null || handFlameBone == null) return;

        _flame.transform.position = handFlameBone.position + handFlameOffset;
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

        if (burning) _flame.Play(true);
        else _flame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
