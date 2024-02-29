using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuwagattanStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 11;
        stats.description = "On attack: Discard your hand.";
        stats.name = "Kuwagattan";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        static IEnumerator OnAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            int thisIndex = targets[0];
            BoardManager.Slot thisSlot;
            bool enemy;
            if (thisIndex > 0)
            {
                thisIndex -= 1;
                thisSlot = friendlySlots[thisIndex];
                enemy = false;
            }
            else
            {
                thisIndex = -1 * thisIndex - 1;
                thisSlot = friendlySlots[thisIndex];
                enemy = true;
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
                
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DiscardHand(!enemy);
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

        //stats.descriptionSize = 3;
        //stats.hasHaste = true;

        stats.imagePath = "kuwagattan";
        return stats;
    }
}
