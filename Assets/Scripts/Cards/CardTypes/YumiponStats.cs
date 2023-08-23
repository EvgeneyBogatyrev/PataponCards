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
        stats.description = "At the end of your turn deal " + yumiponDamage.ToString() + " damage to all enemy characters.";
        stats.name = "Yumipon";
        stats.runes.Add(Runes.Bow);

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
        stats.endTurnEvent = YumiponEndTurn;

        stats.imagePath = "yumipon";
        return stats;
    }
}