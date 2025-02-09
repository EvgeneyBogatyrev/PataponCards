using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DekaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 8;
        stats.description = "This card cannot be played on the first three turns.";
        stats.name = "Dekapon";
        //stats.limitedVision = true;
        stats.firstTurnToPlay = 3;
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "dekapon_hq";
        return stats;
    }
}