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

        stats.description = "Summon " + divineProtectionTateponCount.ToString() + " Tatepons with " + divineProtectionTateponPower.ToString() + " power and Lifelink.";
        stats.name = "Divine Protection";

        stats.nameSize = 4;
        
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.isSpell = true;
        static IEnumerator DivineProtectionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

            CardManager tateponCard = handManager.GenerateCard(CardTypes.TokenTatepon, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            tateponCard.SetPower(divineProtectionTateponPower);

            int count = 0;
            MinionManager prevTatepon = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                if (count >= divineProtectionTateponCount)
                {
                    break;
                }
                if (slot.GetFree())
                {
                    MinionManager curTatepon = boardManager.PlayCard(tateponCard, new Vector3(0f, 0f, 0f), slot, destroy: false, record: false);
                    count += 1;
                    /*
                    if (prevTatepon == null)
                    {
                        foreach (BoardManager.Slot __slot in friendlySlots)
                        {
                            MinionManager portentialHatapon = __slot.GetConnectedMinion();
                            if (portentialHatapon != null && portentialHatapon.GetCardType() == CardTypes.Hatapon)
                            {
                                portentialHatapon.GetCardStats().lifelinkMeTo = -1;
                                portentialHatapon.GetCardStats().lifelinkedTo = new()
                                {
                                    curTatepon
                                };
                            }
                        }
                    }
                    else
                    {
                        prevTatepon.GetCardStats().lifelinkMeTo = -1;
                        prevTatepon.GetCardStats().lifelinkedTo = new()
                        {
                            curTatepon
                        };
                    }
                    prevTatepon = curTatepon;
                    */
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


public static class TokenTateponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.description = "";
        stats.name = "Novice Tatepon";
        stats.power = 2;
        stats.hasShield = true;
        //stats.runes.Add(Runes.Shield);

        //stats.isSpell = true;
        stats.imagePath = "Tatepon";
        return stats;
    }
}