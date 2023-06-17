using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpeedBoost 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        
        stats.description = "Target non-Hatapon creature under your control gains can attack again this turn.";
        stats.name = "Speed Boost";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.isSpell = true;

        static bool SpeedBoostCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (_target < 0)
            {
                return false;
            }
            BoardManager.Slot _targetSlot;

            if (_target > 0)
            {
                _targetSlot = friendlySlots[_target - 1];
            }
            else
            {
                _targetSlot = enemySlots[-_target - 1];
            }
            MinionManager _targetMinion = _targetSlot.GetConnectedMinion();
            if (_targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }

        static void SpeedBoostRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot targetSlot;

            int target = targets[0];
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();

            targetMinion.SetCanAttack(true);

        }

        stats.spell = SpeedBoostRealization;
        stats.checkSpellTarget = SpeedBoostCheckTarget;
        stats.numberOfTargets = 1;

        return stats;
    }
}
