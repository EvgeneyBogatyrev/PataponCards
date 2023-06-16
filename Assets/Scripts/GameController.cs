using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.IO;
using System;
using TMPro;

public class InfoSaver
{
    public static int myHash;
    public static int opponentHash;
}

public class MessageFromServer
{
    public enum Action
    {
        GameSearch,
        GameRequest,
        GameAccept,
        EndTurn,
        PlayCard,
        CastSpell,
        NumberOfCards,
        Attack,
        Move,
        Exchange,
        CastOnPlayCard
    }

    public Action GetAction(string s)
    {
        if (s == "end turn")
        {
            return Action.EndTurn;
        }
        if (s == "play card")
        {
            return Action.PlayCard;
        }
        if (s == "attack")
        {
            return Action.Attack;
        }
        if (s == "move")
        {
            return Action.Move;
        }
        if (s == "exchange")
        {
            return Action.Exchange;
        }
        if (s == "cast spell")
        {
            return Action.CastSpell;
        }
        if (s == "number of cards")
        {
            return Action.NumberOfCards;
        }
        if (s == "game search")
        {
            return Action.GameSearch;
        }
        if (s == "game request")
        {
            return Action.GameRequest;
        }
        if (s == "game accept")
        {
            return Action.GameAccept;
        }
        return Action.EndTurn;
    }

    public int hash;
    public int index;
    public Action action;
    public CardTypes cardIndex;
    public List<int> targets;
    public int creatureTarget;
    public int additionalIndex = -1;
}

public class GameController : MonoBehaviour
{
    public static bool playerTurn = true;
    private HandManager handManager;
    private BoardManager boardManager;

    public List<MessageFromServer> messagesFromServer;

    public bool foundOpponent = false;

    public GameObject inputField;

    public bool actionIsHappening = false;

    private void Start()
    {
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        handManager.SetCanPlayCard(true);

        //if (InfoSaver.opponentHash > InfoSaver.myHash)
        //{ 
        playerTurn = false;
        handManager.SetCanPlayCard(false);
        CursorController.cursorState = CursorController.CursorStates.EnemyTurn;
        //}

        //StartCoroutine(ObtainData());
    }

    public void StartGame()
    {
        if (InfoSaver.opponentHash <= InfoSaver.myHash)
        {
            playerTurn = true;
            handManager.SetCanPlayCard(true);
            CursorController.cursorState = CursorController.CursorStates.Free;
        }
        StartCoroutine(ObtainData());
    }

    
    IEnumerator ObtainData()
    {
        while (true)
        {
        //StartCoroutine(GetRequest("https://script.google.com/macros/s/AKfycbyUMTOZYCjzeFuTG8y7jCkRwaIUusMfo-kJC6sSekI3JAUXB4dHg-XvkvbPypOJKAdU/exec"));
        //yield return webRequest.SendWebRequest();
        if (playerTurn)
        {
            yield return new WaitForSeconds(5);
            continue;
        }
        Debug.Log("Start obtaining....");
        UnityWebRequest www = UnityWebRequest.Get("https://script.google.com/macros/s/AKfycbwHlf0DxUjBKb3blzMbawD3Yn1FfPp9unN8Ho5LGb_DQoc1YcvwhhHaS9hM1FLhMYxk/exec");
        yield return www.SendWebRequest();
        Debug.Log("Finished obtaining!");

        messagesFromServer = new List<MessageFromServer>();
        MessageFromServer currentMessage = new MessageFromServer();

        //string path = "Assets/Resources/test3.txt";
        //Write some text to the test.txt file
        //StreamWriter writer = new StreamWriter(path, true);
       // writer.WriteLine(www.downloadHandler.text);
        //writer.Close();

        string[] batches = www.downloadHandler.text.Split('$');
        int iterIndex = 0;
        int batchIndex = 0;
        int batchIndexMax = 4;
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
                    if (curHash == boardManager.opponentHash && currentMessage.action != MessageFromServer.Action.EndTurn && currentMessage.action != MessageFromServer.Action.Attack && currentMessage.action != MessageFromServer.Action.Move && currentMessage.action != MessageFromServer.Action.Exchange && currentMessage.action != MessageFromServer.Action.NumberOfCards)
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
        StartCoroutine(boardManager.ProcessMessages(messagesFromServer));
        yield return new WaitForSeconds(5);
        }
        //yield return ObtainData();
    }

