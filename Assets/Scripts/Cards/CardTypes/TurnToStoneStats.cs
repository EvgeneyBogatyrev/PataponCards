using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TurnToStoneStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Turn to stone";
        stats.description = "Target non-Hatapon unit loses all abilities.";
       
        //stats.runes.Add(Runes.Bow);
        //card.SetNameSize(4);

        stats.additionalRules.Add("Target unit losses all effects but keeps its power.");
        stats.additionalRules.Add("This does not apply to aura effects that are given to a unit by other units.");
        stats.additionalRules.Add("Unit does not lose its <b>Devotion</b>, however it loses all negative status effects.");


        stats.isSpell = true;
        static IEnumerator TurnToStoneRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            MinionManager chosenMinion = null;
            if (targets[0] > 0)
            {
                chosenMinion = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }
            else
            {
                chosenMinion = enemySlots[-targets[0] - 1].GetConnectedMinion();
            }

            if (chosenMinion != null)
            {
                int power = chosenMinion.GetPower();
                List<Runes> tmpRunes = new List<Runes>();
                foreach (Runes rune in chosenMinion.GetCardStats().runes)
                {
                    tmpRunes.Add(rune);
                }
                chosenMinion.SetCardStats(new CardManager.CardStats());
                chosenMinion.SetPower(power);
                chosenMinion.GetCardStats().runes = tmpRunes;
                chosenMinion.powerSquare.SetActive(true);
                chosenMinion.heartObject.SetActive(false);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (target > 0)
            {
                if (friendlySlots[target - 1].GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                {
                    return false;
                }
            }

            if (target < 0)
            {
                if (enemySlots[-target - 1].GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                {
                    return false;
                }
            }

            return true;
        }

        stats.spell = TurnToStoneRealization;
        stats.checkSpellTarget = CheckTarget;
        stats.numberOfTargets = 1;
        stats.imagePath = "turnToStone";

        return stats;
    }
}
