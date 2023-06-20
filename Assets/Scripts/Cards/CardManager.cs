using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum Runes
{
    Neutral,
    Spear,
    Shield,
    Bow
}

public class CardManager : MonoBehaviour
{
    public delegate IEnumerator EndTurnEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate void Spell(List<int> targets = null, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate void OnPlayEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null);
    public delegate void OnDeathEvent(int index = 0, List<BoardManager.Slot> enemy = null, List<BoardManager.Slot> friendly = null, CardStats thisStats = null);
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
        public bool hasOnPlay = false;
        public OnPlayEvent onPlayEvent = null;
        public OnDeathEvent onDeathEvent = null;
        public bool hasBattlecry = false;
        public bool isStatic = false;
        public int healthCost = 0;
        public List<CardTypes> connectedCards = new List<CardTypes>();
        public bool flying = false;
        public bool dummyTarget = false;
        public List<EndTurnEvent> additionalEndTurnEvents = new List<EndTurnEvent>();
        public int armor = 0;
        public bool hasDeathrattle = false;
        public CardStats savedStats = null;
        public List<MinionManager> connectedMinions = new List<MinionManager>();
        public int damageToHost = -1;
        public List<Runes> runes = new List<Runes>();

        public string imagePath = "500x500";

        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>("Images/" + this.imagePath);
        }

        public CardStats CopyStats()
        {
            CardStats newStats = new CardStats();

            newStats.isSpell = this.isSpell;
            newStats.canAttack = this.canAttack;
            newStats.canDealDamage = this.canDealDamage;
            newStats.hasHaste = this.hasHaste;
            newStats.hasShield = this.hasShield;
            newStats.hasGreatshield = this.hasGreatshield;
            newStats.limitedVision = this.limitedVision;
            newStats.megaVision = this.megaVision;
            newStats.fixedPower = this.fixedPower;
            newStats.endTurnEvent = this.endTurnEvent;
            newStats.startTurnEvent = this.startTurnEvent;
            newStats.spell = this.spell;
            newStats.numberOfTargets = this.numberOfTargets;
            newStats.checkSpellTarget = this.checkSpellTarget;
            newStats.checkSpellTargets = this.checkSpellTargets;
            newStats.hasOnPlay = this.hasOnPlay;
            newStats.onPlayEvent = this.onPlayEvent;
            newStats.onDeathEvent = this.onDeathEvent;
            newStats.hasBattlecry = this.hasBattlecry;
            newStats.isStatic = this.isStatic;
            newStats.healthCost = this.healthCost;

            newStats.connectedCards = new List<CardTypes>();
            foreach (CardTypes ct in this.connectedCards)
            {
                newStats.connectedCards.Add(ct);
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
            newStats.hasDeathrattle = this.hasDeathrattle;
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
    }

    public GameObject powerObject;
    public GameObject powerSquare;
    public GameObject imageObject;
    public GameObject nameObject;
    public GameObject descriptionObject;
    public GameObject nameOutline;
    public GameObject heartObject;
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

    private Vector3 drawEndPosition;
    private Vector3 playEndPosition;

    public List<Arrow> arrowList = null;
    private Arrow curArrow = null;

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Game")
        {
            boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            drawEndPosition = GameObject.Find("DrawnCardDisplay").transform.position;
            playEndPosition = GameObject.Find("PlayedCardPosition").transform.position;
        }
    }

    void Update()
    {
        switch (cardState) 
        {
            case CardState.inHand:
                if (mouseOver && (CursorController.cursorState == CursorController.CursorStates.Free ||
                                    CursorController.cursorState == CursorController.CursorStates.EnemyTurn))
                {
                    transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(positionInHand.x, selectedY, selectedZ), 30f * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0, 0, 0);

                    if (Input.GetMouseButtonDown(0) && handManager.CanPlayCard())
                    {
                        if (cardStats.isSpell && cardStats.numberOfTargets > 0)
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

            case CardState.selected:
                BoardManager.Slot closestSlot = null;
                if (!cardStats.isSpell)
                {
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Quaternion toRotation = Quaternion.Euler((mousePosition.y - transform.position.y) * 25, -(mousePosition.x - transform.position.x) * 25, 0);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 30.0f);
                    Vector3 toPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    toPosition = new Vector3(toPosition.x, toPosition.y, selectedZ);
                    transform.position = Vector3.Lerp(transform.position, toPosition, Time.deltaTime * 30.0f);

                    float minDistance = -1f;
                    foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                    {
                        slot.Highlight(false);
                        float distance = (transform.position - slot.GetPosition()).magnitude;
                        if (minDistance == -1f || minDistance > distance)
                        {
                            minDistance = distance;
                            closestSlot = slot;
                        }
                    }

                    if (closestSlot.GetFree())
                    {
                        closestSlot.Highlight(true);
                    }
                    else
                    {
                        closestSlot = null;
                    }
                }
                else if (cardStats.isSpell)
                {
                    if (cardStats.numberOfTargets == 0)
                    {
                        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Quaternion toRotation = Quaternion.Euler((mousePosition.y - transform.position.y) * 25, -(mousePosition.x - transform.position.x) * 25, 0);
                        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 30.0f);

                        Vector3 toPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        toPosition = new Vector3(toPosition.x, toPosition.y, selectedZ);
                        transform.position = Vector3.Lerp(transform.position, toPosition, Time.deltaTime * 30.0f);

                        
                        
                        transform.localScale = new Vector3(selectedScale, selectedScale, 1f);

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
                    if (!cardStats.isSpell && !cardStats.hasOnPlay)
                    {
                        if (closestSlot == null)
                        {
                            ReturnToHand();
                        }
                        else
                        {
                            boardManager.PlayCard(this, closestSlot);
                            handManager.RemoveCard(GetIndexIHand());
                            handManager.SetCanPlayCard(false);

                            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                            {
                                slot.Highlight(false);
                            }
                            CursorController.cursorState = CursorController.CursorStates.Free;
                        }
                    }
                    else if (cardStats.isSpell)
                    {
                        handManager.RemoveCard(GetIndexIHand());
                        handManager.SetCanPlayCard(false);
                        ServerDataProcesser.instance.CastSpell(this, spellTargets);
                        cardStats.spell(spellTargets, boardManager.enemySlots, boardManager.friendlySlots);
                        spellTargets = new List<int>();
                        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                        {
                            slot.Highlight(false);
                        }
                        CursorController.cursorState = CursorController.CursorStates.Free;
                        DestroyCard();
                    }
                    else if (cardStats.hasOnPlay)
                    {
                        if (closestSlot == null)
                        {
                            ReturnToHand();
                        }
                        else
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

                break;

            case CardState.selectingTargets:

                if (cardStats.isSpell)
                {
                    transform.localScale = new Vector3(selectedScale, selectedScale, 1f);
                    transform.position = new Vector3(positionInHand.x, selectedY, selectedZ);
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
                        boardManager.battlecryTrigger = true;
                        ServerDataProcesser.instance.CastSpell(this, spellTargets);
                        cardStats.spell(spellTargets, boardManager.enemySlots, boardManager.friendlySlots);
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
                        handManager.RemoveCard(GetIndexIHand());
                        handManager.SetCanPlayCard(false);
                        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                        {
                            slot.Highlight(false);
                        }
                        CursorController.cursorState = CursorController.CursorStates.Free;

                        if (cardStats.isSpell)
                        {
                            DestroyCard();
                        }
                        else
                        {
                            boardManager.PlayCard(this, slotToPlay);
                        }
                        boardManager.battlecryTrigger = false;
                    }
                    else
                    {
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
                        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                        {
                            slot.Highlight(false);
                        }
                        CursorController.cursorState = CursorController.CursorStates.Free;
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
                            if (cardStats.checkSpellTarget(index, boardManager.enemySlots, boardManager.friendlySlots))
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

                break;

            case CardState.display:
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "Collection")
                {
                    if (mouseOver && Input.GetMouseButtonDown(0))
                    {
                        if (DeckManager.CheckCardNumber(cardType) && DeckManager.GetDeckSize() + 1 <= DeckManager.minDeckSize)
                        {
                            CollectionControl collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
                            
                            int tmpSpear = collection.spearDevotion;
                            int tmpShield = collection.shieldDevotion;
                            int tmpBow = collection.bowDevotion;

                            bool bad = false;
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
                            
                            if (!bad)
                            {
                                DeckManager.AddCard(cardType);
                                collection.ShowDeck();
                            }
                        }
                    }
                }
                break;

            case CardState.asOption:

                if (mouseOver && Input.GetMouseButtonDown(0))
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
                        cardStats.spell(spellTargets, boardManager.enemySlots, boardManager.friendlySlots);
                        ServerDataProcesser.instance.CastSpell(this, spellTargets);
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
                transform.position = positionInHand;
                transform.rotation = Quaternion.Euler(180f, 0, curRotation);
                
                break;
        
        }
    }

    public void DestroyCard()
    {
        Destroy(gameObject);
    }

    public void ReturnToHand()
    {
        cardState = CardState.inHand;
        CursorController.cursorState = CursorController.CursorStates.Free;

        foreach (BoardManager.Slot slot in boardManager.friendlySlots)
        {
            slot.Highlight(false);
        }

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
    public void ReceiveDamage(int damage)
    {
        SetPower(power - damage);
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
    public void SetDescription(string description)
    {
        cardDescription = description;
        descriptionObject.GetComponent<TextMeshPro>().text = description;
    }
    public void SetPositionInHand(Vector3 newPosition)
    {
        positionInHand = newPosition;
    }
    public Vector3 GetPosiitonInHand()
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
