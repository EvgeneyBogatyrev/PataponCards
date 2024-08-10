using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EarthShatteringBlowStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int smallDamage = 2;
        const int bigDamage = 7;
        const int threshold = 8;
        stats.description = "Deal " + smallDamage.ToString() + " damage to an enemy non-Hatapon unit. If you controll a non-Hatapon unit with power " + threshold.ToString() + " or greater, deal " + bigDamage.ToString() + " damage to this unit instead.";
        stats.name = "Earth Shattering Blow";

        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.descriptionSize = 3;
        stats.nameSize = 3;

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

            bool completed = false;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    if (minion.GetPower() >= threshold)
                    {
                        completed = true;
                        break;
                    }
                }
            }

            if (!completed)
            {
                targetMinion.ReceiveDamage(smallDamage);
            }
            else
            {
                targetMinion.ReceiveDamage(bigDamage);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                if (friendlySlots[target - 1].GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                {
                    return false;
                }
            }

            if (target < 0)
            {
                if (enemySlots[-target - 1].GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                {
                    return false;
                }
            }

            return true;
        }

        stats.spell = Realization;
        stats.checkSpellTarget = CheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "DekaponBlow";
        return stats;
    }
}