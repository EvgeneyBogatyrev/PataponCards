using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NaturalEnemyStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.runes.Add(Runes.Bow);
        stats.description = "Destroy target unit with Devotion to Shield.";
        stats.name = "Natural Enemy";

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot targetSlot;
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            int devotion = targetMinion.GetDevotion(Runes.Shield);
            Debug.Log("Devotion " + devotion.ToString());
            if (devotion > 0f)
            {
                return true;
            }
            return false;
        }

        stats.spell = Realization;
        stats.checkSpellTarget = CheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "natural_enemy";
        return stats;
    }
}