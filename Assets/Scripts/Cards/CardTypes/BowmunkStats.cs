using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BowmunkStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int bowmunkHealing = 2;
        stats.power = 2;
        stats.description = "Summon the rock with Greatshield. At the end of your turn heal your Hatapon by " + bowmunkHealing.ToString() + ".";
        stats.name = "Bowmunk";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);


        static IEnumerator BowmunkEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager connectedMinion = slot.GetConnectedMinion();
                if (connectedMinion != null)
                {
                    if (connectedMinion.GetCardType() == CardTypes.Hatapon)
                    {
                        connectedMinion.Heal(bowmunkHealing);
                    }
                }
            }
            yield return null;
        }
        stats.endTurnEvent = BowmunkEndTurn;


        stats.hasOnPlay = true;

        static void BowmunkRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot thisOne = friendlySlots[targets[0]];

            BoardManager.Slot targetSlot = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (slot.GetFree() && slot != thisOne)
                {
                    targetSlot = slot;
                    break;
                }
            }

            if (targetSlot != null)
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager boulderCard = handManager.GenerateCard(CardTypes.Boulder).GetComponent<CardManager>();
                boardManager.PlayCard(boulderCard, targetSlot, destroy: false, record: false);

                boulderCard.DestroyCard();
            }
        }

        stats.spell = BowmunkRealization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        stats.imagePath = "bowmunk";
        return stats;
    }
}