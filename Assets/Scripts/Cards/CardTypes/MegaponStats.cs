using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MegaponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 1;
        stats.description = "<b>On play</b>: Deal 2 damage split between one or two units. Draw a card for each unit destroyed this way.";
        stats.name = "Megapon";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);


        //stats.hasBattlecry = true;
        stats.hasOnPlaySpell = true;

        static IEnumerator MegaponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            if (targets[1] == targets[2])
            {
                BoardManager.Slot selectedSlot;
                if (targets[1] < 0)
                {
                    selectedSlot = enemySlots[targets[1] * (-1) - 1];
                }
                else
                {
                    selectedSlot = friendlySlots[targets[1] - 1];
                }

                SpearManager soundMain = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[targets[0]].GetPosition()).GetComponent<SpearManager>();
                string imagePath = "Images/megapon_large";
                soundMain.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imagePath);
                soundMain.SetSlotToGo(selectedSlot);
                if (enemySlots[0].GetFriendly())
                {
                    soundMain.isEnemy = true;
                }

                while (!soundMain.reachDestination)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                soundMain.DestroySelf();
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
                bool start = true;
                foreach (int target in targets)
                {
                    if (start)
                    {
                        start = false;
                        continue;
                    }
                    BoardManager.Slot selectedSlot;
                    if (target < 0)
                    {
                        selectedSlot = enemySlots[target * (-1) - 1];
                    }
                    else
                    {
                        selectedSlot = friendlySlots[target - 1];
                    }

                    SpearManager soundSmall = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[targets[0]].GetPosition()).GetComponent<SpearManager>();
                    string imagePath = "Images/megapon_small";
                    soundSmall.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imagePath);
                    soundSmall.SetSlotToGo(selectedSlot);
                    if (enemySlots[0].GetFriendly())
                    {
                        soundSmall.isEnemy = true;
                    }

                    while (!soundSmall.reachDestination)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    soundSmall.DestroySelf();

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
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = MegaponRealization;
        stats.numberOfTargets = 3;
        stats.dummyTarget = true;
        //stats.cycling = true;

        //stats.onPlayEvent = MegaponBattlecry;

        stats.imagePath = "megapon";
        return stats;
    }
}
