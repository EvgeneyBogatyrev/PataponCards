using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RantanStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int heal = 3;
        stats.runes.Add(Runes.Bow);

        stats.power = 2;
        stats.description = "On play: Restore " + heal.ToString() + " health to your Hatapon.";
        stats.name = "Rantan";
        
        stats.hasOnPlay = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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
            yield return null;
        }

        stats.spell = Realization;
        stats.numberOfTargets = 0;

        stats.imagePath = "rantan";
        return stats;
    }
}
