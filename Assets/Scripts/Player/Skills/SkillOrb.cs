using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillOrb : MonoBehaviour
{
    [Header("Config")]
    public float interactRadius = 2f;
    public int skillChoices = 3;
    public List<SkillDefinition> skillPool = new();

    [Header("References")]
    public Transform player;
    public SkillSelectionUI skillUI;

    bool _active = false;
    bool _playerInRange = false;

    void OnEnable()
    {
        EnemySpawner.OnWaveCleared += Activate;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveCleared -= Activate;
    }

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
        var handler = player.GetComponent<StarterAssets.PlayerSkillHandler>();
        var controller = player.GetComponent<StarterAssets.PlayerController>();

        List<SkillCardData> drawn = DrawSkills(handler);

        skillUI.Show(
            drawn,
            skillPool,
            controller,
            handler,
            OnSkillChosen,
            OnPassed
        );

        _active = false;
    }

    List<SkillCardData> DrawSkills(StarterAssets.PlayerSkillHandler handler)
    {
        List<SkillDefinition> newSkills = skillPool
            .Where(s => !handler.HasSkill(s))
            .ToList();

        List<SkillDefinition> upgradeable = skillPool
            .Where(s => handler.CanLevelUp(s))
            .ToList();

        List<SkillCardData> candidates = new();

        foreach (var s in newSkills)
            candidates.Add(new SkillCardData(s, handler.GetSkillLevel(s) + 1, s.weight));

        foreach (var s in upgradeable)
            candidates.Add(new SkillCardData(s, handler.GetSkillLevel(s) + 1, s.weight));

        List<SkillCardData> result = new();
        List<SkillCardData> remaining = new(candidates);

        for (int i = 0; i < skillChoices && remaining.Count > 0; i++)
        {
            float totalWeight = remaining.Sum(c => c.weight);
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int j = 0; j < remaining.Count; j++)
            {
                cumulative += remaining[j].weight;
                if (roll <= cumulative)
                {
                    result.Add(remaining[j]);
                    remaining.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }

    void OnSkillChosen(SkillDefinition skill)
    {
        var handler = player.GetComponent<StarterAssets.PlayerSkillHandler>();
        if (handler != null) handler.ApplySkill(skill);
        gameObject.SetActive(false);
    }

    void OnPassed()
    {
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, interactRadius);
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}

public class SkillCardData
{
    public SkillDefinition skill;
    public int targetLevel;
    public float weight;

    public SkillCardData(SkillDefinition skill, int targetLevel, float weight)
    {
        this.skill = skill;
        this.targetLevel = targetLevel;
        this.weight = weight;
    }
}