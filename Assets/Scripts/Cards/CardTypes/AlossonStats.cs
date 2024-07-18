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
        stats.power = 2;
        stats.description = "<b>On play:</b> Deal " + alossonDamage.ToString() + 
                            " damage to all units. If at least of them dies, " +
                            "repeat the process (Up to " + alossonMax.ToString() + " times).";
        stats.name = "Alosson";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.imagePath = "alosson_final";
        stats.onPlaySound = "alosson_on_play";
        stats.artistName = "korka123";

        stats.hasAfterPlayEvent = true;
        stats.afterPlayEvent = AlossonRealization;
        //stats.numberOfTargets = 1;
        //stats.dummyTarget = true;

        
        //stats.hasOnPlaySpell = true;

        static IEnumerator AlossonRealization(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            yield return new WaitForSeconds(2f);
            int index;
            List<BoardManager.Slot> chooseSlots;
            if (target > 0)
            {
                chooseSlots = friendlySlots;
                index = target - 1;
            }
            else
            {
                chooseSlots = enemySlots;
                index = -target - 1;
            }
            for (int limit = 0; limit < alossonMax; ++limit)
            {
                AudioController.PlaySound("hero_combo_" + (limit % 4).ToString());
                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                List<SpearManager> spearArray = new List<SpearManager>();

                foreach (BoardManager.Slot slot in enemySlots)
                {
                    if (!slot.GetFree())
                    {
                        if (target < 0 && slot.GetIndex() == index)
                        {
                            continue;
                        }
                        SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, chooseSlots[index].GetPosition()).GetComponent<SpearManager>();
                        string imagePath = "Images/alosson_lvl3";
                        if (limit == 0)
                        {
                            imagePath = "Images/alosson_lvl1";
                        }
                        else if (limit == 1)
                        {
                            imagePath = "Images/alosson_lvl2";
                        }
                        spear.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imagePath);
                        spear.SetSlotToGo(slot);
                        spearArray.Add(spear);
                        if (target < 0)
                        {
                            spear.isEnemy = true;
                        }
                    }
                }

                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (!slot.GetFree())
                    {
                        if (target > 0 && slot.GetIndex() == index)
                        {
                            continue;
                        }
                        
                        SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, chooseSlots[index].GetPosition()).GetComponent<SpearManager>();
                        string imagePath = "Images/alosson_lvl3";
                        if (limit == 0)
                        {
                            imagePath = "Images/alosson_lvl1";
                        }
                        else if (limit == 1)
                        {
                            imagePath = "Images/alosson_lvl2";
                        }
                        spear.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imagePath);
                        spear.SetSlotToGo(slot);
                        spearArray.Add(spear);
                        if (target < 0)
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
                yield return new WaitForSeconds(1.8f);     
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        return stats;
    }
}
