using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PonteoStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "On play: Reveal the top of your library until you find a unit with Haste. Draw it and discard the rest.";
        stats.name = "Ponteo";

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.hasOnPlaySpell = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            gameController.actionIsHappening = true;

            while (true)
            {
                if (friendlySlots[0].GetFriendly())
                {
                    if (DeckManager.playDeck.Count == 0)
                    {
                        gameController.actionIsHappening = false;
                        break;
                    }
                    CardTypes card = DeckManager.playDeck[0];

                    CardManager cardObj = handManager.GenerateCard(card, new Vector3(-10f, -10f, -10f)).GetComponent<CardManager>();
                    bool hasHaste = false;
                    if (cardObj.GetCardStats().hasHaste)
                    {
                        hasHaste = true;
                    }

                    cardObj.DestroyCard();

                    if (!hasHaste)
                    {
                        handManager.MillCard();
                    }
                    else
                    {
                        handManager.DrawCard();
                        break;
                    }
                }  
                else
                {
                    if (DeckManager.opponentsDeck.Count == 0)
                    {
                        gameController.actionIsHappening = false;
                        break;
                    }
                    CardTypes card = DeckManager.opponentsDeck[0];

                    CardManager cardObj = handManager.GenerateCard(card, new Vector3(-10f, -10f, -10f)).GetComponent<CardManager>();
                    bool hasHaste = false;
                    if (cardObj.GetCardStats().hasHaste)
                    {
                        hasHaste = true;
                    }

                    cardObj.DestroyCard();

                    if (!hasHaste)
                    {
                        handManager.MillCardOpp();
                    }
                    else
                    {
                        handManager.DrawCardOpponent();
                        break;
                    }
                }
                gameController.UpdateDecks();
                yield return new WaitForSeconds(1f); 
            }           

            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = Realization;
        stats.numberOfTargets = 0;
        //stats.dummyTarget = true;

        stats.imagePath = "ponteo";
        return stats;
    }
}
