using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance;

    [SerializeField] private GameObject promptRoot;

    void Awake()
    {
        Instance = this;
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