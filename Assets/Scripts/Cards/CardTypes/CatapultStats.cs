using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class CatapultStats
{
    const int catapultDamage = 3;
    const int firstCost = 1;
    const int secondCost = 0;
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 4;
        stats.description = "<b>Pacifism. Abilities</b>:\n-" + firstCost.ToString() + ": Deal " + catapultDamage.ToString() + " damage to opponent's Hatapon.\n" +
                            "-" + secondCost.ToString() + ": Destroy other friendly non-Hatapon units to gain their power.";
        stats.name = "Catapult";

        stats.descriptionSize = 3;

        stats.runes.Add(Runes.Spear);
        
        stats.isStatic = true;
        //stats.pacifism = true;
        stats.connectedCards = new List<CardTypes>
        {
            CardTypes.Catapult_option1,
            CardTypes.Catapult_option2
        };

        stats.imagePath = "catapult";

        return stats;
    }
}

public static class Catapult_option1Stats
{
    const int catapultDamage = 2;
    const int firstCost = 1;
    const int secondCost = 0;
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "-" + firstCost.ToString() + ": Deal " + catapultDamage.ToString() + " damage to opponent's Hatapon.";
        stats.name = "Fire!";

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

            host.LoseLife(firstCost);
            
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.ReceiveDamage(catapultDamage);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.spell = Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = firstCost;

        stats.imagePath = "catapult";

        return stats;
    }
}

public static class Catapult_option2Stats
{
    const int catapultDamage = 2;
    const int firstCost = 1;
    const int secondCost = 0;
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
                
        stats.description = "-" + secondCost.ToString() + ": Destroy other friendly non-Hatapon units to gain their power.";
        stats.name = "Reload";

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

            host.LoseLife(secondCost);
            
            int gain = 0;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion != host && minion.GetCardType() != CardTypes.Hatapon)
                {
                    gain += minion.GetPower();
                    minion.DestroyMinion();
                }
            }

            host.Heal(gain);

            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.spell = Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = secondCost;
        stats.imagePath = "catapult";

        return stats;
    }
}
