using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ShopZone : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;

    bool _playerInside;
    bool _shopOpen;
    PlayerController _player;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (_shopOpen)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseShop();
            return;
        }

        if (!_playerInside) return;

        InteractPromptUI.Instance?.Show();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            OpenShop();
    }

    void OpenShop()
    {
        _shopOpen = true;
        InteractPromptUI.Instance?.Hide();
        _player.SetMovementLocked(true);
        shopUI.Open(_player.GetComponent<PlayerStatsAggregator>(), _player.GetComponent<PlayerItemHandler>());
    }

    void CloseShop()
    {
        _shopOpen = false;
        _player?.SetMovementLocked(false);
        shopUI.Close();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        _player = other.GetComponent<PlayerController>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;
        InteractPromptUI.Instance?.Hide();

        if (_shopOpen) CloseShop();
    }
}