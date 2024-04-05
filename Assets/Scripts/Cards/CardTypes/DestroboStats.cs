using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DestroboStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int destroboDamage = 1;
        stats.power = 3;
        stats.description = "<b>On play</b>: Choose a unit. If it's not a Hatapon and has no <b>Devotion</b>, destroy it. Otherwise, deal " + destroboDamage.ToString() +  " damage to it.";
        stats.name = "Destrobo";
        stats.runes.Add(Runes.Shield);

        stats.additionalKeywords.Add("Devotion card");

        stats.hasOnPlaySpell = true;

        static IEnumerator DestroboRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot selectedSlot;
            int target = targets[0];
            if (target < 0)
            {
                target *= -1;
                target -= 1;
                selectedSlot = enemySlots[target];
            }
            else
            {
                target -= 1;
                selectedSlot = friendlySlots[target];
            }

            MinionManager selectedMinion = selectedSlot.GetConnectedMinion();
            
            if (selectedMinion.GetCardType() != CardTypes.Hatapon && selectedMinion.GetDevotion(Runes.Bow) == 0 
                && selectedMinion.GetDevotion(Runes.Spear) == 0 && selectedMinion.GetDevotion(Runes.Shield) == 0)
            {
                selectedMinion.DestroyMinion();
            }
            else
            {
                selectedMinion.ReceiveDamage(destroboDamage);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = DestroboRealization;
        stats.numberOfTargets = 1;

        stats.imagePath = "destrobo";        
        return stats;
    }
}
