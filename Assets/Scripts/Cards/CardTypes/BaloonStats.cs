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
        stats.description = "Cannot be a target of an attack. At the end of your turn deal " + baloonDamage.ToString() + " damage to the enemy Hatapon.";
        stats.name = "Hot Air Ballon";
        stats.flying = true;
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        static IEnumerator BaloonEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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
            yield return null;
        }
        stats.endTurnEvent = BaloonEndTurn;

        stats.imagePath = "hot_air_ballon";
        return stats;
    }
}
