using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StoneFreeStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 1;
        stats.description = "Greatshield.\nCan't attack and move.";
        stats.name = "Petrified Patapon";
        stats.nameSize = 3;
        stats.canAttack = false;
        stats.canDealDamage = false;
        stats.limitedVision = true;
        stats.hasGreatshield = true;
        stats.imagePath = "turnToStone";

        return stats;
    }
}
