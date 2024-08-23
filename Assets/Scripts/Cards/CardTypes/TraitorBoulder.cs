using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TraitorBoulderStats 
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "<b>Pacifism.\nOn play</b>: Your opponent takes control of it.";
        stats.name = "The Rock";

        stats.pacifism = true;
        //stats.hasGreatshield = true;

        //stats.nameSize = 4;
       

        static IEnumerator OnPlay(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager thisMinion;
            BoardManager.Slot enemySlot;
            List<BoardManager.Slot> _enemySlots;
            Debug.Log(index);
            if (index > 0)
            {
                thisMinion = friendlySlots[index - 1].GetConnectedMinion();
                _enemySlots = enemySlots;
                enemySlot = enemySlots[index - 1];
            }
            else
            {
                thisMinion = enemySlots[-index - 1].GetConnectedMinion();
                _enemySlots = friendlySlots;
                enemySlot = friendlySlots[-index - 1];
            }


            if (enemySlot.GetFree())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager iceWallCard = handManager.GenerateCard(CardTypes.TraitorBoulder, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                boardManager.PlayCard(iceWallCard, new Vector3(0f, 0f, 0f), enemySlot, destroy:false, record:false, fromHand:false);
                
                iceWallCard.DestroyCard();       
            } 
            else 
            {

                foreach (BoardManager.Slot slot in _enemySlots)
                {
                    if (slot.GetFree())
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                        BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                        CardManager iceWallCard = handManager.GenerateCard(CardTypes.TraitorBoulder, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                        boardManager.PlayCard(iceWallCard, new Vector3(0f, 0f, 0f), slot, destroy:false, record:false, fromHand:false);
                        
                        iceWallCard.DestroyCard();   
                        break;
                    }
                }
            }

            thisMinion.GetSlot().SetFree(true);
            thisMinion.DestroySelf();
            gameController.actionIsHappening = false;
            yield return null;                            
        }

        stats.hasAfterPlayEvent = true;
        stats.afterPlayEvent = OnPlay;

        stats.imagePath = "rock_hq";

        return stats;
    }
}
