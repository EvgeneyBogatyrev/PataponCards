using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DarkOneStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Start of turn</b>: Discard the top card of your library. If it is a spell, transform into Kiba form. Otherwise, transform into Bird form.";
        stats.name = "The Dark One";

        stats.descriptionSize = 3;

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.relevantCards.Add(CardTypes.KibaForm);
        stats.relevantCards.Add(CardTypes.BirdForm);

        stats.onPlaySound = "dark_one";

        static IEnumerator Realization(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            gameController.actionIsHappening = true;

            MinionManager thisMinion = friendlySlots[index].GetConnectedMinion();

            //Debug.Log(thisMinion.GetCardType());

            if (friendlySlots[0].GetFriendly())
            {
                if (DeckManager.playDeck.Count > 0)
                {
                    CardTypes card = DeckManager.playDeck[0];
                    CardManager cardObj = handManager.GenerateCard(card, new Vector3(-10f, -10f, -10f)).GetComponent<CardManager>();
                    
                    CardTypes transformType;
                    if (cardObj.GetCardStats().isSpell)
                    {
                        transformType = CardTypes.BirdForm;
                    }
                    else
                    {
                        transformType = CardTypes.KibaForm;
                    }
                    CardManager newCard = handManager.GenerateCard(transformType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    BoardManager.Slot slot = thisMinion.GetSlot();
                    thisMinion.DestroySelf(unattach:true);
                    boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), slot, destroy: true, record: false);
            
                    handManager.MillCard();
                    cardObj.DestroyCard();
                }
            }  
            else
            {
                if (DeckManager.opponentsDeck.Count > 0)
                {    
                    CardTypes card = DeckManager.opponentsDeck[0];
                    CardManager cardObj = handManager.GenerateCard(card, new Vector3(-10f, -10f, -10f)).GetComponent<CardManager>();
                    
                    CardTypes transformType;
                    if (cardObj.GetCardStats().isSpell)
                    {
                        transformType = CardTypes.BirdForm;
                    }
                    else
                    {
                        transformType = CardTypes.KibaForm;
                    }
                    CardManager newCard = handManager.GenerateCard(transformType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    BoardManager.Slot slot = thisMinion.GetSlot();
                    thisMinion.DestroySelf(unattach:true);
                    boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), slot, destroy: true, record: false);
            
                    handManager.MillCardOpp();
                    cardObj.DestroyCard();
                }
            }
            gameController.UpdateDecks();
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.startTurnEvent = Realization;
       
        stats.imagePath = "Dark_One_hq";
        return stats;
    }
}


public static class BirdFormStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "Cannot be attacked by units.";
        stats.name = "Bird Form";
        stats.runes.Add(Runes.Spear);

        stats.flying = true;

        stats.imagePath = "Bird_form_hq";
        stats.onPlaySound = "bird_form";
        return stats;
    }
}

public static class KibaFormStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "<b>Haste</b>.\nCan attack any enemy unit on the board.";
        stats.name = "Kiba Form";
        stats.megaVision = true;
        stats.hasHaste = true;
        stats.runes.Add(Runes.Spear);
        
        stats.imagePath = "Kiba_form_hq";
        stats.onPlaySound = "kiba_form";
        return stats;
    }
}
