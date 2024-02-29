using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moforumo : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 5;
        stats.description = "Hexproof (can't be targeted by spells or abilities).";
        stats.name = "Moforumo";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.hexproof = true;

        stats.imagePath = "moforumo";
        return stats;
    }
}
