using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EarthquakeStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 2;
        stats.description = "Deal " + damage.ToString() + " damage to an enemy unit. Other enemy units are pushed to the sides of the board.";
        stats.name = "Earthquake";

        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.descriptionSize = 4;
        stats.nameSize = 4;

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

            // Moving minions to the left
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (minion == targetMinion)
                    {
                        break;
                    }
                    foreach (BoardManager.Slot searchEmptySlot in enemySlots)
                    {
                        if (searchEmptySlot == slot)
                        {
                            break;
                        }
                        if (searchEmptySlot.GetFree())
                        {
                            minion.SetSlot(searchEmptySlot);
                            break;
                        }
                    }
                }
            }

            enemySlots.Reverse();

            // Moving minions to the right
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (minion == targetMinion)
                    {
                        break;
                    }
                    foreach (BoardManager.Slot searchEmptySlot in enemySlots)
                    {
                        if (searchEmptySlot == slot)
                        {
                            break;
                        }

                        if (searchEmptySlot.GetFree())
                        {
                            minion.SetSlot(searchEmptySlot);
                            break;
                        }
                    }
                }
            }

            if (targetMinion != null)
            {
                targetMinion.ReceiveDamage(damage);
            }
            enemySlots.Reverse();


            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                return false;
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