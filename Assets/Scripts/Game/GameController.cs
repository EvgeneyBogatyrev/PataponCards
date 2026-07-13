using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Assertions;
using Networking;

public class InfoSaver
{
    // Saves info between scenes
    public static int myHash;
    public static int opponentHash;
    public static bool victory = false;
    public static int chests = 0;
    public static bool onlineBattle = true;
    public static int botLevel = 0;
    public static bool[] botDefeated = new bool[4] { false, false, false, false};

    // Scene to return to once the Account scene finishes a sign-in/sign-up.
    public static string sceneAfterLogin = "Lobby";

    // Set by FirebaseChallenge once a friend challenge's hash pairing is established (myHash/
    // opponentHash/onlineBattle already set at that point) - read once by LobbyManager.Start()
    // to skip its own random hash generation and jump straight to the Game scene instead of
    // showing the normal Play Online/manual-key UI.
    public static bool challengeAccepted = false;
}

public class QueueData
{
    public enum ActionType
    {
        EndTurn,
        StartTurn,
        OnDeath,
        OnDamage,
        OnAttack,
        OnCycle,
        OnCycleOther,
        CastSpell,
        DrawCard,
        AfterPlayEffect,
    }

    public ActionType actionType;
    //public delegate IEnumerator EndTurnEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator Spell(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator OnCycleEvent(List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator OnAttackEvent(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator OnPlayEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator OnCycleOtherEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    //public delegate IEnumerator OnDeathEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null, CardStats thisStats = null);

    //public EndTurnEvent endTurnEvent = null;
    //public Spell spell = null;
    public MinionManager hostUnit = new();
    public CardManager hostCard = null;
    public int index = -1;
    public int additionalEndTurnIdx = -1;
    public List<int> targets = null;
    public List<BoardManager.Slot> friendlySlots = null;
    public List<BoardManager.Slot> enemySlots = null;
    public CardManager.CardStats thisStats = null;
    public int ephemeral = -1;

