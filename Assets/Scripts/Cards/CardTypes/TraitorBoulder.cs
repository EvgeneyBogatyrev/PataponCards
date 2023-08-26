using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TraitorBoulderStats 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "Pacifism. Greatshield.\nOn death: Your opponent summons Infinite Boulder.";
        stats.name = "Infinite Boulder";

        stats.canAttack = false;
        stats.canDealDamage = false;
        stats.limitedVision = true;
        stats.hasGreatshield = true;

        stats.nameSize = 4;
        stats.hasDeathrattle = true;
       

        static void onDeath(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
        {
            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot.GetFree())
                {
                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                    CardManager iceWallCard = handManager.GenerateCard(CardTypes.TraitorBoulder, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    boardManager.PlayCard(iceWallCard, new Vector3(0f, 0f, 0f), slot, destroy:false, record:false);
                    
                    iceWallCard.DestroyCard();
                    break;
                }
            }
            //yield return null;                            
        }

        stats.onDeathEvent = onDeath;

        stats.imagePath = "traiter_bolder";

        return stats;
    }
}
