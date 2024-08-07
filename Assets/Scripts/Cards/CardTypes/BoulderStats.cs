using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoulderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Pacifism</b>. <b>Lifelink</b>.\n<i>I wish I could pay taxes, but I don't know how.</i>";
        stats.name = "The Boulder";
        stats.pacifism = true;
        stats.hasShield = true;
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "boulder";
        return stats;
    }
}
