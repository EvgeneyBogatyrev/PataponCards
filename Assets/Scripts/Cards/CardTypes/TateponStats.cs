using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TateponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 4;
        stats.description = "Shield (Your Hatapon is immune).";
        stats.name = "Tatepon";
        stats.hasShield = true;
        stats.runes.Add(Runes.Shield);
        stats.imagePath = "tatepon";
        return stats;
    }
}