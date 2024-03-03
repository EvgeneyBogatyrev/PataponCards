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
    private bool canCycleCard = false;
    private BoardManager boardManager;

    private int hataponHealth = 20;
    private int hataponHealthDecrease = 0;
    private Vector3 drawStartPosition;

    private bool CardIsDrawing = false;

    public static float cardDestroyTimer = 4.5f;

    public static bool mulliganing = true;

    void Start()
    {
        mulliganing = true;
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        drawStartPosition = GameObject.Find("DrawFromDeck").transform.position;
        DeckManager.deck = SaveSystem.LoadDeck();
        DeckManager.runes = SaveSystem.LoadRunes();
        DeckManager.CopyDeck();

        for (int i = 0; i < 7; ++i)
        {
            DrawCard();
        }

        SetNumberOfOpponentsCards(0);
        
    }

    public CardManager SetNumberOfOpponentsCards(int number, bool returnCard=false, int ephemeral=-1)
    {
        CardManager returnedCard = null;
        CardManager[] copyHand = new CardManager[opponentHand.Count];
        opponentHand.CopyTo(copyHand);
        for (int i = 0; i < copyHand.Length; ++i)
        {
            if (i >= number)
            {
                opponentHand.Remove(copyHand[i]);
            }
            if (i == opponentHand.Count - 1 && returnCard)
            {
                returnCard = copyHand[i];
                break;
            }
            if (i >= number) 
            {
                copyHand[i].DestroyCard();
                Destroy(copyHand[i].gameObject);
            }
        }

        //opponentHand = new List<CardManager>();
        for (int i = opponentHand.Count; i < number; ++i) 
        {
            CardManager newCard = GenerateCard(CardTypes.Fang, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            opponentHand.Add(newCard);
            newCard.SetCardState(CardManager.CardState.opponentHolding);
            newCard.GetCardStats().ephemeral = ephemeral;
        }

        UpdateHandPositionOpponent();
        return returnedCard;
    }

    public int GetNumberOfCards()
    {
        return hand.Count;
    }

    public int GetNumberOfOpponentsCards()
    {
        return opponentHand.Count;
    }

    public void CheckEphemeral()
    {
        CardManager[] cards = new CardManager[hand.Count];
        hand.CopyTo(cards);
        int index = -1;
        int removed = 0;
        foreach (CardManager card in cards)
        {
            index += 1;
            int eph = card.GetCardStats().ephemeral;
            if (eph == -1)
            {
                continue;
            }
            if (card.GetCardStats().ephemeral == 0)
            {
                hand[index - removed].DestroyCard();
                RemoveCard(index - removed);
                removed += 1;
            }
            card.GetCardStats().ephemeral = eph - 1;
            
        }
        UpdateHandPosition();
        ServerDataProcesser.instance.Discard(removed);
    }

    public void DiscardHand(bool friendly=true)
    {
        if (friendly)
        {
            CardManager[] cards = new CardManager[hand.Count];
            hand.CopyTo(cards);
            int index = -1;
            int removed = 0;
            foreach (CardManager card in cards)
            {
                index += 1;
                hand[index - removed].DestroyCard();
                RemoveCard(index - removed);
                removed += 1;            
            }
            UpdateHandPosition();
        }
        else
        {
            SetNumberOfOpponentsCards(0);
        }
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

    public void MillCard()
    {
        CardTypes card = DeckManager.GetTopCard(remove:true);
        CardManager newCard = GenerateCard(card, drawStartPosition).GetComponent<CardManager>();
        newCard.SetCardState(CardManager.CardState.toMill);
    }

    public void MillCardOpp()
    {
        CardTypes card = DeckManager.GetTopCardOpp(remove:true);
        CardManager newCard = GenerateCard(card, drawStartPosition).GetComponent<CardManager>();
        newCard.SetCardState(CardManager.CardState.toMill);
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
            //DeckManager.PutCardBack(card.GetCardType());
            Destroy(card.gameObject);
        }

        DeckManager.CopyDeck();

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
        ServerDataProcesser.instance.SendDeck(DeckManager.GetEncodedDeck());
        HandManager.mulliganing = false;
        UpdateHandPosition();
        keepHandButtonObject.SetActive(false);
        mulliganButtonObject.SetActive(false);

        // Play Hatapons
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.StartGame();
        PlayHatapons();
        ServerDataProcesser.instance.SendCardNumber(hand.Count);
    }

    public void PlayHatapons()
    {
        BoardManager.Slot slot1 = boardManager.friendlySlots[3];
        CardManager card1 = GenerateCard(CardTypes.Hatapon, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
        card1.SetPower(hataponHealth);
        boardManager.PlayCard(card1, new Vector3(0f, 0f, 0f), slot1, record: false);

        BoardManager.Slot slot2 = boardManager.enemySlots[3];
        CardManager card2 = GenerateCard(CardTypes.Hatapon, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
        card2.SetPower(hataponHealth);
        boardManager.PlayCard(card2, new Vector3(0f, 0f, 0f), slot2, record: false);

        hataponHealth -= hataponHealthDecrease;
    }

    public void DrawCard(int ephemeral=-1)
    {
        if (hand.Count >= 7)
        {
            return;
        }
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        CardTypes cardType = DeckManager.GetTopCard(remove:true);
        if (cardType != CardTypes.Hatapon)
        {
            AddCardToHand(cardType, ephemeral:ephemeral);
        }
        gameController.ProcessCardDraw(friendly:true);
    }

    public void DrawCardOpponent(bool fromDeck=true, int ephemeral=-1)
    {
        if (GetNumberOfOpponentsCards() >= 7)
        {
            return;
        }
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (fromDeck)
        {
            if (gameController.ProcessCardDraw(friendly:false))
            {
                SetNumberOfOpponentsCards(GetNumberOfOpponentsCards() + 1, ephemeral:ephemeral);
            }
        }
        else
        {
            SetNumberOfOpponentsCards(GetNumberOfOpponentsCards() + 1, ephemeral:ephemeral);
        }
    }

    public void AddCardToHand(CardTypes card, int ephemeral=-1)
    {
        StartCoroutine(AddCardToHandAsync(card, ephemeral:ephemeral));
    }

    public IEnumerator AddCardToHandAsync(CardTypes card, int ephemeral=-1)
    {
        while (CardIsDrawing)
        {
            yield return new WaitForSeconds(0.1f);
        }
        CardIsDrawing = true;
        if (hand.Count < 7)
        {
            CardManager newCard = GenerateCard(card, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            hand.Add(newCard);
            UpdateHandPosition();

            newCard.transform.position = drawStartPosition;
            newCard.SetCardState(CardManager.CardState.Drawing);
            newCard.GetCardStats().ephemeral = ephemeral;

            if (!HandManager.mulliganing)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
        CardIsDrawing = false;

        yield return null;
    }

    public GameObject GenerateCard(CardTypes cardType, CardManager origin=null)
    {

        GameObject newCard;
        if (origin == null) 
        {
            newCard = Instantiate(cardPrefab, new Vector3(-10f, -10f, 0f), Quaternion.identity);
        }
        else
        {
            newCard = origin.gameObject;
        }
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        return newCard;
    }

    public GameObject GenerateCard(CardTypes cardType, Vector3 positionFrom, CardManager origin=null)
    {

        GameObject newCard;
        if (origin == null) 
        {
            newCard = Instantiate(cardPrefab, positionFrom, Quaternion.identity);
        }
        else
        {
            newCard = origin.gameObject;
        }
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        return newCard;
    }

    private void UpdateHandPosition()
    {
        if (!mulliganing)
        {
            Vector3 center = new Vector3(-1.2f, -3.2f, -0.75f);
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
        else
        {
            float _cardSpace = 26f / (hand.Count + 1);
            Vector3 center = new Vector3(0f, 1f, -0.75f);
            int numberOfCards = hand.Count;
            float startPoint = center.x - ((numberOfCards - 1) * _cardSpace / 2f);
            //float startRot = 5f * ((float)(numberOfCards - 1) / 2);
            for (int i = 0; i < numberOfCards; ++i)
            {
                hand[i].SetPositionInHand(new Vector3(startPoint + _cardSpace * i, center.y, center.z - (float)i / 5f));
                //hand[i].SetRotation(startRot);
                hand[i].SetIndexInHand(i);
                //startRot -= 5f;
            }
        }
    }

    private void UpdateHandPositionOpponent()
    {
        Vector3 center = new Vector3(-1.2f, 7.7f, -0.75f);
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

     public bool CanCycleCard()
    {
        return canCycleCard;
    }

    public void SetCanPlayCard(bool _can)
    {
        canPlayCard = _can;
    }

    public void SetCanCycleCard(bool _can)
    {
        canCycleCard = _can;
    }

    public void StartRoundActions(int cardIncrement = 3)
    {
        int numberOfDraws = Mathf.Min(cardIncrement, 7 - hand.Count);
        for (int i = 0; i < numberOfDraws; ++i)
        {
            DrawCard();
        }
        int numberOfDrawsOpp = Mathf.Min(cardIncrement, 7 - opponentHand.Count);
        for (int i = 0; i < numberOfDrawsOpp; ++i)
        {
            DrawCardOpponent();
        }

        PlayHatapons();
    }
}