    public void ApplyEffect()
    {
        switch (actionType)
        {
            case ActionType.OnDamage:
                if (hostUnit != null)
                    hostUnit.StartCoroutine(hostUnit.GetCardStats().onDamageEvent(index, enemySlots, friendlySlots));
                break;
            case ActionType.EndTurn:
                if (hostUnit != null)
                {
                    if (additionalEndTurnIdx == -1)
                    {
                        hostUnit.StartCoroutine(hostUnit.GetCardStats().endTurnEvent(index, enemySlots, friendlySlots));
                    }
                    else
                    {
                        hostUnit.StartCoroutine(hostUnit.GetCardStats().additionalEndTurnEvents[additionalEndTurnIdx](index, enemySlots, friendlySlots));
                    }
                }
                break;
            case ActionType.StartTurn:
                if (hostUnit != null)
                    hostUnit.StartCoroutine(hostUnit.GetCardStats().startTurnEvent(index, enemySlots, friendlySlots));
                break;
            case ActionType.OnDeath:
                if (hostUnit != null)
                    hostUnit.StartCoroutine(thisStats.onDeathEvent(index, enemySlots, friendlySlots, thisStats));
                break;
            case ActionType.OnAttack:
                if (hostUnit != null)
                    hostUnit.StartCoroutine(thisStats.onAttackEvent(targets, enemySlots, friendlySlots));
                break;
            case ActionType.OnCycle:
                if (hostCard != null)
                    hostCard.StartCoroutine(hostCard.GetCardStats().onCycleEvent(enemySlots, friendlySlots));
                break;
            case ActionType.OnCycleOther:
                if (hostUnit != null)
                    hostUnit.StartCoroutine(hostUnit.GetCardStats().onCycleOtherEvent(index, enemySlots, friendlySlots));
                break;
            case ActionType.CastSpell:
                if (hostCard != null)
                {
                    // Brackets the ability's synchronous portion (everything before its first
                    // yield, which StartCoroutine runs immediately/inline) so a hasOnPlaySpell
                    // ability can optionally redirect where its own card ends up - see Aiton and
                    // CardManager.CurrentlyResolvingOnPlayCard/SetSlotToPlay.
                    CardManager.CurrentlyResolvingOnPlayCard = hostCard;
                    hostCard.StartCoroutine(thisStats.spell(targets, enemySlots, friendlySlots));
                    CardManager.CurrentlyResolvingOnPlayCard = null;
                }
                break;

            case ActionType.DrawCard:
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
                CardTypes cardType = DeckManager.GetTopCard(remove:true);
                if (cardType != CardTypes.Hatapon)
                {
                    handManager.AddCardToHand(cardType, ephemeral:ephemeral);
                }
                gameController.ProcessCardDraw(friendly:true);
                break;

            case ActionType.AfterPlayEffect:
                if (hostCard != null)
                    hostCard.StartCoroutine(hostCard.GetCardStats().afterPlayEvent(index, enemySlots, friendlySlots));
                break;
        }
    }
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
        CastOnPlayCard,
        Cycle,
        Discard,
        SendDeck,
        Ping,
        ConcedeMatch,
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
        if (s == "cycle card")
        {
            return Action.Cycle;
        }
        if (s == "discard")
        {
            return Action.Discard;
        }
        if (s == "send deck")
        {
            return Action.SendDeck;
        }
        if (s == "ping")
        {
            return Action.Ping;
        }
        if (s == "concede match")
        {
            return Action.ConcedeMatch;
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

    public void Reset(GameObject turnsObject, GameObject enemyTurnsObject, GameObject nextDmd, GameObject enemyNextDmg) 
    {
        friendlyTurnNumber = 0;
        enemyTurnNumber = 0;
        friendlySuddenDeathDamage = 0;
        enemySuddenDeathDamage = 0;

        turnsObject.GetComponent<TextMeshPro>().text = friendlyTurnNumber.ToString();
        nextDmd.GetComponent<TextMeshPro>().text = friendlySuddenDeathDamage.ToString();
        enemyTurnsObject.GetComponent<TextMeshPro>().text = enemyTurnNumber.ToString();
        enemyNextDmg.GetComponent<TextMeshPro>().text = enemySuddenDeathDamage.ToString();
    }

    public void Increment(bool friendly, GameObject turnsObject, GameObject enemyTurnsObject, GameObject nextDmg, GameObject enemyNextDmg)
    {
        if (friendly)
        {
            friendlyTurnNumber += 1;
            if (friendlyTurnNumber >= suddenDeathTurns)
            {
                friendlySuddenDeathDamage += 1;
            }
            turnsObject.GetComponent<TextMeshPro>().text = friendlyTurnNumber.ToString();
            nextDmg.GetComponent<TextMeshPro>().text = friendlySuddenDeathDamage.ToString();

            if (friendlyTurnNumber + 1 >= suddenDeathTurns)
            {
                nextDmg.GetComponent<TextMeshPro>().text = (friendlySuddenDeathDamage + 1).ToString();
            }

            if (enemyTurnNumber + 1 >= suddenDeathTurns)
            {
                enemyNextDmg.GetComponent<TextMeshPro>().text = (enemySuddenDeathDamage + 1).ToString();
            }
        }
        else
        {
            enemyTurnNumber += 1;
            if (enemyTurnNumber >= suddenDeathTurns)
            {
                enemySuddenDeathDamage += 1;
            }
            enemyTurnsObject.GetComponent<TextMeshPro>().text = enemyTurnNumber.ToString();
            enemyNextDmg.GetComponent<TextMeshPro>().text = enemySuddenDeathDamage.ToString();

            if (friendlyTurnNumber + 1 >= suddenDeathTurns)
            {
                nextDmg.GetComponent<TextMeshPro>().text = (friendlySuddenDeathDamage + 1).ToString();
            }

            if (enemyTurnNumber + 1 >= suddenDeathTurns)
            {
                enemyNextDmg.GetComponent<TextMeshPro>().text = (enemySuddenDeathDamage + 1).ToString();
            }
            
        }
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
    // "Round X of 3" - purely derived from gameState.friendlyWins/enemyWins (best-of-3, see
    // CheckGameEnd), not its own tracked state. Optional - left null-safe so this doesn't need a
    // scene change to keep working.
    [SerializeField]
    private GameObject roundObject;

    [SerializeField]
    private GameObject turnsObject;
    [SerializeField]
    private GameObject deckSizeObject;
    [SerializeField]
    private GameObject nextDmgObject;

    [SerializeField]
    private GameObject enemyTurnsObject;
    [SerializeField]
    private GameObject enemyDeckSizeObject;
    [SerializeField]
    private GameObject enemyNextDmgObject;

    [SerializeField]
    private GameObject endTurnButtonObject;
    [SerializeField]
    private GameObject concedeObject;
    [SerializeField]
    private GameObject statsObject;

    [SerializeField]
    private GameObject turnTimeLeft;

    [SerializeField]
    private GameObject myNicknameText;
    [SerializeField]
    private GameObject opponentNicknameText;


    public GameState gameState = new GameState();

    public float secondsBetweenAnimations = 0.5f;
    public bool actionIsHappening = false;

    public bool effectsBlocked = false;
    public List<MinionManager> effectBlockers = new();

    private float timeSinceOpponentResponsed = 0f;
    private readonly float timeSinceOpponentResponsedLimit = 120f;

    // Set by ServerDataProcesser when an opponent action can't be applied safely. Freezes
    // input via CanPerformActions() and shows a banner instead of crashing or corrupting state.
    public bool desyncDetected = false;
    private string desyncReason = "";

    private float gapUnresolvedSeconds = 0f;
    private const float GapBannerDelaySeconds = 5f;
    private const float GapHaltDelaySeconds = 45f;

    public void HaltForDesync(string reason)
    {
        if (desyncDetected)
        {
            return;
        }
        desyncDetected = true;
        desyncReason = reason;
        Debug.LogError("Match halted: " + reason);
    }

    private float pingInterval = 0f;
    private readonly float pingIntervalMax = 25f;

    private int concedeTimes = 0;

    private float turnSecondsMax = 90f;
    private float turnSecondsLeft = 90f;
    // Our own estimate of the opponent's remaining turn time - not authoritative, just mirrors
    // the same turnSecondsMax countdown starting whenever their turn begins. Shown on the same
    // turnTimeLeft display as our own countdown, whichever is currently relevant.
    private float enemyTurnSecondsLeft = 90f;

    public static List<QueueData> eventQueue = new();

    private void Start()
    {
        endTurnButtonObject.SetActive(false);
        turnTimeLeft.SetActive(false);
        concedeObject.SetActive(false);
        statsObject.SetActive(false);
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

        if (myNicknameText != null)
        {
            myNicknameText.GetComponent<TextMeshProUGUI>().text = PlayerProfile.Nickname;
        }
        if (opponentNicknameText != null)
        {
            opponentNicknameText.GetComponent<TextMeshProUGUI>().text = InfoSaver.onlineBattle ? "Opponent" : "Bot";
        }

        StartCoroutine(QueueProcess());
    }

    // The End Turn button stays visible and clickable the whole match now (it used to be
    // deactivated during the opponent's turn, which also hid the ability to concede then) - its
    // label swap ("Opponent's turn") lives in EndTurnButton.cs. Conceding is a separate button
    // (ConcedeButton.cs) that opens a confirmation via RequestConcedeConfirmation(), which itself
    // decides round-concede vs match-concede based on whose turn it is.

    private void Update()
    {
        if (desyncDetected)
        {
            return;
        }

        if (InfoSaver.onlineBattle && ServerDataProcesser.instance != null)
        {
            if (opponentNicknameText != null)
            {
                TextMeshProUGUI opponentNicknameLabel = opponentNicknameText.GetComponent<TextMeshProUGUI>();
                if (opponentNicknameLabel.text != ServerDataProcesser.instance.OpponentNickname)
                {
                    opponentNicknameLabel.text = ServerDataProcesser.instance.OpponentNickname;
                }
            }

            if (ServerDataProcesser.instance.HasUnresolvedGap)
            {
                gapUnresolvedSeconds += Time.deltaTime;
                if (gapUnresolvedSeconds >= GapHaltDelaySeconds && FirebaseDb.IsConnected)
                {
                    // We can reach Firebase fine, so the missing action is very likely stuck
                    // because the opponent's own client crashed/closed, not a transient blip.
                    HaltForDesync("Your opponent's move never arrived - their connection may have been lost.");
                    return;
                }
            }
            else
            {
                gapUnresolvedSeconds = 0f;
            }
        }

        if (!playerTurn)
        {
            // Only a genuinely idle opponent should burn down this timer - not our own dropped
            // connection, and not a delivery in progress that we can still see arriving.
            bool opponentConnectionHealthy = !InfoSaver.onlineBattle || (FirebaseDb.IsConnected &&
                !(ServerDataProcesser.instance != null && ServerDataProcesser.instance.HasUnresolvedGap));
            if (opponentConnectionHealthy)
            {
                timeSinceOpponentResponsed += Time.deltaTime;
            }

            if (timeSinceOpponentResponsed >= timeSinceOpponentResponsedLimit)
            {
                timeSinceOpponentResponsed = 0f;
                Debug.Log("Your opponent left");
                StartCoroutine(EndGame(true));
            }

            // This is only our own estimate (their end-of-turn message may lag a little behind
            // it) - clamp at zero rather than counting into negative time.
            enemyTurnSecondsLeft = Mathf.Max(0f, enemyTurnSecondsLeft - Time.deltaTime);
            string enemyStrTime = FormatTurnTime(enemyTurnSecondsLeft);
            foreach (Transform child in turnTimeLeft.transform)
            {
                child.GetComponent<TextMeshPro>().text = enemyStrTime;
            }
        }
        else
        {
            turnSecondsLeft -= Time.deltaTime;
            if (turnSecondsLeft < 0f)
            {
                foreach (Transform child in turnTimeLeft.transform)
                {
                   child.GetComponent<TextMeshPro>().text = "00:00";
                }
                EndTurn(true);
            }
            else
            {
                string strTime = FormatTurnTime(turnSecondsLeft);
                foreach (Transform child in turnTimeLeft.transform)
                {
                   child.GetComponent<TextMeshPro>().text = strTime;
                }
            }
        }
    }

    private static string FormatTurnTime(float secondsLeft)
    {
        int seconds = ((int)secondsLeft) % 60;
        int minutes = ((int)secondsLeft) / 60;
        string secondsStr = seconds.ToString();
        if (seconds < 10)
        {
            secondsStr = "0" + secondsStr;
        }
        return "0" + minutes.ToString() + ":" + secondsStr;
    }

    // Plain IMGUI overlay - deliberately not scene-authored UI, so it needs no Canvas/TMP
    // wiring and carries no risk of touching the existing .unity scene.
    private void OnGUI()
    {
        // Not gated on InfoSaver.onlineBattle - concede confirmations make just as much sense
        // against a bot/local opponent as they do online.
        if (pendingRoundConcedeConfirmation)
        {
            ConfirmationBanner.Draw(new Color(0.6f, 0.1f, 0.1f, 0.9f), "Concede this round?",
                "Yes, concede round", "Cancel", ConcedeRound, CancelRoundConcede,
                "Concede match instead", ConcedeMatchFromRoundPrompt);
            return;
        }

        if (pendingMatchConcedeConfirmation)
        {
            ConfirmationBanner.Draw(new Color(0.6f, 0.1f, 0.1f, 0.9f), "Concede the ENTIRE MATCH (not just this round)?",
                "Yes, concede match", "Cancel", ConcedeMatch, CancelMatchConcede);
            return;
        }

        if (!InfoSaver.onlineBattle)
        {
            return;
        }

        if (desyncDetected)
        {
            DrawBanner(new Color(0.6f, 0.1f, 0.1f, 0.9f), "Match halted: " + desyncReason);
            float width = 220f;
            Rect buttonRect = new Rect((Screen.width - width) / 2f, 70f, width, 40f);
            if (GUI.Button(buttonRect, "Return to Main Menu"))
            {
                CleanUpOnlineMatch();
                SceneManager.LoadScene("MainMenu");
            }
            return;
        }

        if (!FirebaseDb.IsConnected)
        {
            DrawBanner(new Color(0.6f, 0.5f, 0.0f, 0.9f), "Connection lost - reconnecting...");
            return;
        }

        if (ServerDataProcesser.instance != null && ServerDataProcesser.instance.HasUnresolvedGap
            && gapUnresolvedSeconds > GapBannerDelaySeconds)
        {
            DrawBanner(new Color(0.2f, 0.2f, 0.6f, 0.85f), "Waiting for opponent's move...");
        }
    }

    private void DrawBanner(Color color, string message)
    {
        float width = Mathf.Min(500f, Screen.width - 40f);
        Rect rect = new Rect((Screen.width - width) / 2f, 10f, width, 50f);

        // GUI.Box multiplies its own semi-transparent default skin texture by GUI.color, so the
        // requested alpha ended up compounded (banner looked far more see-through than the color
        // asked for). Drawing a plain white texture instead makes the alpha we pass the actual
        // final alpha.
        Color previousColor = GUI.color;
        Color opaqueBacking = color;
        opaqueBacking.a = Mathf.Max(color.a, 0.95f);
        GUI.color = opaqueBacking;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(rect, message, style);
        GUI.color = previousColor;
    }

    private IEnumerator QueueProcess()
    {
        while (true)
        {
            if (actionIsHappening)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else if (GameController.eventQueue.Count == 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                QueueData tmpQueue = GameController.eventQueue[0];
                GameController.eventQueue.RemoveAt(0);
                tmpQueue.ApplyEffect();
                yield return new WaitForSeconds(secondsBetweenAnimations);
            }
        
        }
    }

    public static bool CanPerformActions()
    {
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameController.actionIsHappening || GameController.eventQueue.Count > 0 || gameController.desyncDetected)
        {
            return false;
        }
        return true;
    }

    public void ReceivePing()
    {
        timeSinceOpponentResponsed = 0;
    }

    public bool EffectsAreBlocked()
    {
        effectsBlocked = false;
        effectBlockers = new();
        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null && minion.GetCardStats().blockEffects)
            {
                effectsBlocked = true;
                effectBlockers.Add(minion);
                minion.circleMyamsarObject.SetActive(true);
            }
        }

