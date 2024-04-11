using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrentStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 10;
        stats.description = "<b>Pacifism</b>.\nWhenever this unit takes damage, it transforms into <i>Treant on Fire</i>.";
        stats.name = "Treant";
        stats.pacifism = true;

        stats.relevantCards.Add(CardTypes.TrentOnFire);

        stats.runes.Add(Runes.Shield);

        static IEnumerator onDamage(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            gameController.actionIsHappening = true;

            MinionManager host = friendlySlots[index].GetConnectedMinion();
            if (host != null)
            {
                CardManager newCard = handManager.GenerateCard(CardTypes.TrentOnFire, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                newCard.SetPower(host.GetPower());
                BoardManager.Slot slot = host.GetSlot();
                //host.TakePower(host.GetPower());
                host.DestroySelf(unattach:true);
                boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), slot, destroy: true, record: false);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.onDamageEvent = onDamage;

        stats.imagePath = "treant";
        return stats;
    }
}

public static class TrentFireStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int selfDamage = 1;
        stats.power = 10;
        stats.description = "<b>End of turn</b>: Deal " + selfDamage.ToString() + " to itself.";
        stats.name = "Treant on Fire";
        stats.runes.Add(Runes.Shield);
        
        static IEnumerator endTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;
            MinionManager minion = friendlySlots[index].GetConnectedMinion();
            if (minion != null) minion.ReceiveDamage(selfDamage);
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.endTurnEvent = endTurn;

        stats.imagePath = "treant_fire";
        return stats;
    }
}