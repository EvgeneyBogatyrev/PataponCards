using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public int friendlyTurnNumber = 0;
    public int enemyTurnNumber = 0;
    public int suddenDeathTurns = 10;
    public int friendlySuddenDeathDamage = 0;
    public int enemySuddenDeathDamage = 0;

    public bool StartOfTheGame()
    {
        if (friendlyWins + enemyWins == 0)
        {
            return true;
        }
        return false;
    }

    public void Reset(GameObject turnsObject) 
    {
        friendlyTurnNumber = 0;
        enemyTurnNumber = 0;
        friendlySuddenDeathDamage = 0;
        enemySuddenDeathDamage = 0;

        turnsObject.GetComponent<TextMeshProUGUI>().text = "Turn: " + friendlyTurnNumber.ToString();
    }

    public void Increment(bool friendly, GameObject turnsObject)
    {
        if (friendly)
        {
            friendlyTurnNumber += 1;
            if (friendlyTurnNumber >= suddenDeathTurns)
            {
                friendlySuddenDeathDamage += 1;
            }
        }
        else
        {
            enemyTurnNumber += 1;
            if (enemyTurnNumber >= suddenDeathTurns)
            {
                enemySuddenDeathDamage += 1;
            }
        }

        turnsObject.GetComponent<TextMeshProUGUI>().text = "Turn: " + friendlyTurnNumber.ToString();
    }

    public int GetSuddenDeathDamage(bool friendly)
    {
        if (friendly)
        {
            return friendlySuddenDeathDamage;
        }
        else
        {
            return enemySuddenDeathDamage;
        }
    }
}

public class GameController : MonoBehaviour
{
    public static bool playerTurn = false;
    private HandManager handManager;
    private BoardManager boardManager;

    [SerializeField]
    private GameObject scoreObject;
    [SerializeField]
    private GameObject turnsObject;
    [SerializeField]
    private GameObject deckSizeObject;

    [SerializeField]
    private GameObject endTurnButtonObject;

    [SerializeField]
    private GameObject concedeObject;

    private GameState gameState = new GameState();

    public float secondsBetweenAnimations = 0.5f;
    public bool actionIsHappening = false;

    private void Start()
    {
        endTurnButtonObject.SetActive(false);
        concedeObject.SetActive(false);
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
    }

    public void StartGame()
    {
        endTurnButtonObject.SetActive(true);
        concedeObject.SetActive(true);
        if (InfoSaver.opponentHash <= InfoSaver.myHash)
        {
            playerTurn = true;
            handManager.SetCanPlayCard(true);
            CursorController.cursorState = CursorController.CursorStates.Free;
            gameState.Increment(friendly:true, turnsObject);
            boardManager.DealSuddenDeathDamage(friendly:true, gameState.GetSuddenDeathDamage(friendly:true));
        }
        else
        {
            playerTurn = false;
            handManager.SetCanPlayCard(false);
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;
            gameState.Increment(friendly:false, turnsObject);
            boardManager.DealSuddenDeathDamage(friendly:false, gameState.GetSuddenDeathDamage(friendly:false));
        }
        if (gameState.StartOfTheGame())
        {
            DeckManager.ResetOpponentsDeck();
        }
        StartCoroutine(ServerDataProcesser.instance.ObtainData());
    }

    public void StartTurn(bool friendly, bool hataponJustDied=false)
    {
        StartCoroutine(IenumStartTurn(friendly, hataponJustDied));
    }

    IEnumerator IenumStartTurn(bool friendly, bool hataponJustDied=false)
    {
        if (!friendly)
        {
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;
        }
        else
        {
            CursorController.cursorState = CursorController.CursorStates.Free;
        }
        GameController.playerTurn = friendly;
        handManager.SetCanPlayCard(friendly);

        gameState.Increment(friendly, turnsObject);
        boardManager.DealSuddenDeathDamage(friendly, gameState.GetSuddenDeathDamage(friendly));
        if (friendly)
        {
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
        if (!friendly && !hataponJustDied)
        {
            ServerDataProcesser.instance.EndTurn();
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
        StartTurn(!friendly);
        yield return null;
    }

    public void EndRound(bool friendly)
    {
        StartCoroutine(OnEndRound(friendly));
    }

    public IEnumerator OnEndRound(bool friendlyVictory)
    {
        RecordGameResult(friendlyVictory);
        while (boardManager.battlecryTrigger)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (CheckGameEnd())
        {
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene("MainMenu");
            yield return null;
        }
        else
        {
            boardManager.ClearBoard();
            handManager.StartRoundActions();
            
            gameState.Reset(turnsObject);

            StartTurn(!friendlyVictory, hataponJustDied:true);
            yield return null;
        }
    }

    public bool NeedToSync()
    {
        if (gameState.friendlyTurnNumber + gameState.enemyTurnNumber == 1)
        {
            return true;
        }
        return false;
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
        scoreObject.GetComponent<TextMeshProUGUI>().text = gameState.friendlyWins.ToString() + ":" + gameState.enemyWins.ToString();
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

    public bool ProcessCardDraw(bool friendly)
    {
        bool couldDraw;
        if (!friendly)
        {
            if (DeckManager.opponentDeckSize > 0)
            {
                DeckManager.opponentDeckSize -= 1;
                couldDraw = true;
            }
            else 
            {
                couldDraw = false;
                //DeckManager.opponentDeckSize = 0;
            }
        }
        else
        {
            if (DeckManager.GetDeckSize() > 0)
            {
                couldDraw = true;
            }
            else
            {
                couldDraw = false;
            }
        }

        deckSizeObject.GetComponent<TextMeshProUGUI>().text = "Decks: " + DeckManager.GetDeckSize().ToString() + ":" + DeckManager.opponentDeckSize.ToString();

        return couldDraw;
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
            ServerDataProcesser.instance.CastSpell(concedeCard, new List<int>());
            concedeCard.DestroyCard();
        }
    }
}
