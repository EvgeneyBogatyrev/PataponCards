using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArmoryStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Pacifism. Abilities:</b>\n-1: Draw a card, your Hatapon loses 2 life.\n-3: Discard your hand, draw 3 cards.";
        stats.name = "Armory";

        stats.descriptionSize = 3;

        stats.runes.Add(Runes.Bow);
        //stats.runes.Add(Runes.Bow);


        stats.isStatic = true;
        //stats.pacifism = true;
        stats.connectedCards = new List<CardTypes>
        {
            CardTypes.Armory_option1,
            CardTypes.Armory_option2
        };

        stats.imagePath = "armory";

        return stats;
    }
}

public static class Armory_option1Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int cost = 1;
        const int damageToHatapon = 2;
                
        stats.description = "-1: Draw a card, your Hatapon loses 2 life.";
        stats.name = "Upgrade weapon";
        stats.nameSize = 4;

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.LoseLife(cost);
            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.DealDamageToThis(damageToHatapon);
                    break;
                }
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.spell = Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = cost;

        stats.imagePath = "armory";

        return stats;
    }
}

public static class Armory_option2Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int cost = 3;
        const int card_draw = 3;

        stats.nameSize = 4;
                
        stats.description = "-3: Discard your hand, draw 3 cards.";
        stats.name = "Sell your weapons";

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.LoseLife(cost);
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            if (!enemySlots[0].GetFriendly())
            {
                handManager.DiscardHand(true);
                for (int i = 0; i < card_draw; ++i)
                {
                    handManager.DrawCard();
                }
            }
            else
            {
                handManager.DiscardHand(false);
                for (int i = 0; i < card_draw; ++i)
                {
                    handManager.DrawCardOpponent();
                }
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.spell = Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = cost;
        stats.imagePath = "armory";

        return stats;
    }
}
