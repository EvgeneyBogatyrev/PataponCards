using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RantanStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int heal = 4;
        const int lesserHeal = 2;

        stats.runes.Add(Runes.Bow);

        stats.power = 2;
        stats.description = "<b>Cycling.\nOn play</b>: Restore " + heal.ToString() + " health to your Hatapon.\nWhenever you <b>cycle</b> this card, restore " + lesserHeal.ToString() + " health to your Hatapon.";
        stats.name = "Rantan";
        
        stats.hasOnPlaySpell = true;

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

        static IEnumerator OnCycle(List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager connectedMinion = slot.GetConnectedMinion();
                if (connectedMinion != null)
                {
                    if (connectedMinion.GetCardType() == CardTypes.Hatapon)
                    {
                        connectedMinion.Heal(lesserHeal);
                    }
                }
            }
            yield return null;
        }

        stats.spell = Realization;
        stats.onCycleEvent = OnCycle;
        stats.numberOfTargets = 0;
        stats.descriptionSize = 3;
        stats.cycling = true;

        stats.imagePath = "rantan";
        return stats;
    }
}
