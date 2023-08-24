using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TurnToStoneStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Turn to stone";
        stats.description = "Summon the last friendly unit that died this round. Give it Pacifism and Greatshield.";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        //stats.runes.Add(Runes.Bow);
        //card.SetNameSize(4);

        stats.isSpell = true;
        static IEnumerator TurnToStoneRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            if (enemySlots[1].GetFriendly())
            {
                if (boardManager.lastDeadOpponent == CardTypes.Hatapon)
                {
                    yield break;
                }
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                
                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (slot.GetFree())
                    {
                        CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadOpponent, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                        minionCard.GetCardStats().canAttack = false;
                        minionCard.GetCardStats().canDealDamage = false;
                        minionCard.GetCardStats().limitedVision = true;
                        minionCard.GetCardStats().hasGreatshield = true;
                        boardManager.PlayCard(minionCard, new Vector3(0f, 0f, 0f), slot, destroy:false, record:false);
                        minionCard.DestroyCard();
                        break;
                    }
                }
            }
            else
            {
                if (boardManager.lastDeadYou == CardTypes.Hatapon)
                {
                    yield break;
                }
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (slot.GetFree())
                    {
                        CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadYou, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                        minionCard.GetCardStats().canAttack = false;
                        minionCard.GetCardStats().canDealDamage = false;
                        minionCard.GetCardStats().limitedVision = true;
                        minionCard.GetCardStats().hasGreatshield = true;
                        boardManager.PlayCard(minionCard, new Vector3(0f, 0f, 0f), slot, destroy:false, record:false);
                        minionCard.DestroyCard();
                        break;
                    }
                }
            }
            yield return null;
        }

        stats.spell = TurnToStoneRealization;
        stats.numberOfTargets = 0;
        stats.imagePath = "turnToStone";

        return stats;
    }
}
