using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoribuStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "Can move to the closest non-adjacent enemy slot. If it is not empty, destroy enemy unit on it.";
        stats.name = "Moribu";
        //stats.hasHaste = true;
       
        stats.runes.Add(Runes.Spear);

        stats.imagePath = "moribu";

        return stats;
    }
}
