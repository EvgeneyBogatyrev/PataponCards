using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MahoponStats
{ 
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int mahoponTargetDamage = 2;
        const int mahoponAoEDamage = 1;
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.power = 1;
        stats.description = "Cycling.\nOn play: Deal " + mahoponTargetDamage.ToString() + " damage to target unit and " + mahoponAoEDamage.ToString() + " damage to all other units.";
        stats.name = "Mahopon";

        stats.hasOnPlaySpell = true;
        stats.cycling = true;

        static IEnumerator MahoponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            selectedSlot.GetConnectedMinion().ReceiveDamage(mahoponTargetDamage);

            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot != selectedSlot && !slot.GetFree())
                {
                    slot.GetConnectedMinion().ReceiveDamage(mahoponAoEDamage);
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (slot != selectedSlot && !slot.GetFree())
                {
                    slot.GetConnectedMinion().ReceiveDamage(mahoponAoEDamage);
                }
            }
            yield return null;
        }

        stats.spell = MahoponRealization;
        stats.numberOfTargets = 1;


        stats.imagePath = "mahopon";
        return stats;
    }
}
