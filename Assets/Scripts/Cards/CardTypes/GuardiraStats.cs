using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GuardiraStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int guardiraPower = 1;

        stats.power = 6;
        stats.description = "Greatshield.\nAlways deals " + guardiraPower.ToString() + " damage regardless of its power.";
        stats.name = "Guardira";
        stats.hasGreatshield = true;
        stats.fixedPower = guardiraPower;
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "Guardira";
        return stats;
    }
}
