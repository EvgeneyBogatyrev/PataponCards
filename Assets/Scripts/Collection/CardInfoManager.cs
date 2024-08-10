using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class CardInfoController
{
    private static bool instanceExists = false;
    public static void Create(CardTypes cardType, GameObject prefab)
    {
        if (instanceExists)
        {
            return;
        }
        SetInstanceExists(true);
        GameObject info = GameObject.Instantiate(prefab);
        info.GetComponent<CardInfoManager>().Create(cardType);        
        info.transform.position = new Vector3(info.transform.position.x - 3.26f, 
                                              info.transform.position.y + 1.52f,
                                              -2f);
              
    }

    public static void SetInstanceExists(bool value)
    {
        instanceExists = value;
    }

    public static string GetMechanicDescription(string mechanic)
    {
        switch (mechanic)
        {
            case "On play":
                return "<b>On play</b> effect triggers when this unit enters the battlefield.";
            case "End of turn":
                return "<b>End of turn</b> effect triggers at the end of your turn.";
            case "Start of turn":
                return "<b>Start of turn</b> effect triggers at the end of your turn.";
            case "On death":
                return "<b>On death</b> effect triggers when this unit is destroyed.";
            case "On attack":
                return "<b>On attack</b> effect triggers when you attack with this unit.";
            case "Cycling":
                return "You can discard cards with <b>Cycling</b> to draw a card. You can cycle once per turn.";
            case "Poison":
                return "<b>Poisoned</b> units receive 1 damage at the start of their controller's turn.";
            case "Devotion deck":
                return "Your <b>Devotion</b> to a specific class equals to the amount of this class symbols you select during deckbuilding.";
            case "Devotion card":
                return "A card's <b>Devotion</b> to a specific class equals to the amount of this class symbols in the top right corner of the card.";
            case "Pacifism":
                return "A unit with <b>Pacifism</b> can't attack, move, or deal damage.";
            case "Abilities":
                return "A unit with <b>Abilities</b> can atcivate it's ability once per turn instead of attacking. Ability's cost is subtracted from unit's power.";
            case "Lifelink":
                return "Your Hatapon can't be damaged when you control a unit with <b>Lifelink</b>. This doesn't apply to poison and fatique damage.";
            case "Hexproof":
                return "A unit with <b>Hexproof</b> can't be targeted by spells and abilities.";
            case "Haste":
                return "A unit with <b>Haste</b> can attack and move as soon as it enters the battlefield from hand.";
            case "Remove doesn't trigger death":
                return "Removing a unit from the battlefield without destroying it does not trigget <b>On death</b> effects.";
            default:
                return "no translation needed";
        }
    }
}

public class CardInfoManager : MonoBehaviour
{
    private CardManager mainCard = null;
    private List<GameObject> relevantCards = new();
    public GameObject cardPosition;
    public GameObject cardReprPosition;
    public GameObject infoText;
    public GameObject cardPrefab;
    public GameObject cardReprPrefab;


    private string GetCardTextFromStats(CardManager.CardStats stats)
    {
        string cardText = "";
        if (!stats.suppressOnPlay && (stats.hasOnPlaySpell || stats.hasAfterPlayEvent))
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("On play") + "\n";
        }
        if (stats.hasOnDeath)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("On death") + "\n";
        }
        if (stats.endTurnEvent != null)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("End of turn") + "\n";
        }
        if (stats.startTurnEvent != null)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Start of turn") + "\n";
        }
        if (stats.onAttackEvent != null)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("On attack") + "\n";
        }
        if (stats.cycling)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Cycling") + "\n";
        }
        if (stats.pacifism)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Pacifism") + "\n";
        }
        if (stats.isStatic)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Pacifism") + "\n";
            cardText += "* " + CardInfoController.GetMechanicDescription("Abilities") + "\n";
        }
        if (stats.hasShield)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Lifelink") + "\n";
        }
        if (stats.hexproof)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Hexproof") + "\n";
        }
        if (stats.hasHaste)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription("Haste") + "\n";
        }
        foreach (string keyword in stats.additionalKeywords)
        {
            cardText += "* " + CardInfoController.GetMechanicDescription(keyword) + "\n";
        }
        return cardText;
    }

    public void Create(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab, this.gameObject.transform);
        mainCard = newCard.GetComponent<CardManager>();
        mainCard.numberOfCardsObject.SetActive(false);
        CardGenerator.CustomizeCard(mainCard, cardType);
        mainCard.SetCardState(CardManager.CardState.hilightOver);

        CardManager.CardStats stats = mainCard.GetCardStats();

        string cardText = GetCardTextFromStats(stats);
        
        float shift = 0.6f;
        int index = 0;
        foreach (CardTypes addCardType in stats.relevantCards)
        {
            GameObject relCard = Instantiate(cardPrefab, this.gameObject.transform);
            CardGenerator.CustomizeCard(relCard.GetComponent<CardManager>(), addCardType);
            CardReprManager cardReprObj = Instantiate(cardReprPrefab, this.gameObject.transform).GetComponent<CardReprManager>();
            string cardName = relCard.GetComponent<CardManager>().GetName();

            cardText += GetCardTextFromStats(relCard.GetComponent<CardManager>().GetCardStats());

            cardReprObj.SetName(cardName);
            cardReprObj.relevantCard = true;
            cardReprObj.type = addCardType;
            cardReprObj.SetVisualNumber();
            
            cardReprObj.gameObject.transform.position = new Vector3(cardReprPosition.transform.position.x, cardReprPosition.transform.position.y - shift * index, 0f);

            cardReprObj.index = index;

            index += 1;

            relevantCards.Add(cardReprObj.gameObject);
            

            Destroy(relCard);
        } 

        foreach (string rule in stats.additionalRules)
        {
            cardText += "* " + rule + "\n";
        }

        cardText += "\nArt by: " + stats.artistName;

        infoText.GetComponent<TextMeshPro>().text = cardText; 
    }

    public void Destroy()
    {
        if (mainCard != null)
        {
            Destroy(mainCard.gameObject);
        }
        foreach (GameObject card in relevantCards)
        {
            CardReprManager cm = card.GetComponent<CardReprManager>();
            if (cm.previewedCard != null)
            {
                Destroy(cm.previewedCard.gameObject);
            }
            Destroy(card);
        }
        Destroy(gameObject);
        CardInfoController.SetInstanceExists(false);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Destroy();
            CursorController.cursorState = CursorController.CursorStates.Free;
        }
    }
}