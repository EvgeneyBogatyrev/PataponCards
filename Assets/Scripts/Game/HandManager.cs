using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using Networking;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;

    public GameObject mulliganButtonObject;
    public GameObject keepHandButtonObject;

    // Optional - shown only during mulligan, telling the player whether they go first or second
    // this match. Wire the parent object (e.g. "TurnOrder", a sibling of Mulligan/KeepHand/Concede
    // under MulliganButtons) here, not its Text child directly - matches GameController.turnTimeLeft's
    // pattern of a container whose world-space TextMeshPro children get the text set on them, so an
    // outline/shadow duplicate child (if one gets added later) is picked up for free. Left unwired,
    // it's simply never shown - no editor change forced on existing Game scene setups. Turn order
    // is already fully determined by InfoSaver.myHash/opponentHash at this point (set back in
    // Lobby, well before this scene loads) even though GameController.StartGame() - which is what
    // actually flips GameController.playerTurn - only runs once mulligan ends, so this uses the
    // exact same comparison ahead of time.
    public GameObject turnOrderObject;

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
    public bool cardsAreDrawing = false;

    public static float cardDestroyTimer = 4.5f;

    public static bool mulliganing = true;

    private static void SetActiveIfPresent(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    void Start()
    {
        mulliganing = true;
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        drawStartPosition = GameObject.Find("DrawFromDeck").transform.position;

        if (InfoSaver.isSpectator)
        {
            // A spectator has no deck/mulligan of their own - the board's starting state
            // (both Hatapons placed) is set up identically and purely locally by both real
            // clients before their first network action ever gets posted, so this client must
            // replicate that same baseline before ServerDataProcesser's catch-up starts
            // replaying the real match's actions from index 0. hand/opponentHand then both
            // start at the same 7-card baseline the real clients had right after their own
            // mulligan ended - subsequent replay (NumberOfCards/PlayCard/etc. messages)
            // corrects counts as it catches up to the live match state. The friendly `hand`
            // list itself is then handed off entirely to PollSpectatedHand() - it's the only
            // source of truth for what's actually in the spectated friend's hand, since we
            // have no deck/RNG of our own to derive it from.
            mulliganing = false;
            SetActiveIfPresent(turnOrderObject, false);
            SetActiveIfPresent(mulliganButtonObject, false);
            SetActiveIfPresent(keepHandButtonObject, false);
            // Skipping DeckManager.CopyDeck() below means playDeck is never assigned for a
            // spectator - left null (or worse, stale from whatever scene was visited earlier
            // this session), GameController.UpdateDecks()/GetPlayDeckSize() NullReferenceException
            // the moment a round transition runs IenumStartTurn(), which permanently kills that
            // coroutine mid-setup (playerTurn/SetCanPlayCard/gameState.Increment never run),
            // desyncing all subsequent turn tracking even though the hand poll and board replay
            // keep working fine on their own. A spectator never actually draws from playDeck
            // locally (hand comes entirely from PollSpectatedHand), so its contents don't matter
            // - it only needs to exist.
            DeckManager.deck = DeckManager.deck ?? new List<CardTypes>();
            DeckManager.playDeck = new List<CardTypes>();
            // Same call the real local player's own KeepHandButton() makes at the same point in
            // the flow - needed for far more than turn-order bookkeeping: it's what starts
            // ServerDataProcesser.ObtainData() in the first place, without which nothing here
            // would ever receive a single message. Deliberately NOT SendDeck/SendCardNumber -
            // those would POST fake actions into the real match's action log under the
            // spectated friend's hash, corrupting it for the two actual players.
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.StartGame();
            PlayHatapons();
            SetNumberOfOpponentsCards(7);
            StartCoroutine(PollSpectatedHand());
            return;
        }

        DeckManager.CopyDeck();

        if (turnOrderObject != null)
        {
            bool goesFirst = InfoSaver.opponentHash <= InfoSaver.myHash;
            string turnOrderMessage = goesFirst ? "You go first" : "You go second";
            foreach (Transform child in turnOrderObject.transform)
            {
                child.GetComponent<TextMeshPro>().text = turnOrderMessage;
            }
            turnOrderObject.SetActive(true);
        }

        for (int i = 0; i < 7; ++i)
        {
            DrawCard();
        }

        SetNumberOfOpponentsCards(7);

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
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
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
                gameController.FireCardDiscardedTrigger(card.GetCardType(), cardFriendly: true);
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
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            CardManager[] cards = new CardManager[hand.Count];
            hand.CopyTo(cards);
            int index = -1;
            int removed = 0;
            foreach (CardManager card in cards)
            {
                index += 1;
                gameController.FireCardDiscardedTrigger(card.GetCardType(), cardFriendly: true);
                hand[index - removed].DestroyCard();
                RemoveCard(index - removed);
                removed += 1;
            }
            UpdateHandPosition();
        }
        else
        {
            // Opponent's hand cards are hidden information (face-down Fang placeholders, real
            // type never known client-side) - no real CardTypes to report, so this trigger
            // deliberately never fires for an opponent-side discard.
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
        if (GameController.eventQueue.Count > 0)
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
        if (GameController.eventQueue.Count > 0)
        {
            return;
        }
        ServerDataProcesser.instance.SendDeck(DeckManager.GetEncodedDeck());
        HandManager.mulliganing = false;
        UpdateHandPosition();
        keepHandButtonObject.SetActive(false);
        mulliganButtonObject.SetActive(false);
        if (turnOrderObject != null)
        {
            turnOrderObject.SetActive(false);
        }

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
        QueueData newEvent = new();
        newEvent.actionType = QueueData.ActionType.DrawCard;
        newEvent.ephemeral = ephemeral;
        GameController.eventQueue.Insert(0, newEvent); 
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
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.actionIsHappening = true;
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
            PublishHandToSpectators();

            // "Card drawn" broadcast trigger - friendly draws only. The opponent's real drawn
            // CardTypes is hidden information never known client-side (their hand is face-down
            // Fang placeholders - see DrawCardOpponent), so this never fires for one.
            gameController.FireCardDrawnTrigger(card, cardFriendly: true);

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
        gameController.actionIsHappening = false;
        yield return null;
    }

    // Publishes this player's current hand so a friend watching via Spectate mode can see it
    // live - a spectator has no way to derive hand contents locally (they come from this
    // client's own RNG deck shuffle/draws, never otherwise transmitted over the network). Only
    // meaningful for a real online match; a no-op for bot matches (not spectate-able) and for a
    // spectator's own client (which never mutates a real hand of its own to publish).
    private void PublishHandToSpectators()
    {
        if (!InfoSaver.onlineBattle || InfoSaver.isSpectator)
        {
            return;
        }
        List<int> cardTypes = new List<int>();
        foreach (CardManager card in hand)
        {
            cardTypes.Add((int)card.GetCardType());
        }
        CoroutineRunner.Run(FirebaseDb.Put("matches/" + ServerDataProcesser.MatchId() + "/hands/" + InfoSaver.myHash, new JArray(cardTypes)));
    }

    // Polls the spectated friend's live-published hand (see PublishHandToSpectators, written
    // by their own real client) and keeps `hand` in sync - this is the ONLY source of truth
    // for a spectator's friendly-side hand, since its real contents can never be derived
    // locally (no deck/RNG of our own to replay it from). Reuses ServerDataProcesser's own
    // polling cadence so a card doesn't visibly lag far behind the board action that caused it.
    private IEnumerator PollSpectatedHand()
    {
        while (true)
        {
            JToken snapshot = null;
            yield return FirebaseDb.Get("matches/" + ServerDataProcesser.MatchId() + "/hands/" + InfoSaver.myHash, token => snapshot = token);

            List<CardTypes> published = new List<CardTypes>();
            if (snapshot is JArray array)
            {
                foreach (JToken entry in array)
                {
                    published.Add((CardTypes)entry.Value<int>());
                }
            }
            ReconcileHand(published);

            yield return new WaitForSeconds(ServerDataProcesser.instance.secondsBetweenServerUpdates);
        }
    }

    // Rebuilds `hand` to match the freshly-polled published list whenever it actually differs.
    // A card that ConsumeSpectatedHandCard already pulled out for a play/cycle animation
    // moments earlier is normally already gone from `hand` locally by the time the next poll
    // lands, so this is mostly a safety net (catching genuine new draws/mulligan swaps) rather
    // than the primary removal path - that's why it's a full rebuild-on-mismatch rather than a
    // diff/patch, simplicity over minimizing churn since mismatches should be rare.
    private void ReconcileHand(List<CardTypes> published)
    {
        bool changed = published.Count != hand.Count;
        if (!changed)
        {
            for (int i = 0; i < hand.Count; ++i)
            {
                if (hand[i].GetCardType() != published[i])
                {
                    changed = true;
                    break;
                }
            }
        }
        if (!changed)
        {
            return;
        }

        foreach (CardManager card in hand)
        {
            card.DestroyCard();
            Destroy(card.gameObject);
        }
        hand = new List<CardManager>();
        foreach (CardTypes type in published)
        {
            CardManager newCard = GenerateCard(type, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            newCard.SetCardState(CardManager.CardState.inHand);
            hand.Add(newCard);
        }
        UpdateHandPosition();
    }

    // Spectator-mode helper: removes and returns one real card of the given type from the
    // spectated friend's visible hand, mirroring what SetNumberOfOpponentsCards(returnCard:
    // true) does for the hidden opponent hand - lets ServerDataProcesser visually reuse/morph
    // the actual hand card into the one flying onto the board, same as the opponent side
    // already does with its revealed placeholder. Returns null on the rare timing race where
    // the next poll hasn't landed yet - callers already handle a null origin by spawning a
    // fresh card of the same type instead, so the visual result is identical either way.
    public CardManager ConsumeSpectatedHandCard(CardTypes type)
    {
        for (int i = 0; i < hand.Count; ++i)
        {
            if (hand[i].GetCardType() == type)
            {
                CardManager card = hand[i];
                hand.RemoveAt(i);
                UpdateHandPosition();
                return card;
            }
        }
        return null;
    }

    public GameObject GenerateCard(CardTypes cardType, CardManager origin=null)
    {
        return GenerateCard(cardType, new Vector3(-10f, -10f, 0f), origin);
        /*
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
        */
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
        PublishHandToSpectators();
    }
    public bool CanPlayCard()
    {
        // A spectator's canPlayCard still gets flipped true/false by turn-order logic in
        // GameController.StartGame()/StartTurn()/EndTurn() same as a real player's would (it
        // has to, so those methods don't need spectator-specific branches of their own) - this
        // is the one place that actually needs to veto it, since CardManager's hand-hover check
        // treats CursorStates.EnemyTurn as an allowed state alongside Free (see CursorController
        // for why locking cursorState alone isn't enough to fully block this path).
        return canPlayCard && !InfoSaver.isSpectator;
    }

     public bool CanCycleCard()
    {
        return canCycleCard && !InfoSaver.isSpectator;
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
