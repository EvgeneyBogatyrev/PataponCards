using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BabattaStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "On play: Replace your hand with 'Babatto Swarm' cards";
        stats.name = "Babatto";

        stats.power = 3;

        stats.hasOnPlaySpell = true;

        static IEnumerator FangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                int cardNumber = handManager.GetNumberOfCards();
                handManager.DiscardHand(true);
                for (int i = 0; i < cardNumber; ++i)
                {
                    handManager.AddCardToHand(CardTypes.BabattaSwarm);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = FangRealization;

        stats.imagePath = "babatto";
        return stats;
    }
}


public static class BabattaSwarmStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 3;
        stats.description = "Deal " + damage.ToString() + " damage.";
        stats.name = "Babatto Swarm";

        stats.isSpell = true;
        static IEnumerator FangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot targetSlot;

            int target = targets[0];
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            targetMinion.ReceiveDamage(damage);
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = FangRealization;
        stats.numberOfTargets = 1;

        stats.imagePath = "babatto_swarm";
        return stats;
    }
}