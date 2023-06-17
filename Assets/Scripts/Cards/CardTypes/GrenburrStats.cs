using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GrenburrStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int grenburrPower = 6;

        stats.power = 3;
        stats.description = "Always deals " + grenburrPower.ToString() + " damage regardless of its power.";
        stats.name = "Grenburr";
        stats.fixedPower = grenburrPower;
        stats.runes.Add(Runes.Shield);

        stats.imagePath = "grenburr";
        return stats;
    }
}
