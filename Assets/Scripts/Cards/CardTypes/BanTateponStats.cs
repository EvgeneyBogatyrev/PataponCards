using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BanTateponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int banTateponThreshold = 8;
        const int banTateponPower = 2;
        stats.power = 5;
        stats.description = "On play: If you control a minion with power " + banTateponThreshold.ToString() + " or greater, fill your board with Tatepons with " + banTateponPower +  " power, Greatshield and Haste, and draw a card.";
        stats.name = "Ban Tatepon";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        
        stats.hasOnPlay = true;

        stats.legendary = true;
        stats.descriptionSize = 3;

        static IEnumerator BanTateponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            bool completed = false;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    if (minion.GetPower() >= banTateponThreshold)
                    {
                        completed = true;
                        break;
                    }
                }
            }

            if (completed)
            {
                HandManager _handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager tateponCard = _handManager.GenerateCard(CardTypes.Tatepon).GetComponent<CardManager>();
                tateponCard.SetPower(banTateponPower);
                tateponCard.GetCardStats().hasGreatshield = true;
                tateponCard.GetCardStats().hasHaste = true;

                foreach (BoardManager.Slot slot in friendlySlots)
                {
                    if (slot.GetFree() && slot != friendlySlots[targets[0]])
                    {
                        boardManager.PlayCard(tateponCard, slot, destroy:false, record:false);
                    }
                }

                tateponCard.DestroyCard();

                if (friendlySlots[1].GetFriendly())
                {
                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                    handManager.DrawCard();
                }
                else
                {
                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                    handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                }

            }
            yield return null;
        }

        stats.spell = BanTateponRealization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;
        stats.imagePath = "banTatepon";

        return stats;
    }
}
