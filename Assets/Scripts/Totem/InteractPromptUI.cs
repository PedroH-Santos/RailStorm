using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    static InteractPromptUI _instance;

    public static InteractPromptUI Instance
    {
        get
        {
            if (_instance == null)
            {
                var found = FindFirstObjectByType<InteractPromptUI>(FindObjectsInactive.Include);
                if (found != null && !found.gameObject.activeSelf)
                    found.gameObject.SetActive(true);
            }

            return _instance;
        }
    }

    [SerializeField] private GameObject promptRoot;

    void Awake()
    {
        _instance = this;
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    public void Show()
    {
        if (promptRoot != null) promptRoot.SetActive(true);
    }

    public void Hide()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }
}
