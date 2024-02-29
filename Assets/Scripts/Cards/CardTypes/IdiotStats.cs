using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IdiotStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int burusSelfDamage = 1;
        stats.power = 2;
        stats.description = "Pacifism. Start of turn: Draw a card and deal " + burusSelfDamage.ToString() + " damage to Bent Compass.";
        stats.name = "Bent Compass";
        stats.canAttack = false;
        stats.limitedVision = true;
        stats.canDealDamage = false;

        static IEnumerator FuckingIdiotStartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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
            friendlySlots[index].GetConnectedMinion().ReceiveDamage(burusSelfDamage);
            yield return null;
        }

        stats.startTurnEvent = FuckingIdiotStartTurn;

        stats.imagePath = "bent_compass";
        return stats;
    }
}
