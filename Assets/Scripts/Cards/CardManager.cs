using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public enum Runes
{
    Neutral,
    Spear,
    Shield,
    Bow
}

public class CardManager : MonoBehaviour
{
    static bool DEBUG = false;
    public delegate IEnumerator EndTurnEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator Spell(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator OnCycleEvent(List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator OnAttackEvent(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator OnPlayEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator OnCycleOtherEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate IEnumerator OnDeathEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null, CardStats thisStats = null);
    public delegate bool CheckSpellTarget(int target = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate bool CheckSpellTargets(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public class CardStats
    {
        public string name = "default-name";
        public string description = "default-description";
        public int power = 1;
        public bool isSpell = false;
        public bool canAttack = true;
        public bool canDealDamage = true;
        public bool hasHaste = false;
        public bool pacifism = false;
        public bool hasShield = false;
        public bool hasGreatshield = false;
        public bool limitedVision = false;
        public bool megaVision = false;
        public int fixedPower = -1;
        public EndTurnEvent endTurnEvent = null;
        public EndTurnEvent startTurnEvent = null;
        public Spell spell = null;
        public int numberOfTargets = 0;
        public CheckSpellTarget checkSpellTarget = null;
        public CheckSpellTargets checkSpellTargets = null;
        public bool hasOnPlaySpell = false;
        public OnPlayEvent afterPlayEvent = null;
        public EndTurnEvent onDamageEvent = null;
        public OnDeathEvent onDeathEvent = null;
        public OnCycleEvent onCycleEvent = null;
        public OnCycleOtherEvent onCycleOtherEvent = null;
        public OnAttackEvent onAttackEvent = null;
        public bool hasAfterPlayEvent = false;
        public bool isStatic = false;
        public int healthCost = 0;
        public List<CardTypes> connectedCards = new List<CardTypes>();
        public bool flying = false;
        public bool dummyTarget = false;
        public List<EndTurnEvent> additionalEndTurnEvents = new List<EndTurnEvent>();
        public int armor = 0;
        public bool hasOnDeath = false;
        public CardStats savedStats = null;
        public List<MinionManager> connectedMinions = new List<MinionManager>();
        public int damageToHost = -1;
        public List<Runes> runes = new List<Runes>();
        public string imagePath = "500x500";
        public bool legendary = false;
        public int nameSize = 6;
        public int descriptionSize = 4;
        public bool poisoned = false;
        public bool hexproof = false;
        public bool cycling = false;
        public bool blockEffects = false;
        public int ephemeral = -1;
        public bool giveCyclingToCardsInHand = false;
        public int cardsDrawnByThis = 0;
        public List<MinionManager> lifelinkedTo = new();
        public int lifelinkMeTo = -1;
        public List<string> additionalKeywords = new();
        public List<string> additionalRules = new();
        public bool suppressOnPlay = false;
        public string artistName = "noone (stolen)";
        public List<CardTypes> relevantCards = new();
        public string onPlaySound = null;
        public string onDeathSound = null;
       

        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>("Images/" + this.imagePath);
        }

        public bool HaveCycling()
        {
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (minion.GetCardStats().giveCyclingToCardsInHand)
                    {
                        return true;
                    }
                }
            }
            return cycling;
        }

        public int GetDevotion(Runes kind)
        {
            int count = 0;
            foreach (Runes rune in runes)
            {
                if (rune == kind)
                {
                    count += 1;
                }
            }
            return count;
        }

        public CardStats CopyStats()
        {
            CardStats newStats = new()
            {
                isSpell = this.isSpell,
                canAttack = this.canAttack,
                canDealDamage = this.canDealDamage,
                hasHaste = this.hasHaste,
                hasShield = this.hasShield,
                hasGreatshield = this.hasGreatshield,
                limitedVision = this.limitedVision,
                megaVision = this.megaVision,
                fixedPower = this.fixedPower,
                endTurnEvent = this.endTurnEvent,
                startTurnEvent = this.startTurnEvent,
                spell = this.spell,
                numberOfTargets = this.numberOfTargets,
                checkSpellTarget = this.checkSpellTarget,
                checkSpellTargets = this.checkSpellTargets,
                hasOnPlaySpell = this.hasOnPlaySpell,
                afterPlayEvent = this.afterPlayEvent,
                onDeathEvent = this.onDeathEvent,
                hasAfterPlayEvent = this.hasAfterPlayEvent,
                isStatic = this.isStatic,
                healthCost = this.healthCost,
                hexproof = this.hexproof,
                cycling = this.cycling,
                onCycleEvent = this.onCycleEvent,
                onCycleOtherEvent = this.onCycleOtherEvent,
                onAttackEvent = this.onAttackEvent,
                blockEffects = this.blockEffects,
                ephemeral = this.ephemeral,
                onDamageEvent = this.onDamageEvent,
                giveCyclingToCardsInHand = this.giveCyclingToCardsInHand,
                cardsDrawnByThis = this.cardsDrawnByThis,
                lifelinkedTo = this.lifelinkedTo,
                lifelinkMeTo = this.lifelinkMeTo,

                connectedCards = new List<CardTypes>()
            };
            foreach (CardTypes ct in this.connectedCards)
            {
                newStats.connectedCards.Add(ct);
            }

            foreach (MinionManager mn in this.lifelinkedTo)
            {
                newStats.lifelinkedTo.Add(mn);
            }


            //newStats.connectedCards = new List<CardTypes>(this.connectedCards);

            newStats.flying = this.flying;
            newStats.dummyTarget = this.dummyTarget;
            
            newStats.additionalEndTurnEvents = new List<EndTurnEvent>();
            foreach (EndTurnEvent ev in this.additionalEndTurnEvents)
            {
                newStats.additionalEndTurnEvents.Add(ev);
            }
           
            newStats.armor = this.armor;
            newStats.hasOnDeath = this.hasOnDeath;
            newStats.savedStats = this.savedStats;

            newStats.connectedMinions = new List<MinionManager>();
            foreach (MinionManager mm in this.connectedMinions)
            {
                newStats.connectedMinions.Add(mm);
            }

            newStats.damageToHost = this.damageToHost;

            newStats.runes = new List<Runes>();
            foreach (Runes rn in this.runes)
            {
                newStats.runes.Add(rn);
            }

            newStats.imagePath = this.imagePath;

            newStats.legendary = this.legendary;

            newStats.nameSize = this.nameSize;
            newStats.descriptionSize = this.descriptionSize;
            newStats.onPlaySound = this.onPlaySound;
            newStats.onDeathSound = this.onDeathSound;

            return newStats;
        }
    }

    public enum CardState
    {
        display, // Just like an image. Use to show in collection
        inHand,
        Drawing,
        selected,
        selectingTargets, // For spells with more than one target
        asOption,
        opponentPlayed,
        opponentHolding,
        hilightOver,
        Mill,
        toMill,
        ShuffleIntoDeck,
        alreadyPlayed,
        openedFromPack,
    }

    public GameObject powerObject;
    public GameObject powerSquare;
    public GameObject imageObject;
    public GameObject nameObject;
    public GameObject backObject;
    public GameObject descriptionObject;
    public GameObject nameOutline;
    public GameObject heartObject;
    public GameObject numberOfCardsObject;
    public GameObject numberOfCardsText;
    public List<GameObject> runeObjects;

    public Material spearMaterial;
    public Material shieldMaterial;
    public Material bowMaterial;
    public Material neutralMaterial;
    public Material multiclassMaterial;
    
    public float normalScale = 1f;
    public float selectedScale = 0.75f;
    public float selectedZ = -2f;
    public float selectedY = -3.5f;
    public MinionManager hostMinion;
    public float destroyTimer;

    private CardState cardState = CardState.inHand;
    private CardTypes cardType;
    private CardStats cardStats;
    private int power = 0;
    private string cardName = "default-card";
    private string cardDescription = "no translation needed";
    
    private Vector3 positionInHand;
    private int indexInHand = 0;
    private bool mouseOver = false;
    public List<int> spellTargets;
    private BoardManager.Slot slotToPlay;
    private float curRotation;
    
    private BoardManager boardManager;
    private HandManager handManager;
    private GameController gameController;

    private Vector3 drawEndPosition;
    private Vector3 playEndPosition;
    private Vector3 MillPosition;

    public List<Arrow> arrowList = null;
    private Arrow curArrow = null;

    public GameObject infoPrefab;
    private float secondsHold = 0f;
    private bool cardIsPlayed = false;

    public float rotationFromPack = 180f;
    public float rotationFromPackSpeed = 25f;

    public int numberOfCopies = 0;

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Game")
        {
            boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            gameController = GameObject.Find("GameController").GetComponent<GameController>();

            drawEndPosition = GameObject.Find("DrawnCardDisplay").transform.position;
            playEndPosition = GameObject.Find("PlayedCardPosition").transform.position;
            MillPosition = GameObject.Find("MillPosition").transform.position;
            numberOfCardsObject.SetActive(false);
        }
        else if (scene.name == "OpenChest")
        {
            numberOfCardsText.GetComponent<TextMeshPro>().text = "1x";
        }
    }

