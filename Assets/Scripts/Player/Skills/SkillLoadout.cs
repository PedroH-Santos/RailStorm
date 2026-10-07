using System.Collections.Generic;
using UnityEngine;

public class SkillLoadout
{
    readonly SkillSlot[] _slots;

    public SkillLoadout(int slotCount, int unlockedAtStart)
    {
        _slots = new SkillSlot[slotCount];
        for (int i = 0; i < slotCount; i++) _slots[i] = new SkillSlot(i);

        int unlocked = Mathf.Clamp(unlockedAtStart, 1, slotCount);
        for (int i = 0; i < unlocked; i++) _slots[i].Unlock();
        UnlockedCount = unlocked;
    }

    public IReadOnlyList<SkillSlot> Slots => _slots;
    public int UnlockedCount { get; private set; }
    public bool IsEmpty => FirstFilledSlot() == null;

    public SkillSlot Get(int index) => index >= 0 && index < _slots.Length ? _slots[index] : null;

    public SkillSlot SlotHolding(SkillDefinition definition)
    {
        foreach (var slot in _slots)
            if (slot.Holds(definition)) return slot;
        return null;
    }

    public SkillSlot FirstEmptyUnlockedSlot()
    {
        foreach (var slot in _slots)
            if (slot.IsUnlocked && slot.IsEmpty) return slot;
        return null;
    }

    public bool Equip(int index, OwnedSkill skill)
    {
        var target = Get(index);
        if (target == null || !target.IsUnlocked || skill == null || target.Skill == skill) return false;

        var slotWhereItIsNow = SlotHolding(skill.Definition);
        if (slotWhereItIsNow != null) slotWhereItIsNow.SwapContentsWith(target);
        else target.Equip(skill);

        return true;
    }

    public bool Unequip(int index)
    {
        var slot = Get(index);
        if (slot == null || slot.IsEmpty) return false;

        slot.Empty();
        return true;
    }

    public bool UnlockNextSlot()
    {
        if (UnlockedCount >= _slots.Length) return false;

        _slots[UnlockedCount].Unlock();
        UnlockedCount++;
        return true;
    }

    public void Tick(float deltaTime)
    {
        foreach (var slot in _slots) slot.Tick(deltaTime);
    }

    SkillSlot FirstFilledSlot()
    {
        foreach (var slot in _slots)
            if (!slot.IsEmpty) return slot;
        return null;
    }
}
