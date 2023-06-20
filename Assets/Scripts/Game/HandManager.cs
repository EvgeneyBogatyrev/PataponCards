using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;

    public GameObject mulliganButtonObject;
    public GameObject keepHandButtonObject;

    public float cardSpace;
    private int cardMulligan = 7;
    private int cardMulliganMin = 1;

    private List<CardManager> hand = new List<CardManager>();
    private List<CardManager> opponentHand = new List<CardManager>();
    private bool canPlayCard = false;
    private BoardManager boardManager;

    private int hataponHealth = 20;
    private int hataponHealthDecrease = 5;
    private Vector3 drawStartPosition;

    private bool CardIsDrawing = false;

    void Start()
    {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        drawStartPosition = GameObject.Find("DrawFromDeck").transform.position;
        DeckManager.CopyDeck();

        for (int i = 0; i < 7; ++i)
        {
            DrawCard();
        }

        SetNumberOfOpponentsCards(0);
        
    }

    public void SetNumberOfOpponentsCards(int number)
    {
        // TODO: Check if deck is empty
        foreach (CardManager card in opponentHand)
        {
            card.DestroyCard();
            Destroy(card.gameObject);
        }

        opponentHand = new List<CardManager>();
        for (int i = 0; i < number; ++i) 
        {
            CardManager newCard = GenerateCard(CardTypes.Hatapon).GetComponent<CardManager>();
            opponentHand.Add(newCard);
            newCard.SetCardState(CardManager.CardState.opponentHolding);
        }

        UpdateHandPositionOpponent();
    }

    public int GetNumberOfOpponentsCards()
    {
        return opponentHand.Count;
    }

    public static void DestroyDisplayedCards()
    {
        CardManager[] cards = (CardManager[]) GameObject.FindObjectsOfType (typeof(CardManager));

        foreach (CardManager card in cards)
        {
            if (card.GetCardState() == CardManager.CardState.opponentPlayed)
            {
                card.DestroyCard();
            }
        }
    }


    public void Mulligan()
    {
        if (CardIsDrawing)
        {
            return;
        }
        cardMulligan -= 1;
        
        foreach (CardManager card in hand)
        {
            DeckManager.PutCardBack(card.GetCardType());
            Destroy(card.gameObject);
        }

        hand = new List<CardManager>();

        for (int i = 0; i < cardMulligan; ++i)
        {
            DrawCard();
        }

        if (cardMulligan <= cardMulliganMin)
        {
            KeepHandButton();
        }
    }

    public void KeepHandButton()
    {
        if (CardIsDrawing)
        {
            return;
        }
        keepHandButtonObject.SetActive(false);
        mulliganButtonObject.SetActive(false);

        // Play Hatapons
        PlayHatapons();
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.StartGame();
        ServerDataProcesser.instance.SendCardNumber(hand.Count);
    }

    public void PlayHatapons()
    {
        BoardManager.Slot slot1 = boardManager.friendlySlots[3];
        CardManager card1 = GenerateCard(CardTypes.Hatapon).GetComponent<CardManager>();
        card1.SetPower(hataponHealth);
        boardManager.PlayCard(card1, slot1, record: false);

        BoardManager.Slot slot2 = boardManager.enemySlots[3];
        CardManager card2 = GenerateCard(CardTypes.Hatapon).GetComponent<CardManager>();
        card2.SetPower(hataponHealth);
        boardManager.PlayCard(card2, slot2, record: false);

        hataponHealth -= hataponHealthDecrease;
    }

    public void DrawCard()
    {
        if (hand.Count >= 7)
        {
            return;
        }
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        CardTypes cardType = DeckManager.GetRandomCard(remove:true);
        if (cardType != CardTypes.Hatapon)
        {
            AddCardToHand(cardType);
        }
        gameController.ProcessCardDraw(friendly:true);
    }

    public void DrawCardOpponent()
    {
        if (GetNumberOfOpponentsCards() >= 7)
        {
            return;
        }
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameController.ProcessCardDraw(friendly:false))
        {
            SetNumberOfOpponentsCards(GetNumberOfOpponentsCards() + 1);
        }
    }

    public void AddCardToHand(CardTypes card)
    {
        StartCoroutine(AddCardToHandAsync(card));
    }

    public IEnumerator AddCardToHandAsync(CardTypes card)
    {
        while (CardIsDrawing)
        {
            yield return new WaitForSeconds(0.1f);
        }
        CardIsDrawing = true;
        if (hand.Count < 7)
        {
            CardManager newCard = GenerateCard(card).GetComponent<CardManager>();
            hand.Add(newCard);
            UpdateHandPosition();

            newCard.transform.position = drawStartPosition;
            newCard.SetCardState(CardManager.CardState.Drawing);

            yield return new WaitForSeconds(1f);
        }
        CardIsDrawing = false;

        yield return null;
    }

    public GameObject GenerateCard(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab);
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        return newCard;
    }

    private void UpdateHandPosition()
    {
        Vector3 center = new Vector3(0f, -3.2f, -0.75f);
        int numberOfCards = hand.Count;
        float startPoint = center.x - ((numberOfCards - 1) * cardSpace / 2f);
        float startRot = 5f * ((float)(numberOfCards - 1) / 2);
        for (int i = 0; i < numberOfCards; ++i)
        {
            hand[i].SetPositionInHand(new Vector3(startPoint + cardSpace * i, center.y, center.z - (float)i / 5f));
            hand[i].SetRotation(startRot);
            hand[i].SetIndexInHand(i);
            startRot -= 5f;
        }
    }

    private void UpdateHandPositionOpponent()
    {
        Vector3 center = new Vector3(0f, 7.7f, -0.75f);
        int numberOfCards = opponentHand.Count;
        float startPoint = center.x - ((numberOfCards - 1) * cardSpace / 2f);
        float startRot = 5f * ((float)(numberOfCards - 1) / 2);
        for (int i = 0; i < opponentHand.Count; ++i)
        {
            opponentHand[i].SetPositionInHand(new Vector3(startPoint + cardSpace * i, center.y, center.z - (float)i / 5f));
            opponentHand[i].SetRotation(startRot);
            opponentHand[i].SetIndexInHand(i);
            startRot -= 5f;
        }
    }


    public void RemoveCard(int index)
    {
        hand.RemoveAt(index);
        UpdateHandPosition();
    }
    public bool CanPlayCard()
    {
        return canPlayCard;
    }
    public void SetCanPlayCard(bool _can)
    {
        canPlayCard = _can;
    }

    public void StartRoundActions(int cardIncrement = 3)
    {
        for (int i = 0; i < cardIncrement; ++i)
        {
            DrawCard();
            DrawCardOpponent();
        }

        PlayHatapons();
    }
}
