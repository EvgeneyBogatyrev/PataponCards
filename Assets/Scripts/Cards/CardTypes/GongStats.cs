using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GongStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int yumiponDamage = 1;

        stats.power = 5;
        stats.description = "Hexproof.\nAt the end of your turn deal " + yumiponDamage.ToString() + " damage to all enemy characters.";
        stats.name = "Gong the Hawkeye";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Shield);

        stats.legendary = true;
        stats.hexproof = true;

        stats.nameSize = 5;

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

        stats.imagePath = "gong_the_hawkeye";
        return stats;
    }
}
