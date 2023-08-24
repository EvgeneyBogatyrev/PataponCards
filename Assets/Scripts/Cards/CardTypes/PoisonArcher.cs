using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoisonArcherStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int damage = 1;
        stats.power = 3;
        stats.description = "On play: deal " + damage.ToString() + " damage to a unit and poison it.\nOn attack: poison the attacked unit.";
        stats.name = "Poison archer";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.hasOnPlay = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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
            
            selectedMinion.ReceiveDamage(damage);
            selectedMinion.PoisonMinion();
            yield return null;
        }

        static IEnumerator onAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int enemyIndex = targets[1];
            MinionManager selectedMinion;
            if (enemyIndex < 0)
            {
                enemyIndex *= -1;
                enemyIndex -= 1;
                selectedMinion = enemySlots[enemyIndex].GetConnectedMinion();
            }
            else
            {
                enemyIndex -= 1;
                selectedMinion = friendlySlots[enemyIndex].GetConnectedMinion();
            }
            selectedMinion.PoisonMinion();
            yield return null;
        }

        stats.spell = Realization;
        stats.onAttackEvent = onAttack;
        stats.numberOfTargets = 1;


        stats.imagePath = "poison_bow";

        return stats;
    }
}
