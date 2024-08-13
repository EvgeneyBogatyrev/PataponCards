using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;


public class ServerDataProcesser : MonoBehaviour
{
    public static ServerDataProcesser instance;
    
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSduAm0qxm9BoCbLBGjvCsHXgT303MOgX04oVumEYn-sNaPMEQ/formResponse";
    private string GOOGLE_API_URL = "https://script.google.com/macros/s/AKfycbwHlf0DxUjBKb3blzMbawD3Yn1FfPp9unN8Ho5LGb_DQoc1YcvwhhHaS9hM1FLhMYxk/exec";

    private BoardManager boardManager;

    public int messageId = 0;
    public List<MessageFromServer> doneActions = new List<MessageFromServer>();
    public List<MessageFromServer> messagesFromServer;
    public float secondsBetweenServerUpdates = 5f;
    public Bot bot = null;
    
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
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
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

    IEnumerator Post(string key, string action, string cardIdx, string targets)
    {
        if (!InfoSaver.NEGR)
        {
            yield return null;
        }
        WWWForm form = new WWWForm();
        form.AddField("entry.107701670", "$" + InfoSaver.myHash.ToString() + "@" + messageId.ToString() + "$");
        form.AddField("entry.967178439", "$" + action + "$");
        form.AddField("entry.1929307381", "$" + cardIdx + "$");
        form.AddField("entry.790727074", "$" + targets + "$");
        messageId += 1;

        using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }

