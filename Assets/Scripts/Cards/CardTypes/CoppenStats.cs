using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoppenStats 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "Summon an Ice Wall for your opponent.";
        stats.name = "Coppen";
        stats.runes.Add(Runes.Bow);


        stats.hasOnPlay = true;

        static void CoppenRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot.GetFree())
                {
                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                    CardManager iceWallCard = handManager.GenerateCard(CardTypes.IceWall).GetComponent<CardManager>();
                    boardManager.PlayCard(iceWallCard, slot, destroy:false, record:false);
                    
                    iceWallCard.DestroyCard();
                    break;
                }
            }                                   
        }

        stats.spell = CoppenRealization;
        stats.numberOfTargets = 0;

        return stats;
    }
}
