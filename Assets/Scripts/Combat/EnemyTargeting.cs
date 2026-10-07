using UnityEngine;

public static class EnemyTargeting
{
    const string EnemyTag = "Enemy";

    public static bool TryGetLivingEnemy(Collider collider, out LifeSystem enemy)
    {
        enemy = collider != null ? collider.GetComponentInParent<LifeSystem>() : null;
        return enemy != null && !enemy.IsDead && enemy.CompareTag(EnemyTag);
    }
}
