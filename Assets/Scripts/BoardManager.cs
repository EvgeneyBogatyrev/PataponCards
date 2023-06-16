using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    
    public class Slot
    {
        private GameObject slotObject;
        private MinionManager connectedMinion;
        private int index;
        private bool friendly;
        private bool free;


        public Slot(Vector3 position, int _index, bool _friendly)
        {
            slotObject = Instantiate(BoardManager.slotPrefabStatic);
            slotObject.transform.position = position;
            index = _index;
            friendly = _friendly;
            free = true;
            Highlight(false);
        }

        public void Highlight(bool _active = true)
        {
            if (_active)
            {
                slotObject.GetComponent<SpriteRenderer>().color = BoardManager.highlightedColorStatic;
            }
            else
            {
                slotObject.GetComponent<SpriteRenderer>().color = BoardManager.normalColorStatic;
            }
        }

        public int GetIndex()
        {
            return index;
        }

        public bool GetFriendly()
        {
            return friendly;
        }

        public Vector3 GetPosition()
        {
            return slotObject.transform.position;
        }

        public GameObject GetSlotObject()
        {
            return slotObject;
        }

        public bool GetFree()
        {
            return free;
        }

        public void SetFree(bool _free)
        {
            free = _free;
        }

        public MinionManager GetConnectedMinion()
        {
            return connectedMinion;
        }

        public void SetConnectedMinion(MinionManager min)
        {
            connectedMinion = min;
        }
    }

    public static GameObject slotPrefabStatic;
    public GameObject slotPrefab;

    public static Color highlightedColorStatic;
    public Color highlightedColor;

    public static Color normalColorStatic;
    public Color normalColor;

    public GameObject minionPrefab;

    public List<Slot> friendlySlots = new List<Slot>();
    public List<Slot> enemySlots = new List<Slot>();

    public int numberOfSlots = 7;
    public float friendlySlotsPosition = 1f;
    public float enemySlotsPosition = 3f;
    public float leftSlotOffset = 10f;
    public float minionRotation = 0f;

    [SerializeField]
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSduAm0qxm9BoCbLBGjvCsHXgT303MOgX04oVumEYn-sNaPMEQ/formResponse";

    public int messageId = 0;
    public int lastIdFromOpponent = -1;

    public int randomHash;
    public int opponentHash;

    public GameController gameController = null;
    public enum ConnectionState
    {
        searchHost,
        waitHostReply,
        becameHost,
        sendAccept,
        connected
    }

    public ConnectionState connectionState = ConnectionState.searchHost;
    public int wantToConnect;
    public List<int> failedConnections = new List<int>();

    public List<MessageFromServer> doneActions = new List<MessageFromServer>();

    public bool battlecryTrigger = false;

    public CardTypes lastDeadYou = CardTypes.Hatapon;
    public CardTypes lastDeadOpponent = CardTypes.Hatapon;

    void Start()
    {
        InitStaticObjects();
        float step = 2 * leftSlotOffset / (numberOfSlots + 1);
        for (int i = 1; i <= numberOfSlots; ++i)
        {
            Slot newSlot = new Slot(new Vector3(-leftSlotOffset + i * step, friendlySlotsPosition, 1.5f), i - 1, true);            
            friendlySlots.Add(newSlot);
        }
        for (int i = 1; i <= numberOfSlots; ++i)
        {
            Slot newSlot = new Slot(new Vector3(-leftSlotOffset + i * step, enemySlotsPosition, 1.5f), i - 1, false);
            enemySlots.Add(newSlot);
        }

        randomHash = InfoSaver.myHash;
        opponentHash = InfoSaver.opponentHash;

        lastDeadOpponent = CardTypes.Hatapon;
        lastDeadYou = CardTypes.Hatapon;
    }

    public void InitStaticObjects()
    {
        slotPrefabStatic = slotPrefab;
        highlightedColorStatic = highlightedColor;
        normalColorStatic = normalColor;
    }

    public void PlayCard(CardManager card, Slot slot = null, bool destroy=true, bool record=true)
    {     
        GameObject newMinion = Instantiate(minionPrefab, card.transform.position, Quaternion.identity);
        newMinion.transform.rotation = Quaternion.Euler(minionRotation, 0f, 0f);
        newMinion.GetComponent<MinionManager>().CustomizeMinion(card, slot);

        if (card.GetCardStats().hasBattlecry)
        {
            int index;
            if (!slot.GetFriendly())
            {
                index = slot.GetIndex() * (-1) - 1;
            }
            else
            {
                index = slot.GetIndex() + 1;
            }

            card.GetCardStats().onPlayEvent(index, enemySlots, friendlySlots);
        }

        if (destroy)
        {
            Destroy(card.gameObject);
        }


        if (record)
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
    }

    public void CastSpell(CardManager card, List<int> targets)
    {
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

    public void CallEndRound(bool looser)
    {
        StartCoroutine(EndRound(looser));
    }

    IEnumerator EndRound(bool looser)
    {
        while (battlecryTrigger)
        {
            yield return new WaitForSeconds(0.1f);
        }
        foreach (Slot slot in friendlySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null)
            {
                minion.DestroySelf();
            }
        }

        foreach (Slot slot in enemySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null)
            {
                minion.DestroySelf();
            }
        }

        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        for (int i = 0; i < 3; ++i)
        {
            handManager.DrawCard();
        }
        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 3);

        handManager.PlayHatapons();


        if (!looser)
        {
            GameController.playerTurn = false;
            handManager.SetCanPlayCard(false);
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;
        }
        else
        {
            GameController.playerTurn = true;
            handManager.SetCanPlayCard(true);
            CursorController.cursorState = CursorController.CursorStates.Free;
        }

        lastDeadOpponent = CardTypes.Hatapon;
        lastDeadYou = CardTypes.Hatapon;
        yield return null;
    }

    IEnumerator Post(string key, string action, string cardIdx, string targets)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.107701670", "$" + randomHash.ToString() + "@" + messageId.ToString() + "$");
        form.AddField("entry.967178439", "$" + action + "$");
        form.AddField("entry.1929307381", "$" + cardIdx + "$");
        form.AddField("entry.790727074", "$" + targets + "$");
        messageId += 1;
        byte[] rawdata = form.data;
        WWW www = new WWW(BASE_URL, rawdata);
        yield return www;
        yield return new WaitForSeconds(1); 
    }

    public IEnumerator ProcessMessages(List<MessageFromServer> messages)
    {
        if (gameController == null)
        {
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }

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
                CardManager newCard = handManager.GenerateCard(type).GetComponent<CardManager>();

                if (!newCard.GetCardStats().hasOnPlay)
                {
                    processedMessages.Add(messages[messageIndex]);
                }
                else
                {
                    if (messageIndex < messages.Count() - 1 && 
                        messages[messageIndex + 1].cardIndex == messages[messageIndex].cardIndex && 
                        messages[messageIndex + 1].action == MessageFromServer.Action.PlayCard)
                    {
                        MessageFromServer newMessage = new MessageFromServer();
                        newMessage.action = MessageFromServer.Action.CastOnPlayCard;
                        newMessage.cardIndex = messages[messageIndex].cardIndex;
                        newMessage.hash = messages[messageIndex].hash;
                        newMessage.index = messages[messageIndex].index;
                        newMessage.targets = messages[messageIndex].targets;
                        newMessage.creatureTarget = messages[messageIndex + 1].targets[0];
                        newMessage.additionalIndex = messages[messageIndex + 1].index;

                        processedMessages.Add(newMessage);
                    }
                    messageIndex += 1;
                }

                newCard.DestroyCard();
            }
        }

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
            BoardManager.Slot fromSlot, toSlot;
            CardManager newCard;
            CardTypes spellType;

            switch (message.action)
            {
                case MessageFromServer.Action.EndTurn:
                    gameController.EndTurn(false);
                    gameController.StartTurn(true);
                    break;
                case MessageFromServer.Action.PlayCard:
                    CardTypes type = message.cardIndex;
                    int target = message.targets[0];
                    Slot targetSlot;
                    if (target > 0)
                    {
                        target -= 1;
                        targetSlot = enemySlots[target];
                    }
                    else
                    {
                        target = (-1 * target) - 1;
                        targetSlot = friendlySlots[target];
                    }
                    
                    newCard = handManager.GenerateCard(type).GetComponent<CardManager>();
                    PlayCard(newCard, targetSlot, destroy: false, record: false);
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    break;

                case MessageFromServer.Action.Move:
                    fromSlot = enemySlots[message.targets[0] - 1];
                    toSlot = enemySlots[message.targets[1] - 1];

                    fromSlot.GetConnectedMinion().Move(toSlot);
                    break;

               case MessageFromServer.Action.Exchange:
                    fromSlot = enemySlots[message.targets[0] - 1];
                    toSlot = enemySlots[message.targets[1] - 1];

                    fromSlot.GetConnectedMinion().Exchange(toSlot);
                    break;

                case MessageFromServer.Action.Attack:
                    int from = message.targets[0] * (-1);
                    int to = message.targets[1] * (-1);
                    MinionManager fromMinion, toMinion;
                    if (from < 0)
                    {
                        fromMinion = enemySlots[from * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        fromMinion = friendlySlots[from - 1].GetConnectedMinion();
                    }

                    if (to < 0)
                    {
                        toMinion = enemySlots[to * (-1) - 1].GetConnectedMinion();
                    }
                    else
                    {
                        toMinion = friendlySlots[to - 1].GetConnectedMinion();
                    }
                    fromMinion.Attack(toMinion);
                    break;
                case MessageFromServer.Action.CastSpell:
                    spellType = message.cardIndex;
                    newCard = handManager.GenerateCard(spellType).GetComponent<CardManager>();
                    newCard.GetCardStats().spell(message.targets, friendlySlots, enemySlots);
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;

                    if (newCard.GetCardStats().damageToHost == -1 && newCard.GetCardType() != CardTypes.Concede)
                    {
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    }
                    break;

                case MessageFromServer.Action.CastOnPlayCard:
                    battlecryTrigger = true;
                    spellType = message.cardIndex;
                    newCard = handManager.GenerateCard(spellType).GetComponent<CardManager>();
                    newCard.GetCardStats().spell(message.targets, friendlySlots, enemySlots);

                    if (message.creatureTarget > 0)
                    {
                        message.creatureTarget -= 1;
                        fromSlot = enemySlots[message.creatureTarget];
                    }
                    else
                    {
                        message.creatureTarget = (-1 * message.creatureTarget) - 1;
                        fromSlot = friendlySlots[message.creatureTarget];
                    }
                    PlayCard(newCard, fromSlot, destroy: false, record: false);


                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;

                    battlecryTrigger = false;
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    break;

                case MessageFromServer.Action.NumberOfCards:
                    handManager.SetNumberOfOpponentsCards(message.targets[0]);
                    break;
            }

            doneActions.Add(message);
            
            yield return new WaitForSeconds(2); 
        }
    }
}


