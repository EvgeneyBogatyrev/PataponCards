using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KibaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Haste</b>.";
        stats.name = "Kibapon";
        stats.hasHaste = true;
        //stats.hasShield = true;
        stats.runes.Add(Runes.Spear);
        //stats.runes.Add(Runes.Spear);

        stats.onPlaySound = "kibapon";

        stats.imagePath = "kiba_art";
        stats.artistName = "Pavel Shpagin (Poki)";

        return stats;
    }
}