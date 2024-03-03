using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BadaDrumStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int threshold = 3;
        stats.description = "Gain control of target non-Hatapon unit your opponent controls with power " + threshold.ToString() + " or less.";
        stats.name = "Zigoton Drum";
        
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot targetSlot;

            int target = targets[0];
            bool friendly;
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
                friendly = false;
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
                friendly = true;
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            
            if (friendly)
            {
                BoardManager.Slot slotToGo = friendlySlots[targetMinion.GetSlot().GetIndex()];
                if (!slotToGo.GetFree())
                {
                    foreach (BoardManager.Slot _slot in friendlySlots)
                    {
                        if (_slot.GetFree())
                        {
                            slotToGo = _slot;
                            break;
                        }
                    }
                }

                if (!slotToGo.GetFree())
                {
                    targetMinion.DestroyMinion();
                }
                else
                {
                    targetMinion.GetSlot().SetFree(true);
                    targetMinion.GetSlot().SetConnectedMinion(null);
                    targetMinion.SetSlot(slotToGo);
                    slotToGo.SetFree(false);
                    slotToGo.SetConnectedMinion(targetMinion);
                    targetMinion.SetFriendly(true);
                }
            }
            else
            {
                BoardManager.Slot slotToGo = enemySlots[targetMinion.GetSlot().GetIndex()];
                if (!slotToGo.GetFree())
                {
                    foreach (BoardManager.Slot _slot in enemySlots)
                    {
                        if (_slot.GetFree())
                        {
                            slotToGo = _slot;
                            break;
                        }
                    }
                }

                if (!slotToGo.GetFree())
                {
                    targetMinion.DestroyMinion();
                }
                else
                {
                    targetMinion.GetSlot().SetFree(true);
                    targetMinion.GetSlot().SetConnectedMinion(null);
                    targetMinion.SetSlot(slotToGo);
                    slotToGo.SetFree(false);
                    slotToGo.SetConnectedMinion(targetMinion);
                    targetMinion.SetFriendly(false);

                }
            }


            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                return false;
            }
            if (enemySlots[-target - 1].GetConnectedMinion().GetPower() > threshold)
            {
                return false;
            }
            if (enemySlots[-target - 1].GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }

        stats.spell = Realization;
        stats.checkSpellTarget = CheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "bada_drum";
        return stats;
    }
}