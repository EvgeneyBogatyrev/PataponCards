using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WooyariStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int destroboDamage = 1;
        stats.power = 2;
        stats.description = "On play: Choose a unit. If it has Lifelink, destroy it.";
        stats.name = "Wooyari";
        stats.runes.Add(Runes.Spear);

        stats.hasOnPlaySpell = true;

        stats.additionalRules.Add("If a chosen unit doesn't have Lifelink, this effect does nothing.");


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
            
            if (selectedMinion.GetCardStats().hasShield)
            {
                selectedMinion.DestroyMinion();
            }
            
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = DestroboRealization;
        stats.numberOfTargets = 1;

        stats.imagePath = "Wooyari";        
        return stats;
    }
}
