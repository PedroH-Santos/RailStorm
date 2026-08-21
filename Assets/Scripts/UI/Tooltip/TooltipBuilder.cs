using UnityEngine;

public static class TooltipBuilder
{
    public const int MaxUpgradeLines = 5;

    public static TooltipData Build(IDrawable drawable)
    {
        switch (drawable)
        {
            case WeaponDefinition weapon: return BuildWeapon(weapon);
            case SkillDefinition skill: return BuildSkill(skill);
            case ItemDefinition item: return BuildItem(item);
            default: return null;
        }
    }

    static TooltipData BuildWeapon(WeaponDefinition weapon)
    {
        var data = NewData("Estatísticas da Arma", weapon);
        data.Description = weapon.description;

        var stats = weapon.GetEffectiveStats();
        if (stats == null) return data;

        data.Stats.Add(new TooltipStatLine("Dano", $"{stats.damage}"));
        data.Stats.Add(new TooltipStatLine("Cadência", $"{stats.attackRate:0.##}/s"));
        data.Stats.Add(new TooltipStatLine("Alcance", $"{stats.range:0.#}m"));

        if (stats is ArrowLevelData arrow)
        {
            data.Stats.Add(new TooltipStatLine("Flechas", $"{arrow.arrowCount}"));
            data.Stats.Add(new TooltipStatLine("Velocidade", $"{arrow.speed:0.#}"));
        }
        else if (stats is MagicLevelData magic)
        {
            data.Stats.Add(new TooltipStatLine("Área", $"{magic.area:0.#}m"));
            data.Stats.Add(new TooltipStatLine("Conjuração", $"{magic.castTime:0.##}s"));
        }

        foreach (var skill in weapon.AppliedSkills)
        {
            if (skill == null || !skill.IsAcquired) continue;

            if (data.Upgrades.Count >= MaxUpgradeLines)
            {
                data.HiddenUpgrades++;
                continue;
            }

            var level = skill.GetLevelForRarity(skill.CurrentRarity);
            string effect = $"{StatLabels.Of(skill.statTarget)} {FormatDelta(level.statValue, level.isMultiplier, true)}";
            string levelLabel = $"Nv. {skill.CurrentRarity + 1}/{skill.levels.Count}";

            data.Upgrades.Add(new TooltipUpgradeLine(skill.DisplayName, effect, levelLabel));
        }

        return data;
    }

    static TooltipData BuildSkill(SkillDefinition skill)
    {
        var data = NewData("Habilidade", skill);
        data.Description = skill.description;

        int rarity = Mathf.Max(skill.CurrentRarity, 0);
        var level = skill.GetLevelForRarity(rarity);

        data.Stats.Add(new TooltipStatLine(StatLabels.Of(skill.statTarget), FormatDelta(level.statValue, level.isMultiplier, false)));
        data.Stats.Add(new TooltipStatLine("Nível", $"{rarity + 1} / {skill.levels.Count}"));

        return data;
    }

    static TooltipData BuildItem(ItemDefinition item)
    {
        var data = NewData("Item", item);
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

    static TooltipData NewData(string header, IDrawable drawable)
    {
        int rarity = Mathf.Max(drawable.CurrentRarity, 0);
        return new TooltipData
        {
            HeaderLabel = header,
            Title = drawable.DisplayName,
            Icon = drawable.Icon,
            RarityLabel = RarityHelper.DisplayName(rarity),
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
