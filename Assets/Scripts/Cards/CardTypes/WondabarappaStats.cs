using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WondabarappaStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int targetDamage = 1;
        const int heal = 3;
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.power = 1;
        stats.description = "Haste.\nOn play: Deal " + targetDamage.ToString() + " damage. Draw a card. Restore " + heal.ToString() + " health to your Hatapon.";
        stats.name = "Wondabarappa";
        
        stats.hasHaste = true;
        stats.hasOnPlay = true;

        static void Battlecry(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            // Draw a card
            if (index > 0)
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
            }
        }

        static void Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            // Deal damage
            BoardManager.Slot selectedSlot;
            int target = targets[0];
            if (target < 0)
            {
                target *= -1;
                target -= 1;
                selectedSlot = enemySlots[target];
            }
            else
            {
                target -= 1;
                selectedSlot = friendlySlots[target];
            }

            selectedSlot.GetConnectedMinion().ReceiveDamage(targetDamage);

            // Restore Health
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager connectedMinion = slot.GetConnectedMinion();
                if (connectedMinion != null)
                {
                    if (connectedMinion.GetCardType() == CardTypes.Hatapon)
                    {
                        connectedMinion.Heal(heal);
                    }
                }
            }
        }

        stats.spell = Realization;
        stats.numberOfTargets = 1;

        stats.hasBattlecry = true;
        stats.onPlayEvent = Battlecry;


        stats.imagePath = "wondabarappa";
        return stats;
    }
}
