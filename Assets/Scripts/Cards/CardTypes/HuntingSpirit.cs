using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HuntingSpiritStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int drawAmount = 3;
        
        stats.description = "Draw " + drawAmount.ToString() + " cards. At the end of your next turn they will be discarded.";
        stats.name = "Hunting Spirit";

        //stats.nameSize = 4;
        
        stats.runes.Add(Runes.Spear);
        //stats.runes.Add(Runes.Spear);

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
                    handManager.DrawCard(ephemeral:1);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }
        
        stats.spell = Realization;

        stats.imagePath = "hunting_spirit";
        return stats;
    }
}