using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaNovaStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Nova Nova";
        stats.description = "Destroy all units.";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.legendary = true;

        stats.isSpell = true;
        static IEnumerator NovaNovaRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
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
