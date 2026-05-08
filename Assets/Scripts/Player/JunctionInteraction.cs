using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem; // <- troca o using

[RequireComponent(typeof(SplineJunction))]
public class JunctionInteraction : MonoBehaviour
{
    SplineJunction _junction;
    PlayerController _playerInside;
    bool _menuOpen = false;
    List<int> _blockedAtThisJunction = new List<int>();

    void Awake()
    {
        _junction = GetComponent<SplineJunction>();
    }

    void Update()
    {
        if (_playerInside == null) return;

        bool hasBlocked = _junction.GetBlockedSplines().Count > 0;

        // Abre menu
        if (hasBlocked && !_menuOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenMenu();
            return;
        }

        // Fecha menu
        if (_menuOpen && (Keyboard.current.eKey.wasPressedThisFrame
                       || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseMenu();
            return;
        }

        // Captura 1-4 para desbloquear
        if (_menuOpen)
        {
            HandleUnlockInput();
        }
    }

    void HandleUnlockInput()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryUnlock(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) TryUnlock(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) TryUnlock(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) TryUnlock(3);
    }

    void TryUnlock(int menuIndex)
    {
        if (menuIndex >= _blockedAtThisJunction.Count) return;

        int splineIndex = _blockedAtThisJunction[menuIndex];
        int cost = _junction.GetUnlockCost(splineIndex); // <- custo individual

        if (_playerInside.Coins < cost)
        {
            JunctionUIManager.Instance.ShowInsufficientFunds();
            return;
        }

        _playerInside.SpendCoins(cost);
        _junction.Unblock(splineIndex);
        _blockedAtThisJunction.Remove(splineIndex);

        JunctionUIManager.Instance.UpdateMenu(
            _blockedAtThisJunction,
            _junction,              // <- passa a junction para pegar custo por slot
            _playerInside.Coins
        );

        if (_blockedAtThisJunction.Count == 0)
            CloseMenu();
    }


    void OpenMenu()
    {
        _menuOpen = true;
        _blockedAtThisJunction = _junction.GetBlockedSplines();
        _playerInside.SetMovementLocked(true);
        JunctionUIManager.Instance.ShowMenu(
            _blockedAtThisJunction,
            _junction,              // <- passa a junction
            _playerInside.Coins
        );
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _playerInside.SetMovementLocked(false);
        JunctionUIManager.Instance.HideMenu();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = other.GetComponent<PlayerController>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_menuOpen) CloseMenu();
        _playerInside = null;
    }
}