using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZigotonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 5;
        stats.description = "<i>A Zigoton prophecy once said: \"If the Patapons reach Earthend, the Zigoton empire would be destroyed\"</i>";
        stats.name = "Zigoton Troops";
        stats.nameSize = 4;
        stats.descriptionSize = 4;
        stats.imagePath = "zigoton";
        return stats;
    }
}
