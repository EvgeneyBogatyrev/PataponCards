using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoulderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "Greatshield.\nI wish I could pay taxes, but I don't know how.";
        stats.name = "The Boulder";
        stats.canAttack = false;
        stats.canDealDamage = false;
        stats.limitedVision = true;
        stats.hasGreatshield = true;

        stats.imagePath = "boulder";
        return stats;
    }
}
