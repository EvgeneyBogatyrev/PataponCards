using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannasaultStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "<b>Haste</b>.\n<b>On attack</b>: Gain \"End turn and start turn effects can't trigger\" until the start of your next turn.";
        stats.name = "Cannasault";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Spear);

        stats.additionalRules.Add("When <i>Cannasault</i> dies, its effect is nullified .");

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
                float angle = 0f;
                while (angle <= 360f)
                {
                    minion.transform.eulerAngles = new Vector3(0f, 0f, angle);
                    angle += 10f;
                    yield return new WaitForSeconds(0.02f);
                }
                
                minion.GetCardStats().blockEffects = true;
                minion.onAttackActionProgress = false;
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator StartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager minion = friendlySlots[index].GetConnectedMinion();
            if (minion != null)
            {
                minion.GetCardStats().blockEffects = false;
            }
            yield return null;
        }

        stats.onAttackEvent = OnAttack;
        stats.startTurnEvent = StartTurn;

        stats.descriptionSize = 3;
        stats.hasHaste = true;

        stats.imagePath = "cannasault";
        return stats;
    }
}
