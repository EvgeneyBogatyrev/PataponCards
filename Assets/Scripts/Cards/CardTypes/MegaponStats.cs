using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MegaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "On play: Deal 2 damage split between one or two units. Draw a card for each unit destroyed this way.";
        stats.name = "Megapon";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);


        //stats.hasBattlecry = true;
        stats.hasOnPlaySpell = true;

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

                bool dead = selectedSlot.GetConnectedMinion().ReceiveDamage(2);
                if (dead)
                {
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

                    bool dead = selectedSlot.GetConnectedMinion().ReceiveDamage(1);
                    if (dead)
                    {
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
                }
            }
            yield return null;
        }

        stats.spell = MegaponRealization;
        stats.numberOfTargets = 2;
        //stats.cycling = true;

        //stats.onPlayEvent = MegaponBattlecry;

        stats.imagePath = "megapon";
        return stats;
    }
}
