using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TurnToStoneStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Turn to stone";
        stats.description = "Summon the last friendly unit died this round. It has Greatshield, can't attack and deal damage.";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        //stats.runes.Add(Runes.Bow);
        //card.SetNameSize(4);

        stats.isSpell = true;
        static void TurnToStoneRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int powerToSet = 0;
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            if (enemySlots[1].GetFriendly())
            {
                if (boardManager.lastDeadOpponent == CardTypes.Hatapon)
                {
                    return;
                }
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadOpponent).GetComponent<CardManager>();
                powerToSet = minionCard.GetPower();
                minionCard.DestroyCard();

                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (slot.GetFree())
                    {
                        CardManager boulderCard = handManager.GenerateCard(CardTypes.StoneFree).GetComponent<CardManager>();
                        boulderCard.SetPower(powerToSet);
                        boardManager.PlayCard(boulderCard, slot, destroy:false, record:false);
                        boulderCard.DestroyCard();
                        break;
                    }
                }
            }
            else
            {
                if (boardManager.lastDeadYou == CardTypes.Hatapon)
                {
                    return;
                }
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadYou).GetComponent<CardManager>();
                powerToSet = minionCard.GetPower();
                minionCard.DestroyCard();
                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (slot.GetFree())
                    {
                        CardManager boulderCard = handManager.GenerateCard(CardTypes.StoneFree).GetComponent<CardManager>();
                        boulderCard.SetPower(powerToSet);
                        boardManager.PlayCard(boulderCard, slot, destroy:false, record:false);
                        boulderCard.DestroyCard();
                        break;
                    }
                }
            }
        }

        stats.spell = TurnToStoneRealization;
        stats.numberOfTargets = 0;
        stats.imagePath = "turnToStone";

        return stats;
    }
}