using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ToriponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "Can't be attacked by units.";
        stats.name = "Toripon";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.flying = true;

        stats.imagePath = "toripon";
        return stats;
    }
}
