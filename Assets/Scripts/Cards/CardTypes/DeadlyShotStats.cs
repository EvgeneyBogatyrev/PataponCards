using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyShotStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "Destroy target unit.";
        stats.name = "Deadly Shot";

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;
        static IEnumerator realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
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
            targetMinion.DestroyMinion();
            
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot targetSlot;
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            if (targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }
        
        stats.checkSpellTarget = CheckTarget;

        stats.spell = realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "deadly_shot";
        return stats;
    }
}
