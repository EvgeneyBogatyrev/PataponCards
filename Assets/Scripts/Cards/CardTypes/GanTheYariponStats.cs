using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GanTheYariponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "<b>On play</b>: Repeat all <b>End of turn</b> effents of cards you played this game.";
        stats.name = "Gan Yaripon";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.additionalKeywords.Add("End of turn");
        stats.additionalRules.Add("<b>End of turn</b> effects are repeated in order they were played.");

        stats.hasAfterPlayEvent = true;
        //stats.hasOnPlay = true;

        static IEnumerator OnPlay(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            
            //while (gameController.actionIsHappening)
            //{
            //    yield return new WaitForSeconds(0.1f);
            //}
            
            gameController.actionIsHappening = true;

            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();

            //MinionManager thisMinion;
            List<CardTypes> cardsList;
            if (index > 0)
            {
                //thisMinion = friendlySlots[index - 1].GetConnectedMinion();
                cardsList = boardManager.playedCards;
                //Debug.Log("Me");
            }
            else
            {
                //thisMinion = enemySlots[-index - 1].GetConnectedMinion();
                cardsList = boardManager.playedCardsOpponent;
                //Debug.Log("Opp");
            }
            //Debug.Log("HEEREEERERRERE");
            yield return new WaitForSeconds(1.2f);
            foreach (CardTypes cardType in cardsList)
            {
                HandManager.DestroyDisplayedCards();
                CardManager newCard = handManager.GenerateCard(cardType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                newCard.SetCardState(CardManager.CardState.opponentPlayed);
                newCard.destroyTimer = HandManager.cardDestroyTimer;
                
                if (newCard.GetCardStats().endTurnEvent != null)
                {
                    int realIndex;
                    if (index > 0)
                    {
                        realIndex = index - 1;
                        newCard.StartCoroutine(newCard.GetCardStats().endTurnEvent(realIndex, enemySlots, friendlySlots));
                    
                    } 
                    else if (index < 0)
                    {
                        realIndex = -index - 1;
                        newCard.StartCoroutine(newCard.GetCardStats().endTurnEvent(realIndex, friendlySlots, enemySlots));
                    
                    }
                    while (gameController.actionIsHappening)
                    {
                        //Debug.Log("Action");
                        yield return new WaitForSeconds(0.1f);
                    }
                    gameController.actionIsHappening = true;
                    yield return new WaitForSeconds(1.5f);
                }

                newCard.DestroyCard();

            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.afterPlayEvent = OnPlay;
        //stats.numberOfTargets = 2;
        //stats.cycling = true;

        //stats.onPlayEvent = MegaponBattlecry;

        stats.imagePath = "gan_yaripon";
        return stats;
    }
}
