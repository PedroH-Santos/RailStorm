using System.Collections.Generic;
using UnityEngine;

public class SkillBarUI : MonoBehaviour
{
    [SerializeField] private PlayerSkillHandler handler;
    [SerializeField] private PlayerSkillCaster caster;
    [SerializeField] private List<SkillSlotHUD> slots = new();

    void Awake()
    {
        if (handler == null) handler = FindFirstObjectByType<PlayerSkillHandler>();
        if (caster == null) caster = FindFirstObjectByType<PlayerSkillCaster>();
    }

    void OnEnable()
    {
        if (handler != null)
        {
            handler.OnLoadoutChanged += Rebuild;
            handler.OnSkillCast += HandleCast;
        }

        Rebuild();
    }

    void OnDisable()
    {
        if (handler == null) return;
        handler.OnLoadoutChanged -= Rebuild;
        handler.OnSkillCast -= HandleCast;
    }

    void Rebuild()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            bool visible = handler != null && handler.IsSlotUnlocked(i);
            slot.gameObject.SetActive(visible);
            if (visible) slot.Bind(handler.GetSlot(i));
        }
    }

    void LateUpdate()
    {
        if (handler == null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null || !slot.gameObject.activeSelf) continue;

            if (caster != null) slot.SetKey(caster.GetKeyLabel(i));

            var skill = handler.GetSlot(i);
            float duration = skill != null ? skill.GetCooldown(handler.GetLevel(skill)) : 0f;
            slot.Tick(handler.HasCooldown(i), handler.CooldownRemaining(i), handler.CooldownNormalized(i), duration);
        }
    }

    void HandleCast(int slot)
    {
        if (slot >= 0 && slot < slots.Count && slots[slot] != null && slots[slot].gameObject.activeSelf)
            slots[slot].PlayCast();
    }
}
