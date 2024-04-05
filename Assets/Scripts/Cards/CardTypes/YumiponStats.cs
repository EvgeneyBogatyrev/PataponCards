using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class YumiponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int yumiponDamage = 1;

        stats.power = 2;
        stats.description = "<b>On attack</b>: Deal " + yumiponDamage.ToString() + " damage to all enemy units.";
        stats.name = "Yumipon";
        stats.runes.Add(Runes.Bow);

        static IEnumerator YumiponOnAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int thisIndex = targets[0];
            BoardManager.Slot thisSlot;
            if (thisIndex > 0)
            {
                thisIndex -= 1;
                thisSlot = friendlySlots[thisIndex];
            }
            else
            {
                thisIndex = -1 * thisIndex - 1;
                thisSlot = friendlySlots[thisIndex];
            }

            MinionManager minion = thisSlot.GetConnectedMinion();
            if (minion != null)
            {
                GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
                gameController.actionIsHappening = true;

                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                List<SpearManager> spearArray = new List<SpearManager>();

                foreach (BoardManager.Slot slot in enemySlots)
                {
                    if (!slot.GetFree())
                    {
                        //slot.GetConnectedMinion().GetDamage(yumiponDamage);
                        SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[thisIndex].GetPosition()).GetComponent<SpearManager>();
                        spear.SetSlotToGo(slot);
                        spearArray.Add(spear);
                        if (enemySlots[thisIndex].GetFriendly())
                        {
                            spear.isEnemy = true;
                        }
                    }
                }

                bool arrowsExists = true;

                while (arrowsExists)
                {
                    arrowsExists = false;

                    foreach (SpearManager spear in spearArray)
                    {
                        if (!spear.reachDestination)
                        {
                            arrowsExists = true;
                        }
                        else if (!spear.exhausted)
                        {
                            spear.DestroySelf();
                            spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(yumiponDamage);
                            spear.exhausted = true;
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                minion.onAttackActionProgress = false;
                gameController.actionIsHappening = false;
            }
            
            yield return null;
        }

        static IEnumerator YumiponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            List<SpearManager> spearArray = new List<SpearManager>();

            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (!slot.GetFree())
                {
                    //slot.GetConnectedMinion().GetDamage(yumiponDamage);
                    SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                    spear.SetSlotToGo(slot);
                    spearArray.Add(spear);
                    if (enemySlots[index].GetFriendly())
                    {
                        spear.isEnemy = true;
                    }
                }
            }

            bool arrowsExists = true;

            while (arrowsExists)
            {
                arrowsExists = false;

                foreach (SpearManager spear in spearArray)
                {
                    if (!spear.reachDestination)
                    {
                        arrowsExists = true;
                    }
                    else if (!spear.exhausted)
                    {
                        spear.DestroySelf();
                        spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(yumiponDamage);
                        spear.exhausted = true;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }
        //stats.endTurnEvent = YumiponEndTurn;
        stats.onAttackEvent = YumiponOnAttack;

        stats.imagePath = "yumipon";
        return stats;
    }
}