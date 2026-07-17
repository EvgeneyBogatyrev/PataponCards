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
                                              -3f);
              
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
                return "<b>Start of turn</b> effect triggers at the start of your turn.";
            case "On death":
                return "<b>On death</b> effect triggers when this unit is destroyed.";
            case "On attack":
                return "<b>On attack</b> effect triggers when you attack with this unit.";
            case "Cycling":
                return "Right-click on a card with <b>Cycling</b> to discard it and draw a card. You can cycle once per turn.";
            case "Poison":
                return "<b>Poisoned</b> units receive 1 damage at the start of their controller's turn.";
            case "Devotion deck":
                return "Your <b>Devotion</b> to a specific class equals to the number of this class symbols you select during deck building.";
            case "Devotion card":
                return "A card's <b>Devotion</b> to a specific class equals to the number of this class symbols in the top right corner of the card.";
            case "Pacifism":
                return "A unit with <b>Pacifism</b> can not attack, move, or deal damage.";
            case "Abilities":
                return "A unit with <b>Abilities</b> can atcivate its ability once per turn instead of attacking. Ability's cost is subtracted from this unit's power.";
            case "Lifelink":
                return "Your Hatapon can not be damaged when you control a unit with <b>Lifelink</b>. This does not apply to poison and fatigue damage.";
            case "Hexproof":
                return "A unit with <b>Hexproof</b> can not be targeted by spells and abilities.";
            case "Haste":
                return "A unit with <b>Haste</b> can attack and move as soon as it enters the battlefield.";
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

    // Whatever CursorController.cursorState was right before this popup opened - restored on
    // close instead of hardcoding Free, since a caller in an active match (see MinionManager's
    // right-click preview) may have opened this during the opponent's turn (CursorStates.EnemyTurn),
    // not just the Free/idle state the Collection scene's own caller always opens it from.
    // Forcing Free unconditionally would wrongly re-enable playing cards mid-opponent-turn once
    // the popup closes.
    private CursorController.CursorStates previousCursorState;

    // Only non-null in the Game scene (Collection has no "Board" object) - see Create()/Destroy().
    private BoardManager boardManager;

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
            //cardText += "* " + CardInfoController.GetMechanicDescription("Pacifism") + "\n";
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
        // Captured before the caller (CardManager's Collection click handler, or MinionManager's
        // in-match right-click handler) moves the cursor into Select right after this call returns.
        previousCursorState = CursorController.cursorState;

        // Board minions have real (fairly large) BoxColliders - disabling them while this popup
        // is open stops them from winning the mouse raycast over the relevant-card repr icons
        // below (which can render at any screen position, not necessarily clear of the board).
        // They're already non-interactive while a popup is open anyway (CursorController.Select
        // gating throughout MinionManager), so this doesn't change any actual gameplay behavior -
        // it just removes a stale raycast target. No-op in the Collection scene (no "Board").
        GameObject boardObject = GameObject.Find("Board");
        if (boardObject != null)
        {
            boardManager = boardObject.GetComponent<BoardManager>();
            boardManager.SetAllMinionCollidersEnabled(false);
        }

        // This root object also carries its own BoxCollider (spanning most of the popup's own
        // footprint) that no script here actually uses - CardInfoManager has no OnMouseX handlers
        // of its own. An unused collider still blocks raycasts to anything behind/overlapping it
        // regardless of whether a script reacts to its events, so disable it defensively too.
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        GameObject newCard = Instantiate(cardPrefab, this.gameObject.transform);
        mainCard = newCard.GetComponent<CardManager>();
        // A fresh instance of the standard Card prefab carries its own full-size BoxCollider
        // (needed for CardManager's own hover/drag logic elsewhere, but this card is purely a
        // static hilightOver display here) - disable it so it can't block the mouse raycast to
        // the relevant-card repr icons sitting nearby in the same popup.
        Collider mainCardCollider = newCard.GetComponent<Collider>();
        if (mainCardCollider != null)
        {
            mainCardCollider.enabled = false;
        }
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
            
            // Z must follow cardReprPosition's own depth (not a hardcoded 0) - harmless in the
            // Collection scene where nothing else competes for the mouse raycast nearby, but in
            // the Game scene board minions sit at a more-negative (closer-to-camera) Z, so their
            // colliders would win over a repr icon stuck at Z=0, silently eating its hover events.
            cardReprObj.gameObject.transform.position = new Vector3(cardReprPosition.transform.position.x, cardReprPosition.transform.position.y - shift * index, cardReprPosition.transform.position.z);

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
        if (boardManager != null)
        {
            boardManager.SetAllMinionCollidersEnabled(true);
        }
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
            CursorController.cursorState = previousCursorState;
        }
    }
}