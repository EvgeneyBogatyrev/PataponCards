using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TakeThatShieldStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int takeThatShieldGain = 3;
        stats.description = "Target non-Hatapon unit under your control gains +" + takeThatShieldGain.ToString() + " power. Heal your Hatapon by that unit's power.";
        stats.name = "Take That Shield";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.nameSize = 4;

        stats.isSpell = true;

        static bool TakeThatShieldCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

        static IEnumerator TakeThatShieldRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            targetMinion.Heal(takeThatShieldGain);

            foreach (BoardManager.Slot _slot in friendlySlots)
            {
                MinionManager minion = _slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.Heal(targetMinion.GetPower());
                }
            }

            yield return null;
        }

        stats.spell = TakeThatShieldRealization;
        stats.checkSpellTarget = TakeThatShieldCheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "take_shield";

        return stats;
    }
}
