using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AlossonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int alossonDamage = 1;
        const int alossonMax = 14;
        stats.power = 4;
        stats.description = "Deal " + alossonDamage.ToString() + " damage to all characters. If at least of them dies, repeat the process (Up to " + alossonMax.ToString() + " times).";
        stats.name = "Alosson";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        

        stats.hasOnPlay = true;

        static IEnumerator AlossonRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            int index = targets[0];
            for (int limit = 0; limit < alossonMax; ++limit)
            {
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

                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (!slot.GetFree())
                    {
                        if (slot == friendlySlots[index])
                        {
                            continue;
                        }
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

                Debug.Log(spearArray.Count);

                bool someoneDied = false;
                bool arrowsExists = true;
                while (arrowsExists)
                {
                    arrowsExists = false;

                    foreach (SpearManager spear in spearArray)
                    {
                        //Debug.Log(spear.exhausted);
                        //Debug.Log(spear.reachDestination);
                        if (!spear.reachDestination)
                        {
                            arrowsExists = true;
                        }
                        else if (!spear.exhausted)
                        {
                            spear.DestroySelf();
                            spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(alossonDamage);
                            //Debug.Log("deal damage");
                            MinionManager connectedMinion = spear.GetSlotToGo().GetConnectedMinion();
                            if (connectedMinion == null || connectedMinion.GetPower() <= 0)
                            {
                                someoneDied = true;
                            }
                            spear.exhausted = true;
                        }
                        Debug.Log(arrowsExists);
                    }
                    Debug.Log("Before new Time");
                    yield return new WaitForSeconds(0.1f);
                    Debug.Log("Arrows exists");
                    Debug.Log(arrowsExists);
                }
                
                if (!someoneDied)
                {
                    break;
                }      
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = AlossonRealization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        stats.imagePath = "alosson";

        return stats;
    }
}
