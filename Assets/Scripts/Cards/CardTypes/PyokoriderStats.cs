using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PyokoriderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int pyokoriderStartTurnPower = 2;

        stats.power = 6;
        stats.description = "Haste.\nOn attack: Reduce this unit's power by 1 (can't be below 1).";
        stats.name = "Ladodon";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        static IEnumerator PyokoriderStartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            friendlySlots[index].GetConnectedMinion().SetPower(pyokoriderStartTurnPower);
            yield return null;
        }

        static IEnumerator OnAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            int thisIndex = targets[0];
            BoardManager.Slot thisSlot;
            if (thisIndex > 0)
            {
                thisIndex -= 1;
                thisSlot = friendlySlots[thisIndex];
            }
            else
            {
                thisIndex = -1 * thisIndex - 1;
                thisSlot = friendlySlots[thisIndex];
            }

            MinionManager minion = thisSlot.GetConnectedMinion();
            if (minion != null)
            { 
                if (minion.GetPower() > 1)
                {
                    minion.LoseLife(1);
                }
                minion.onAttackActionProgress = false;
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.onAttackEvent = OnAttack;
        //stats.startTurnEvent = PyokoriderStartTurn;
        stats.hasHaste = true;

        stats.imagePath = "ladodon";
        return stats;
    }
}
