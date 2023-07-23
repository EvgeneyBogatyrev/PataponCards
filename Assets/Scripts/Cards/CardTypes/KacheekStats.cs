using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KacheekStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "-1: Add Fang to your hand.\n-1: Give your creatures +1 power.";
        stats.name = "Kacheek";

        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.GiveFang);
        stats.connectedCards.Add(CardTypes.Nutrition);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.imagePath = "kacheek";
        return stats;
    }
}

public static class GiveFangStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int giveFangHealthCost = 1;

        stats.description = "Add Fang to your hand.";
        stats.name = "Bone Weapon";

        stats.isSpell = true;
        static IEnumerator GiveFangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.TakePower(giveFangHealthCost);
            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.AddCardToHand(CardTypes.Fang);
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent(fromDeck:false);
                //handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
            }
            yield return null;
        }
        stats.spell = GiveFangRealization;
        stats.numberOfTargets = 0;
        stats.damageToHost = giveFangHealthCost;

        stats.imagePath = "Fang";
        return stats;
    }
}


public static class NutritionStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int nutritionHeal = 1;
        const int nutritionHealthCost = 1;

        stats.description = "Restore " + nutritionHeal.ToString() + " power to all allies.";
        stats.name = "Nutrition";

        stats.isSpell = true;

        static IEnumerator NutritionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.TakePower(nutritionHealthCost);

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion != host)
                {
                    minion.Heal(nutritionHeal);
                }
            }
            yield return null;
        }
        stats.spell = NutritionRealization;
        stats.numberOfTargets = 0;
        stats.damageToHost = nutritionHealthCost;

        stats.imagePath = "Meat";
        return stats;
    }
}