        foreach (BoardManager.Slot slot in boardManager.enemySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null && minion.GetCardStats().blockEffects)
            {
                effectsBlocked = true;
                effectBlockers.Add(minion);
                minion.circleMyamsarObject.SetActive(true);
            }
        }
        return effectsBlocked;
    }

    // True while a minion with blockSpells (e.g. Slogturtle) is alive on either side. Symmetric
    // by design - both players independently compute this from the same synced board state, so
    // it's enforced purely by gating the local casting UI (CardManager.cs), with no separate
    // check needed on the receiving side of a network message.
    public bool SpellsAreBlocked()
    {
        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null && minion.GetCardStats().blockSpells)
            {
                return true;
            }
        }
        foreach (BoardManager.Slot slot in boardManager.enemySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null && minion.GetCardStats().blockSpells)
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator CheckBoardEffects()
    {
        while (true)
        {
            effectsBlocked = false;
            effectBlockers = new();
            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardStats().blockEffects)
                {
                    effectsBlocked = true;
                    effectBlockers.Add(minion);
                    minion.circleMyamsarObject.SetActive(true);
                }
            }

            foreach (BoardManager.Slot slot in boardManager.enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardStats().blockEffects)
                {
                    effectsBlocked = true;
                    effectBlockers.Add(minion);
                    minion.circleMyamsarObject.SetActive(true);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void StartGame()
    {
        UpdateRoundLabel();
        endTurnButtonObject.SetActive(true);
        turnTimeLeft.SetActive(true);
        concedeObject.SetActive(true);
        statsObject.SetActive(true);
        if (InfoSaver.opponentHash <= InfoSaver.myHash)
        {
            playerTurn = true;
            handManager.SetCanPlayCard(true);
            handManager.SetCanCycleCard(true);
            CursorController.cursorState = CursorController.CursorStates.Free;
            gameState.Increment(friendly:true, turnsObject, enemyTurnsObject, nextDmgObject, enemyNextDmgObject);
            boardManager.DealSuddenDeathDamage(friendly:true, gameState.GetSuddenDeathDamage(friendly:true));
            turnSecondsLeft = turnSecondsMax;
        }
        else
        {
            playerTurn = false;
            handManager.SetCanPlayCard(false);
            handManager.SetCanCycleCard(false);
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;
            gameState.Increment(friendly:false, turnsObject, enemyTurnsObject, nextDmgObject, enemyNextDmgObject);
            boardManager.DealSuddenDeathDamage(friendly:false, gameState.GetSuddenDeathDamage(friendly:false));
            enemyTurnSecondsLeft = turnSecondsMax;
        }
        if (gameState.StartOfTheGame())
        {
            DeckManager.ResetOpponentsDeck();
        }
        StartCoroutine(ServerDataProcesser.instance.ObtainData());
        //StartCoroutine(CheckBoardEffects());
    }

    private Coroutine startTurnRoutine;
    private Coroutine endTurnRoutine;

    public void StartTurn(bool friendly, bool hataponJustDied=false)
    {
        if (friendly)
        {
            turnSecondsLeft = turnSecondsMax;
        }
        else
        {
            if (ServerDataProcesser.instance.bot != null)
            {
                ServerDataProcesser.instance.bot.botActive = true;
                ServerDataProcesser.instance.bot.cardPlayed = false;
            }
            enemyTurnSecondsLeft = turnSecondsMax;
        }

        // A concede can trigger a round transition (-> StartTurn) while the opponent's own
        // IenumStartTurn from their still-in-progress turn is mid-flight - if that stale
        // coroutine resumes afterward, it re-applies SetCanAttack(true) (turning the ready
        // outline on) to whatever minion now occupies that board slot, which by then is the
        // NEW round's Hatapon. Stopping it here means it never reaches that point.
        if (startTurnRoutine != null)
        {
            StopCoroutine(startTurnRoutine);
        }
        startTurnRoutine = StartCoroutine(IenumStartTurn(friendly, hataponJustDied));
    }

    IEnumerator IenumStartTurn(bool friendly, bool hataponJustDied=false)
    {
        while (actionIsHappening)
        {
            yield return new WaitForSeconds(0.5f);
        }
        UpdateDecks();
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
        handManager.SetCanCycleCard(friendly);

        gameState.Increment(friendly, turnsObject, enemyTurnsObject, nextDmgObject, enemyNextDmgObject);
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
                    if (minion.GetCardStats().poisoned)
                    {
                        minion.LoseLife(1);
                    }
                }
            }

            if (!EffectsAreBlocked())
            {
                order.Reverse();
                foreach (MinionManager minion in order)
                {
                    CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                    if (thisStartTurnEvent != null)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.StartTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.friendlySlots;
                        newEvent.enemySlots = boardManager.enemySlots;
                        GameController.eventQueue.Insert(0, newEvent);
                        /*
                        // Start turn event
                        StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                        do {
                            yield return new WaitForSeconds(secondsBetweenAnimations);
                        } while(actionIsHappening);
                        */
                    }
                }
            }
            else
            {
                effectBlockers.Reverse();
                foreach (MinionManager minion in effectBlockers)
                {
                    if (minion.GetFriendly())
                    {
                        CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                        if (thisStartTurnEvent != null)
                        {
                            QueueData newEvent = new();
                            newEvent.actionType = QueueData.ActionType.StartTurn;
                            newEvent.hostUnit = minion;
                            newEvent.index = minion.GetIndex();
                            newEvent.friendlySlots = boardManager.friendlySlots;
                            newEvent.enemySlots = boardManager.enemySlots;
                            GameController.eventQueue.Insert(0, newEvent);

                            //while (GameController.eventQueue.Count > 0)
                            //{
                            //    yield return new WaitForSeconds(secondsBetweenAnimations);
                            //}
                            // End turn event
                            //StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                            //do {
                            //    yield return new WaitForSeconds(secondsBetweenAnimations);
                            //} while(actionIsHappening);
                        }
                    }
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
                    if (minion.GetCardStats().poisoned)
                    {
                        minion.LoseLife(1);
                    }
                }
            }

            if (!EffectsAreBlocked())
            {
                order.Reverse();
                foreach (MinionManager minion in order)
                {
                    CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                    if (thisStartTurnEvent != null)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.StartTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.enemySlots;
                        newEvent.enemySlots = boardManager.friendlySlots ;
                        GameController.eventQueue.Insert(0, newEvent);
                        /*while (GameController.eventQueue.Count > 0)
                        {
                            yield return new WaitForSeconds(secondsBetweenAnimations);
                        }
                        */
                        // End turn event
                        //StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                        //do {
                        //    yield return new WaitForSeconds(secondsBetweenAnimations);
                        //} while(actionIsHappening);
                    }
                }
            }
            else
            {
                effectBlockers.Reverse();
                foreach (MinionManager minion in effectBlockers)
                {
                    if (!minion.GetFriendly())
                    {
                        CardManager.EndTurnEvent thisStartTurnEvent = minion.GetCardStats().startTurnEvent;
                        if (thisStartTurnEvent != null)
                        {
                            QueueData newEvent = new();
                            newEvent.actionType = QueueData.ActionType.StartTurn;
                            newEvent.hostUnit = minion;
                            newEvent.index = minion.GetIndex();
                            newEvent.friendlySlots = boardManager.enemySlots;
                            newEvent.enemySlots = boardManager.friendlySlots;
                            GameController.eventQueue.Insert(0, newEvent);
                            /*while (GameController.eventQueue.Count > 0)
                            {
                                yield return new WaitForSeconds(secondsBetweenAnimations);
                            }*/
                            // End turn event
                            //StartCoroutine(thisStartTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                            //do {
                            //    yield return new WaitForSeconds(secondsBetweenAnimations);
                            //} while(actionIsHappening);
                        }
                    }
                }
            }
            
        }
        if (!friendly && !hataponJustDied)
        {
            ServerDataProcesser.instance.EndTurn();
            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                if (!slot.GetFree())
                {
                    slot.GetConnectedMinion().SetAbilityToAttack(false);
                }
            }

        }
        yield return null;
    }

    public void EndTurn(bool friendly)
    {
        GameController.playerTurn = !friendly;

        if (endTurnRoutine != null)
        {
            StopCoroutine(endTurnRoutine);
        }
        endTurnRoutine = StartCoroutine(IenumEndTurn(friendly));
    }

    private IEnumerator IenumEndTurn(bool friendly)
    {
        do {
            yield return new WaitForSeconds(secondsBetweenAnimations);
        } while(actionIsHappening);
        if (friendly)
        {
            handManager.CheckEphemeral();
            handManager.SetCanPlayCard(false);
            handManager.SetCanCycleCard(false);
            CursorController.cursorState = CursorController.CursorStates.EnemyTurn;

            List<MinionManager> order = new List<MinionManager>();

            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    order.Add(minion);
                    minion.SetState(MinionManager.MinionState.Free);
                    minion.SetAbilityToAttack(false);
                }
            }
            if (!EffectsAreBlocked())
            {
                order.Reverse();
                foreach (MinionManager minion in order)
                {
                    CardManager.EndTurnEvent thisEndTurnEvent = minion.GetCardStats().endTurnEvent;
                    if (thisEndTurnEvent != null)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.EndTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.friendlySlots;
                        newEvent.enemySlots = boardManager.enemySlots;
                        GameController.eventQueue.Insert(0, newEvent);
                        /*while (GameController.eventQueue.Count > 0)
                        {
                            yield return new WaitForSeconds(secondsBetweenAnimations);
                        }*/
                        // End turn event 
                        //StartCoroutine(thisEndTurnEvent(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                        //do {
                        //    yield return new WaitForSeconds(secondsBetweenAnimations);
                        //} while(actionIsHappening);
                    }
                    int addIdx = 0;
                    foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.EndTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.friendlySlots;
                        newEvent.enemySlots = boardManager.enemySlots;
                        newEvent.additionalEndTurnIdx = addIdx;
                        GameController.eventQueue.Insert(0, newEvent);
                        addIdx += 1;
                        // End turn event
                        //StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots));
                        //do {
                        //    yield return new WaitForSeconds(secondsBetweenAnimations);
                        //} while(actionIsHappening);
                    }
                }
            }
        }
        else
        {
            //handManager.CheckEphemeral();
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
            if (!EffectsAreBlocked())
            {
                order.Reverse();
                foreach (MinionManager minion in order)
                {
                    CardManager.EndTurnEvent thisEndTurnEvent = minion.GetCardStats().endTurnEvent;
                    if (thisEndTurnEvent != null)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.EndTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.enemySlots;
                        newEvent.enemySlots = boardManager.friendlySlots;
                        GameController.eventQueue.Insert(0, newEvent);
                        /*while (GameController.eventQueue.Count > 0)
                        {
                            yield return new WaitForSeconds(secondsBetweenAnimations);
                        }*/
                        // End turn event
                        //StartCoroutine(thisEndTurnEvent(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                        //do {
                        //    yield return new WaitForSeconds(secondsBetweenAnimations);
                        //} while(actionIsHappening);
                    }
                    int addIdx = 0;
                    foreach (CardManager.EndTurnEvent addEndTurn in minion.GetCardStats().additionalEndTurnEvents)
                    {
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.EndTurn;
                        newEvent.hostUnit = minion;
                        newEvent.index = minion.GetIndex();
                        newEvent.friendlySlots = boardManager.enemySlots;
                        newEvent.enemySlots = boardManager.friendlySlots;
                        newEvent.additionalEndTurnIdx = addIdx;
                        GameController.eventQueue.Insert(0, newEvent);
                        addIdx += 1;
                        // End turn event
                        //StartCoroutine(addEndTurn(minion.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots));
                        //do {
                        //    yield return new WaitForSeconds(secondsBetweenAnimations);
                        //} while(actionIsHappening);
                    }
                }
            }
        }
        do {
            yield return new WaitForSeconds(secondsBetweenAnimations);
        } while(GameController.eventQueue.Count > 0);
        StartTurn(!friendly);
        yield return null;
    }

    public void EndRound(bool friendly)
    {
        StartCoroutine(OnEndRound(friendly));
    }

    private void SetNumberOfChests(bool friendlyVictory)
    {
        InfoSaver.victory = friendlyVictory;
        if (friendlyVictory)
        {
            if (InfoSaver.onlineBattle)
            {
                InfoSaver.chests = 3;
            }
            else
            {
                if (!InfoSaver.botDefeated[InfoSaver.botLevel + 1])
                {
                    InfoSaver.chests = 10;
                    InfoSaver.botDefeated[InfoSaver.botLevel + 1] = true;
                    List<bool> botStats = new List<bool>();
                    for (int i = 0; i < InfoSaver.botDefeated.Length; ++i)
                    {
                        botStats.Add(InfoSaver.botDefeated[i]);
                    }
                    SaveSystem.SaveBotStats(botStats);
                }
                else
                {
                    if (InfoSaver.botLevel == 2)
                    {
                        InfoSaver.chests = 1;
                    }
                    else
                    {
                        InfoSaver.chests = 0;
                    }
                }
            }
        }
        else
        {
            if (InfoSaver.onlineBattle)
            {
                InfoSaver.chests = 1;
            }
            else
            {
                InfoSaver.chests = 0;
            }
        }
    }

    private void CleanUpOnlineMatch()
    {
        if (InfoSaver.onlineBattle)
        {
            // Best-effort cleanup - don't let a slow/failed delete strand the player here.
            StartCoroutine(FirebaseDb.Delete("matches/" + ServerDataProcesser.MatchId()));
        }
    }

    public IEnumerator EndGame(bool friendlyVictory)
    {
        boardManager.ClearBoard();
        yield return new WaitForSeconds(3f);
        SetNumberOfChests(friendlyVictory);
        CleanUpOnlineMatch();
        if (InfoSaver.chests > 0)
        {
            SceneManager.LoadScene("OpenChest");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
        yield return null;
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
            SetNumberOfChests(friendlyVictory);
            CleanUpOnlineMatch();
            if (InfoSaver.chests > 0)
            {
                SceneManager.LoadScene("OpenChest");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }

            yield return null;
        }
        else
        {
            boardManager.ClearBoard();
            handManager.StartRoundActions();
            
            gameState.Reset(turnsObject, enemyTurnsObject, nextDmgObject, enemyNextDmgObject);
            LogController.instance.ClearLog();
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
        UpdateRoundLabel();
    }

    private void UpdateRoundLabel()
    {
        if (roundObject == null)
        {
            return;
        }
        int round = gameState.friendlyWins + gameState.enemyWins + 1;
        roundObject.GetComponent<TextMeshProUGUI>().text = "Round " + round + " of 3";
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
            if (DeckManager.opponentsDeck.Count > 0)
            {
                //DeckManager.opponentDeckSize -= 1;
                couldDraw = true;
                //Debug.Log("Opponent draws");
                //Debug.Log(DeckManager.opponentsDeck[0]);
                DeckManager.opponentsDeck.RemoveAt(0);
            }
            else 
            {
                couldDraw = false;
                //DeckManager.opponentDeckSize = 0;
            }
        }
        else
        {
            if (DeckManager.GetPlayDeckSize() > 0)
            {
                couldDraw = true;
            }
            else
            {
                couldDraw = false;
            }
        }
        //Debug.Log("Me: " + DeckManager.GetDeckSize().ToString());
        //Debug.Log("Opp: " + DeckManager.opponentsDeck.Count.ToString());

        deckSizeObject.GetComponent<TextMeshPro>().text = DeckManager.GetPlayDeckSize().ToString();
        enemyDeckSizeObject.GetComponent<TextMeshPro>().text = DeckManager.opponentsDeck.Count.ToString();

        return couldDraw;
    }

    public void UpdateDecks()
    {
        deckSizeObject.GetComponent<TextMeshPro>().text = DeckManager.GetPlayDeckSize().ToString();
        enemyDeckSizeObject.GetComponent<TextMeshPro>().text = DeckManager.opponentsDeck.Count.ToString();
    }

    // Buttons
    public void EndTurnButton()
    {
        if (playerTurn && CursorController.cursorState == CursorController.CursorStates.Free)
        {
            EndTurn(true);
        }
    }

    // Any caller (the Concede button, etc.) routes through here rather than conceding directly -
    // both the round-concede and match-concede paths now go through a Yes/Cancel confirmation
    // (see OnGUI) instead of a press-and-hold gesture, so a single click can't end anything by
    // accident. Which confirmation shows depends on whose turn it is: on your own turn conceding
    // only costs the current round; on the opponent's turn there's no continuing round for an
    // in-flight opponent action to race against, so it ends the whole match instead.
    public void RequestConcedeConfirmation()
    {
        // Free (my turn, idle) or EnemyTurn (their turn, idle) are both fine - only reject
        // while mid-interaction with something else (Hold/Select/ChooseOption).
        bool cursorAllowsConcede = CursorController.cursorState == CursorController.CursorStates.Free ||
            CursorController.cursorState == CursorController.CursorStates.EnemyTurn;
        if (!cursorAllowsConcede)
        {
            return;
        }

        if (playerTurn)
        {
            pendingRoundConcedeConfirmation = true;
        }
        else
        {
            pendingMatchConcedeConfirmation = true;
        }
    }

    public bool pendingRoundConcedeConfirmation = false;

    public void CancelRoundConcede()
    {
        pendingRoundConcedeConfirmation = false;
    }

    public void ConcedeRound()
    {
        pendingRoundConcedeConfirmation = false;
        if (concedeTimes < 3)
        {
            concedeTimes += 1;
            CardManager concedeCard = handManager.GenerateCard(CardTypes.Concede, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            StartCoroutine(concedeCard.GetCardStats().spell(new List<int>(), boardManager.enemySlots, boardManager.friendlySlots));
            ServerDataProcesser.instance.CastSpell(concedeCard, new List<int>());
            concedeCard.DestroyCard();
        }
    }

    // "Concede match instead" option on the round-concede prompt - lets a player give up the
    // whole match even on their own turn, not just when conceding mid-opponent-turn forces it.
    public void ConcedeMatchFromRoundPrompt()
    {
        pendingRoundConcedeConfirmation = false;
        ConcedeMatch();
    }

    public bool pendingMatchConcedeConfirmation = false;

    public void CancelMatchConcede()
    {
        pendingMatchConcedeConfirmation = false;
    }

    public void ConcedeMatch()
    {
        pendingMatchConcedeConfirmation = false;
        if (InfoSaver.onlineBattle)
        {
            ServerDataProcesser.instance.ConcedeMatch();
        }
        StartCoroutine(EndGame(false));
    }
}
