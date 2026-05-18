using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SplineJunction))]
public class JunctionInteraction : MonoBehaviour
{
    SplineJunction _junction;
    PlayerController _playerInside;
    PlayerStatsAggregator _stats;
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

        if (hasBlocked && !_menuOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenMenu();
            return;
        }

        if (_menuOpen && (Keyboard.current.eKey.wasPressedThisFrame
                       || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseMenu();
            return;
        }

        if (_menuOpen)
            HandleUnlockInput();
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
        int cost = _junction.GetUnlockCost(splineIndex);

        if (_stats.Coins < cost)
        {
            JunctionUIManager.Instance.ShowInsufficientFunds();
            return;
        }

        _stats.Coins -= cost;
        _junction.Unblock(splineIndex);
        _blockedAtThisJunction.Remove(splineIndex);

        JunctionUIManager.Instance.UpdateMenu(
            _blockedAtThisJunction,
            _junction,
            _stats.Coins
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
            _junction,
            _stats.Coins
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
        _stats = other.GetComponent<PlayerStatsAggregator>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_menuOpen) CloseMenu();
        _playerInside = null;
        _stats = null;
    }
}