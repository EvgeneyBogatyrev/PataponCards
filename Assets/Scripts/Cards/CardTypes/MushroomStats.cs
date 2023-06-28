using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 1;
        const int selfDamage = 1;

        stats.power = 4;
        stats.description = "At the start of your turn poison the adjacent minions, then deal " + selfDamage.ToString() + " damage to itself.";
        stats.name = "Mushroom";
        stats.runes.Add(Runes.Bow);
        stats.canAttack = false;
        stats.canDealDamage = false;
        stats.limitedVision = true;

        static IEnumerator StartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (index > 0) 
            {
                MinionManager minion = friendlySlots[index - 1].GetConnectedMinion();
                if (minion != null)
                {
                    //minion.ReceiveDamage(damage);
                    minion.PoisonMinion();
                }
            }

            if (index < 6) 
            {
                MinionManager minion = friendlySlots[index + 1].GetConnectedMinion();
                if (minion != null)
                {
                    //minion.ReceiveDamage(damage);
                    minion.PoisonMinion();
                }
            }

            MinionManager me = friendlySlots[index].GetConnectedMinion();
            if (me != null)
            {
                me.ReceiveDamage(selfDamage);
            }
            yield return null;
        }

        stats.startTurnEvent = StartTurn;

        stats.imagePath = "mushroom";
        return stats;
    }
}
