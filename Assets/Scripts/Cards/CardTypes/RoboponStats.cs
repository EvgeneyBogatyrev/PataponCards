using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoboponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "At the end and the start of your turn gain +1 power.";
        stats.name = "Robopon";
        stats.runes.Add(Runes.Shield);


        static IEnumerator RoboponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            friendlySlots[index].GetConnectedMinion().Heal(1);
            yield return null;
        }

        stats.endTurnEvent = RoboponEndTurn;
        stats.startTurnEvent = RoboponEndTurn;

        stats.imagePath = "robopon";
        return stats;
    }
}
