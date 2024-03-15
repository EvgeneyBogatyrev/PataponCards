using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DesperadoStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.runes.Add(Runes.Bow);
        stats.description = "Draw X cards where X is your Devotion to Bow. Your Hatapon loses twice as much life.";
        stats.name = "Desperado";

        stats.additionalKeywords.Add("Devotion deck");

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            
            bool enemy = enemySlots[1].GetFriendly();
            int devotion = DeckManager.GetDeckDevotion(Runes.Bow, enemy);
            
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.DealDamageToThis(devotion * 2);
                    break;
                }
            }

            if (!enemy)
            {
                for (int i = 0; i < devotion; ++i)
                {
                    handManager.DrawCard();
                }
            }
            else
            {
                for (int i = 0; i < devotion; ++i)
                {
                    handManager.DrawCardOpponent();
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = Realization;

        stats.imagePath = "desperado";
        return stats;
    }
}