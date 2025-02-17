using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaloonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int baloonDamage = 1;
        stats.power = 2;
        stats.description = "Can't be attacked by units.\n<b>End of turn</b>: Deal " + baloonDamage.ToString() + " damage to the enemy Hatapon.";
        stats.name = "Hot Air Ballon";
        stats.flying = true;
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        static IEnumerator BaloonEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager connectedMinion = null;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot.GetConnectedMinion() != null && slot.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                {
                    connectedMinion = slot.GetConnectedMinion();
                }
            }
            
            if (connectedMinion != null)
            {
                connectedMinion.ReceiveDamage(baloonDamage);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = BaloonEndTurn;

        stats.imagePath = "hot_air_ballon";
        stats.artistName = "Unused game assets";
        return stats;
    }
}
