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

    private bool deckSend = false;
    public bool cardPlayed = false;

    public List<CardTypes> botDeck = new List<CardTypes>()
    {
        CardTypes.Tatepon,
        CardTypes.Tatepon,
        CardTypes.Tatepon,
        CardTypes.MyamsarHero,
        CardTypes.MyamsarHero,
        CardTypes.Dekapon,
        CardTypes.Dekapon,
        CardTypes.Dekapon,
        CardTypes.ZigotonTroops,
        CardTypes.ZigotonTroops,
        CardTypes.ZigotonTroops,
        CardTypes.Yaripon,
        CardTypes.Yaripon,
        CardTypes.Yaripon,
        CardTypes.Yumipon,
        CardTypes.Yumipon,
        CardTypes.Yumipon,
        CardTypes.GongTheHawkeye,
        CardTypes.GongTheHawkeye,
        CardTypes.FuckingIdiot,
        CardTypes.FuckingIdiot,
        CardTypes.FuckingIdiot,
        CardTypes.Coppen,
        CardTypes.Coppen,
    };

    public List<CardTypes> botHand = new List<CardTypes>();

    private List<BotMove> GetAllBotMoves(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        return null;
    }

    public BotMove GetNextMove(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        if (!botActive)
        {
            return null;
        }

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

        if (!deckSend)
        {
            deckSend = true;
            BotMove deckMove = new BotMove();
            deckMove.deck = DeckManager.GetEncodedDeck(runes:true);
            return deckMove;
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
                    if (!playerSlots[slotIdx].GetFree())
                    {
                        attackPositions.Add(-(slotIdx + 1));
                    }
                    if (slotIdx > 0 && !playerSlots[slotIdx - 1].GetFree())
                    {
                        attackPositions.Add(-slotIdx);
                    }
                    if (slotIdx < 6 && !playerSlots[slotIdx + 1].GetFree())
                    {
                        attackPositions.Add(-(slotIdx + 2));
                    }
                    if (attackPositions.Count > 0)
                    {
                        int attackTarget = attackPositions[UnityEngine.Random.Range(0, attackPositions.Count - 1)];
                        BotMove __move = new BotMove();
                        __move.attackCell = attackTarget;
                        __move.cellNumber = slotIdx + 1;
                        return __move;
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
                        int moveTarget = movePositions[UnityEngine.Random.Range(0, movePositions.Count - 1)];
                        BotMove __move = new BotMove();
                        __move.moveCell = moveTarget;
                        __move.cellNumber = slotIdx + 1;
                        return __move;
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
                BoardManager.Slot slotToPlay = freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
                CardTypes plyedCard = botHand[UnityEngine.Random.Range(0, botHand.Count)];
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

    public List<BotMove> GetBotMoves(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        if (!botActive)
        {
            return null;
        }

        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        List<BotMove> moves = new List<BotMove>();

        for (int i = 0; i < handManager.GetNumberOfOpponentsCards() - botHand.Count; ++i)
        {
            CardTypes newCard = DrawCard();
            if (newCard == CardTypes.Hatapon)
            {
                break;
            }
            botHand.Add(newCard);
        }

        // Send Deck
        if (!deckSend)
        {
            deckSend = true;
            BotMove deckMove = new BotMove();
            deckMove.deck = DeckManager.GetEncodedDeck(runes:true);
            moves.Add(deckMove);
        }

        // Play card
        if (handManager.GetNumberOfOpponentsCards() > 0 && botHand.Count > 0)
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
                BoardManager.Slot slotToPlay = freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
                CardTypes plyedCard = botHand[UnityEngine.Random.Range(0, botHand.Count)];
                botHand.Remove(plyedCard);
                BotMove move = new BotMove();
                move.playCard = plyedCard;
                move.cellNumber = slotToPlay.GetIndex() + 1;
                moves.Add(move);
                Debug.Log("Playing");
                Debug.Log(plyedCard);
            }
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
                    if (!playerSlots[slotIdx].GetFree())
                    {
                        attackPositions.Add(-(slotIdx + 1));
                    }
                    if (slotIdx > 0 && !playerSlots[slotIdx - 1].GetFree())
                    {
                        attackPositions.Add(-slotIdx);
                    }
                    if (slotIdx < 6 && !playerSlots[slotIdx + 1].GetFree())
                    {
                        attackPositions.Add(-(slotIdx + 2));
                    }
                    if (attackPositions.Count > 0)
                    {
                        int attackTarget = attackPositions[UnityEngine.Random.Range(0, attackPositions.Count - 1)];
                        BotMove __move = new BotMove();
                        __move.attackCell = attackTarget;
                        __move.cellNumber = slotIdx + 1;
                        moves.Add(__move);
                    }
                }
            }
        }
        botActive = false;
        return moves;
    } 
}