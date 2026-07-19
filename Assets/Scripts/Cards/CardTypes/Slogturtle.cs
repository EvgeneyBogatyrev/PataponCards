using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SlogturtleStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 9;
        stats.description = "<b>Pacifism</b>. <b>Lifelink</b>.\nPlayers cannot cast spells.";
        stats.name = "Slogturtle";
        stats.pacifism = true;
        stats.hasShield = true;
        stats.blockSpells = true;
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "slogturtle";
        stats.onPlaySound = "slogturtle";
        stats.artistName = "Official render";
        return stats;
    }
}
