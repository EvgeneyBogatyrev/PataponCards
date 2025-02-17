using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparringPartnerStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        
        const int gain = 2;
        stats.description = "<b>On play</b>: Target non-Hatapon unit under your control gains +" + gain.ToString() + " power and <b>Lifelink</b>.";
        stats.name = "Sparring Partner";
        stats.runes.Add(Runes.Shield);
        stats.power = 3;

        stats.nameSize = 4;

        stats.hasOnPlaySpell = true;

        static bool CheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            CardManager.CardStats stats = targetMinion.GetCardStats();
            stats.hasShield = true;
            targetMinion.SetCardStats(stats);
            

            targetMinion.Heal(gain);
            yield return null;
        }

        stats.spell = Realization;
        stats.checkSpellTarget = CheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "sparring_partner";

        return stats;
    }
}
