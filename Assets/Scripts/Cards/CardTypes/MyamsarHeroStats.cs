using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyamsarHeroStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 3;
        stats.description = "<b>On attack</b>: Destroy the weakest enemy unit if its power is less than Myamsar's power.";
        stats.name = "Myamsar";
        stats.runes.Add(Runes.Shield);
        //stats.runes.Add(Runes.Shield);
        //stats.runes.Add(Runes.Shield);

        //stats.descriptionSize = 3;
        stats.additionalRules.Add("If attacked unit is destroyed during the resolution of Myamsar's <b>On attack</b> effect, Myamsar won't receive any damage.");
        stats.additionalRules.Add("Myamsar's On attack effect can destroy enemy Hatapon.");

        static IEnumerator OnAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                MinionManager weakestEnemy = null;
                foreach (BoardManager.Slot slot in enemySlots)
                {
                    if (!slot.GetFree())
                    {
                        MinionManager enemy = slot.GetConnectedMinion();
                        if (weakestEnemy == null || enemy.GetPower() < weakestEnemy.GetPower())
                        {
                            weakestEnemy = enemy;
                        }
                    }
                }

                if (weakestEnemy.GetPower() < minion.GetPower())
                {
                    while ((minion.transform.position - weakestEnemy.transform.position).magnitude > 0.1f)
                    {
                        //Debug.Log((minion.transform.position - weakestEnemy.transform.position).magnitude);
                        minion.transform.position = Vector3.Lerp(minion.transform.position, weakestEnemy.transform.position, 0.3f);
                        yield return new WaitForSeconds(0.03f);
                    }

                    weakestEnemy.circleMyamsarObject.SetActive(true);
                    weakestEnemy.DestroyMinion();
                }

               
                minion.onAttackActionProgress = false;
                gameController.actionIsHappening = false;
            }
            
            yield return null;
        }

        stats.onAttackEvent = OnAttack;

        stats.imagePath = "myamsar";

        return stats;
    }
}
