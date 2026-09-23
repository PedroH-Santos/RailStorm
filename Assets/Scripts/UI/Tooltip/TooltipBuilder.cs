using UnityEngine;

public static class TooltipBuilder
{
    public const int MaxUpgradeLines = 5;

    public static TooltipData Build(IDrawable drawable, int currentRarity)
    {
        switch (drawable)
        {
            case CarWeaponDefinition weapon: return BuildWeapon(weapon, currentRarity);
            case PerkDefinition perk: return BuildPerk(perk, currentRarity);
            case SkillDefinition skill: return BuildSkill(skill);
            case ItemDefinition item: return BuildItem(item);
            default: return null;
        }
    }

    static TooltipData BuildWeapon(CarWeaponDefinition weapon, int currentRarity)
    {
        var data = NewData("Estatísticas da Arma", weapon, currentRarity);
        data.Description = weapon.description;

        var weaponHandler = PlayerCarWeaponHandler.Instance;

        var stats = weaponHandler != null
            ? weaponHandler.GetEffectiveStats(weapon)
            : weapon.GetStatsForRarity(Mathf.Max(currentRarity, 0));

        if (stats == null) return data;

        foreach (var target in stats.DisplayStats)
            data.Stats.Add(new TooltipStatLine(StatLabels.Of(target), CarWeaponStatFormatting.Format(target, stats.GetStatValue(target))));

        if (weaponHandler == null) return data;

        foreach (var perk in weaponHandler.GetAppliedPerks(weapon))
        {
            if (perk == null) continue;

            int perkRarity = weaponHandler.GetRarity(perk);
            if (perkRarity < 0) continue;

            if (data.Upgrades.Count >= MaxUpgradeLines)
            {
                data.HiddenUpgrades++;
                continue;
            }

            var level = perk.GetLevelForRarity(perkRarity);
            string effect = $"{StatLabels.Of(perk.statTarget)} {FormatDelta(level.statValue, level.isMultiplier, true)}";
            string levelLabel = $"Nv. {perkRarity + 1}/{perk.LevelCount}";

            data.Upgrades.Add(new TooltipUpgradeLine(perk.DisplayName, effect, levelLabel));
        }

        return data;
    }

    static TooltipData BuildPerk(PerkDefinition perk, int currentRarity)
    {
        var data = NewData("Perk", perk, currentRarity);
        data.Description = perk.description;

        int rarity = Mathf.Max(currentRarity, 0);
        var level = perk.GetLevelForRarity(rarity);

        data.Stats.Add(new TooltipStatLine(StatLabels.Of(perk.statTarget), FormatDelta(level.statValue, level.isMultiplier, false)));
        data.Stats.Add(new TooltipStatLine("Nível", $"{rarity + 1} / {perk.LevelCount}"));

        return data;
    }

    static TooltipData BuildSkill(SkillDefinition skill)
    {
        var data = NewData("Skill", skill, 0);
        data.Description = skill.description;

        var handler = PlayerSkillHandler.Instance;
        int level = handler != null ? Mathf.Max(handler.GetLevel(skill), 0) : 0;

        foreach (var target in skill.VisibleStats(level))
            data.Stats.Add(new TooltipStatLine(StatLabels.Of(target), SkillStatFormatting.Format(target, skill.GetStatValue(level, target))));

        if (!skill.HasCooldown(level))
            data.Stats.Add(new TooltipStatLine(StatLabels.Of(ESkillStatTarget.Cooldown), "Sem recarga"));

        data.Stats.Add(new TooltipStatLine("Nível", $"{level + 1} / {skill.LevelCount}"));

        int slot = handler != null ? handler.IndexOf(skill) : -1;
        if (slot >= 0 && PlayerSkillCaster.Instance != null)
            data.Stats.Add(new TooltipStatLine("Tecla", PlayerSkillCaster.Instance.GetKeyLabel(slot)));

        return data;
    }

    static TooltipData BuildItem(ItemDefinition item)
    {
        var data = NewData("Item", item, item.rarity);
        data.Description = item.description;

        if (item.effectType == EItemEffectType.StatChange)
        {
            data.Stats.Add(new TooltipStatLine(StatLabels.Of(item.statTarget), FormatDelta(item.statValue, item.isMultiplier, true)));
            return data;
        }

        data.HasAbility = true;
        data.AbilityName = item.abilityName;
        data.AbilityDescription = !string.IsNullOrWhiteSpace(item.abilityDescription)
            ? item.abilityDescription
            : "Efeito passivo aplicado ao jogador enquanto o item estiver no inventário.";

        return data;
    }

    static TooltipData NewData(string header, IDrawable drawable, int currentRarity)
    {
        int rarity = Mathf.Max(currentRarity, 0);
        return new TooltipData
        {
            HeaderLabel = header,
            Title = drawable.DisplayName,
            Icon = drawable.Icon,
            RarityLabel = RarityHelper.DisplayName(rarity),
            RarityIndex = rarity,
            RarityColor = RarityHelper.Color(rarity)
        };
    }

    static string FormatDelta(float value, bool isMultiplier, bool multiplierIsPercent)
    {
        if (isMultiplier)
            return multiplierIsPercent ? $"{Signed(value)}%" : $"×{value:0.##}";

        return Signed(value);
    }

    static string Signed(float value) => value >= 0f ? $"+{value:0.##}" : $"{value:0.##}";
}
