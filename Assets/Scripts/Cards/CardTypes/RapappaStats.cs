using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyamsarStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "<b>End of turn</b>: Summon a copy of this unit with 1 less power.";
        stats.name = "Rapappa";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.onPlaySound = "rapappa";

        stats.additionalRules.Add("If a copy would be summoned with 0 or less power, it is not summoned instead.");


        static IEnumerator MyamsarEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager thisOne = friendlySlots[index].GetConnectedMinion();

            // A same-frame-batched end-of-turn effect from another card (e.g. Alldemonium's
            // recurring damage) may have already killed this unit before its own end-of-turn
            // event gets its turn to run (queued events for the same minion process in a fixed
            // order, not necessarily this one first) - if so, there's nothing to summon a copy
            // of, and no error should follow from it just having died normally.
            if (thisOne == null)
            {
                gameController.actionIsHappening = false;
                yield break;
            }

            BoardManager.Slot targetSlot = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (slot.GetFree())
                {
                    targetSlot = slot;
                    break;
                }
            }

            if (targetSlot != null)
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                int newPower = thisOne.GetPower() - 1;
                if (newPower > 0)
                {
                    CardManager newCard = handManager.GenerateCard(thisOne.GetCardType(), new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    newCard.SetPower(newPower);

                    boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), targetSlot, destroy: true, record: false, fromHand:false);
                }
            }
            //GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = MyamsarEndTurn;

        stats.imagePath = "rapappa_HQ";
        return stats;
    }
}
