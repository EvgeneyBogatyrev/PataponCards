using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KibaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "<b>Haste</b> (can attack and move as soon as it enters the battlefield).";
        stats.name = "Kibapon";
        stats.hasHaste = true;
        //stats.hasShield = true;
        stats.runes.Add(Runes.Spear);
        //stats.runes.Add(Runes.Spear);

        stats.imagePath = "kibapon";

        return stats;
    }
}