using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using Networking;


public class ServerDataProcesser : MonoBehaviour
{
    public static ServerDataProcesser instance;

    private BoardManager boardManager;

    public int messageId = 0;
    public List<MessageFromServer> doneActions = new List<MessageFromServer>();
    public List<MessageFromServer> messagesFromServer;
    public float secondsBetweenServerUpdates = 1.5f;
    public Bot bot = null;

    // Spectator-only: true once a full fetch has produced zero newly-processed messages AND
    // HasUnresolvedGap is false, i.e. we've genuinely reached the live tail of the match, not
    // just paused mid-pair waiting for a CastSpell's companion PlayCard. While false, ObtainData
    // skips its own pacing delay and ProcessMessages skips its per-message delay, so a
    // mid-match join fetches/replays the whole action history back-to-back instead of once
    // every ~1.5s with a 2s gap per action.
    private bool spectatorCatchUpDone = false;
    
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            instance = this; 
        } 
    }

    private void Start()
    {
        bot = new Bot();
        bot.botLevel = InfoSaver.botLevel;
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        if (InfoSaver.onlineBattle)
        {
            StartCoroutine(RegisterInMatch());
            StartCoroutine(FetchOpponentNickname());
            if (InfoSaver.isSpectator)
            {
                StartCoroutine(FetchFriendlyNickname());
            }
        }
    }

    // "Opponent" until FetchOpponentNickname resolves - shown as a placeholder in the meantime.
    public string OpponentNickname = "Opponent";

    private IEnumerator FetchOpponentNickname()
    {
        while (true)
        {
            JToken token = null;
            yield return FirebaseDb.Get("matches/" + MatchId() + "/nicknames/" + InfoSaver.opponentHash, t => token = t);
            if (token != null && token.Type != JTokenType.Null)
            {
                OpponentNickname = token.ToString();
                yield break;
            }
            yield return new WaitForSeconds(2f);
        }
    }

    // Spectator-only: the spectated friend's own nickname. A real participant never needs this
    // (their own nickname is already known locally via PlayerProfile.Nickname), but a spectator
    // has no local identity of their own on the "friendly" side - InfoSaver.myHash means the
    // spectated friend, not the viewer - so it has to be fetched the same way OpponentNickname
    // already is, just keyed by myHash instead of opponentHash.
    public string FriendlyNickname = "Player";

    private IEnumerator FetchFriendlyNickname()
    {
        while (true)
        {
            JToken token = null;
            yield return FirebaseDb.Get("matches/" + MatchId() + "/nicknames/" + InfoSaver.myHash, t => token = t);
            if (token != null && token.Type != JTokenType.Null)
            {
                FriendlyNickname = token.ToString();
                yield break;
            }
            yield return new WaitForSeconds(2f);
        }
    }

    // Matches are keyed by the two players' hashes so both sides land on the same path
    // without needing a server-assigned id.
    public static string MatchId()
    {
        int a = Mathf.Min(InfoSaver.myHash, InfoSaver.opponentHash);
        int b = Mathf.Max(InfoSaver.myHash, InfoSaver.opponentHash);
        return a + "_" + b;
    }

    private bool matchRegistered = false;

    // True whenever we can see a later opponent action but are still missing an earlier one -
    // used by GameController to show a "waiting for opponent" banner and to pause the
    // AFK-victory timer instead of misprocessing out-of-order actions.
    public bool HasUnresolvedGap { get; private set; }

    // Registers our uid under the match so security rules can scope read/write access to it.
    // Post() waits on matchRegistered so no action can race ahead of this write.
    private IEnumerator RegisterInMatch()
    {
        yield return FirebaseDb.EnsureSignedIn();
        yield return FirebaseDb.Put("matches/" + MatchId() + "/players/" + FirebaseConfig.Uid, true);
        // A spectator must never publish into a real match's nickname slot - that key is
        // InfoSaver.myHash, which for a spectator means the SPECTATED FRIEND's hash, not their
        // own. Writing here would silently replace the real player's displayed nickname (shown
        // to their actual opponent) with the spectator's own.
        if (!InfoSaver.isSpectator)
        {
            // Keyed by matchmaking hash (not uid) so the opponent can look it up without
            // needing to resolve our Firebase identity - they already know our hash from
            // matchmaking.
            yield return FirebaseDb.Put("matches/" + MatchId() + "/nicknames/" + InfoSaver.myHash, PlayerProfile.Nickname);
        }
        matchRegistered = true;
    }

    public void PlayCard(CardManager card, BoardManager.Slot slot)
    {
        int cardIndex = (int)card.GetCardType();
        int slotIndex;
        if (slot.GetFriendly())
        {
            slotIndex = slot.GetIndex() + 1;
        }
        else
        {
            slotIndex = slot.GetIndex() + 1;
            slotIndex *= -1;
        }
        StartCoroutine(Post("1", "play card", cardIndex.ToString(), slotIndex.ToString()));
    }

    public void CycleCard(CardManager card)
    {
        LogController.instance.AddCycleToLog(card.GetCardType(), true);
        int cardIndex = (int)card.GetCardType();
        StartCoroutine(Post("1", "cycle card", cardIndex.ToString(), ""));
    }

    public void SendDeck(List<int> encodedDeck)
    {
        string targetString = "";
        List<Runes> runes = DeckManager.runes;
        bool start = true;
        foreach (Runes rune in runes)
        {
            if (start)
            {
                start = false;
            }
            else
            {
                targetString += ",";
            }
            targetString += ((int) rune).ToString();
        }
        for (int i = 0; i < encodedDeck.Count; ++i)
        {
            targetString += ",";
            targetString += encodedDeck[i].ToString();
        }
        StartCoroutine(Post("1", "send deck", "", targetString));
    }

    public void Discard(int number)
    {
        if (number > 0)
        {
            StartCoroutine(Post("1", "discard", number.ToString(), ""));
        }
    }

    public void CastSpell(CardManager card, List<int> targets)
    {
        LogController.instance.AddPlayCardToLog(card.GetCardType(), targets, true);
        string targetString = "";
        for (int i = 0; i < targets.Count; ++i)
        {
            if (i != 0)
            {
                targetString += ",";
            }
            targetString += targets[i].ToString();
        }
        int cardIndex = (int)card.GetCardType();

        StartCoroutine(Post("1", "cast spell", cardIndex.ToString(), targetString));
    }

    public void Attack(int from, int to)
    {
        LogController.instance.AddAttackToLog(from, to, true);
        StartCoroutine(Post("1", "attack", "", from.ToString() + "," + to.ToString()));
    }

    public void Move(int from, int to)
    {
        StartCoroutine(Post("1", "move", "", from.ToString() + "," + to.ToString()));
    }

    public void Exchange(int from, int to)
    {
        StartCoroutine(Post("1", "exchange", "", from.ToString() + "," + to.ToString()));
    }

    public void EndTurn()
    {
        StartCoroutine(Post("1", "end turn", "", ""));
    }

    public void SendCardNumber(int number)
    {
        StartCoroutine(Post("1", "number of cards", "", number.ToString()));
    }

    public void Ping()
    {
        StartCoroutine(Post("1", "ping", UnityEngine.Random.Range(0, 99999).ToString(), ""));
    }

    // Conceding on the opponent's turn ends the whole match immediately (not just the round) -
    // there's no "next round" left afterward, so there's nothing for an in-flight opponent
    // action to race against.
    public void ConcedeMatch()
    {
        StartCoroutine(Post("1", "concede match", "", ""));
    }

    IEnumerator Post(string key, string action, string cardIdx, string targets)
    {
        if (!InfoSaver.onlineBattle)
        {
            yield break;
        }

        if (GameObject.Find("GameController").GetComponent<GameController>().desyncDetected)
        {
            yield break;
        }

        while (!matchRegistered)
        {
            yield return null;
        }

        JObject payload = new JObject
        {
            ["hash"] = InfoSaver.myHash,
            ["index"] = messageId,
            ["action"] = action,
            ["cardIdx"] = cardIdx,
            ["targets"] = targets
        };
        messageId += 1;

        yield return FirebaseDb.Post("matches/" + MatchId() + "/actions", payload);
    }

    public IEnumerator ProcessMessages(List<MessageFromServer> messages, bool noDelay = false)
    {
        messages = messages.OrderBy(x => x.index).ToList();
        List<MessageFromServer> processedMessages = new List<MessageFromServer>();
        for (int messageIndex = 0; messageIndex < messages.Count(); ++messageIndex)
        {
            if (messages[messageIndex].action != MessageFromServer.Action.CastSpell && messages[messageIndex].action != MessageFromServer.Action.PlayCard)
            {
                processedMessages.Add(messages[messageIndex]);
            }
            else
            {
                CardTypes type = messages[messageIndex].cardIndex;
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                CardManager _newCard = handManager.GenerateCard(type, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();

                if (!_newCard.GetCardStats().hasOnPlaySpell)
                {
                    processedMessages.Add(messages[messageIndex]);
                }
                else
                {
                    if (messageIndex < messages.Count() - 1 &&
                        messages[messageIndex + 1].cardIndex == messages[messageIndex].cardIndex &&
                        messages[messageIndex + 1].hash == messages[messageIndex].hash &&
                        messages[messageIndex + 1].action == MessageFromServer.Action.PlayCard)
                    {
                        MessageFromServer newMessage = new MessageFromServer
                        {
                            action = MessageFromServer.Action.CastOnPlayCard,
                            cardIndex = messages[messageIndex].cardIndex,
                            hash = messages[messageIndex].hash,
                            index = messages[messageIndex].index,
                            targets = messages[messageIndex].targets,
                            creatureTarget = messages[messageIndex + 1].targets[0],
                            additionalIndex = messages[messageIndex + 1].index
                        };

                        processedMessages.Add(newMessage);
                        // Only skip the next raw message when we actually consumed it as this
                        // card's paired PlayCard - otherwise leave messageIndex alone.
                        messageIndex += 1;
                    }
                    // else: this CastSpell's paired PlayCard hasn't arrived in this batch yet.
                    // Leave it unprocessed (not added to doneActions) so the whole pair gets
                    // retried together on a later poll once the companion shows up - do not
                    // add it standalone and do not skip whatever message follows it.
                }

                _newCard.DestroyCard();
            }
        }

        // Spectator mode routes BOTH real players' actions through this same pipeline (a
        // spectator has no local/instant side of its own), so "the opponent's" card needs to
        // become "whichever side didn't post this particular message" - tracked separately per
        // side (index 0 = friendly/spectated side, 1 = enemy side) so an arrow-cleanup on one
        // side's in-flight card can never stomp the other side's. In normal 2-player play,
        // ObtainData() only ever hands us opponent-hash messages, so index 0 is simply unused.
        CardManager[] lastGeneratedCard = new CardManager[2];
        foreach (MessageFromServer message in processedMessages)
        {
            bool found = false;
            foreach (MessageFromServer doneMessage in doneActions)
            {
                // Must match on hash too, not just index - each real player's own message
                // index counter starts at 0 and increments independently, so in spectator mode
                // (the only time doneActions ever holds more than one sender) the friend's
                // message #5 and the opponent's message #5 are unrelated actions that happen to
                // share a number. Comparing index alone would mark the opponent's #5 as "already
                // processed" the moment the friend's #5 lands, silently dropping it forever.
                if (doneMessage.index == message.index && doneMessage.hash == message.hash)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                continue;
            }

            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            BoardManager.Slot fromSlot, toSlot;

            CardTypes spellType;
            gameController.ReceivePing();

            // Whether THIS message was posted by the spectated friend's client (repurposed as
            // "friendly" - see InfoSaver.isSpectator) rather than their opponent. Always false
            // outside spectator mode, since ObtainData() never hands us our own hash's messages
            // otherwise - so sideSlots/otherSlots/sidePlayedList below reduce to exactly
            // enemySlots/friendlySlots/playedCardsOpponent (the original hardcoded values) for
            // a normal 2-player match, unchanged.
            bool isFriendlySide = InfoSaver.isSpectator && message.hash == InfoSaver.myHash;
            int sideIdx = isFriendlySide ? 0 : 1;
            List<BoardManager.Slot> sideSlots = isFriendlySide ? boardManager.friendlySlots : boardManager.enemySlots;
            List<BoardManager.Slot> otherSlots = isFriendlySide ? boardManager.enemySlots : boardManager.friendlySlots;
            List<CardTypes> sidePlayedList = isFriendlySide ? boardManager.playedCards : boardManager.playedCardsOpponent;
            CardManager newCard = lastGeneratedCard[sideIdx];

            if (message.action == MessageFromServer.Action.CastOnPlayCard)
            {
                // Contains a yield (waits for the board to be clear), which C# does not allow
                // inside a try/catch - the contiguous-ordering gate in ObtainData() is the
                // primary defense against this case ever running against inconsistent state.
                if (newCard != null)
                {
                    if (newCard.arrowList != null)
                    {
                        foreach (Arrow arrow in newCard.arrowList)
                        {
                            arrow.DestroyArrow();
                        }
                        newCard.arrowList = null;
                    }
                }
                spellType = message.cardIndex;
                ConsumeSideHandCard(handManager, isFriendlySide, spellType, reuse: false);
                boardManager.battlecryTrigger = true;
                sidePlayedList.Add(spellType);
                // Log.cs's ResolveTargets indexes straight into board.friendlySlots/enemySlots by
                // the sign of each target - "friendly" here has to mean "resolve this message's
                // targets as belonging to the friendly/spectated side", i.e. isFriendlySide, not
                // hardcoded false. Passing the wrong value here doesn't just mislabel the log
                // entry, it makes ResolveTargets look in the WRONG slot list entirely, which
                // throws (GetConnectedMinion() null -> NullReferenceException on GetCardType())
                // the moment that slot happens to be empty.
                LogController.instance.AddPlayCardToLog(spellType, message.targets, isFriendlySide);
                newCard = handManager.GenerateCard(spellType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                if (newCard.GetCardStats().dummyTarget)
                {
                    newCard.spellTargets = message.targets.GetRange(1, message.targets.Count - 1);
                }
                else
                {
                    newCard.spellTargets = message.targets;
                }
                newCard.spellTargetsFromFriendlySide = isFriendlySide;
                HandManager.DestroyDisplayedCards();
                newCard.SetCardState(CardManager.CardState.opponentPlayed);
                newCard.transform.position = new Vector3(0f, 10f, 0f);
                newCard.destroyTimer = HandManager.cardDestroyTimer;

                QueueData _newEvent = new();
                _newEvent.actionType = QueueData.ActionType.CastSpell;
                _newEvent.thisStats = newCard.GetCardStats();
                _newEvent.hostCard = newCard;
                _newEvent.targets = message.targets;
                _newEvent.friendlySlots = sideSlots;
                _newEvent.enemySlots = otherSlots;
                GameController.eventQueue.Insert(0, _newEvent);
                // Cast spell
                //StartCoroutine(newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots));

                if (message.creatureTarget > 0)
                {
                    message.creatureTarget -= 1;
                    fromSlot = sideSlots[message.creatureTarget];
                }
                else
                {
                    message.creatureTarget = (-1 * message.creatureTarget) - 1;
                    fromSlot = otherSlots[message.creatureTarget];
                }

                CardManager playedCard = handManager.GenerateCard(spellType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                playedCard.transform.position = fromSlot.GetPosition();
                playedCard.SetCardState(CardManager.CardState.alreadyPlayed);
                while (!GameController.CanPerformActions())
                {
                    yield return new WaitForSeconds(0.3f);
                }

                boardManager.PlayCard(newCard, fromSlot.GetPosition(), fromSlot, destroy: false, record: false);
                playedCard.DestroyCard();

                boardManager.battlecryTrigger = false;

                doneActions.Add(message);
                lastGeneratedCard[sideIdx] = newCard;
                if (!noDelay)
                {
                    yield return new WaitForSeconds(2f);
                }
                continue;
            }

            try
            {
            switch (message.action)
            {
                case MessageFromServer.Action.EndTurn:
                    gameController.EndTurn(isFriendlySide);
                    //gameController.StartTurn(true);
                    break;
                case MessageFromServer.Action.PlayCard:
                    if (newCard != null)
                    {
                        if (newCard.arrowList != null)
                        {
                            foreach (Arrow arrow in newCard.arrowList)
                            {
                                arrow.DestroyArrow();
                            }
                            newCard.arrowList = null;
                        }
                    }
                    CardTypes type = message.cardIndex;
                    newCard = ConsumeSideHandCard(handManager, isFriendlySide, type, reuse: true);
                    sidePlayedList.Add(type);
                    LogController.instance.AddPlayCardToLog(type, null, isFriendlySide);
                    int target = message.targets[0];
                    BoardManager.Slot targetSlot;
                    if (target > 0)
                    {
                        target -= 1;
                        targetSlot = sideSlots[target];
                    }
                    else
                    {
                        target = (-1 * target) - 1;
                        targetSlot = otherSlots[target];
                    }

                    newCard = handManager.GenerateCard(type, newCard).GetComponent<CardManager>();
                    boardManager.PlayCard(newCard, new Vector3(0f, 10f, 0f), targetSlot, destroy: false, record: false);
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);

                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;
                    break;

                case MessageFromServer.Action.Move:
                    fromSlot = sideSlots[message.targets[0] - 1];

                    if (message.targets[1] > 0)
                    {
                        toSlot = sideSlots[message.targets[1] - 1];
                    }
                    else
                    {
                        toSlot = otherSlots[-message.targets[1] - 1];
                        MinionManager connectedMinion = toSlot.GetConnectedMinion();
                        if (connectedMinion != null)
                        {
                            connectedMinion.DestroyMinion();
                        }
                    }

                    fromSlot.GetConnectedMinion().Move(toSlot);
                    break;

               case MessageFromServer.Action.Exchange:
                    fromSlot = sideSlots[message.targets[0] - 1];
                    toSlot = sideSlots[message.targets[1] - 1];

                    fromSlot.GetConnectedMinion().Exchange(toSlot);
                    break;

                case MessageFromServer.Action.Attack:
                    int from = message.targets[0] * (-1);
                    int to = message.targets[1] * (-1);
                    // AddAttackToLog resolves both ends internally always as if reading from the
                    // ORIGINAL POSTER's own perspective (Log.cs's ResolveTargets is called with a
                    // hardcoded/forced "true" there, unlike AddPlayCardToLog) - that's exactly
                    // what from/to already are for a normal opponent message (the *(-1) above
                    // undoes the poster's own sign convention back into that shape), so it's
                    // never needed a friendly-aware fix before now. For a friendly-side
                    // (spectated) message the physical slot lists are swapped relative to that
                    // fixed assumption - sideSlots below resolves them correctly by hash, but
                    // AddAttackToLog has no such awareness, so undo one more negation here
                    // specifically for the log call to keep it pointed at the right side.
                    int logFrom = isFriendlySide ? -from : from;
                    int logTo = isFriendlySide ? -to : to;
                    LogController.instance.AddAttackToLog(logFrom, logTo, isFriendlySide);
                    MinionManager fromMinion, toMinion;
                    if (from < 0)
                    {
                        fromMinion = sideSlots[from * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        fromMinion = otherSlots[from - 1].GetConnectedMinion();
                    }

                    if (to < 0)
                    {
                        toMinion = sideSlots[to * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        toMinion = otherSlots[to - 1].GetConnectedMinion();
                    }
                    fromMinion.Attack(toMinion);
                    // Not SetCanAttack(false) - that resets attacked/moved as a side effect,
                    // which would silently erase the attacked=true this same Attack() call just
                    // set (e.g. breaking Robopon's "didn't attack this turn" end-of-turn check
                    // on the receiving client only). The local attack flow in MinionManager.cs
                    // uses SetAbilityToAttack(false) for exactly this reason - match it here.
                    fromMinion.SetAbilityToAttack(false);
                    break;
                case MessageFromServer.Action.CastSpell:
                    if (newCard != null)
                    {
                        if (newCard.arrowList != null)
                        {
                            foreach (Arrow arrow in newCard.arrowList)
                            {
                                arrow.DestroyArrow();
                            }
                            newCard.arrowList = null;
                        }
                    }

                    spellType = message.cardIndex;
                    sidePlayedList.Add(spellType);
                    LogController.instance.AddPlayCardToLog(spellType, message.targets, isFriendlySide);
                    newCard = handManager.GenerateCard(spellType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();

                    if (newCard.GetCardStats().damageToHost == -1 && newCard.GetCardType() != CardTypes.Concede)
                    {
                        ConsumeSideHandCard(handManager, isFriendlySide, spellType, reuse: false);
                    }

                    newCard.arrowList = null;
                    newCard.spellTargets = message.targets;
                    newCard.spellTargetsFromFriendlySide = isFriendlySide;
                    // Cast spell
                    QueueData newEvent = new();
                    newEvent.actionType = QueueData.ActionType.CastSpell;
                    newEvent.thisStats = newCard.GetCardStats();
                    newEvent.hostCard = newCard;
                    newEvent.targets = newCard.spellTargets;
                    newEvent.friendlySlots = sideSlots;
                    newEvent.enemySlots = otherSlots;
                    GameController.eventQueue.Insert(0, newEvent);
                    //StartCoroutine(newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots));
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;

                    if (newCard.GetCardStats().damageToHost != -1)
                    {
                        newCard.transform.position = sideSlots[message.targets[0] - 1].GetPosition();
                        newCard.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
                    }
                    break;

                case MessageFromServer.Action.NumberOfCards:
                    // Friendly-side (spectated) hand count is never derived from this message -
                    // HandManager.PollSpectatedHand() is the sole source of truth for it, kept
                    // live via the friend's own client publishing its real hand on every change.
                    if (!isFriendlySide && message.targets[0] != handManager.GetNumberOfOpponentsCards())
                    {
                        handManager.SetNumberOfOpponentsCards(message.targets[0]);
                        /*
                        int difference = message.targets[0] - handManager.GetNumberOfOpponentsCards();
                        if (difference > 0)
                        {
                            for (int i = 0; i < difference; ++i)
                            {
                                handManager.DrawCardOpponent();
                            }
                        }
                        else
                        {
                            handManager.SetNumberOfOpponentsCards(message.targets[0]);
                        }
                        */
                    }
                    break;

                case MessageFromServer.Action.Cycle:
                    if (newCard != null)
                    {
                        if (newCard.arrowList != null)
                        {
                            foreach (Arrow arrow in newCard.arrowList)
                            {
                                arrow.DestroyArrow();
                            }
                            newCard.arrowList = null;
                        }
                    }
                    CardTypes cycleType = message.cardIndex;
                    newCard = ConsumeSideHandCard(handManager, isFriendlySide, cycleType, reuse: true);

                    LogController.instance.AddCycleToLog(cycleType, isFriendlySide);

                    newCard = handManager.GenerateCard(cycleType, newCard).GetComponent<CardManager>();
                    newCard.SetName("Cycling: " + newCard.GetName());
                    newCard.SetNameSize(3);
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;
                    newCard.transform.position = new Vector3(0f, 10f, 0f);

                    if (newCard.GetCardStats().onCycleEvent != null)
                    {
                        QueueData __newEvent = new();
                        __newEvent.actionType = QueueData.ActionType.OnCycle;
                        __newEvent.hostCard = newCard;

                        __newEvent.friendlySlots = sideSlots;
                        __newEvent.enemySlots = otherSlots;

                        GameController.eventQueue.Insert(0, __newEvent);
                        // On cycle
                        //StartCoroutine(newCard.GetCardStats().onCycleEvent(boardManager.friendlySlots, boardManager.enemySlots));
                    }

                    //BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
                    int idx = 0;
                    foreach (BoardManager.Slot slot in sideSlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null)
                        {
                            if (minion.GetCardStats().onCycleOtherEvent != null)
                            {
                                QueueData newEvent_ = new();
                                newEvent_.actionType = QueueData.ActionType.OnCycleOther;
                                newEvent_.hostUnit = minion;
                                newEvent_.index = minion.GetIndex();

                                if (minion.GetFriendly())
                                {
                                    newEvent_.friendlySlots = boardManager.friendlySlots;
                                    newEvent_.enemySlots = boardManager.enemySlots;
                                }
                                else
                                {
                                    newEvent_.friendlySlots = boardManager.enemySlots;
                                    newEvent_.enemySlots = boardManager.friendlySlots;
                                }
                                GameController.eventQueue.Insert(0, newEvent_);
                                /*
                                do {
                                    yield return new WaitForSeconds(0.1f);
                                } while (gameController.actionIsHappening);
                                // On cycle
                                StartCoroutine(minion.GetCardStats().onCycleOtherEvent(idx, boardManager.friendlySlots, boardManager.enemySlots));
                                do {
                                    yield return new WaitForSeconds(0.1f);
                                } while (gameController.actionIsHappening);
                                */
                            }
                        }
                        idx += 1;
                    }

                    if (!isFriendlySide)
                    {
                        handManager.DrawCardOpponent();
                    }
                    break;

                case MessageFromServer.Action.Discard:
                    // Friendly-side (spectated) hand already reflects the discard via
                    // HandManager.PollSpectatedHand() once the friend's own client publishes
                    // its post-discard hand - nothing to do here for that side.
                    if (!isFriendlySide)
                    {
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - (int)message.cardIndex);
                    }
                    break;

                case MessageFromServer.Action.SendDeck:
                    // Spectator mode has no local deck simulation for either real player (a
                    // spectator never runs DeckManager.CopyDeck() - see HandManager.Start()), so
                    // deck-content-dependent effects mid-match (mill, deck reveal, etc.) remain a
                    // known limitation - but each side's SendDeck (fired once, right after their
                    // own mulligan ends) at least gives an accurate starting count/content for
                    // GameController.UpdateDecks() to show, via ReceiveFriendlyDeckForSpectator
                    // for the spectated friend's own side.
                    if (isFriendlySide)
                    {
                        DeckManager.ReceiveFriendlyDeckForSpectator(message.targets);
                    }
                    else
                    {
                        DeckManager.ReceiveOpponentsDeck(message.targets);
                    }
                    break;

                case MessageFromServer.Action.Ping:
                    break;

                case MessageFromServer.Action.ConcedeMatch:
                    // The opponent conceded the whole match (they triggered this on their own
                    // turn's opposite side, i.e. during ours) - we win outright, no more rounds.
                    // For a spectator, EndGame() short-circuits to MainMenu regardless of this
                    // flag (see GameController.EndGame's isSpectator branch), so it's harmless
                    // that "true" doesn't actually mean "the friendly side won" here.
                    gameController.StartCoroutine(gameController.EndGame(true));
                    break;
            }
            }
            catch (Exception ex)
            {
                Debug.LogError("Desync while processing opponent action " + message.action + " (index " + message.index + "): " + ex);
                gameController.HaltForDesync("A move from your opponent could not be applied (" + message.action + "). The match has been halted.");
                yield break;
            }

            doneActions.Add(message);
            lastGeneratedCard[sideIdx] = newCard;
            if (!noDelay)
            {
                yield return new WaitForSeconds(2f);
            }
            /*
            if (message.action == MessageFromServer.Action.PlayCard ||
                    message.action == MessageFromServer.Action.CastSpell ||
                        message.action == MessageFromServer.Action.CastOnPlayCard)
            {
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            */
        }
    }

    // Spectator-mode hand-consumption helper shared by PlayCard/CastSpell/CastOnPlayCard/Cycle.
    // For the enemy side this is exactly the pre-existing SetNumberOfOpponentsCards behavior
    // (decrement the hidden placeholder hand, optionally handing back a card object to
    // reuse/morph into the revealed type). For the friendly (spectated) side, `hand` already
    // holds the real card - HandManager.ConsumeSpectatedHandCard removes it directly; when the
    // caller isn't going to reuse the returned object (reuse:false), it must be explicitly
    // destroyed here since ConsumeSpectatedHandCard only unlists it, unlike
    // SetNumberOfOpponentsCards which destroys internally when returnCard isn't requested.
    private CardManager ConsumeSideHandCard(HandManager handManager, bool isFriendlySide, CardTypes type, bool reuse)
    {
        if (!isFriendlySide)
        {
            return handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1, returnCard: reuse);
        }

        CardManager consumed = handManager.ConsumeSpectatedHandCard(type);
        if (!reuse && consumed != null)
        {
            consumed.DestroyCard();
            Destroy(consumed.gameObject);
            return null;
        }
        return consumed;
    }

    public IEnumerator ObtainData()
    {
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        while (true)
        {
            if (gameController.desyncDetected)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Deliberately does NOT skip fetching just because it's our own turn: the opponent
            // is now allowed to concede on any turn (not just theirs), and skipping fetches
            // during our turn would leave that concede unprocessed until our turn ends - by
            // which point our own further actions and their concede could land out of order
            // next round. CanPerformActions() still paces fetching while an animation/event is
            // actively resolving.
            if (!gameController.NeedToSync() && !GameController.CanPerformActions())
            {
                yield return new WaitForSeconds(secondsBetweenServerUpdates);
                continue;
            }

            if (!InfoSaver.onlineBattle)
            {
                if (GameController.playerTurn || gameController.actionIsHappening)
                {
                    yield return new WaitForSeconds(secondsBetweenServerUpdates);
                    continue;
                }
                List<MessageFromServer> list = new List<MessageFromServer>();
                Bot.BotMove move = bot.GetNextMove(boardManager.friendlySlots, boardManager.enemySlots);
                if (move == null)
                {
                    yield return new WaitForSeconds(secondsBetweenServerUpdates);
                    continue;
                }
                
                MessageFromServer _m = new MessageFromServer();
                if (move.deck != null)
                {
                    _m.action = MessageFromServer.Action.SendDeck;
                    _m.targets = move.deck;
                }
                else if (move.playCard != CardTypes.Hatapon)
                {
                    _m.action = MessageFromServer.Action.PlayCard;
                    _m.targets = new List<int>() {move.cellNumber};
                    _m.cardIndex = move.playCard;
                }
                else if (move.endTurn)
                {
                    _m.action = MessageFromServer.Action.EndTurn;
                }
                else if (move.attackCell != -1)
                {
                    _m.action = MessageFromServer.Action.Attack;
                    _m.targets = new List<int>() {move.cellNumber, -move.attackCell};
                }
                else if (!move.exchange)
                {
                    _m.action = MessageFromServer.Action.Move;
                    _m.targets = new List<int>() {move.cellNumber, move.moveCell};
                }
                else
                {
                    _m.action = MessageFromServer.Action.Exchange;
                    _m.targets = new List<int>() {move.cellNumber, move.moveCell};
                }

                _m.index = messageId;
                messageId += 1;

                list.Add(_m);

                // Awaited, not fire-and-forget: a slow-to-resolve action (e.g. a spell whose
                // effect chains into a minion death and a round transition) must fully finish
                // applying before we look at the board again, or a second overlapping
                // ProcessMessages call could double-apply it.
                yield return ServerDataProcesser.instance.ProcessMessages(list);
                yield return new WaitForSeconds(secondsBetweenServerUpdates / 3f);
                continue;
            }

            Debug.Log("Start obtaining....");
            JToken snapshot = null;
            yield return FirebaseDb.Get("matches/" + MatchId() + "/actions", token => snapshot = token);
            Debug.Log("Finished obtaining!");

            messagesFromServer = new List<MessageFromServer>();

            if (snapshot is JObject actions)
            {
                foreach (JProperty entryProp in actions.Properties())
                {
                    JToken entry = entryProp.Value;
                    int curHash = entry["hash"]?.Value<int>() ?? 0;
                    // A spectator has no local/instant side of its own, so BOTH real players'
                    // actions have to arrive over the network here - see ProcessMessages'
                    // isFriendlySide handling. A real participant still only ever fetches their
                    // opponent's, exactly as before (their own actions are applied instantly/
                    // synchronously when performed, never fetched back).
                    bool wanted = curHash == boardManager.opponentHash || (InfoSaver.isSpectator && curHash == InfoSaver.myHash);
                    if (!wanted)
                    {
                        continue;
                    }

                    MessageFromServer currentMessage = new MessageFromServer();
                    currentMessage.hash = curHash;
                    currentMessage.index = entry["index"]?.Value<int>() ?? 0;
                    currentMessage.action = currentMessage.GetAction(entry["action"]?.Value<string>() ?? "");

                    if (currentMessage.action != MessageFromServer.Action.EndTurn && currentMessage.action != MessageFromServer.Action.Attack && currentMessage.action != MessageFromServer.Action.Move && currentMessage.action != MessageFromServer.Action.Exchange && currentMessage.action != MessageFromServer.Action.NumberOfCards && currentMessage.action != MessageFromServer.Action.SendDeck && currentMessage.action != MessageFromServer.Action.ConcedeMatch)
                    {
                        currentMessage.cardIndex = (CardTypes)Int32.Parse(entry["cardIdx"]?.Value<string>() ?? "0");
                    }

                    List<int> targets = new List<int>();
                    string targetsString = entry["targets"]?.Value<string>() ?? "";
                    if (targetsString != "")
                    {
                        foreach (string num in targetsString.Split(','))
                        {
                            targets.Add(Int32.Parse(num));
                        }
                    }
                    currentMessage.targets = targets;
                    messagesFromServer.Add(currentMessage);
                }
            }

            // Only hand over a contiguous run starting at the next expected index - a later
            // action arriving before an earlier one (send retried, briefly out of order, etc.)
            // must wait rather than be applied against board state that assumes the missing
            // action already happened. Nothing is discarded: the full collection is re-fetched
            // every cycle, so a gap that later fills in gets caught up in one shot.
            // A merged CastOnPlayCard message only records its *first* raw message's index in
            // .index - the paired PlayCard's raw index lives in .additionalIndex. Both must
            // count as "done", or the paired PlayCard's raw index looks unprocessed forever and
            // gets re-fed into ProcessMessages on a later poll as if it were a fresh message.
            // Computed PER SENDER HASH, not as one shared counter: each real player's own
            // messageId starts at 0 and increments independently, so in spectator mode (the
            // only time more than one hash shows up here) the two senders' index numbers
            // overlap in value and are not one combined sequence - a normal 2-player match only
            // ever has a single sender here, so this reduces to exactly the previous behavior.
            messagesFromServer = messagesFromServer.OrderBy(m => m.index).ToList();
            List<MessageFromServer> readyToProcess = new List<MessageFromServer>();
            bool anyGap = false;
            foreach (int hash in messagesFromServer.Select(m => m.hash).Distinct())
            {
                List<MessageFromServer> forHash = messagesFromServer.Where(m => m.hash == hash).ToList();
                List<MessageFromServer> doneForHash = doneActions.Where(m => m.hash == hash).ToList();
                int expected = doneForHash.Count == 0
                    ? 0
                    : doneForHash.Max(m => Math.Max(m.index, m.additionalIndex)) + 1;
                foreach (MessageFromServer m in forHash)
                {
                    if (m.index < expected)
                    {
                        continue;
                    }
                    if (m.index != expected)
                    {
                        break;
                    }
                    readyToProcess.Add(m);
                    expected++;
                }
                if (forHash.Any(m => m.index >= expected))
                {
                    anyGap = true;
                }
            }
            readyToProcess = readyToProcess.OrderBy(m => m.index).ToList();
            HasUnresolvedGap = anyGap;

            // Spectator catch-up: while behind (mid-match join, or briefly after any gap), skip
            // both this method's own inter-poll delay and ProcessMessages' per-message delay so
            // the whole match history is fetched/replayed back-to-back instead of once every
            // ~1.5s with a 2s gap per action. See spectatorCatchUpDone's declaration for exactly
            // when catch-up is considered finished. Always false for a real participant, so
            // this has no effect on normal 2-player pacing.
            bool wasCatchingUp = InfoSaver.isSpectator && !spectatorCatchUpDone;

            // Awaited, not fire-and-forget - see comment in the bot-move branch above. Without
            // this, a fast local round transition (e.g. a self-targeted spell killing your own
            // Hatapon and immediately starting the next round) can post a follow-up action
            // before we've finished applying the slower-to-resolve message that preceded it,
            // spawning a second overlapping ProcessMessages call that double-applies it.
            yield return ServerDataProcesser.instance.ProcessMessages(readyToProcess, noDelay: wasCatchingUp);

            if (wasCatchingUp && readyToProcess.Count == 0 && !HasUnresolvedGap)
            {
                // A full fetch produced nothing new to apply and there's no message we're still
                // waiting on (e.g. a CastSpell's paired PlayCard) - we've genuinely reached the
                // live tail of the match, not just paused mid-pair. Resume normal pacing.
                spectatorCatchUpDone = true;
            }

            if (wasCatchingUp && !spectatorCatchUpDone)
            {
                continue;
            }

            yield return new WaitForSeconds(secondsBetweenServerUpdates);
        }
    }
}
