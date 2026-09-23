using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PerkOrb : MonoBehaviour
{
    public float interactRadius = 2f;
    [FormerlySerializedAs("abilityChoices")] public int perkChoices = 3;

    public Transform player;
    public Transform playerCart;
    [FormerlySerializedAs("abilityUI")] public PerkSelectionUI perkUI;

    List<PerkDefinition> _perkPool = new();
    List<CarWeaponDefinition> _weaponPool = new();
    List<WeaponPerkDefinition> _weaponPerkPool = new();

    bool _active = false;
    bool _playerInRange = false;

    void Awake()
    {
        LoadPools();
        EnemySpawner.OnWaveCleared += Activate;
    }

    void OnDestroy() => EnemySpawner.OnWaveCleared -= Activate;

    void LoadPools()
    {
        _perkPool = new List<PerkDefinition>(Resources.LoadAll<PerkDefinition>("Perks"));
        _weaponPool = new List<CarWeaponDefinition>(Resources.LoadAll<CarWeaponDefinition>("CarWeapons"));
        _weaponPerkPool = new List<WeaponPerkDefinition>(Resources.LoadAll<WeaponPerkDefinition>("WeaponPerks"));

        Debug.Log($"[PerkOrb] {_perkPool.Count} perks, {_weaponPool.Count} car weapons, {_weaponPerkPool.Count} weapon perks carregadas.");
    }

    void Activate() { _active = true; gameObject.SetActive(true); }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        _playerInRange = dist <= interactRadius;

        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
            OpenUI();
    }

    void OpenUI()
    {
        var perkHandler = player.GetComponent<StarterAssets.PlayerPerkHandler>();
        var controller = player.GetComponent<StarterAssets.PlayerController>();
        var weaponHandler = playerCart.GetComponent<PlayerCarWeaponHandler>();

        var drawn = PerkDrawer.Draw(
            _perkPool, _weaponPool, _weaponPerkPool, perkHandler, weaponHandler, perkChoices);

        perkUI.Show(
            drawn, _perkPool, _weaponPool, _weaponPerkPool,
            controller, perkHandler, weaponHandler,
            OnPassed, OnClosed);

        _active = false;
    }

    void OnPassed() { gameObject.SetActive(false); EnemySpawner.NotifyReady(); }
    void OnClosed() { _active = false; gameObject.SetActive(false); EnemySpawner.NotifyReady(); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
