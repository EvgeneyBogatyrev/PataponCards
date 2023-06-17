using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AlossonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int alossonDamage = 2;
        const int alossonMax = 5;
        stats.power = 4;
        stats.description = "Deal " + alossonDamage.ToString() + " damage to the unit with highest power. If it survives, repeat the process (Up to " + alossonMax.ToString() + ")";
        stats.name = "Alosson";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        

        stats.hasOnPlay = true;

        static void AlossonRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {

            for (int limit = 0; limit < alossonMax; ++limit)
            {
                MinionManager strongestMinion = null;
                foreach (BoardManager.Slot slot in enemySlots)
                {
                    MinionManager minion = slot.GetConnectedMinion();
                    if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                    {
                        if (strongestMinion == null || strongestMinion.GetPower() < minion.GetPower())
                        {
                            strongestMinion = minion;
                        }
                    }
                }
                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    MinionManager minion = slot.GetConnectedMinion();
                    if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                    {
                        if (strongestMinion == null || strongestMinion.GetPower() < minion.GetPower())
                        {
                            strongestMinion = minion;
                        }
                    }
                }

                if (strongestMinion == null)
                {
                    break;
                }
                strongestMinion.ReceiveDamage(alossonDamage);
                if (strongestMinion.GetPower() <= 0)
                {
                    break;
                }
            }
        }

        stats.spell = AlossonRealization;
        stats.numberOfTargets = 0;

        return stats;
    }
}
