using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuzzcraveStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "Haste.\nCan attack any enemy unit on the board.";
        stats.name = "Buzzcrave";
        stats.megaVision = true;
        stats.hasHaste = true;
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.legendary = true;

        stats.imagePath = "buzzcrave";
        return stats;
    }
}
