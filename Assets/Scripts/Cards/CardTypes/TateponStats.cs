using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TateponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 4;
        stats.description = "Lifelink (your Hatapon is immune to attacks and spell damage).";
        stats.name = "Tatepon";
        stats.runes.Add(Runes.Shield);
        stats.imagePath = "tatepon";
        stats.hasShield = true;

        //stats.hasOnPlay = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            BoardManager.Slot selectedSlot;
            int target = targets[1];
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
            //selectedMinion.GetCardStats().lifelinkMeTo = targets[0];
            //Debug.Log("dummy target " + targets[0].ToString());
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target < 0)
            {
                return false;
            }
            return true;
        }

        //stats.spell = Realization;
        //stats.checkSpellTarget = CheckTarget;
        //stats.numberOfTargets = 2;
        //stats.dummyTarget = true;


        return stats;
    }
}