    public void EndTurnButton()
    {
        if (playerTurn && CursorController.cursorState == CursorController.CursorStates.Free)
        {
            //StartCoroutine(ObtainData());
            EndTurn(true);
            
            //StartTurn(false);
            //boardManager.EndTurn();   now in EndTurn
            
            // Start coroutine to manage opponent's turn
            //EndTurn(false); // Fix
            //StartTurn(true); //Fix
        }

    }

    public void StartTurn(bool friendly)
    {
        StartCoroutine(IenumStartTurn(friendly));
    }

    IEnumerator IenumStartTurn(bool friendly)
    {
        if (friendly)
        {
            playerTurn = true;
            CursorController.cursorState = CursorController.CursorStates.Free;
            handManager.SetCanPlayCard(true);

            List<MinionManager> order = new List<MinionManager>();

            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    order.Add(minion);
                    minion.SetCanAttack(true);
                }
            }

            foreach (MinionManager minion in order)
            {
                CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                if (thisStartTurnEvent != null)
                {
                    StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
            }
        }
        else
        {
            List<MinionManager> order = new List<MinionManager>();

            foreach (BoardManager.Slot slot in boardManager.enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    order.Add(minion);
                    minion.SetCanAttack(true);
                }
            }

            foreach (MinionManager minion in order)
            {
                CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                if (thisStartTurnEvent != null)
                {
                    StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
            }
        }
        if (!friendly)
        {
            boardManager.EndTurn();
            playerTurn = false;
        }
        yield return null;
    }

    public void Concede()
    {
        if (playerTurn && CursorController.cursorState == CursorController.CursorStates.Free)
        {
            CardManager concedeCard = handManager.GenerateCard(CardTypes.Concede).GetComponent<CardManager>();
            concedeCard.GetCardStats().spell(new List<int>(), boardManager.enemySlots, boardManager.friendlySlots);
            boardManager.CastSpell(concedeCard, new List<int>());
            concedeCard.DestroyCard();
            //EndTurn(true);
        }
    }

    public void EndTurn(bool friendly)
    {
        StartCoroutine(IenumEndTurn(friendly));
    }

    private IEnumerator IenumEndTurn(bool friendly)
    {
        if (friendly)
        {
            handManager.SetCanPlayCard(false);
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;

            List<MinionManager> order = new List<MinionManager>();

            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    order.Add(minion);
                    minion.SetState(MinionManager.MinionState.Free);
                }
            }

            foreach (MinionManager minion in order)
            {
                CardManager.EndTurnEvent thisEndTurnEvent = minion.GetCardStats().endTurnEvent;
                if (thisEndTurnEvent != null)
                {
                    StartCoroutine(thisEndTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
                foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                {
                    StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
            }
        }
        else
        {
            List<MinionManager> order = new List<MinionManager>();
            foreach (BoardManager.Slot slot in boardManager.enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    order.Add(minion);
                    minion.SetState(MinionManager.MinionState.Free);
                }
            }
            foreach (MinionManager minion in order)
            {
                CardManager.EndTurnEvent thisEndTurnEvent = minion.GetCardStats().endTurnEvent;
                if (thisEndTurnEvent != null)
                {
                    StartCoroutine(thisEndTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
                foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                {
                    StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                    do {
                        yield return new WaitForSeconds(0.5f);
                    } while(actionIsHappening);
                }
            }
        }
        if (friendly)
        {
            StartTurn(false);
        }
        yield return null;
    }
}
