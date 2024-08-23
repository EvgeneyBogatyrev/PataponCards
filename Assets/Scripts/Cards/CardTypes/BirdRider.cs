using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BirdRiderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "<b>On play</b>: Summon all other copies of Bird Rider from your deck.";
        stats.name = "Bird Rider";

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.additionalRules.Add("All copies of <i>'Bird Rider'</i> cards are removed from your library and placed on the battlefield.");

        stats.hasAfterPlayEvent = true;

        static IEnumerator Realization(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            int index;
            List<BoardManager.Slot> chooseSlots;
            List<BoardManager.Slot> oppSlots;
            if (target > 0)
            {
                chooseSlots = friendlySlots;
                oppSlots = enemySlots;
                index = target - 1;
            }
            else
            {
                chooseSlots = enemySlots;
                oppSlots = friendlySlots;
                index = -target - 1;
            }

            BoardManager.Slot thisOne = chooseSlots[index];

            int summonNumber = 0;
            if (chooseSlots[0].GetFriendly())
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
                foreach (BoardManager.Slot slot in chooseSlots)
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
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.afterPlayEvent = Realization;
        //stats.numberOfTargets = 1;
        //stats.dummyTarget = true;

        stats.imagePath = "bird_rider";
        stats.artistName = "Unused game assets";
        return stats;
    }
}
