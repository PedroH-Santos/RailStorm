using UnityEngine;

public static class RarityRoller
{
    public static int Roll(int minRi, int maxRi, float luck)
    {
        float total = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
            total += RarityHelper.GetWeight(ri, luck);

        if (total <= 0f) return minRi;

        float roll = Random.Range(0f, total);
        float acc = 0f;
        for (int ri = minRi; ri <= maxRi; ri++)
        {
            acc += RarityHelper.GetWeight(ri, luck);
            if (roll <= acc) return ri;
        }
        return maxRi;
    }
}
