using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomistStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int aoeDamage = 2;
        stats.name = "Venomist";
        stats.description = "Deal " + aoeDamage.ToString() + " damage to all units. Summon a poisonous Mushroom on place of every one who died.";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        //stats.legendary = true;

        stats.isSpell = true;
        static IEnumerator realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.ReceiveDamage(aoeDamage);
                    if (minion.GetPower() <= 0)
                    {
                        yield return new WaitForSeconds(0.2f);
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                        CardManager card = handManager.GenerateCard(CardTypes.Mushroom, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                        
                        boardManager.PlayCard(card, new Vector3(0f, 0f, 0f), slot, destroy: false, record: false);
                        card.DestroyCard();
            
                    }
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.ReceiveDamage(aoeDamage);
                    if (minion.GetPower() <= 0)
                    {
                        yield return new WaitForSeconds(0.2f);
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                        CardManager card = handManager.GenerateCard(CardTypes.Mushroom, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                        
                        boardManager.PlayCard(card, new Vector3(0f, 0f, 0f), slot, destroy: false, record: false);
                        card.DestroyCard();
            
                    }
                }
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = realization;
        stats.numberOfTargets = 0;
        stats.imagePath = "venomist";
        return stats;
    }
}
