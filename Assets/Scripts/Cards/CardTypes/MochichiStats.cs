using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MochichiStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Pacifism. Abilities:</b>\n-0: Give Motiti +1 power.\n-0: Transform into Angry Motiti with <b>Haste</b>.";
        stats.name = "Motiti";

        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.Motiti_option1);
        stats.connectedCards.Add(CardTypes.Motiti_option2);

        stats.pacifism = true;

        stats.relevantCards.Add(CardTypes.MotitiAngry);

        stats.imagePath = "motiti_hq";
        return stats;
    }
}

public static class MochiAccumStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int motiti1HealthCost = 0;
        const int motiti1Heal = 1;

        stats.description = "-0: Give Motiti +" + motiti1Heal.ToString() + " power.";
        stats.name = "Accumulate power";

        stats.nameSize = 4;
        stats.isSpell = true;
        static IEnumerator MotitiOpt1Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            host.LoseLife(motiti1HealthCost);
            host.Heal(motiti1Heal);
            yield return null;
        }
        stats.spell = MotitiOpt1Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = motiti1HealthCost;

        stats.imagePath = "motiti_hq";
        return stats;
    }
}

public static class MochiciCounterStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int motiti2HealthCost = 0;

        stats.description = "-0: Transform Motiti into Angry Motiti with <b>Haste</b>.";
        stats.name = "Motiti Counterattack";

        stats.nameSize = 4;
        stats.isSpell = true;
        static IEnumerator MotitiOpt2Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

            host.LoseLife(motiti2HealthCost);
            CardManager newCard = handManager.GenerateCard(CardTypes.MotitiAngry, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            newCard.SetPower(host.GetPower());
            BoardManager.Slot slot = host.GetSlot();
            //host.TakePower(host.GetPower());
            host.DestroySelf(unattach:true);
            boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), slot, destroy: true, record: false);
            yield return null;
        }
        stats.spell = MotitiOpt2Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = motiti2HealthCost;

        stats.imagePath = "motiti_angry_hq";
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

        stats.imagePath = "motiti_angry_hq";
        return stats;
    }
}