using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanThePakaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "Cards in your hand have <b>Cycling</b>.";
        stats.name = "Pan the Pakapon";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        //stats.descriptionSize = 3;
        stats.nameSize = 5;

        stats.giveCyclingToCardsInHand = true;

        stats.additionalKeywords.Add("Cycling");
         stats.artistName = "Pavel Shpagin (Poki)";



        stats.imagePath = "pan_pakapon";
        return stats;
    }
}
