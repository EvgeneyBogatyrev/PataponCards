using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoppenStats 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>On play</b>: Summon an Ice Wall for your opponent in front of Coppen.";
        stats.name = "Coppen";
        stats.runes.Add(Runes.Bow);

        stats.additionalRules.Add("If a space in from of the <i>Coppen</i> isn't empty, <i>Coppen</i>'s <b>On play</b> effect does nothing.");

        stats.relevantCards.Add(CardTypes.IceWall);

        stats.hasAfterPlayEvent = true;

        static IEnumerator CoppenRealization(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            int index;
            List<BoardManager.Slot> chooseSlots;
            List<BoardManager.Slot> oppSlots;
            if (target > 0)
            {
                chooseSlots = friendlySlots;
                oppSlots = enemySlots;
                index = target - 1;
            }
            else
            {
                chooseSlots = enemySlots;
                oppSlots = friendlySlots;
                index = -target - 1;
            }
            BoardManager.Slot slot;
            slot = oppSlots[index];
            if (slot.GetFree())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager iceWallCard = handManager.GenerateCard(CardTypes.IceWall, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                boardManager.PlayCard(iceWallCard,chooseSlots[index].GetPosition(), slot, destroy:false, record:false);
                
                iceWallCard.DestroyCard();
            }
            gameController.actionIsHappening = false;
            yield return null;                            
        }

        stats.afterPlayEvent = CoppenRealization;
        //stats.dummyTarget = true;
        //stats.numberOfTargets = 1;

        stats.imagePath = "coppen";

        return stats;
    }
}
