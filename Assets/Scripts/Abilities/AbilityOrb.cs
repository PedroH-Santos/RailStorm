using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityOrb : MonoBehaviour
{
    public float interactRadius = 2f;
    public int abilityChoices = 3;

    public Transform player;
    public AbilitySelectionUI abilityUI;

    List<SkillDefinition> _skillPool = new();
    List<WeaponDefinition> _weaponPool = new();

    bool _active = false;
    bool _playerInRange = false;

    void Awake()
    {
        LoadPools();
        EnemySpawner.OnWaveCleared += Activate;
        gameObject.SetActive(false);
    }

    void OnDestroy() => EnemySpawner.OnWaveCleared -= Activate;

    void LoadPools()
    {
        _skillPool = new List<SkillDefinition>(Resources.LoadAll<SkillDefinition>("Skills"));
        _weaponPool = new List<WeaponDefinition>(Resources.LoadAll<WeaponDefinition>("Weapons"));

        Debug.Log($"[AbilitiesOrb] {_skillPool.Count} skills, {_weaponPool.Count} weapons carregadas.");
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
        var skillHandler = player.GetComponent<StarterAssets.PlayerSkillHandler>();
        var controller = player.GetComponent<StarterAssets.PlayerController>();
        var weaponHandler = player.GetComponent<CarWeaponHandler>()
                         ?? FindFirstObjectByType<CarWeaponHandler>();

        var drawn = AbilityDrawer.Draw(
            _skillPool, _weaponPool, skillHandler, weaponHandler, abilityChoices);

        abilityUI.Show(
            drawn, _skillPool, _weaponPool,
            controller, skillHandler, weaponHandler,
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
