using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;

    [Tooltip("Força do pulo do botão ao ser acionado (clique ou Esc).")]
    [SerializeField] private float pressPunch = 0.12f;

    Action _onPressed;

    public bool Interactable
    {
        get => button != null && button.interactable;
        set { if (button != null) button.interactable = value; }
    }

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(Press);
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public void SetAction(Action onPressed) => _onPressed = onPressed;

    void Update()
    {
        bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepadBack = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (escape || gamepadBack) Press();
    }

    void Press()
    {
        if (!Interactable || _onPressed == null) return;

        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * pressPunch, 0.25f, 8, 0.6f).AsUI(gameObject);

        var callback = _onPressed;
        callback();
    }
}
