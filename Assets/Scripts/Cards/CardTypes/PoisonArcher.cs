using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoisonArcherStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        
        stats.description = "<b>On play: Poison</b> target unit.\n<b>On attack: Poison</b> attack target.";
        stats.name = "Poison Archer";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.hasOnPlaySpell = true;

        stats.additionalKeywords.Add("Poison");

        static IEnumerator OnPlay(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot selectedSlot;
            int target = targets[0];
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

            MinionManager selectedMinion = selectedSlot.GetConnectedMinion();
            selectedMinion.PoisonMinion();
            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator onAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            int thisIndex = targets[0];
            BoardManager.Slot thisSlot;
            
            int enemyIndex = targets[1];
            MinionManager selectedMinion;

            if (enemyIndex > 0)
            {
                enemyIndex -= 1;
                if (thisIndex > 0) 
                {
                    selectedMinion = friendlySlots[enemyIndex].GetConnectedMinion();
                }
                else
                {
                    selectedMinion = enemySlots[enemyIndex].GetConnectedMinion();
                }
            }
            else
            {
                enemyIndex *= -1;
                enemyIndex -= 1;
                if (thisIndex > 0)
                {
                    selectedMinion = enemySlots[enemyIndex].GetConnectedMinion();
                }
                else
                {
                    selectedMinion = friendlySlots[enemyIndex].GetConnectedMinion();
                }
            }
            selectedMinion.PoisonMinion();

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
                float angle = 0f;
                while (angle <= 360f)
                {
                    minion.transform.eulerAngles = new Vector3(0f, 0f, angle);
                    angle += 10f;
                    yield return new WaitForSeconds(0.02f);
                }
                
                minion.GetCardStats().blockEffects = true;
                minion.onAttackActionProgress = false;
            }
            gameController.actionIsHappening = false;
            yield return null;
        }


        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot selectedSlot;
            int target = targets[0];
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

            MinionManager selectedMinion = selectedSlot.GetConnectedMinion();
            
            

            selectedMinion.GetCardStats().onAttackEvent = onAttack;
            yield return null;
        }

        
        stats.spell = OnPlay;
        stats.onAttackEvent = onAttack;
        stats.numberOfTargets = 1;
        stats.power = 3;

        stats.imagePath = "poison_bow";

        return stats;
    }
}
