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
        
    }

    public List<CardTypes> cardPool = new List<CardTypes>()
    {
        CardTypes.Tatepon,
        CardTypes.Guardira,
    };

    public List<BotMove> GetBotMoves(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        List<BotMove> moves = new List<BotMove>();

        // Play card
        List<BoardManager.Slot> freeSlots = new List<BoardManager.Slot>();
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (slot.GetFree())
            {
                Debug.Log(slot.GetIndex());
                freeSlots.Add(slot);
            }
        }

        if (freeSlots.Count > 0)
        {
            BoardManager.Slot slotToPlay = freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
            CardTypes plyedCard = cardPool[UnityEngine.Random.Range(0, cardPool.Count)];
            BotMove move = new BotMove();
            move.playCard = plyedCard;
            move.cellNumber = slotToPlay.GetIndex() + 1;
            Debug.Log("Play index");
            Debug.Log(move.cellNumber);
            moves.Add(move);
        }
        return moves;
    } 
}