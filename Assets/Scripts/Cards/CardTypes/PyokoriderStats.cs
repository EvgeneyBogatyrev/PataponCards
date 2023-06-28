using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PyokoriderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int pyokoriderStartTurnPower = 2;

        stats.power = 5;
        stats.description = "Haste.\nAt the start of your turn set this creature's power to " + pyokoriderStartTurnPower.ToString();
        stats.name = "Ladodon";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        static IEnumerator PyokoriderStartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            friendlySlots[index].GetConnectedMinion().SetPower(pyokoriderStartTurnPower);
            yield return null;
        }

        stats.startTurnEvent = PyokoriderStartTurn;
        stats.hasHaste = true;

        stats.imagePath = "ladodon";
        return stats;
    }
}
