using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WebStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int gain = 2;
        stats.power = 1;
        stats.description = "<b>Pacifism.\nOn play</b>: Give all friendly units +" + gain.ToString() + " power.\n<b>On death</b>: Draw a card.";
        stats.name = "Wep";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.pacifism = true;

        stats.hasOnPlaySpell = true;
        stats.hasOnDeath = true;

        static IEnumerator OnPlay(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.Heal(gain);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator OnDeath(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
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
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = OnPlay;
        stats.onDeathEvent = OnDeath;

        stats.imagePath = "web";        
        return stats;
    }
}
