using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoppenStats 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "On play: Summon an Ice Wall for your opponent in front of Coppen.";
        stats.name = "Coppen";
        stats.runes.Add(Runes.Bow);


        stats.hasOnPlay = true;

        static IEnumerator CoppenRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int index = targets[0];
            BoardManager.Slot slot;
            slot = enemySlots[index];
            if (slot.GetFree())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager iceWallCard = handManager.GenerateCard(CardTypes.IceWall, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                boardManager.PlayCard(iceWallCard, new Vector3(0f, 0f, 0f), slot, destroy:false, record:false);
                
                iceWallCard.DestroyCard();
            }
            yield return null;                            
        }

        stats.spell = CoppenRealization;
        stats.dummyTarget = true;
        stats.numberOfTargets = 1;

        stats.imagePath = "coppen";

        return stats;
    }
}
