// SkillOrb.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillOrb : MonoBehaviour
{
    [Header("Config")]
    public float interactRadius = 2f;
    public int skillChoices = 3;

    [Header("Pools")]
    public List<SkillDefinition> skillPool = new();
    public List<WeaponDefinition> weaponPool = new();

    [Header("References")]
    public Transform player;
    public SkillSelectionUI skillUI;

    bool _active = false;
    bool _playerInRange = false;

    void Awake()
    {
        EnemySpawner.OnWaveCleared += Activate;
        gameObject.SetActive(false);
    }

    void OnDestroy() => EnemySpawner.OnWaveCleared -= Activate;

    void Activate() { _active = true; gameObject.SetActive(true); }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        _playerInRange = dist <= interactRadius;

        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
            OpenSkillUI();
    }

    void OpenSkillUI()
    {
        var skillHandler = player.GetComponent<StarterAssets.PlayerSkillHandler>();
        var controller = player.GetComponent<StarterAssets.PlayerController>();
        var weaponHandler = player.GetComponent<CarWeaponHandler>();

        var drawn = SkillDrawer.Draw(
            skillPool, weaponPool, skillHandler, weaponHandler, skillChoices);

        skillUI.Show(
            drawn, skillPool, weaponPool,
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