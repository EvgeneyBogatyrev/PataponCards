using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoboponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 1;
        stats.description = "End of turn: Gain +X power, where X is your Devotion to Shield.";
        stats.name = "Robopon";
        stats.runes.Add(Runes.Shield);


        static IEnumerator RoboponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            if (friendlySlots[1].GetFriendly())
            {
                friendlySlots[index].GetConnectedMinion().Heal(DeckManager.GetDeckDevotion(Runes.Shield, false));
            }
            else
            {
                friendlySlots[index].GetConnectedMinion().Heal(DeckManager.GetDeckDevotion(Runes.Shield, true));
            }

            
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.endTurnEvent = RoboponEndTurn;

        stats.imagePath = "robopon";
        return stats;
    }
}
