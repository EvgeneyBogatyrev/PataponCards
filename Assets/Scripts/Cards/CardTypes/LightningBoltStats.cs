using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LightningBoltStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 2;
        stats.description = "Deal " + damage.ToString() + " damage to a unit.";
        stats.name = "Shock";

        stats.isSpell = true;
        stats.hasOwnSound = true;
        static IEnumerator FangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;
            BoardManager.Slot targetSlot;

            AudioController.PlaySound("Kharma-shock");

            int target = targets[0];
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            targetMinion.ReceiveDamage(damage);
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = FangRealization;
        stats.numberOfTargets = 1;

        stats.imagePath = "Shock_hq";
        return stats;
    }
}