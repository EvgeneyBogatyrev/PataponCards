using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AlossonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int alossonDamage = 1;
        const int alossonMax = 4;
        stats.power = 2;
        stats.description = "On play: Deal " + alossonDamage.ToString() + 
                            " damage to all units. If at least of them dies, " +
                            "repeat the process (Up to " + alossonMax.ToString() + " times).";
        stats.name = "Alosson";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.imagePath = "alosson";

        stats.spell = AlossonRealization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        
        stats.hasOnPlaySpell = true;

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
                        
                        SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                        spear.SetSlotToGo(slot);
                        spearArray.Add(spear);
                        if (enemySlots[index].GetFriendly())
                        {
                            spear.isEnemy = true;
                        }
                    }
                }

                bool someoneDied = false;
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
                            spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(alossonDamage);
                            MinionManager connectedMinion = spear.GetSlotToGo().GetConnectedMinion();
                            if (connectedMinion == null || connectedMinion.GetPower() <= 0)
                            {
                                someoneDied = true;
                            }
                            spear.exhausted = true;
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                
                if (!someoneDied)
                {
                    break;
                }      
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        return stats;
    }
}
