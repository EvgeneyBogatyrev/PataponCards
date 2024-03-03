using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BringThePoisonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int poisonNumber = 3;
        
        stats.description = "Add " + poisonNumber.ToString() + " 'Doom Shroom' cards to your hand.";
        stats.name = "Bring the Poison";

        stats.nameSize = 4;
        
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;

            if (!enemySlots[0].GetFriendly())
            {
                for (int i = 0; i < poisonNumber; ++i)
                {
                    handManager.AddCardToHand(CardTypes.DoomShroom);
                }
            }
            else
            {
                for (int i = 0; i < poisonNumber; ++i)
                {
                    handManager.DrawCardOpponent(fromDeck:false);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        //stats.checkSpellTarget = CheckTarget;
        stats.spell = Realization;
        stats.numberOfTargets = 0;

        stats.imagePath = "bring_the_poison";
        return stats;
    }
}