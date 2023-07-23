using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannasaultStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "Has 1 armor.\nAll end turn and start turn effects can't trigger.";
        stats.name = "Cannasault";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Spear);

        stats.armor = 1;

        stats.imagePath = "cannasault";
        return stats;
    }
}
