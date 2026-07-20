using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PonteoStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Haste</b>.\n<b>On play</b>: Reveal the top of your library until you find a unit with <b>Haste</b>. Draw it and discard the rest.";
        stats.name = "Ponteo";
        stats.descriptionSize = 3;

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.onPlaySound = "kibapon";

        stats.hasOnPlaySpell = true;
        stats.hasHaste = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            // Hand off to a coroutine hosted on GameController (a persistent object) instead of
            // running the mill loop directly here: this coroutine is started on the temporary
            // spell-hosting card, which self-destructs after HandManager.cardDestroyTimer (4.5s)
            // regardless of whether the effect is done - milling through several non-Haste
            // cards (1s wait each) can easily take longer than that, and losing that race would
            // silently kill this coroutine mid-effect, leaving actionIsHappening stuck true and
            // freezing the match.
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.StartCoroutine(MillUntilHaste(gameController, friendlySlots));
            yield return null;
        }

        static IEnumerator MillUntilHaste(GameController gameController, List<BoardManager.Slot> friendlySlots)
        {
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

        stats.imagePath = "ponteo_hq";
        return stats;
    }
}
