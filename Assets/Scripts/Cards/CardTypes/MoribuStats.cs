using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoribuStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "Can move in 'L' shape to the opponents board. If the slot is occupied by an enemy non-Hatapon unit, destroy it.";
        stats.name = "Moribu";
        //stats.hasHaste = true;
       
        stats.runes.Add(Runes.Spear);

        stats.imagePath = "moribu";

        stats.onPlaySound = "patapon_sound_" + UnityEngine.Random.Range(1, 5);

        return stats;
    }
}
