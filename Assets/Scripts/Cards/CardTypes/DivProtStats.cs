using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DivProtStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int divineProtectionTateponCount = 3;
        const int divineProtectionTateponPower = 2;

        stats.description = "Summon " + divineProtectionTateponCount.ToString() + " Tatepons with " + divineProtectionTateponPower.ToString() + " power.";
        stats.name = "Divine Protection";
        
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.isSpell = true;
        static IEnumerator DivineProtectionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

            CardManager tateponCard = handManager.GenerateCard(CardTypes.Tatepon).GetComponent<CardManager>();
            tateponCard.SetPower(divineProtectionTateponPower);

            int count = 0;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (count >= divineProtectionTateponCount)
                {
                    break;
                }
                if (slot.GetFree())
                {
                    boardManager.PlayCard(tateponCard, slot, destroy: false, record: false);
                    count += 1;
                }
            }

            tateponCard.DestroyCard();
            yield return null;
        }

        stats.spell = DivineProtectionRealization;
        stats.numberOfTargets = 0;

        stats.imagePath = "DivineProtection";
        return stats;
    }
}