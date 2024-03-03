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
        stats.description = "On play: Summon a Boulder with Pacifism and Lifelink.\nEnd of turn: Heal your Hatapon by " + bowmunkHealing.ToString() + ".";
        stats.name = "Bowmunk";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.descriptionSize = 3;


        static IEnumerator BowmunkEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
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
            //GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = BowmunkEndTurn;


        stats.hasOnPlaySpell = true;

        static IEnumerator BowmunkRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                CardManager boulderCard = handManager.GenerateCard(CardTypes.Boulder, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                MinionManager boulder = boardManager.PlayCard(boulderCard, new Vector3(0f, 0f, 0f), targetSlot, destroy: false, record: false);

                /*
                foreach (BoardManager.Slot sl in friendlySlots)
                {
                    MinionManager mn = sl.GetConnectedMinion();
                    if (mn == null || mn == boulder)
                    {
                        continue;
                    }

                    mn.GetCardStats().lifelinkMeTo = -1;
                    mn.GetCardStats().lifelinkedTo = new()
                    {
                        boulder
                    };
                }
                */

                boulderCard.DestroyCard();
            }
            yield return null;
        }

        stats.spell = BowmunkRealization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        stats.imagePath = "bowmunk";
        return stats;
    }
}
