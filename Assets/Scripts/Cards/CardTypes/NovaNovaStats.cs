using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaNovaStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Nova Nova";
        stats.description = "Destroy all non-Hatapon units. Deal 3 damage to your Hatapon for each unit destroyed this way.";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        //stats.legendary = true;

        stats.isSpell = true;
        static IEnumerator NovaNovaRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int healthLoss = 0;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
                    healthLoss += 3;
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
                    healthLoss += 3;
                }
            }
            
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.TakePower(healthLoss);
                }
            }

            yield return null;
        }

        stats.spell = NovaNovaRealization;
        stats.numberOfTargets = 0;
        stats.imagePath = "NovaNova";
        return stats;
    }
}
