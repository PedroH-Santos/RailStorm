using Unity.Cinemachine;
using UnityEngine;

public class ChooseWayCameraFraming : MonoBehaviour
{
    static ChooseWayCameraFraming _instance;
    public static ChooseWayCameraFraming Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<ChooseWayCameraFraming>(FindObjectsInactive.Include);
                if (found != null)
                {
                    found.gameObject.SetActive(true);
                    _instance = found;
                }
            }

            return _instance;
        }
    }

    [Header("Rig")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineFollow follow;
    [SerializeField] private CinemachineBrain brain;

    [Header("Enquadramento durante a escolha de caminho")]
    [SerializeField, Range(1f, 2f)] private float zoomOut = 1.25f;
    [SerializeField] private float screenPan = 3.2f;
    [SerializeField] private float transitionDuration = 0.35f;

    float _baseOrthographicSize;
    Vector3 _baseFollowOffset;
    Vector3 _panAxis = Vector3.up;
    float _blend;
    bool _focused;
    bool _baseIgnoreTimeScale;

    void Awake()
    {
        _instance = this;

        if (virtualCamera == null) virtualCamera = GetComponent<CinemachineCamera>();
        if (follow == null) follow = GetComponent<CinemachineFollow>();
        if (brain == null && Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();

        if (virtualCamera != null) _baseOrthographicSize = virtualCamera.Lens.OrthographicSize;
        if (follow != null) _baseFollowOffset = follow.FollowOffset;

        if (virtualCamera == null || follow == null)
            Debug.LogWarning("ChooseWayCameraFraming precisa ficar no mesmo GameObject da CinemachineCamera com CinemachineFollow.", this);
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public void SetFocused(bool focused)
    {
        if (_focused == focused) return;
        _focused = focused;

        if (focused)
        {
            ResolvePanAxis();

            if (brain != null)
            {
                _baseIgnoreTimeScale = brain.IgnoreTimeScale;
                brain.IgnoreTimeScale = true;
            }
        }
        else if (brain != null)
        {
            brain.IgnoreTimeScale = _baseIgnoreTimeScale;
        }
    }

    void ResolvePanAxis()
    {
        var camera = Camera.main;
        _panAxis = camera != null ? camera.transform.up : Vector3.up;
    }

    void LateUpdate()
    {
        float target = _focused ? 1f : 0f;
        if (Mathf.Approximately(_blend, target)) return;

        float step = transitionDuration <= 0f ? 1f : Time.unscaledDeltaTime / transitionDuration;
        _blend = Mathf.MoveTowards(_blend, target, step);

        float eased = Easing.CubicOut(_blend);

        if (virtualCamera != null)
        {
            var lens = virtualCamera.Lens;
            lens.OrthographicSize = Mathf.Lerp(_baseOrthographicSize, _baseOrthographicSize * zoomOut, eased);
            virtualCamera.Lens = lens;
        }

        if (follow != null)
            follow.FollowOffset = _baseFollowOffset - _panAxis * (screenPan * eased);
    }
}