        yield return new WaitForSeconds(1);  
    }

    public IEnumerator ProcessMessages(List<MessageFromServer> messages)
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
                    }
                    messageIndex += 1;
                }

                _newCard.DestroyCard();
            }
        }

        CardManager newCard = null;
        foreach (MessageFromServer message in processedMessages)
        {
            bool found = false;
            foreach (MessageFromServer doneMessage in doneActions)
            {
                if (doneMessage.index == message.index) 
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
            switch (message.action)
            {
                case MessageFromServer.Action.EndTurn:
                    gameController.EndTurn(false);
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
                    newCard = handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1, returnCard:true);
                    CardTypes type = message.cardIndex;
                    boardManager.playedCardsOpponent.Add(type);
                    LogController.instance.AddPlayCardToLog(type, null, false);
                    int target = message.targets[0];
                    BoardManager.Slot targetSlot;
                    if (target > 0)
                    {
                        target -= 1;
                        targetSlot = boardManager.enemySlots[target];
                    }
                    else
                    {
                        target = (-1 * target) - 1;
                        targetSlot = boardManager.friendlySlots[target];
                    }
                    
                    newCard = handManager.GenerateCard(type, newCard).GetComponent<CardManager>();
                    boardManager.PlayCard(newCard, new Vector3(0f, 10f, 0f), targetSlot, destroy: false, record: false);
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);

                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;
                    break;

                case MessageFromServer.Action.Move:
                    fromSlot = boardManager.enemySlots[message.targets[0] - 1];
                    
                    if (message.targets[1] > 0)
                    {
                        toSlot = boardManager.enemySlots[message.targets[1] - 1];
                    }
                    else
                    {
                        toSlot = boardManager.friendlySlots[-message.targets[1] - 1];
                        MinionManager connectedMinion = toSlot.GetConnectedMinion();
                        if (connectedMinion != null)
                        {
                            connectedMinion.DestroyMinion();
                        }
                    }

                    fromSlot.GetConnectedMinion().Move(toSlot);
                    break;

               case MessageFromServer.Action.Exchange:
                    fromSlot = boardManager.enemySlots[message.targets[0] - 1];
                    toSlot = boardManager.enemySlots[message.targets[1] - 1];

                    fromSlot.GetConnectedMinion().Exchange(toSlot);
                    break;

                case MessageFromServer.Action.Attack:
                    int from = message.targets[0] * (-1);
                    int to = message.targets[1] * (-1);
                    LogController.instance.AddAttackToLog(from, to, false);
                    MinionManager fromMinion, toMinion;
                    if (from < 0)
                    {
                        fromMinion = boardManager.enemySlots[from * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        fromMinion = boardManager.friendlySlots[from - 1].GetConnectedMinion();
                    }

                    if (to < 0)
                    {
                        toMinion = boardManager.enemySlots[to * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        toMinion =boardManager.friendlySlots[to - 1].GetConnectedMinion();
                    }
                    fromMinion.Attack(toMinion);
                    fromMinion.SetCanAttack(false);
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
                    boardManager.playedCardsOpponent.Add(spellType);
                    LogController.instance.AddPlayCardToLog(spellType, message.targets, false);
                    newCard = handManager.GenerateCard(spellType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    
                    if (newCard.GetCardStats().damageToHost == -1 && newCard.GetCardType() != CardTypes.Concede)
                    {
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    }
                    
                    newCard.arrowList = null;
                    newCard.spellTargets = message.targets;
                    // Cast spell
                    QueueData newEvent = new();
                    newEvent.actionType = QueueData.ActionType.CastSpell;
                    newEvent.thisStats = newCard.GetCardStats();
                    newEvent.hostCard = newCard;
                    newEvent.targets = newCard.spellTargets;
                    newEvent.friendlySlots = boardManager.enemySlots;
                    newEvent.enemySlots = boardManager.friendlySlots;
                    GameController.eventQueue.Insert(0, newEvent);
                    //StartCoroutine(newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots));
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;

                    if (newCard.GetCardStats().damageToHost != -1)
                    {
                        newCard.transform.position = boardManager.enemySlots[message.targets[0] - 1].GetPosition();
                        newCard.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
                    }
                    break;

                case MessageFromServer.Action.CastOnPlayCard:
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
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    boardManager.battlecryTrigger = true;
                    spellType = message.cardIndex;
                    boardManager.playedCardsOpponent.Add(spellType);
                    LogController.instance.AddPlayCardToLog(spellType, message.targets, false);
                    newCard = handManager.GenerateCard(spellType, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    if (newCard.GetCardStats().dummyTarget)
                    {
                        newCard.spellTargets = message.targets.GetRange(1, message.targets.Count - 1);
                    }
                    else
                    {
                        newCard.spellTargets = message.targets;
                    }
                    HandManager.DestroyDisplayedCards();
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = HandManager.cardDestroyTimer;

                    QueueData _newEvent = new();
                    _newEvent.actionType = QueueData.ActionType.CastSpell;
                    _newEvent.thisStats = newCard.GetCardStats();
                    _newEvent.hostCard = newCard;
                    _newEvent.targets = message.targets;
                    _newEvent.friendlySlots = boardManager.enemySlots;
                    _newEvent.enemySlots = boardManager.friendlySlots;
                    GameController.eventQueue.Insert(0, _newEvent);
                    // Cast spell
                    //StartCoroutine(newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots));

                    if (message.creatureTarget > 0)
                    {
                        message.creatureTarget -= 1;
                        fromSlot = boardManager.enemySlots[message.creatureTarget];
                    }
                    else
                    {
                        message.creatureTarget = (-1 * message.creatureTarget) - 1;
                        fromSlot = boardManager.friendlySlots[message.creatureTarget];
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
                    
                    break;

                case MessageFromServer.Action.NumberOfCards:
                    
                    if (message.targets[0] != handManager.GetNumberOfOpponentsCards())
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
                    newCard = handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1, returnCard:true);
                    CardTypes cycleType = message.cardIndex;
                    
                    LogController.instance.AddCycleToLog(cycleType, false);

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

                        __newEvent.friendlySlots = boardManager.enemySlots;
                        __newEvent.enemySlots = boardManager.friendlySlots;

                        GameController.eventQueue.Insert(0, __newEvent);
                        // On cycle
                        //StartCoroutine(newCard.GetCardStats().onCycleEvent(boardManager.friendlySlots, boardManager.enemySlots));
                    }

                    //BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
                    int idx = 0;
                    foreach (BoardManager.Slot slot in boardManager.enemySlots)
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

                    handManager.DrawCardOpponent();
                    break;

                case MessageFromServer.Action.Discard:
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - (int)message.cardIndex);
                    break;

                case MessageFromServer.Action.SendDeck:
                    DeckManager.ReceiveOpponentsDeck(message.targets);
                    break;

                case MessageFromServer.Action.Ping:
                    break;
            }

            doneActions.Add(message);
            yield return new WaitForSeconds(2f); 
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

    public IEnumerator ObtainData()
    {
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        while (true)
        {
            if (!gameController.NeedToSync())
            {
                if (GameController.playerTurn)
                {
                    yield return new WaitForSeconds(secondsBetweenServerUpdates);
                    continue;
                }
                else if (!GameController.CanPerformActions())
                {
                    yield return new WaitForSeconds(secondsBetweenServerUpdates);
                    continue;
                }
            }

            if (!InfoSaver.NEGR)
            {
                Debug.Log(GameController.playerTurn);
                if (GameController.playerTurn || gameController.actionIsHappening)
                {
                    Debug.Log("Skipping....");
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
                    _m.targets = new List<int>() {move.cellNumber, move.attackCell};
                }
                else
                {
                    _m.action = MessageFromServer.Action.Move;
                    _m.targets = new List<int>() {move.cellNumber, move.moveCell};
                }

                _m.index = messageId;
                messageId += 1;

                list.Add(_m);

                StartCoroutine(ServerDataProcesser.instance.ProcessMessages(list));
                yield return new WaitForSeconds(secondsBetweenServerUpdates / 3f);
                continue;
            }

            Debug.Log("Start obtaining....");
            UnityWebRequest www = UnityWebRequest.Get(GOOGLE_API_URL);
            yield return www.SendWebRequest();
            Debug.Log("Finished obtaining!");

            messagesFromServer = new List<MessageFromServer>();
            MessageFromServer currentMessage = new MessageFromServer();

            string[] batches = www.downloadHandler.text.Split('$');
            int iterIndex = 0;
            int batchIndex = 0;
            int curHash = 0;
            foreach (string s in batches)
            {
                if (iterIndex == 0)
                {
                    iterIndex = 1;
                }
                else
                {
                    iterIndex = 0;
                    if (batchIndex == 0)
                    {
                        currentMessage = new MessageFromServer();
                        string[] words = s.Split('@');
                        curHash = Int32.Parse(words[0]);
                        currentMessage.hash = curHash;
                        currentMessage.index = Int32.Parse(words[1]);
                    }
                    else if (batchIndex == 1)
                    {
                        if (curHash == boardManager.opponentHash)
                        {
                            currentMessage.action = currentMessage.GetAction(s);
                        }
                    }
                    else if (batchIndex == 2)
                    {
                        if (curHash == boardManager.opponentHash && currentMessage.action != MessageFromServer.Action.EndTurn && currentMessage.action != MessageFromServer.Action.Attack && currentMessage.action != MessageFromServer.Action.Move && currentMessage.action != MessageFromServer.Action.Exchange && currentMessage.action != MessageFromServer.Action.NumberOfCards && currentMessage.action != MessageFromServer.Action.SendDeck)
                        {
                            currentMessage.cardIndex = (CardTypes)Int32.Parse(s);
                        }
                    }
                    else if (batchIndex == 3)
                    {
                        if (curHash == boardManager.opponentHash)
                        {
                            List<int> targets = new List<int>();
                            if (s != "")
                            {
                                string[] numbers = s.Split(',');
                                foreach (string num in numbers)
                                {
                                    targets.Add(Int32.Parse(num));
                                }
                            }
                            currentMessage.targets = targets;
                            messagesFromServer.Add(currentMessage);
                        }
                        batchIndex = -1;
                    }
                    batchIndex += 1;
                }
            }
            StartCoroutine(ServerDataProcesser.instance.ProcessMessages(messagesFromServer));
            yield return new WaitForSeconds(secondsBetweenServerUpdates);
        }
    }
}
