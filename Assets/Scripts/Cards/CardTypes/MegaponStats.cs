using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MegaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "Deal two damage split between 1 or 2 creatures.\nDraw a card.";
        stats.name = "Megapon";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);


        stats.hasBattlecry = true;
        stats.hasOnPlay = true;

        static void MegaponBattlecry(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (index > 0)
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
        }

        static IEnumerator MegaponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (targets[0] == targets[1])
            {
                BoardManager.Slot selectedSlot;
                if (targets[0] < 0)
                {
                    selectedSlot = enemySlots[targets[0] * (-1) - 1];
                }
                else
                {
                    selectedSlot = friendlySlots[targets[0] - 1];
                }

                selectedSlot.GetConnectedMinion().ReceiveDamage(2);
            }
            else
            {
                foreach (int target in targets)
                {
                    BoardManager.Slot selectedSlot;
                    if (target < 0)
                    {
                        selectedSlot = enemySlots[target * (-1) - 1];
                    }
                    else
                    {
                        selectedSlot = friendlySlots[target - 1];
                    }

                    selectedSlot.GetConnectedMinion().ReceiveDamage(1);
                }
            }

            if (enemySlots[1].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                //handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                handManager.DrawCardOpponent();
            }
            yield return null;
        }

        stats.spell = MegaponRealization;
        stats.numberOfTargets = 2;

        stats.onPlayEvent = MegaponBattlecry;

        stats.imagePath = "megapon";
        return stats;
    }
}
