using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    
    public class Slot
    {
        private GameObject slotObject; // game object of this slot
        private MinionManager connectedMinion; // unit on this slot or null
        private int index; // index in slot array
        private bool friendly; // is it in friendly slots or enemy ones
        private bool free; // true if connectedMinion is null
        private bool cycling; // true if it is a cycling slot
        
        public Slot(Vector3 position, int _index, bool _friendly, bool _cycling=false)
        {
            if (!_cycling) 
            {
                slotObject = Instantiate(BoardManager.slotPrefabStatic);
            }
            else
            {
                slotObject = Instantiate(BoardManager.cyclePrefabStatic);
            }
            slotObject.transform.position = position;
            index = _index;
            friendly = _friendly;
            free = true;
            cycling = _cycling;
            Highlight(false);
        }

        public void Highlight(bool _active = true)
        {
            // Make it green if active, otherwise, make it normal
            if (_active)
            {
                slotObject.SetActive(true);
                slotObject.GetComponent<SpriteRenderer>().color = BoardManager.highlightedColorStatic;
            }
            else
            {
                if (cycling)
                {
                    slotObject.SetActive(false);
                }
                else
                {
                    slotObject.GetComponent<SpriteRenderer>().color = BoardManager.normalColorStatic;
                }
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
            /*
            if (!cycling)
            {
                return slotObject.transform.position;
            }
            return new Vector3(slotObject.transform.position.x + 10f/8f, 0f, 0f); // Fix that! 
            */
        }

        public GameObject GetSlotObject()
        {
            return slotObject;
        }

        public bool GetFree()
        {
            return (GetConnectedMinion() == null);
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

    public static GameObject cyclePrefabStatic;
    public GameObject cyclePrefab;

    public static Color highlightedColorStatic;
    public Color highlightedColor;

    public static Color normalColorStatic;
    public Color normalColor;

    public GameObject minionPrefab;

    public List<Slot> friendlySlots = new List<Slot>();
    public List<Slot> enemySlots = new List<Slot>();
    public Slot cyclingSlot;

    public int numberOfSlots = 7;
    public float friendlySlotsPosition = 1f;
    public float enemySlotsPosition = 3f;
    public float leftSlotOffset = 10f;
    public float minionRotation = 0f;
    public float leftSlotSlide = -5f;

    public int randomHash;
    public int opponentHash;

    public GameController gameController = null;

    public bool battlecryTrigger = false;

    public CardTypes lastDeadYou = CardTypes.Hatapon;
    public CardTypes lastDeadOpponent = CardTypes.Hatapon;

    public List<CardTypes> playedCards = new();
    public List<CardTypes> playedCardsOpponent = new();

    void Start()
    {
        InitStaticObjects();
        // Generate slots
        float step = 2 * leftSlotOffset / (numberOfSlots + 1);
        for (int i = 1; i <= numberOfSlots; ++i)
        {
            Slot newSlot = new Slot(new Vector3(leftSlotSlide + i * step, friendlySlotsPosition, 1.5f), i - 1, true);            
            friendlySlots.Add(newSlot);
        }
        for (int i = 1; i <= numberOfSlots; ++i)
        {
            Slot newSlot = new Slot(new Vector3(leftSlotSlide + i * step, enemySlotsPosition, 1.5f), i - 1, false);
            enemySlots.Add(newSlot);
        }
        cyclingSlot = new Slot(new Vector3(leftSlotSlide + numberOfSlots * step + 0.35f * step, friendlySlotsPosition - 5.2f, 1.5f), numberOfSlots, true, _cycling: true);

        randomHash = InfoSaver.myHash;
        opponentHash = InfoSaver.opponentHash;

        lastDeadOpponent = CardTypes.Hatapon;
        lastDeadYou = CardTypes.Hatapon;

        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void InitStaticObjects()
    {
        slotPrefabStatic = slotPrefab;
        cyclePrefabStatic = cyclePrefab;
        highlightedColorStatic = highlightedColor;
        normalColorStatic = normalColor;
    }

    public MinionManager PlayCard(CardManager card, Vector3 position, Slot slot = null, bool destroy=true, bool record=true, bool fromHand=true)
    {
        GameObject newMinion = Instantiate(minionPrefab, position, Quaternion.identity);
        newMinion.transform.rotation = Quaternion.Euler(minionRotation, 0f, 0f);
        newMinion.GetComponent<MinionManager>().CustomizeMinion(card, slot);

        if (record)
        {
            if (!slot.GetFriendly())
            {
                playedCardsOpponent.Add(card.GetCardType());
            }
            else
            {
                playedCards.Add(card.GetCardType());
            }
            if (!card.GetCardStats().hasOnPlaySpell) 
            {
                LogController.instance.AddPlayCardToLog(card.GetCardType(), null, slot.GetFriendly());
            }
        }
        
        AudioController.PlaySound(card.GetCardStats().onPlaySound);

        slot.SetConnectedMinion(newMinion.GetComponent<MinionManager>());
        if (fromHand && card.GetCardStats().hasAfterPlayEvent)
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

            
            // Cast spell fake
            //StartCoroutine(card.GetCardStats().afterPlayEvent(index, enemySlots, friendlySlots));
            QueueData newEventAfterPlay = new();
            newEventAfterPlay.actionType = QueueData.ActionType.AfterPlayEffect;
            newEventAfterPlay.thisStats = card.GetCardStats();
            newEventAfterPlay.hostCard = card;
            newEventAfterPlay.index = index;
            newEventAfterPlay.friendlySlots = friendlySlots;
            newEventAfterPlay.enemySlots = enemySlots;
            GameController.eventQueue.Insert(0, newEventAfterPlay);
        }

        List<Slot> _slots;
        if (slot.GetFriendly())
        {
            _slots = friendlySlots;
        }
        else
        {
            _slots = enemySlots;
        }

        foreach (Slot _slot in _slots)
        {
            MinionManager _minion = _slot.GetConnectedMinion();
            if (_minion != null)
            {
                if (_minion.GetCardStats().lifelinkMeTo == slot.GetIndex())
                {
                    _minion.GetCardStats().lifelinkMeTo = -1;
                    _minion.GetCardStats().lifelinkedTo = new()
                    {
                        newMinion.GetComponent<MinionManager>()
                    };
                }
            }
        }
        

        if (destroy)
        {
            card.DestroyCard();
        }


        if (record)
        {
            ServerDataProcesser.instance.PlayCard(card, slot);
        }

        return newMinion.GetComponent<MinionManager>();
    }

    public IEnumerator CycleCard(CardManager card, bool destroy=true, bool record=true)
    {
        if (record)
        {
            ServerDataProcesser.instance.CycleCard(card);
        }

        card.SetCardState(CardManager.CardState.Mill);
        
        
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
        int idx = 0;
        foreach (Slot slot in friendlySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null)
            {
                if (minion.GetCardStats().onCycleOtherEvent != null)
                {
                    QueueData newEvent = new();
                    newEvent.actionType = QueueData.ActionType.OnCycleOther;
                    newEvent.hostUnit = minion;
                    newEvent.index = minion.GetIndex();

                    if (minion.GetFriendly())
                    {
                        newEvent.friendlySlots = friendlySlots;
                        newEvent.enemySlots = enemySlots;
                    }
                    else
                    {
                        newEvent.friendlySlots = enemySlots;
                        newEvent.enemySlots = friendlySlots;
                    }
                    GameController.eventQueue.Insert(0, newEvent);
                    /*
                    do {
                        yield return new WaitForSeconds(0.1f);
                    } while (gameController.actionIsHappening);
                    // On cycle
                    StartCoroutine(minion.GetCardStats().onCycleOtherEvent(idx, enemySlots, friendlySlots));
                    do {
                        yield return new WaitForSeconds(0.1f);
                    } while (gameController.actionIsHappening);
                    */
                }
            }
            idx += 1;
        }

        if (destroy)
        {
            card.DestroyCard();
        }
        
        yield return null;
    }

    public MinionManager PlayCard(CardManager card, Slot slot = null, bool destroy=true, bool record=true, bool fromHand=true)
    {     
        return PlayCard(card, card.transform.position, slot, destroy, record, fromHand);
        /*
        GameObject newMinion = Instantiate(minionPrefab, card.transform.position, Quaternion.identity);
        newMinion.transform.rotation = Quaternion.Euler(minionRotation, 0f, 0f);
        newMinion.GetComponent<MinionManager>().CustomizeMinion(card, slot);

        if (record)
        {
            if (!slot.GetFriendly())
            {
                playedCardsOpponent.Add(card.GetCardType());
            }
            else
            {
                playedCards.Add(card.GetCardType());
            }
            if (!card.GetCardStats().hasOnPlaySpell) 
            {
                LogController.instance.AddPlayCardToLog(card.GetCardType(), null, slot.GetFriendly());
            }
        }

        if (fromHand && card.GetCardStats().hasAfterPlayEvent)
        {
            Debug.Log("Battlecry (me)");
            int index;
            if (!slot.GetFriendly())
            {
                index = slot.GetIndex() * (-1) - 1;
            }
            else
            {
                index = slot.GetIndex() + 1;
            }
            Debug.Log(card.GetCardStats().afterPlayEvent);
            StartCoroutine(card.GetCardStats().afterPlayEvent(index, enemySlots, friendlySlots));
        }

        List<Slot> _slots;
        if (slot.GetFriendly())
        {
            _slots = friendlySlots;
        }
        else
        {
            _slots = enemySlots;
        }

        foreach (Slot _slot in _slots)
        {
            MinionManager _minion = _slot.GetConnectedMinion();
            if (_minion != null)
            {
                if (_minion.GetCardStats().lifelinkMeTo == slot.GetIndex())
                {
                    _minion.GetCardStats().lifelinkMeTo = -1;
                    _minion.GetCardStats().lifelinkedTo = new()
                    {
                        newMinion.GetComponent<MinionManager>()
                    };
                }
            }
        }

        if (destroy)
        {
            card.DestroyCard();
        }


        if (record)
        {
            ServerDataProcesser.instance.PlayCard(card, slot);
        }
        return newMinion.GetComponent<MinionManager>();
        */
    }

    
    public void DealSuddenDeathDamage(bool friendly, int amount)
    {
        if (amount == 0)
        {
            return;
        }
        MinionManager hatapon = null;
        List<Slot> searchSlots;
        if (friendly)
        {
            searchSlots = friendlySlots;
        }
        else
        {
            searchSlots = enemySlots;
        }

        foreach (Slot slot in searchSlots)
        {
            MinionManager curMinion = slot.GetConnectedMinion();
            if (curMinion != null && curMinion.GetCardType() == CardTypes.Hatapon)
            {
                hatapon = curMinion;
                break;
            }
        }

        if (hatapon == null)
        {
            Debug.Log("Can't find Hatapon in DealSuddenDeathDamage");
        }

        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 

        LogController.instance.AddPlayCardToLog(CardTypes.Fatique, new List<int>(), friendly);
        CardManager newCard = handManager.GenerateCard(CardTypes.Fatique, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    
        HandManager.DestroyDisplayedCards();
        newCard.SetCardState(CardManager.CardState.opponentPlayed);
        newCard.transform.position = new Vector3(0f, 10f, 0f);
        newCard.destroyTimer = HandManager.cardDestroyTimer;

        hatapon.LoseLife(amount);
    }

    public void ClearBoard()
    {
        foreach (Slot slot in friendlySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null)
            {
                minion.DestroySelf(unattach:true);
            }
        }

        foreach (Slot slot in enemySlots)
        {
            MinionManager minion = slot.GetConnectedMinion();
            if (minion != null)
            {
                minion.DestroySelf(unattach:true);
            }
        }
        lastDeadOpponent = CardTypes.Hatapon;
        lastDeadYou = CardTypes.Hatapon;
    }
}


