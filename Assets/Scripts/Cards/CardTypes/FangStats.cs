using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FangStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int fangDamage = 3;
        stats.description = "Deal " + fangDamage.ToString() + " damage to an enemy unit.";
        stats.name = "Fang";

        stats.isSpell = true;
        static IEnumerator FangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;
            BoardManager.Slot targetSlot;

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
            targetMinion.ReceiveDamage(fangDamage);
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool FangCheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                return false;
            }
            return true;
        }

        stats.spell = FangRealization;
        stats.checkSpellTarget = FangCheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "fang_hq";
        stats.artistName = "Official render";
        return stats;
    }
}