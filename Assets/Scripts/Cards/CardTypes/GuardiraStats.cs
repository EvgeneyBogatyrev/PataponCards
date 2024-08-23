using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GuardiraStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int guardiraPower = 1;

        stats.power = 8;
        stats.description = "<b>Lifelink</b>.\nAlways deals " + guardiraPower.ToString() + " damage regardless of its power.";
        stats.name = "Guardira";
        stats.fixedPower = guardiraPower;
        stats.runes.Add(Runes.Shield);
        //stats.runes.Add(Runes.Shield);
        stats.hasShield = true;
        //stats.hasOnPlay = true;

        stats.additionalRules.Add("Damage prevention effects are still applyed when <i>Guardira's</i> damage is calculated.");

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
            selectedMinion.GetCardStats().lifelinkMeTo = targets[0];
            Debug.Log("dummy target " + targets[0].ToString());
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

        stats.imagePath = "Guardira-Chin";
        stats.artistName = "Official render";
        return stats;
    }
}
