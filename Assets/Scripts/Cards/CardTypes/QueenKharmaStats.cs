using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QueenKharmaStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        
        stats.description = "<b>On play</b>: Add Queen Kharma's abilities to your hand.";
        stats.name = "Queen Kharma";

        stats.power = 3;
        
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Bow);

        stats.relevantCards.Add(CardTypes.LightningBolt);
        stats.relevantCards.Add(CardTypes.SleepingDust);
        stats.relevantCards.Add(CardTypes.MeteorRain);
        stats.relevantCards.Add(CardTypes.BadaDrum);

        stats.hasOnPlaySpell = true;

        stats.onPlaySound = "Kharma_enters";

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;

            AudioController.PlaySound("Kharma-effect");

            if (!enemySlots[0].GetFriendly())
            {
                handManager.AddCardToHand(CardTypes.LightningBolt);
                handManager.AddCardToHand(CardTypes.SleepingDust);
                handManager.AddCardToHand(CardTypes.MeteorRain);
                handManager.AddCardToHand(CardTypes.BadaDrum);
            }
            else
            {
                for (int i = 0; i < 4; ++i)
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

        stats.imagePath = "queen_kharma_hq";
        stats.artistName = "Official render";
        return stats;
    }
}
