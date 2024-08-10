using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DekaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 8;
        stats.description = "Can't move.";
        stats.name = "Dekapon";
        stats.limitedVision = true;
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "dekapon";
        return stats;
    }
}