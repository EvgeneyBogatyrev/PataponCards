using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bot
{
    public class BotMove
    {
        public CardTypes playCard = CardTypes.Hatapon;
        public int cellNumber = -1;
        public int attackCell = -1;
        public int moveCell = -1;
        public bool exchange = false;
        public List<int> deck = null;
        public bool endTurn = false;
    }

    public bool botActive = false;
    public int botLevel = 2;

    private bool deckSend = false;
    public bool cardPlayed = false;

    public List<CardTypes> botDeck = null;

    private void InitDeck()
    {
        if (botDeck != null)
        {
            return;
        }
        if (botLevel == 2)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.Dekapon,
                CardTypes.Dekapon,
                CardTypes.Kuwagattan,
                CardTypes.Kuwagattan,
                CardTypes.Moforumo,
                CardTypes.Piekron,
                CardTypes.Piekron,
                CardTypes.Piekron,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
            };
        }
        else if (botLevel == 1)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.Dekapon,
                CardTypes.Moforumo,
                CardTypes.PanThePakapon,
                CardTypes.ZigotonTroops,
                CardTypes.ZigotonTroops,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.GongTheHawkeye,
            };
        }
        else if (botLevel <= 0)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.TraitorBoulder,
                CardTypes.TraitorBoulder,
                CardTypes.PanThePakapon,
                CardTypes.PanThePakapon,
                CardTypes.PanThePakapon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
            };
        }
    }

    public List<CardTypes> botHand = new List<CardTypes>();

    private bool CheckLifelink(List<BoardManager.Slot> slots)
    {
        foreach (BoardManager.Slot _slot in slots)
        {
            if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
            {
                return true;
            }
        }
        return false;
    }

    private BoardManager.Slot FindSlotWithCard(List<BoardManager.Slot> slots, CardTypes type)
    {
        foreach(BoardManager.Slot _slot in slots)
        {
            if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardType() == type)
            {
                return _slot;
            }
        }
        return null;
    }

    private BoardManager.Slot RateSlotsToPlayCard(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<BoardManager.Slot> selectedSlots, CardTypes cardToPlay)
    {
        BoardManager.Slot selectedSlot = null;
        BoardManager.Slot targetSlot = null;

        if (botLevel >= 2 && (cardToPlay == CardTypes.Yumipon || cardToPlay == CardTypes.FuckingIdiot))
        {
            foreach (BoardManager.Slot _slot in selectedSlots)
            {
                int idx = _slot.GetIndex();
                if (selectedSlot == null || (playerSlots[idx].GetFree() && (idx == 6 || playerSlots[idx + 1].GetFree()) && (idx == 0 || playerSlots[idx - 1].GetFree())))
                {
                    selectedSlot = _slot;
                }
            }
            return selectedSlot;
        }

        if (CheckLifelink(playerSlots) && botLevel >= 1)
        {
            foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
                {
                    targetSlot = _slot;
                }
            }
        }
        else
        {
            /*foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().endTurnEvent != null)
                {
                    targetSlot = _slot;
                }
            }
            */
            if (targetSlot == null)
            {
                targetSlot = FindSlotWithCard(playerSlots, CardTypes.Hatapon);
            }
        }
        
        if (targetSlot == null)
        {
            return selectedSlots[0];
        }
        foreach (BoardManager.Slot _slot in selectedSlots)
        {
            if (selectedSlot == null || Math.Abs(selectedSlot.GetIndex() - targetSlot.GetIndex()) > Math.Abs(_slot.GetIndex() - targetSlot.GetIndex()))
            {
                selectedSlot = _slot;
            }
        }
        return selectedSlot;
        
    }

    public float EvaluateAttackUnit(MinionManager enemyMinion, MinionManager myMinion, bool enemyLifelink)
    {
        float score = 0f;

        if (enemyMinion.GetCardStats().hasShield && botLevel >= 1)
        {
            score += 0.15f;
        }
        if (enemyMinion.GetCardStats().endTurnEvent != null && botLevel >= 2)
        {
            score += 0.35f;
        }
        if (enemyMinion.GetCardStats().hasOnDeath && botLevel >= 2)
        {
            score -= 10f;
        }

        if (myMinion.GetCardType() == CardTypes.Yaripon || myMinion.GetCardType() == CardTypes.Yumipon)
        {
            score -= 10f;
        }

        if (enemyMinion.GetCardType() == CardTypes.Hatapon)
        {
            if (!enemyLifelink)
            {
                if (myMinion.GetPower() > enemyMinion.GetPower())
                {
                    return 100f;
                }
                return score + myMinion.GetPower() / (enemyMinion.GetPower() - myMinion.GetPower());
            }
            else
            {
                return score - 100f;
            }
        }
        else if (!enemyMinion.GetCardStats().canDealDamage)
        {
            return score - (enemyMinion.GetPower() - myMinion.GetPower() + 1) / 20f;
        }
        
        return score - (enemyMinion.GetPower() - myMinion.GetPower()) / 5f;
    }

    public float EvaluateMoveUnit(BoardManager.Slot slotFrom, BoardManager.Slot slotTo, List<BoardManager.Slot> playerSlots)
    {
        if (slotFrom.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
        {
            float value = 0f;
            for (int i = slotTo.GetIndex() - 1; i <= slotTo.GetIndex() + 1; ++i)
            {
                if (i >= 0 && i <= 6 && !playerSlots[i].GetFree())
                {
                    if (playerSlots[i].GetConnectedMinion().GetCardStats().canDealDamage && botLevel >= 1)
                    {
                        value -= playerSlots[i].GetConnectedMinion().GetPower();
                    }
                }
            }
            value += 0.01f * Mathf.Abs(3 - slotTo.GetIndex());
            Debug.Log($"value of {slotTo.GetIndex()}: {value}");
            return value;
        }

        BoardManager.Slot hataponSlot = null;
        if (CheckLifelink(playerSlots))
        {
            foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
                {
                    hataponSlot = _slot;
                }
            }
        }
        else
        {
            hataponSlot = FindSlotWithCard(playerSlots, CardTypes.Hatapon);
        }
        if (hataponSlot == null)
        {
            return 0f;
        }
        return -Mathf.Abs(slotTo.GetIndex() - hataponSlot.GetIndex());

    }

    private int RateAttackTargets(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<int> moveTargets, BoardManager.Slot mySlot)
    {
        int bestIndex = -1;
        float bestEval = 0f;

        bool lifelink = CheckLifelink(playerSlots);

        foreach (int target in moveTargets)
        {
            float curEval = EvaluateAttackUnit(playerSlots[target - 1].GetConnectedMinion(), mySlot.GetConnectedMinion(), lifelink);
            Debug.Log($"Cost of {target}: {curEval}");
            if (bestIndex == -1 || curEval > bestEval)
            {
                bestEval = curEval;
                bestIndex = target;
            }
        }

        if (bestEval < -5f)
        {
            return -1;
        }
        
        return bestIndex;
    
    }

    private int RateMoveTargets(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<int> moveTargets, BoardManager.Slot mySlot)
    {
        int bestIndex = -1;
        float bestEval = 0f;

        foreach (int target in moveTargets)
        {
            float curEval = EvaluateMoveUnit(mySlot, botSlots[target - 1], playerSlots);
            if (bestIndex == -1 || curEval > bestEval)
            {
                bestEval = curEval;
                bestIndex = target;
            }
            Debug.Log(curEval);
        }
        
        return bestIndex;
    
    }

    public BotMove GetNextMove(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        if (!botActive || GameController.eventQueue.Count != 0)
        {
            return null;
        }

        InitDeck();

        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        for (int i = 0; i < handManager.GetNumberOfOpponentsCards() - botHand.Count; ++i)
        {
            CardTypes newCard = DrawCard();
            if (newCard == CardTypes.Hatapon)
            {
                break;
            }
            botHand.Add(newCard);
        }
        int numberOfCadsInBotHand = botHand.Count;
        for (int i = 0; i < numberOfCadsInBotHand - handManager.GetNumberOfOpponentsCards(); ++i)
        {
            botHand.RemoveAt(0);
        }

        if (!deckSend)
        {
            deckSend = true;
            BotMove deckMove = new BotMove();
            deckMove.deck = DeckManager.GetEncodedDeck(runes:true);
            return deckMove;
        }

        Debug.Log($"Bot level={botLevel}");
        if (botLevel < 0)
        {
            // End Turn
            BotMove endTurnMove = new BotMove();
            endTurnMove.endTurn = true;
            botActive = false;
            return endTurnMove;
        }

        //Attack
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree())
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanAttackBot())
                {
                    List<int> attackPositions = new List<int>();
                    if (!playerSlots[slotIdx].GetFree() && !playerSlots[slotIdx].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx + 1);
                    }
                    if (slotIdx > 0 && !playerSlots[slotIdx - 1].GetFree() && !playerSlots[slotIdx - 1].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && !playerSlots[slotIdx + 1].GetFree() && !playerSlots[slotIdx + 1].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx + 2);
                    }
                    if (attackPositions.Count > 0)
                    {
                        int attackTarget = RateAttackTargets(playerSlots, botSlots, attackPositions, slot);
                        if (attackTarget != -1)
                        {
                            BotMove __move = new BotMove();
                            __move.attackCell = attackTarget;
                            __move.cellNumber = slotIdx + 1;
                            return __move;
                        }
                    }
                }
            }
        }

        //Exchange
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree() && slot.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanMoveBot())
                {
                    List<int> movePositions = new List<int>();
                    if (slotIdx > 0 && !botSlots[slotIdx - 1].GetFree() && botSlots[slotIdx - 1].GetConnectedMinion().GetCanMoveBot())
                    {
                        movePositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && !botSlots[slotIdx + 1].GetFree() && botSlots[slotIdx + 1].GetConnectedMinion().GetCanMoveBot())
                    {
                        movePositions.Add(slotIdx + 2);
                    }
                    if (movePositions.Count > 0)
                    {
                        int currentMoveIdx = slotIdx + 1;
                        movePositions.Add(currentMoveIdx);

                        int moveTarget = RateMoveTargets(playerSlots, botSlots, movePositions, slot);
                        if (moveTarget != currentMoveIdx)
                        {
                            BotMove __move = new BotMove();
                            __move.moveCell = moveTarget;
                            __move.cellNumber = slotIdx + 1;
                            __move.exchange = true;
                            return __move;
                        }
                    }
                }
            }
        }

        //Move
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree())
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanMoveBot())
                {
                    List<int> movePositions = new List<int>();
                    if (slotIdx > 0 && botSlots[slotIdx - 1].GetFree())
                    {
                        movePositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && botSlots[slotIdx + 1].GetFree())
                    {
                        movePositions.Add(slotIdx + 2);
                    }
                    if (movePositions.Count > 0)
                    {
                        int currentMoveIdx = slotIdx + 1;
                        movePositions.Add(currentMoveIdx);

                        int moveTarget = RateMoveTargets(playerSlots, botSlots, movePositions, slot);
                        if (moveTarget != currentMoveIdx)
                        {
                            BotMove __move = new BotMove();
                            __move.moveCell = moveTarget;
                            __move.cellNumber = slotIdx + 1;
                            return __move;
                        }
                    }
                }
            }
        }

        // Play card
        if (!cardPlayed && handManager.GetNumberOfOpponentsCards() > 0 && botHand.Count > 0)
        {
            List<BoardManager.Slot> freeSlots = new List<BoardManager.Slot>();
            foreach (BoardManager.Slot slot in botSlots)
            {
                if (slot.GetFree())
                {
                    freeSlots.Add(slot);
                }
            }

            if (freeSlots.Count > 0)
            {
                CardTypes plyedCard = botHand[UnityEngine.Random.Range(0, botHand.Count)];
                BoardManager.Slot slotToPlay = RateSlotsToPlayCard(playerSlots, botSlots, freeSlots, plyedCard);//freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
                botHand.Remove(plyedCard);
                BotMove playCardMove = new BotMove();
                playCardMove.playCard = plyedCard;
                playCardMove.cellNumber = slotToPlay.GetIndex() + 1;
                cardPlayed = true;
                return playCardMove;
            }
        }

        // End Turn
        BotMove move = new BotMove();
        move.endTurn = true;
        botActive = false;
        return move;
    }

    private CardTypes DrawCard()
    {
        if (botDeck.Count == 0)
        {
            return CardTypes.Hatapon;
        }

        CardTypes returnValue = botDeck[UnityEngine.Random.Range(0, botDeck.Count - 1)];
        botDeck.Remove(returnValue);
        return returnValue;
    }
}