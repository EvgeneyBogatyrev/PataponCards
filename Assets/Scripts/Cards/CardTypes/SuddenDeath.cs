using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FatiqueStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "<i>After ten turns both Hatapons start to take damage at the start of the turn.</i>";
        stats.name = "Fatique!";

        //stats.nameSize = 4;

        stats.isSpell = true;
        static IEnumerator ConcedeRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            yield return null;
        }

        stats.spell = ConcedeRealization;
        stats.numberOfTargets = 0;

        stats.imagePath = "logo";

        return stats;
    }
}
