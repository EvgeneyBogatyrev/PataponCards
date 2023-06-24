using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DestroboStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int destroboDamage = 1;
        stats.power = 2;
        stats.description = "Choose a creature. If it's an artifact or it can't attack, destroy it. Otherwise, deal " + destroboDamage.ToString() +  " damage.";
        stats.name = "Destrobo";
        stats.runes.Add(Runes.Shield);

        stats.hasOnPlay = true;

        static IEnumerator DestroboRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
            
            if (selectedMinion.GetCardStats().isStatic || (!selectedMinion.GetCardStats().canAttack && selectedMinion.GetCardType() != CardTypes.Hatapon))
            {
                selectedMinion.DestroyMinion();
            }
            else
            {
                selectedMinion.ReceiveDamage(destroboDamage);
            }
            yield return null;
        }

        stats.spell = DestroboRealization;
        stats.numberOfTargets = 1;


        stats.imagePath = "destrobo";

        return stats;
    }
}
