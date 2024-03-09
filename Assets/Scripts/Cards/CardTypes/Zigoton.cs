using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZigotonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 5;
        stats.description = "<b>Bold text</b>\nNormal text\n<i>Italic text</i>";
        stats.name = "Zigoton Troops";
        stats.nameSize = 4;
        stats.imagePath = "zigoton";
        return stats;
    }
}
