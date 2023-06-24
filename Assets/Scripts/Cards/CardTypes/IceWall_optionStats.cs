using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IceWall_optionStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int IceWallHealthCost = 0;
        const int IceWallSelfDamage = 1;
        
        stats.description = "Deal 1 damage to this.";
        stats.name = "Melt Down";

        stats.isSpell = true;
        static IEnumerator IceWall_optionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.TakePower(IceWallHealthCost);
            host.ReceiveDamage(IceWallSelfDamage);
            yield return null;
        }
        stats.spell = IceWall_optionRealization;
        stats.numberOfTargets = 0;
        stats.damageToHost = IceWallHealthCost;

        stats.imagePath = "ice_wall";

        return stats;
    }
}
