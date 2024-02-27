using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TropicalTailwindStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int drawAmount = 2;
        const int powerIncrease = 1;
        
        stats.description = "Draw " + drawAmount.ToString() + " cards. Your units with Devotion to Bow get +" + powerIncrease.ToString() + " power.";
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
                int cardIncrement = System.Math.Min(drawAmount, 7 - handManager.GetNumberOfCards());
                for (int i = 0; i < cardIncrement; ++i)
                {
                    handManager.DrawCard();
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    int devotionToBow = minion.GetDevotion(Runes.Bow);
                    if (devotionToBow > 0f)
                    {
                        minion.Heal(powerIncrease);
                    }
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator _old_Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;

            float secondsBetweenAnimations = 0.5f;

            MinionManager minion = friendlySlots[targets[0] - 1].GetConnectedMinion();
            if (minion.GetCardStats().endTurnEvent != null)
            {
                if (boardManager.friendlySlots[0].GetFriendly())
                {
                    gameController.StartCoroutine(minion.GetCardStats().endTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                }
                else
                {
                    gameController.StartCoroutine(minion.GetCardStats().endTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                }
                do {
                        yield return new WaitForSeconds(secondsBetweenAnimations);
                    } while(gameController.actionIsHappening);
            }
            gameController.actionIsHappening = true;
            foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
            {
                if (boardManager.friendlySlots[0].GetFriendly())
                {
                    gameController.StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                }
                else
                {
                    gameController.StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                }
                
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

        //stats.checkSpellTarget = CheckTarget;
        stats.spell = Realization;
        stats.numberOfTargets = 0;

        stats.imagePath = "tropical_tailwind";
        return stats;
    }
}