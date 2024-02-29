using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OfferingToKamiStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int buff = 1;
        stats.description = "Destroy a friendly unit. Your units gain +" + buff.ToString() + " power.";
        stats.name = "Offering to Kami";
        stats.nameSize = 5;

        stats.runes.Add(Runes.Spear);

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
            targetMinion.DestroyMinion();

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.Heal(buff);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool FangCheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target < 0)
            {
                return false;
            }
            return true;
        }

        stats.spell = FangRealization;
        stats.checkSpellTarget = FangCheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "offering";
        return stats;
    }
}