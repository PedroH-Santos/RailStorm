using System.Collections.Generic;
using UnityEngine;

public class SkillBarUI : MonoBehaviour
{
    [SerializeField] private PlayerSkillHandler handler;
    [SerializeField] private PlayerSkillCaster caster;
    [SerializeField] private List<SkillSlotHUD> slots = new();

    [Header("Leque")]
    [Tooltip("Graus de inclinação entre uma carta e a vizinha.")]
    [SerializeField] private float fanAngle = 7f;
    [Tooltip("Quanto cada carta desce por passo de distância do centro do leque.")]
    [SerializeField] private float fanDrop = 8f;

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
            var card = slots[i];
            if (card == null) continue;

            var slot = handler != null ? handler.GetSlot(i) : null;
            bool visible = slot != null && slot.IsUnlocked;
            card.gameObject.SetActive(visible);
            if (visible) card.ShowSkill(slot.IsEmpty ? null : slot.Skill.Definition);
        }

        ApplyFan();
    }

    void ApplyFan()
    {
        int visibleCount = 0;
        foreach (var slot in slots)
            if (slot != null && slot.gameObject.activeSelf) visibleCount++;

        float center = (visibleCount - 1) * 0.5f;
        int index = 0;
        foreach (var slot in slots)
        {
            if (slot == null || !slot.gameObject.activeSelf) continue;
            float offset = index - center;
            slot.SetFan(-offset * fanAngle, Mathf.Abs(offset) * fanDrop);
            index++;
        }
    }

    void LateUpdate()
    {
        if (handler == null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            var card = slots[i];
            if (card == null || !card.gameObject.activeSelf) continue;

            if (caster != null) card.SetKey(caster.GetKeyLabel(i));

            var slot = handler.GetSlot(i);
            if (slot == null) continue;

            card.ShowCooldown(slot);
            card.ShowCharges(slot.Charges);
        }
    }

    void HandleCast(int slot)
    {
        if (slot >= 0 && slot < slots.Count && slots[slot] != null && slots[slot].gameObject.activeSelf)
            slots[slot].PlayCast();
    }
}