    private BoardManager.Slot SelectClosestSlot()
    {
        BoardManager.Slot closestSlot = null;
        
        FollowMouse();

        // Dehighlight all slots
        boardManager.cyclingSlot.Highlight(false);
        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            slot.Highlight(false);
        }

        float minDistance = -1f;
        if (handManager.CanPlayCard())
        {
            // Iterate over friendly slots
            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                float distance = Mathf.Abs(transform.position.x - slot.GetPosition().x);
                if (minDistance == -1f || minDistance > distance)
                {
                    minDistance = distance;
                    closestSlot = slot;
                }
            }
        }

        if (cardStats.HaveCycling() && handManager.CanCycleCard())
        {
            // Check if this card can be cycled
            float distanceToCycle = Mathf.Abs(transform.position.x - boardManager.cyclingSlot.GetPosition().x);
            if (minDistance == -1f || minDistance > distanceToCycle || (distanceToCycle < 0.8f * minDistance && transform.position.y < 4f))
            {
                closestSlot = boardManager.cyclingSlot;
            }
        }

        if (closestSlot == boardManager.cyclingSlot)
        {
            closestSlot.Highlight(true);
        }
        else if (closestSlot != null && closestSlot.GetFree())
        {
            closestSlot.Highlight(true);
        }
        else
        {
            closestSlot = null;
        }

        return closestSlot;
    }

    private void FollowMouse(bool scale=false)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Quaternion toRotation = Quaternion.Euler((mousePosition.y - transform.position.y) * 25, -(mousePosition.x - transform.position.x) * 25, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 30.0f);

        Vector3 toPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        toPosition = new Vector3(toPosition.x, toPosition.y, selectedZ);
        transform.position = Vector3.Lerp(transform.position, toPosition, Time.deltaTime * 30.0f);

        if (scale)
        {
            transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
        }
    }

    private void CycleProcedure()
    {
        handManager.RemoveCard(GetIndexIHand());
        handManager.SetCanCycleCard(false);
        CursorController.cursorState = CursorController.CursorStates.Free;
        boardManager.cyclingSlot.Highlight(false);
        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            slot.Highlight(false);
        }
        if (cardStats.onCycleEvent != null)
        {
            QueueData newEvent = new();
            newEvent.actionType = QueueData.ActionType.OnCycle;
            newEvent.hostCard = this;

            newEvent.friendlySlots = boardManager.friendlySlots;
            newEvent.enemySlots = boardManager.enemySlots;

            GameController.eventQueue.Insert(0, newEvent);
            // On cycle
            //StartCoroutine(cardStats.onCycleEvent(boardManager.enemySlots, boardManager.friendlySlots));
        }
        // Cycle card
        StartCoroutine(boardManager.CycleCard(this));
        handManager.DrawCard();
    }

    private void PlayCard(BoardManager.Slot closestSlot=null, bool spell=false)
    {
        if (!cardIsPlayed)
        {
            cardIsPlayed = true;
            cardState = CardState.alreadyPlayed;
        }
        else
        {
            return;
        }
        StartCoroutine(IenumPlayCard(closestSlot, spell));
    }

    private IEnumerator IenumPlayCard(BoardManager.Slot closestSlot=null, bool spell=false)
    {
        boardManager.battlecryTrigger = true;
        handManager.RemoveCard(GetIndexIHand());
        handManager.SetCanPlayCard(false);
        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            slot.Highlight(false);
        }
        boardManager.cyclingSlot.Highlight(false);
        CursorController.cursorState = CursorController.CursorStates.Free;
        if (spell)
        {
            ServerDataProcesser.instance.CastSpell(this, spellTargets);
            // Cast spell
            QueueData newEvent = new();
            newEvent.actionType = QueueData.ActionType.CastSpell;
            newEvent.thisStats = cardStats;
            newEvent.hostCard = this;
            newEvent.targets = spellTargets;
            newEvent.friendlySlots = boardManager.friendlySlots;
            newEvent.enemySlots = boardManager.enemySlots;
            GameController.eventQueue.Insert(0, newEvent);
            //StartCoroutine(cardStats.spell(spellTargets, boardManager.enemySlots, boardManager.friendlySlots));
            if (cardStats.dummyTarget)
            {
                int _old = spellTargets[0];
                spellTargets = new List<int>();
                spellTargets.Add(_old);
            }
            else
            {
                spellTargets = new List<int>();
            }
            if (cardStats.isSpell)
            {
                DestroyCard();
            }
            else
            {
                while (!GameController.CanPerformActions())
                {
                    yield return new WaitForSeconds(0.3f);
                }
                boardManager.PlayCard(this, slotToPlay);
            }
        }
        else
        {
            boardManager.PlayCard(this, closestSlot);
        }        
        boardManager.battlecryTrigger = false;
        yield return null;
    }

    void Update()
    {
        switch (cardState) 
        {
            case CardState.inHand:
                if (!HandManager.mulliganing && mouseOver && (CursorController.cursorState == CursorController.CursorStates.Free ||
                                    CursorController.cursorState == CursorController.CursorStates.EnemyTurn))
                {
                    transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(positionInHand.x, selectedY, selectedZ), 30f * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0, 0, 0);

                    if (Input.GetMouseButtonDown(0) && (handManager.CanPlayCard() || (handManager.CanCycleCard() && cardStats.HaveCycling())))
                    {
                        if (cardStats.isSpell && cardStats.numberOfTargets > 0 && !cardStats.HaveCycling())
                        {
                            cardState = CardState.selectingTargets;
                        }
                        else
                        {
                            cardState = CardState.selected;
                        }
                        CursorController.cursorState = CursorController.CursorStates.Select;
                        spellTargets = new List<int>();
                    }
                }
                else
                {
                    transform.localScale = new Vector3(normalScale, normalScale, 1f);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(positionInHand.x, positionInHand.y, positionInHand.z), 10f * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0, 0, curRotation);
                }
                break;

            case CardState.Drawing:
                
                float spd, dst;
                if (HandManager.mulliganing)
                {
                    spd = 20f;
                    dst = 0.25f;   
                }
                else
                {
                    spd = 5f;
                    dst = 0.05f; 
                }

                transform.localScale = new Vector3(1.35f * normalScale, 1.35f * normalScale, 1f);
                transform.position = Vector3.Lerp(transform.position, drawEndPosition, Time.deltaTime * spd);

                if ((transform.position - drawEndPosition).magnitude < dst)
                {
                    cardState = CardState.inHand;
                }
                
                break;

            case CardState.toMill:
                float _spd = 5f;
                float _dst = 0.05f;   

                transform.localScale = new Vector3(1.35f * normalScale, 1.35f * normalScale, 1f);
                transform.position = Vector3.Lerp(transform.position, drawEndPosition, Time.deltaTime * _spd);

                if ((transform.position - drawEndPosition).magnitude < _dst)
                {
                    cardState = CardState.Mill;
                }
                
                break;

            case CardState.Mill:
                transform.position = Vector3.Lerp(transform.position, MillPosition, Time.deltaTime * 10f);

                if ((transform.position - MillPosition).magnitude < 0.05f)
                {
                    DestroyCard();
                }
                
                break;

            case CardState.ShuffleIntoDeck:
                transform.position = Vector3.Lerp(transform.position, MillPosition + new Vector3(2f, 0f, 0f), Time.deltaTime * 1f);

                if ((transform.position - MillPosition).magnitude < 0.05f)
                {
                    DestroyCard();
                }
                
                break;

            case CardState.selected:
                BoardManager.Slot closestSlot = null;
                if (!cardStats.isSpell)
                {
                    closestSlot = SelectClosestSlot();
                }
                else if (cardStats.isSpell)
                {    

                    boardManager.cyclingSlot.Highlight(false);
                    if (cardStats.HaveCycling() && handManager.CanCycleCard())
                    {
                        closestSlot = boardManager.cyclingSlot;
                        closestSlot.Highlight(true);
                    }

                    // There's a chance a player can't play a card, but the card has cycling   
                    if (transform.position.y > 2f && !handManager.CanPlayCard())
                    {
                        ReturnToHand();
                    }
                    else if (transform.position.y > 2f && cardStats.numberOfTargets > 0)
                    {
                        boardManager.cyclingSlot.Highlight(false);
                        cardState = CardState.selectingTargets;
                    }

                    if (cardStats.numberOfTargets == 0 || cardStats.HaveCycling())
                    {
                        FollowMouse(scale:true);
                    }
                    else
                    {
                        Debug.Log("Should be in selectingTargets state");
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    ReturnToHand();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    float distanceToCycle = Mathf.Abs(transform.position.x - boardManager.cyclingSlot.GetPosition().x);
                    if (!cardStats.isSpell && !cardStats.hasOnPlaySpell)
                    {
                        if (closestSlot == null)
                        {
                            ReturnToHand();
                        }
                        else if (closestSlot == boardManager.cyclingSlot && handManager.CanCycleCard() && distanceToCycle < 2f)
                        {
                            CycleProcedure();
                        }
                        else if (handManager.CanPlayCard())
                        {
                            PlayCard(closestSlot:closestSlot, spell:false);
                        }
                    }
                    else if (cardStats.isSpell)
                    {
                        if (cardStats.HaveCycling() && distanceToCycle < 2f)
                        {
                            CycleProcedure();
                        }
                        else if (cardStats.numberOfTargets == 0 && transform.position.y > 2f)
                        {
                            PlayCard(spell:true);
                        }
                        else
                        {
                            ReturnToHand();
                        }
                    }
                    else if (cardStats.hasOnPlaySpell)
                    {
                        if (closestSlot == null)
                        {
                            ReturnToHand();
                        }
                        else if (closestSlot == boardManager.cyclingSlot && handManager.CanCycleCard() && distanceToCycle < 2f)
                        {
                            CycleProcedure();
                        }
                        else if (handManager.CanPlayCard())
                        {
                            cardState = CardState.selectingTargets;
                            CursorController.cursorState = CursorController.CursorStates.Select;
                            spellTargets = new List<int>();

                            if (cardStats.dummyTarget)
                            {
                                spellTargets.Add(closestSlot.GetIndex());
                            }

                            slotToPlay = closestSlot;
                        }
                    }
                }

                if (!GameController.playerTurn)
                {
                    ReturnToHand();
                }

                break;


            case CardState.alreadyPlayed:
                descriptionObject.SetActive(false);
                backObject.SetActive(false);
                nameObject.SetActive(false);
                nameOutline.SetActive(false);
                foreach (GameObject rune_ in runeObjects)
                {
                    rune_.SetActive(false);
                }
                transform.localScale = new Vector3(normalScale, normalScale, 1f);
                transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
                break;

            case CardState.selectingTargets:

                if (cardStats.isSpell)
                {
                    transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                    transform.position = new Vector3(positionInHand.x, selectedY, selectedZ);
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    transform.localScale = new Vector3(normalScale, normalScale, 1f);
                    transform.position = new Vector3(slotToPlay.GetPosition().x, slotToPlay.GetPosition().y, selectedZ);
                    transform.rotation = Quaternion.identity;
                }

                if (arrowList == null)
                {
                    arrowList = new List<Arrow>();
                    curArrow = new Arrow(transform.position);
                    arrowList.Add(curArrow);
                }
                curArrow.UpdatePosition();

                if (spellTargets.Count >= cardStats.numberOfTargets)
                {
                    if (cardStats.checkSpellTargets(spellTargets, boardManager.enemySlots, boardManager.friendlySlots))
                    {
                        PlayCard(closestSlot:slotToPlay, spell:true);
                    }
                    else
                    {
                        ReturnToHand();
                    }

                    if (arrowList != null)
                    {
                        foreach (Arrow arrow in arrowList)
                        {
                            arrow.DestroyArrow();
                        }
                        curArrow = null;
                        arrowList = null;
                    }
                }


                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.name == "Minion(Clone)") // Fix this shit!
                        {
                            MinionManager target = hit.collider.GetComponent<MinionManager>();
                          
                            int index = target.GetIndex() + 1;
                            if (!target.GetFriendly())
                            {
                                index *= -1;
                            }

                            if (cardStats.checkSpellTarget(index, boardManager.enemySlots, boardManager.friendlySlots) && !target.GetCardStats().hexproof)
                            {
                                spellTargets.Add(index);
                                curArrow = new Arrow(transform.position);
                                arrowList.Add(curArrow);
                            }
                            else
                            {
                                Debug.Log("Wrong target, idiot!");
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    ReturnToHand();
                }

                if (!GameController.playerTurn)
                {
                    ReturnToHand();
                }

                break;

            case CardState.display:
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "Collection")
                {
                    transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                }
                if (scene.name == "Collection" && mouseOver)
                {
                    if (CursorController.cursorState == CursorController.CursorStates.Free)
                    {
                        bool showStats = false;
                        if (Input.GetMouseButton(0))
                        {
                            secondsHold += Time.deltaTime;
                            if (secondsHold > 0.5f)
                            {
                                showStats = true;
                            }
                        }

                        if (showStats || Input.GetMouseButtonUp(1))
                        {
                            CardInfoController.Create(cardType, infoPrefab);
                            CursorController.cursorState = CursorController.CursorStates.Select;
                        }
                        else if (Input.GetMouseButtonUp(0))
                        {
                            secondsHold = 0f;
                            if (DeckManager.CheckCardNumber(cardType) && DeckManager.GetDeckSize() + 1 <= DeckManager.minDeckSize)
                            {
                                CollectionControl collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
                                
                                int tmpSpear = collection.spearDevotion;
                                int tmpShield = collection.shieldDevotion;
                                int tmpBow = collection.bowDevotion;

                                bool bad = false;
                                if (DeckManager.GetCardQty(cardType) >= DeckManager.collection[cardType])
                                {
                                    bad = true;
                                }


                                foreach (Runes rune in cardStats.runes)
                                {
                                    if (rune == Runes.Spear)
                                    {
                                        tmpSpear--;
                                    }
                                    else if (rune == Runes.Shield)
                                    {
                                        tmpShield--;
                                    }
                                    else if (rune == Runes.Bow)
                                    {
                                        tmpBow--;
                                    }

                                    if (tmpBow < 0 || tmpShield < 0 || tmpSpear < 0)
                                    {
                                        bad = true;
                                        break;
                                    }
                                }
                                
                                if (DEBUG || !bad)
                                {
                                    DeckManager.AddCard(cardType);
                                    collection.ShowDeck();
                                }
                            }
                        }
                    }
                }
                else
                {
                    secondsHold = 0f;
                }
                break;

            case CardState.asOption:

                if (mouseOver && Input.GetMouseButtonDown(0) && GameController.CanPerformActions())
                {
                    spellTargets = new List<int>();

                    int target;
                    if (hostMinion.GetFriendly())
                    {
                        target = hostMinion.GetIndex() + 1;
                    }
                    else
                    {
                        target = -hostMinion.GetIndex() - 1;
                    }
                    spellTargets.Add(target);

                    if (cardStats.damageToHost <= hostMinion.GetPower()) 
                    {
                        ServerDataProcesser.instance.CastSpell(this, spellTargets);
                        
                        QueueData newEvent = new();
                        newEvent.actionType = QueueData.ActionType.CastSpell;
                        newEvent.thisStats = cardStats;
                        newEvent.hostCard = this;
                        newEvent.targets = spellTargets;
                        newEvent.friendlySlots = boardManager.friendlySlots;
                        newEvent.enemySlots = boardManager.enemySlots;
                        GameController.eventQueue.Insert(0, newEvent);
                        
                        // Cast spell
                        //StartCoroutine(cardStats.spell(spellTargets, boardManager.enemySlots, boardManager.friendlySlots));
                        spellTargets = new List<int>();
                        hostMinion.ReturnToNormalAfterOptions();
                        hostMinion.SetCanAttack(false);
                    }
                }

                transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                break;

            case CardState.opponentPlayed:

                if (arrowList == null)
                {
                    arrowList = new List<Arrow>();
                    foreach (int target in spellTargets)
                    {
                        BoardManager.Slot targetSlot;
                        if (target < 0)
                        {
                            targetSlot = boardManager.friendlySlots[-target - 1];
                        }
                        else
                        {
                            targetSlot = boardManager.enemySlots[target - 1];
                        }

                        arrowList.Add(new Arrow(transform.position, targetSlot.GetPosition()));
                    }
                }

                foreach (Arrow arrow in arrowList)
                {
                    arrow.UpdatePosition(transform.position);
                }

                transform.position = Vector3.Lerp(transform.position, playEndPosition, 3.5f * Time.deltaTime);
                Vector3 targetLocalScale = new Vector3(0.8f, 0.8f, 0.8f);

                transform.localScale = Vector3.Lerp(transform.localScale, targetLocalScale, 3.5f * Time.deltaTime);

                destroyTimer -= 1f * Time.deltaTime;

                if (destroyTimer < 0f || (mouseOver && Input.GetMouseButtonDown(0)))
                {
                    if (arrowList != null)
                    {
                        foreach (Arrow arrow in arrowList)
                        {
                            arrow.DestroyArrow();
                        }
                        curArrow = null;
                        arrowList = null;
                    }
                    DestroyCard();
                }

                break;

            case CardState.opponentHolding:
                transform.localScale = new Vector3(normalScale, normalScale, 1f);
                transform.position = Vector3.Lerp(transform.position, positionInHand, 15f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(180f, 0, curRotation);
                
                break;

            case CardState.hilightOver:
                transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                //transform.position = Vector3.Lerp(transform.position, positionInHand, 15f * Time.deltaTime);
                //transform.rotation = Quaternion.Euler(180f, 0, curRotation);
                break;

            case CardState.openedFromPack:
                transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                transform.rotation = Quaternion.Euler(0, rotationFromPack, 0);
                rotationFromPack -= rotationFromPackSpeed * Time.deltaTime;
                if (rotationFromPack <= 0f)
                {
                    rotationFromPack = 0f;
                }
                break;
        
        }
    }
    
    public void DestroyCard()
    {
        StartCoroutine(IenumDestroyCard());
    }
    
    public IEnumerator IenumDestroyCard()
    {
        transform.position = new Vector3(-50f, 0f, 0f);
        cardState = CardState.asOption;

        while (!GameController.CanPerformActions())
        {
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(gameObject);

        yield return null;
    }

    public void ReturnToHand()
    {
        cardState = CardState.inHand;
        CursorController.cursorState = CursorController.CursorStates.Free;

        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            slot.Highlight(false);
        }
        boardManager.cyclingSlot.Highlight(false);

        spellTargets = new List<int>();
        
        if (arrowList != null)
        {
            foreach (Arrow arrow in arrowList)
            {
                arrow.DestroyArrow();
            }
            curArrow = null;
            arrowList = null;
        }

    }

    public void SetNameSize(int size)
    {
        nameObject.GetComponent<TextMeshPro>().fontSize = size;
    }

    public void SetDescriptionSize(int size)
    {
        descriptionObject.GetComponent<TextMeshPro>().fontSize = size;
    }

    public void SetCardType(CardTypes type)
    {
        cardType = type;
    }

    public CardTypes GetCardType()
    {
        return cardType;
    }

    public void SetCardState(CardState state)
    {
        this.cardState = state;
    }
    public CardState GetCardState()
    {
        return cardState;
    }

    public void SetCardStats(CardStats stats)
    {
        cardStats = stats;
    }
    public CardStats GetCardStats()
    {
        return cardStats;
    }
    public void SetRotation(float rot)
    {
        curRotation = rot;
    }

    public float GetRotation()
    {
        return curRotation;
    }

    public void SetPower(int p)
    {
        power = p;
        powerObject.GetComponent<TextMeshPro>().text = p.ToString();
    }
    public int GetPower()
    {
        return power;
    }
    public void SetName(string name)
    {
        cardName = name;
        nameObject.GetComponent<TextMeshPro>().text = name;
    }
    public string GetName()
    {
        return cardName;
    }
    public int GetNumberOfCopies()
    {
        return this.numberOfCopies;
    }
    public void SetNumberOfCopies(int number)
    {
        this.numberOfCopies = number;
        numberOfCardsText.GetComponent<TextMeshPro>().text = number.ToString() + "x";
    }
    public void SetDescription(string description)
    {
        cardDescription = description;
        descriptionObject.GetComponent<TextMeshPro>().text = description;
    }
    public void SetPositionInHand(Vector3 newPosition)
    {
        positionInHand = newPosition;
    }
    public Vector3 GetPositionInHand()
    {
        return positionInHand;
    }
    public void SetIndexInHand(int index)
    {
        indexInHand = index;
    }
    public int GetIndexIHand()
    {
        return indexInHand;
    }
    public void SetDisplay()
    {
        cardState = CardState.display; 
    }
    private void OnMouseOver()
    {
        mouseOver = true;
    }
    private void OnMouseExit()
    {
        mouseOver = false;
    }
}
