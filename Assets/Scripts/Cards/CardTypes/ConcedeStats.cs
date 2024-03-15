using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConcedeStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "<i>You win this round!</i>";
        stats.name = "Opponent concedes";

        stats.nameSize = 4;

        stats.isSpell = true;
        static IEnumerator ConcedeRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (minion.GetCardType() == CardTypes.Hatapon)
                    {
                        minion.DestroyMinion();
                        break;
                    }
                }
            }
            yield return null;
        }

        stats.spell = ConcedeRealization;
        stats.numberOfTargets = 0;

        stats.imagePath = "Hatapon";

        return stats;
    }
}
