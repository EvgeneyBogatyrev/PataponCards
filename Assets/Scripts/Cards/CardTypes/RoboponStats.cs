using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoboponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 1;
        stats.description = "<b>End of turn:</b> If Robopon didn't attack or move this turn, it gains +X power, where X is your <b>Devotion to Shield</b>.";
        stats.name = "Robopon";
        stats.runes.Add(Runes.Shield);

        stats.additionalKeywords.Add("Devotion deck");

        static IEnumerator RoboponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            MinionManager robopon = friendlySlots[index].GetConnectedMinion();
            if (!robopon.attacked && !robopon.moved)
            {
                robopon.Heal(DeckManager.GetDeckDevotion(Runes.Shield, !friendlySlots[1].GetFriendly()));
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.endTurnEvent = RoboponEndTurn;

        stats.imagePath = "robopon_hq";
        return stats;
    }
}
