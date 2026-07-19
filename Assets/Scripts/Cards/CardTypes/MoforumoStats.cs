using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moforumo : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 5;
        stats.description = "<b>Hexproof</b>.";
        stats.name = "Moforumo";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.hexproof = true;

        stats.imagePath = "moforumo";
        stats.onPlaySound = "dekaton_regular";
        return stats;
    }
}
