using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HealingScepterStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int healing = 10;

        stats.description = "Restore " + healing.ToString() + " health to your Hatapon.";
        stats.name = "Healing Scepter";

        stats.nameSize = 4;
        
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.Heal(healing);
                    break;
                }
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = Realization;
        stats.numberOfTargets = 0;

        stats.imagePath = "healing_scepter_hq";
        return stats;
    }
}