using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BirdRiderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "On play: Summon all other copies of Bird Rider from your deck.";
        stats.name = "Bird Rider";

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.hasOnPlaySpell = true;

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            BoardManager.Slot thisOne = friendlySlots[targets[0]];

            int summonNumber = 0;
            if (friendlySlots[0].GetFriendly())
            {
                while (DeckManager.RemoveCardFromDeck(CardTypes.BirdRider))
                {
                    summonNumber++;
                }
            }
            else
            {
                while (DeckManager.RemoveCardFromOppDeck(CardTypes.BirdRider))
                {
                    summonNumber++;
                }
            }


            for (int _ = 0; _ < summonNumber; _ ++)
            {
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

                    CardManager boulderCard = handManager.GenerateCard(CardTypes.BirdRider, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                    boardManager.PlayCard(boulderCard, new Vector3(0f, 0f, 0f), targetSlot, destroy: false, record: false);

                    boulderCard.DestroyCard();
                }
            }

            gameController.UpdateDecks();
            yield return null;
        }

        stats.spell = Realization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        stats.imagePath = "bird_rider";
        return stats;
    }
}
