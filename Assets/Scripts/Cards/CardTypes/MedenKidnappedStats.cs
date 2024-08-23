using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedenKidnappedStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "Put target non-Hatapon unit on top of your deck.";
        stats.name = "Meden Kidnapped";
        stats.nameSize = 5;

        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.additionalKeywords.Add("Remove doesn't trigger death");

        stats.isSpell = true;
        static IEnumerator realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            gameController.actionIsHappening = true;
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
            CardManager cardIntoDeck = handManager.GenerateCard(cardType).GetComponent<CardManager>();
            cardIntoDeck.SetCardState(CardManager.CardState.ShuffleIntoDeck);

            bool opp;
            if (enemySlots[1].GetFriendly())
            {
                opp = true;
            }
            else
            {
                opp = false;
            }

            DeckManager.PutCardOnTop(cardType, opp);

            targetMinion.GetSlot().SetFree(true);
            targetMinion.DestroySelf();

            /*
            if (opp)
            {
                handManager.DrawCard();
            }
            else
            {
                handManager.DrawCardOpponent();
            }
            */
            
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
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            if (targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }
        
        stats.checkSpellTarget = CheckTarget;

        stats.spell = realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "meden_kidnapped_hq";
        return stats;
    }
}
