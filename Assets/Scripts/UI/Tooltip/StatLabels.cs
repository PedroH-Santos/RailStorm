using System.Collections.Generic;

public static class StatLabels
{
    static readonly Dictionary<EStatTarget, string> PlayerStats = new()
    {
        { EStatTarget.MoveSpeed, "Velocidade" },
        { EStatTarget.MaxHP, "Vida Máxima" },
        { EStatTarget.HP, "Vida" },
        { EStatTarget.LuckPercent, "Sorte" },
        { EStatTarget.Coins, "Moedas" },
        { EStatTarget.FireballDamage, "Dano do Projétil" },
        { EStatTarget.FireballSpeed, "Velocidade do Projétil" },
        { EStatTarget.FireballRange, "Alcance do Projétil" },
        { EStatTarget.AttackRate, "Cadência" },
        { EStatTarget.CarWeaponDamage, "Dano das Armas" },
        { EStatTarget.CarFireRate, "Cadência das Armas" },
        { EStatTarget.CarRange, "Alcance das Armas" },
        { EStatTarget.CarMaxWeapons, "Slots de Arma" },
        { EStatTarget.CarSpeed, "Velocidade do Vagão" },
        { EStatTarget.EnemyDamage, "Dano dos Inimigos" },
        { EStatTarget.EnemySpeed, "Velocidade dos Inimigos" },
        { EStatTarget.EnemyHP, "Vida dos Inimigos" },
        { EStatTarget.EnemyAttackRate, "Cadência dos Inimigos" },
        { EStatTarget.SpawnRate, "Taxa de Spawn" },
        { EStatTarget.WaveSize, "Tamanho da Wave" },
        { EStatTarget.CoinDropRate, "Moedas por Inimigo" },
        { EStatTarget.XpMultiplier, "Multiplicador de XP" },
    };

    static readonly Dictionary<EWeaponStatTarget, string> WeaponStats = new()
    {
        { EWeaponStatTarget.Damage, "Dano" },
        { EWeaponStatTarget.AttackRate, "Cadência" },
        { EWeaponStatTarget.Range, "Alcance" },
        { EWeaponStatTarget.Speed, "Velocidade" },
        { EWeaponStatTarget.ArrowCount, "Flechas" },
        { EWeaponStatTarget.Area, "Área" },
        { EWeaponStatTarget.CastTime, "Conjuração" },
    };

    public static string Of(EStatTarget target)
        => PlayerStats.TryGetValue(target, out var label) ? label : target.ToString();

    public static string Of(EWeaponStatTarget target)
        => WeaponStats.TryGetValue(target, out var label) ? label : target.ToString();
}
