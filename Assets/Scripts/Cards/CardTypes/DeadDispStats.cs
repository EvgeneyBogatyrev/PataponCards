using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeadDispStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "Target unit attacks another target unit.";
        stats.name = "Deadly Dispute";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.isSpell = true;
        static IEnumerator DeadlyDisputeRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            List<MinionManager> minions = new List<MinionManager>();

            for (int i = 0; i < 2; ++i)
            {
                int target = targets[i];
                if (target < 0)
                {
                    target *= -1;
                    target -= 1;
                    minions.Add(enemySlots[target].GetConnectedMinion());
                }
                else
                {
                    target -= 1;
                    minions.Add(friendlySlots[target].GetConnectedMinion());
                }
            }

            minions[0].Attack(minions[1]);
            yield return null;
        }

        static bool DeadlyDisputeCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (targets[0] == targets[1])
            {
                return false;
            }
            return true;
        }

        stats.spell = DeadlyDisputeRealization;
        stats.checkSpellTargets = DeadlyDisputeCheckTargets;
        stats.numberOfTargets = 2;

        stats.imagePath = "deadly_dispute";
        return stats;
    }
}