using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProfessionalWithStandards
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 2;
        stats.description = "When it dies, you draw a card.";
        stats.name = "Professional With Standards";

        static void ProfessionalWIthStandardsDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            if (index > 0)
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
            }
        }

        stats.hasDeathrattle = true;
        stats.onDeathEvent = ProfessionalWIthStandardsDeathrattle;

        return stats;
    }
}
