using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProfessionalWithStandards
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 2;
        stats.description = "On death: You draw a card.";
        stats.name = "Questing beast";

        static void ProfessionalWIthStandardsDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            //Debug.Log(index);
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

        stats.hasDeathrattle = true;
        stats.onDeathEvent = ProfessionalWIthStandardsDeathrattle;

        stats.imagePath = "questing_beast";

        return stats;
    }
}
