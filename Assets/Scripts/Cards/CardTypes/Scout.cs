using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScoutStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "End of turn: Draw a card. Your Hatapon loses 1 life for each time this ability has triggered.";
        stats.name = "Scout";
        stats.imagePath = "scout";
        
        stats.runes.Add(Runes.Bow);
        //stats.runes.Add(Runes.Bow);

        stats.additionalRules.Add("Damage happens even if you didn't draw a card for any reason.");


        static IEnumerator EndTurn(int thisIndex, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;

            int cardsDrawn = 0;

            BoardManager.Slot thisSlot;
            thisSlot = friendlySlots[thisIndex];
           
            MinionManager minion = thisSlot.GetConnectedMinion();
            if (minion != null)
            { 
               minion.GetCardStats().cardsDrawnByThis = minion.GetCardStats().cardsDrawnByThis + 1;
               Debug.Log(minion.GetCardStats().cardsDrawnByThis);
               cardsDrawn = minion.GetCardStats().cardsDrawnByThis;
            }

            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion_ = slot.GetConnectedMinion();
                if (minion_ != null && minion_.GetCardType() == CardTypes.Hatapon)
                {
                    minion_.DealDamageToThis(cardsDrawn);
                    break;
                }
            }
            
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = EndTurn;

        return stats;
    }
}
