using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProfessionalWithStandards
{
    static int beastDamage = 3;
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 3;
        stats.description = "<b>Cycling</b>.\nWhenever you Cycle Questing Beast, your Hatapon loses " + beastDamage.ToString() + " life.";
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

        static IEnumerator OnCycle(List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager connectedMinion = slot.GetConnectedMinion();
                if (connectedMinion != null)
                {
                    if (connectedMinion.GetCardType() == CardTypes.Hatapon)
                    {
                        connectedMinion.DealDamageToThis(beastDamage);
                    }
                }
            }
            yield return null;
        }

        stats.onCycleEvent = OnCycle;

        //stats.hasDeathrattle = true;
        //stats.onDeathEvent = ProfessionalWIthStandardsDeathrattle;
        stats.cycling = true;

        stats.imagePath = "questing_beast";

        return stats;
    }
}
