using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class YariponBushwackerStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int destroboDamage = 1;
        stats.power = 2;
        stats.description = "<b>Haste</b> (can attack and move as soon as it enters the battlefield).\n<b>On play</b>: Deal " + destroboDamage.ToString() +  " damage.";
        stats.name = "Yaripon Bushwacker";
        stats.hasHaste = true;
        //stats.runes.Add(Runes.Shield);
        stats.nameSize = 4;
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
            
            selectedMinion.ReceiveDamage(destroboDamage);
            
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = DestroboRealization;
        stats.numberOfTargets = 1;

        stats.imagePath = "bushwacker";        
        return stats;
    }
}
