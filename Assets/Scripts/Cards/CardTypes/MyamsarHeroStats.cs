using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyamsarHeroStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 3;
        stats.description = "When it enters the battlefield, enemy non-Hatapon minion in front of it loses all abilities, can't attack and move until this is alive.";
        stats.name = "Myamsar, hero";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.hasBattlecry = true;
        stats.hasDeathrattle = true;
        stats.descriptionSize = 3;

        stats.legendary = true;

        static void MyamsarBattlecry(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int targetMinionIndex = -1 * index;

            CardManager.CardStats newStats = new CardManager.CardStats();
            newStats.canAttack = false;
            newStats.limitedVision = true;

            MinionManager targetMinion;
            if (targetMinionIndex > 0) 
            {
                targetMinion = friendlySlots[targetMinionIndex - 1].GetConnectedMinion();
            }
            else
            {
                targetMinion = enemySlots[-1 * targetMinionIndex - 1].GetConnectedMinion();
            }
            if (targetMinion == null)
            {
                return;
            }
            if (targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return;
            }
            newStats.savedStats = targetMinion.GetCardStats();

            targetMinion.SetCardStats(newStats);
            CardManager.CardStats thisStats;
            if (index > 0)
            {
                thisStats = friendlySlots[index - 1].GetConnectedMinion().GetCardStats();
            }
            else
            {
                thisStats = enemySlots[-index - 1].GetConnectedMinion().GetCardStats();
            }

            thisStats.connectedMinions.Add(targetMinion);

            if (index > 0)
            {
                friendlySlots[index - 1].GetConnectedMinion().SetCardStats(thisStats);
            }
            else
            {
                enemySlots[-index - 1].GetConnectedMinion().SetCardStats(thisStats);
            }

        }


        static void MyamsarDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            foreach (MinionManager connectedMinion in thisStats.connectedMinions)
            {
                foreach (BoardManager.Slot slot in enemySlots)
                {
                    MinionManager slotMinion = slot.GetConnectedMinion();
                    if (slotMinion != null && slotMinion == connectedMinion)
                    {
                            connectedMinion.SetCardStats(connectedMinion.GetCardStats().savedStats);
                    }
                }
            }
        }


        stats.onPlayEvent = MyamsarBattlecry;
        stats.onDeathEvent = MyamsarDeathrattle; 

        stats.imagePath = "myamsar";

        return stats;
    }
}
