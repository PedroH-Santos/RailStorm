using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ShopZone : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private ShopManager shopManager;

    [SerializeField] private SellUI sellUI;
    [SerializeField] private SellManager sellManager;

    bool _playerInside;
    bool _shopOpen;
    bool _sellMode;
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
            if (sellUI != null && Keyboard.current.tabKey.wasPressedThisFrame)
                ToggleMode();

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
        _sellMode = false;
        InteractPromptUI.Instance?.Hide();
        _player.SetMovementLocked(true);

        OpenCurrentMode();
    }

    void ToggleMode()
    {
        _sellMode = !_sellMode;

        if (_sellMode) shopUI.Close();
        else sellUI.Close();

        OpenCurrentMode();
    }

    void OpenCurrentMode()
    {
        var stats = _player.GetComponent<PlayerStatsAggregator>();
        var itemHandler = _player.GetComponent<PlayerItemHandler>();

        if (_sellMode)
        {
            SellManager manager = sellManager != null ? sellManager : SellManager.Instance;
            sellUI.Open(stats, itemHandler, manager);
        }
        else
        {
            // Se nenhum ShopManager específico foi arrastado no Inspector, usa o
            // compartilhado — assim várias lojas no mapa vendem o mesmo estoque por padrão.
            ShopManager manager = shopManager != null ? shopManager : ShopManager.Instance;
            shopUI.Open(stats, itemHandler, manager);
        }
    }

    void CloseShop()
    {
        _shopOpen = false;
        _player?.SetMovementLocked(false);

        shopUI.Close();
        sellUI?.Close();
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
