using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TargetDummyStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "Greatshield.";
        stats.name = "Target dumby";
        stats.hasGreatshield = true;
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "target_dummy";
        return stats;
    }
}
