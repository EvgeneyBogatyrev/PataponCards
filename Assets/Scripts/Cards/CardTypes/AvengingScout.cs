using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AvengingScoutStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 2;
        stats.description = "On death: Deal 1 + X damage to the strongest enemy unit, where X is your Devotion to Spear.";
        stats.name = "Avenging Scout";
        stats.runes.Add(Runes.Spear);
        
       
        stats.hasOnDeath = true;
        static IEnumerator OnDeath(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager selectedMinion = null;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (selectedMinion == null || minion.GetPower() > selectedMinion.GetPower())
                    {
                        selectedMinion = minion;
                    }
                }
            }

            if (selectedMinion != null)
            {
                bool enemy = enemySlots[1].GetFriendly();
                int devotion = DeckManager.GetDeckDevotion(Runes.Spear, enemy);
                selectedMinion.ReceiveDamage(1 + devotion);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }


        
        stats.onDeathEvent = OnDeath;
        

        stats.imagePath = "AvengingScout";        
        return stats;
    }
}
