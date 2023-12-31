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

    public int randomHash;
    public int opponentHash;

    public GameController gameController = null;

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

        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void InitStaticObjects()
    {
        slotPrefabStatic = slotPrefab;
        highlightedColorStatic = highlightedColor;
        normalColorStatic = normalColor;
    }

    public void PlayCard(CardManager card, Vector3 position, Slot slot = null, bool destroy=true, bool record=true)
    {
        GameObject newMinion = Instantiate(minionPrefab, position, Quaternion.identity);
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
            card.DestroyCard();
        }


        if (record)
        {
            ServerDataProcesser.instance.PlayCard(card, slot);
        }
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
            card.DestroyCard();
        }


        if (record)
        {
            ServerDataProcesser.instance.PlayCard(card, slot);
        }
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

        hatapon.TakePower(amount);
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


