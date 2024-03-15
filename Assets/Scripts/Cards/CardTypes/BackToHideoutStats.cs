using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackToHideoutStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "Return a friendly non-Hatapon unit to your hand. You can play another card this turn.";
        stats.name = "Back to Hideout";
        stats.runes.Add(Runes.Bow);
        stats.nameSize = 4;

        stats.additionalKeywords.Add("Remove doesn't trigger death");

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            
            BoardManager.Slot targetSlot;

            int target = targets[0];
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            
            CardTypes cardType = targetMinion.GetCardType();

            targetMinion.GetSlot().SetFree(true);
            targetMinion.DestroySelf();

            if (!enemySlots[0].GetFriendly())
            {
                handManager.AddCardToHand(cardType);
                handManager.SetCanPlayCard(true);
            }
            else
            {
                handManager.DrawCardOpponent(fromDeck:false);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot targetSlot;
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                return false;
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            if (targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }
        
        stats.checkSpellTarget = CheckTarget;

        stats.spell = Realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "BackToHideout";
        return stats;
    }
}