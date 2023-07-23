using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyamsarStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "At the end of your turn summon a copy of this minion with 1 less power.";
        stats.name = "Rapappa";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        static IEnumerator MyamsarEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager thisOne = friendlySlots[index].GetConnectedMinion();

            BoardManager.Slot targetSlot = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (slot.GetFree())
                {
                    targetSlot = slot;
                    break;
                }
            }

            if (targetSlot == null)
            {
                yield return null;
            }
            else
            {

                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                int newPower = thisOne.GetPower() - 1;
                if (newPower > 0)
                {
                    CardManager newCard = handManager.GenerateCard(CardTypes.Myamsar, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    newCard.SetPower(newPower);
                    boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), targetSlot, destroy: true, record: false);
                }
            yield return null;
            }
        }
        stats.endTurnEvent = MyamsarEndTurn;

        stats.imagePath = "rapappa";
        return stats;
    }
}
