using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;


public class LogController : MonoBehaviour
{
    public class LogMessage
    {
        public enum Action
        {
            KeepHand,
            PlayCard,
            Attack,
            CycleCard
        }
        public Action action;
        public bool friendly;
        public int number;
        public CardTypes card;
        public List<CardTypes> targets;
    }
    public static LogController instance = null;
    public GameObject logItemPrefab;
    private List<LogMessage> log = null;
    private const int logLimit = 8;
    private List<GameObject> logVis = new();
    public float logStep = 1.2f;
    public float logBottom = -2.2f;
    
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
            log = new List<LogMessage>();
        } 
    }

    public void ClearLog()
    {
        log = new List<LogMessage>();
        ClearVisuals();
    }

    public void ClearVisuals()
    {
        foreach (GameObject obj in logVis)
        {
            CardManager connectedCard = obj.GetComponent<LogReprManager>().previewedCard;
            if (connectedCard != null)
            {
                connectedCard.DestroyCard();
            }
            foreach (CardManager card in obj.GetComponent<LogReprManager>().targetCards)
            {
                card.DestroyCard();
            }
            obj.GetComponent<LogReprManager>().targetCards = new();
            Arrow arrow = obj.GetComponent<LogReprManager>().arrow;
            if (arrow != null)
            {
                arrow.DestroyArrow();
            }
            Destroy(obj);
        }
        logVis = new();
    }

    private void AddToLog(LogMessage newMessage)
    {
        log.Add(newMessage);

        if (log.Count > logLimit)
        {
            log.RemoveAt(0);
        }

        ClearVisuals();
        int i = 0;
        foreach (LogMessage message in log)
        {
            LogReprManager logRepr = Instantiate(logItemPrefab).GetComponent<LogReprManager>();
            if (message.action == LogMessage.Action.CycleCard)
            {
                logRepr.isCycled = true;
            }
            else if (message.action == LogMessage.Action.Attack)
            {
                logRepr.isAttacking = true;
            }
            logRepr.Customize(message.card, message.friendly);
            logRepr.index = i;
            logRepr.transform.position = new Vector3(transform.position.x, logBottom + i * logStep, transform.position.z);
            logVis.Add(logRepr.gameObject);
            logRepr.targets = message.targets;
            
            i += 1;
        }
    }

    public void AddKeepHandToLog(int numberOfCards, bool friendly)
    {
        LogMessage newMessage = new LogMessage();
        newMessage.action = LogMessage.Action.KeepHand;
        newMessage.number = numberOfCards;
        newMessage.friendly = friendly;

        AddToLog(newMessage);
    }

    public void AddCycleToLog(CardTypes cardType, bool friendly)
    {
        AddPlayCardToLog(cardType, null, friendly, cycle:true);
    }

    public void AddAttackToLog(int from, int target, bool friendly)
    {
        List<int> fromTarget = new List<int>();
        fromTarget.Add(from);
        CardTypes cardType = ResolveTargets(fromTarget, true)[0];
        List<int> attackTarget = new List<int>();
        attackTarget.Add(target);
        AddPlayCardToLog(cardType, attackTarget, friendly, attack:true);
    }

    public void AddPlayCardToLog(CardTypes cardType, List<int> targets, bool friendly, bool cycle=false, bool attack=false)
    {
        LogMessage newMessage = new LogMessage();
        newMessage.action = cycle ? LogMessage.Action.CycleCard : (attack ? LogMessage.Action.Attack : LogMessage.Action.PlayCard);
        newMessage.card = cardType;
        newMessage.friendly = friendly;

        if (CardTypeToStats.GetCardStats(cardType).dummyTarget && !attack)
        {
            List<int> newTargets = new();
            int i = 0;
            foreach (int target in targets)
            {
                if (i == 0)
                {
                    i = 1;
                    continue;
                }
                newTargets.Add(target);
            }
            newMessage.targets = ResolveTargets(newTargets, friendly);
        }
        else
        {
            newMessage.targets = ResolveTargets(targets, friendly || attack);
        }
        AddToLog(newMessage);
    }

    private List<CardTypes> ResolveTargets(List<int> targets, bool friendly)
    {
        BoardManager board = GameObject.Find("Board").GetComponent<BoardManager>();
        List<CardTypes> types = new();
        if (targets == null)
        {
            return types;
        }

        foreach (int target in targets)
        {
            Debug.Log(target);
            if ((target > 0 && friendly) || (target < 0 && !friendly))
            {
                types.Add(board.friendlySlots[Math.Abs(target) - 1].GetConnectedMinion().GetCardType());
            }
            else
            {
                types.Add(board.enemySlots[Math.Abs(target) - 1].GetConnectedMinion().GetCardType());
            }
            Debug.Log(types[types.Count - 1]);
        }
        return types;
    }


   
}
