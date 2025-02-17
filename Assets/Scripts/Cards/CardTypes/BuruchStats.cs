using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuruchStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 9;
        stats.description = "<b>On play</b>: Destroy target friendly unit.";
        stats.name = "Buruch";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);


        stats.hasOnPlaySpell = true;

        static IEnumerator BuruchRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {

            BoardManager.Slot selectedSlot;
            MinionManager selectedMinion;
            foreach (int curTarget in targets)
            {
                int target = curTarget;
                if (target < 0)
                {
                    target *= -1;
                    target -= 1;
                    selectedSlot = enemySlots[target];
                }
                else
                {
                    target -= 1;
                    selectedSlot = friendlySlots[target];
                }

                selectedMinion = selectedSlot.GetConnectedMinion();
                
                selectedMinion.DestroyMinion();   
            }  
            yield return null;               
        }

        static bool BuruchCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (_target < 0)
            {
                return false;
            }
            return true;
        }

        static bool BuruchCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            /*
            if (targets[0] == targets[1])
            {
                return false;
            }
            return true;
            */
            return true;
        }

        stats.spell = BuruchRealization;
        stats.checkSpellTarget = BuruchCheckTarget;
        stats.checkSpellTargets = BuruchCheckTargets;
        stats.numberOfTargets = 1;
        stats.imagePath = "buruch_hq";

        return stats;
    }
}
