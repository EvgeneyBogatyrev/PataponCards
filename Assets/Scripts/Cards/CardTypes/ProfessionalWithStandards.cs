using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProfessionalWithStandards
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 3;
        stats.description = "Cycling.";
        stats.name = "Questing beast";

        static void ProfessionalWIthStandardsDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            if (friendlySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
            }
        }

        //stats.hasDeathrattle = true;
        //stats.onDeathEvent = ProfessionalWIthStandardsDeathrattle;
        stats.cycling = true;

        stats.imagePath = "questing_beast";

        return stats;
    }
}
