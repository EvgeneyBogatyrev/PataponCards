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
    // Saves info between scenes
    public static int myHash;
    public static int opponentHash;
}

public class MessageFromServer
{
    // Struct that holds info batch received from server
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

public class GameState
{
    public int friendlyWins = 0;
    public int enemyWins = 0;

}

public class GameController : MonoBehaviour
{
    public static bool playerTurn = true;
    private HandManager handManager;
    private BoardManager boardManager;

    [SerializeField]
    private GameObject scoreObject;

    private string GOOGLE_API_URL = "https://script.google.com/macros/s/AKfycbwHlf0DxUjBKb3blzMbawD3Yn1FfPp9unN8Ho5LGb_DQoc1YcvwhhHaS9hM1FLhMYxk/exec";
    private GameState gameState = new GameState();

    public List<MessageFromServer> messagesFromServer;

    public float secondsBetweenServerUpdates = 5f;
    public float secondsBetweenAnimations = 0.5f;
    public bool actionIsHappening = false;

    private void Start()
    {
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
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
            if (playerTurn)
            {
                yield return new WaitForSeconds(secondsBetweenServerUpdates);
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
            yield return new WaitForSeconds(secondsBetweenServerUpdates);
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
                        yield return new WaitForSeconds(secondsBetweenAnimations);
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
                        yield return new WaitForSeconds(secondsBetweenAnimations);
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
                        yield return new WaitForSeconds(secondsBetweenAnimations);
                    } while(actionIsHappening);
                }
                foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                {
                    StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                    do {
                        yield return new WaitForSeconds(secondsBetweenAnimations);
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
                        yield return new WaitForSeconds(secondsBetweenAnimations);
                    } while(actionIsHappening);
                }
                foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                {
                    StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                    do {
                        yield return new WaitForSeconds(secondsBetweenAnimations);
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

    public void RecordGameResult(bool friendlyVictory)
    {
        if (friendlyVictory)
        {
            gameState.friendlyWins += 1;
        }
        else
        {
            gameState.enemyWins += 1;
        }
        scoreObject.GetComponent<TextMeshProUGUI>().text = gameState.friendlyWins.ToString() + ":" + gameState.enemyWins.ToString();;
    }

    public bool CheckGameEnd()
    {
        if (gameState.friendlyWins >= 2)
        {
            return true;
        }
        if (gameState.enemyWins >= 2)
        {
            return true;
        }
        return false;
    }

    // Buttons
    public void EndTurnButton()
    {
        if (playerTurn && CursorController.cursorState == CursorController.CursorStates.Free)
        {
            EndTurn(true);
        }
    }

    public void Concede()
    {
        if (playerTurn && CursorController.cursorState == CursorController.CursorStates.Free)
        {
            CardManager concedeCard = handManager.GenerateCard(CardTypes.Concede).GetComponent<CardManager>();
            concedeCard.GetCardStats().spell(new List<int>(), boardManager.enemySlots, boardManager.friendlySlots);
            boardManager.CastSpell(concedeCard, new List<int>());
            concedeCard.DestroyCard();
        }
    }
}
