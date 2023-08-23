using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TropicalTailwindStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int drawAmount = 2;
        
        stats.description = "Choose a unit under your controll. Trigger its end turn effect.\nDraw " + drawAmount.ToString() + " cards.";
        stats.name = "Tropical Tailwind";

        stats.nameSize = 4;
        
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;

            float secondsBetweenAnimations = 0.5f;

            MinionManager minion = friendlySlots[targets[0] - 1].GetConnectedMinion();
            if (minion.GetCardStats().endTurnEvent != null)
            {
                gameController.StartCoroutine(minion.GetCardStats().endTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                do {
                        yield return new WaitForSeconds(secondsBetweenAnimations);
                    } while(gameController.actionIsHappening);
            }
            gameController.actionIsHappening = true;
            foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
            {
                gameController.StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                do {
                    yield return new WaitForSeconds(secondsBetweenAnimations);
                } while(gameController.actionIsHappening);
                gameController.actionIsHappening = true;
            }

            if (enemySlots[0].GetFriendly())
            {
                int cardIncrement = System.Math.Min(drawAmount, 7 - handManager.GetNumberOfOpponentsCards());
                for (int i = 0; i < cardIncrement; ++i)
                {
                    handManager.DrawCardOpponent();
                }
            }
            else
            {
                Debug.Log("In tailwind:" + handManager.GetNumberOfCards().ToString());
                int cardIncrement = System.Math.Min(drawAmount, 7 - handManager.GetNumberOfCards());
                for (int i = 0; i < cardIncrement; ++i)
                {
                    handManager.DrawCard();
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                return true;
            }
            return false;
        }

        stats.checkSpellTarget = CheckTarget;
        stats.spell = Realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "tropical_tailwind";
        return stats;
    }
}