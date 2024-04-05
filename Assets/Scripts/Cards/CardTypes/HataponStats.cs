using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HataponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 20;
        stats.description = "Can't attack and deal damage.\nWhen Hatapon dies, you lose this round.\n<i>Protect him at all cost!</i>";
        stats.name = "Hatapon";
        stats.canAttack = false;
        stats.canDealDamage = false;
        stats.hasHaste = true;

        stats.imagePath = "Hatapon";

        return stats;
    }
}
