using UnityEngine;

[System.Serializable]
public abstract class WeaponLevelData
{
    public int damage = 10;
    public float attackRate = 1f;
    public float range = 15f;
}

[System.Serializable]
public class ArrowLevelData : WeaponLevelData
{
    public float speed = 20f;
    public int arrowCount = 1;

}

[System.Serializable]
public class MagicLevelData : WeaponLevelData
{
    public float area = 3f;
    public float castTime = 0.5f;
}