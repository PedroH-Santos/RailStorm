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

    void OnEnable() => EnemySpawner.OnWaveCleared += Activate;
    void OnDisable() => EnemySpawner.OnWaveCleared -= Activate;

    void Activate()
    {
        _active = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!_active) return;

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

        List<SkillCardData> drawn = SkillDrawer.Draw(
            skillPool, weaponPool,
            skillHandler, weaponHandler,
            skillChoices);

        skillUI.Show(
            drawn,
            skillPool, weaponPool,
            controller, skillHandler, weaponHandler,
            OnSkillChosen, OnPassed);

        _active = false;
    }

    void OnSkillChosen(SkillDefinition skill)
    {
        player.GetComponent<StarterAssets.PlayerSkillHandler>()?.ApplySkill(skill);
        gameObject.SetActive(false);
    }

    void OnPassed() => gameObject.SetActive(false);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}