using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;


public class ServerDataProcesser : MonoBehaviour
{
    public static ServerDataProcesser instance;
    
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSduAm0qxm9BoCbLBGjvCsHXgT303MOgX04oVumEYn-sNaPMEQ/formResponse";

    public int messageId = 0;
    public List<MessageFromServer> doneActions = new List<MessageFromServer>();
    
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

    IEnumerator Post(string key, string action, string cardIdx, string targets)
    {
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
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
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
                    
                    newCard = handManager.GenerateCard(type).GetComponent<CardManager>();
                    boardManager.PlayCard(newCard, targetSlot, destroy: false, record: false);
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    break;

                case MessageFromServer.Action.Move:
                    fromSlot = boardManager.enemySlots[message.targets[0] - 1];
                    toSlot = boardManager.enemySlots[message.targets[1] - 1];

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
                    break;
                case MessageFromServer.Action.CastSpell:
                    spellType = message.cardIndex;
                    newCard = handManager.GenerateCard(spellType).GetComponent<CardManager>();
                    newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots);
                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;

                    if (newCard.GetCardStats().damageToHost == -1 && newCard.GetCardType() != CardTypes.Concede)
                    {
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() - 1);
                    }
                    break;

                case MessageFromServer.Action.CastOnPlayCard:
                    boardManager.battlecryTrigger = true;
                    spellType = message.cardIndex;
                    newCard = handManager.GenerateCard(spellType).GetComponent<CardManager>();
                    newCard.GetCardStats().spell(message.targets, boardManager.friendlySlots, boardManager.enemySlots);

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
                    boardManager.PlayCard(newCard, fromSlot, destroy: false, record: false);


                    newCard.SetCardState(CardManager.CardState.opponentPlayed);
                    newCard.transform.position = new Vector3(0f, 10f, 0f);
                    newCard.destroyTimer = 5f;

                    boardManager.battlecryTrigger = false;
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