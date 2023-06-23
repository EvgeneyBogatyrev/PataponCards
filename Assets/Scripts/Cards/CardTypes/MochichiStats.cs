using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MochichiStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "+0: Add +2 power.\n+0: Transform into Angry Motiti with Haste.";
        stats.name = "Motiti";

        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.Motiti_option1);
        stats.connectedCards.Add(CardTypes.Motiti_option2);

        stats.imagePath = "motiti";
        return stats;
    }
}

public static class MochiAccumStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int motiti1HealthCost = 0;
        const int motiti1Heal = 2;

        stats.description = "Give Motiti +" + motiti1Heal.ToString() + " power.";
        stats.name = "Accumulate power";

        stats.isSpell = true;
        static void MotitiOpt1Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            host.TakePower(motiti1HealthCost);
            host.Heal(motiti1Heal);
        }
        stats.spell = MotitiOpt1Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = motiti1HealthCost;

        stats.imagePath = "motiti";
        return stats;
    }
}

public static class MochiciCounterStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int motiti2HealthCost = 0;

        stats.description = "Transform Motiti into Angry Motiti with Haste.";
        stats.name = "Motiti Counteratack";

        stats.isSpell = true;
        static void MotitiOpt2Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

            host.TakePower(motiti2HealthCost);
            CardManager newCard = handManager.GenerateCard(CardTypes.MotitiAngry).GetComponent<CardManager>();
            newCard.SetPower(host.GetPower());
            BoardManager.Slot slot = host.GetSlot();
            //host.TakePower(host.GetPower());
            host.DestroySelf(unattach:true);
            boardManager.PlayCard(newCard, slot, destroy: true, record: false);

        }
        stats.spell = MotitiOpt2Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = motiti2HealthCost;

        stats.imagePath = "MotitiAngry";
        return stats;
    }
}

public static class MochichiAngryStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "Haste.";
        stats.name = "Angry Motiti";

        stats.hasHaste = true;

        stats.imagePath = "MotitiAngry";
        return stats;
    }
}