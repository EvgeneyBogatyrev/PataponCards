using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DonTheYumiponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 2;

        stats.power = 2;
        stats.description = "You can <b>cycle</b> any number of cards per turn.\nWhenever you <b>cycle</b> a card, deal " + damage.ToString() + " damage to the weakest enemy unit.";
        stats.name = "Don Yumipon";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.descriptionSize = 3;
        //stats.nameSize = 5;

        stats.additionalKeywords.Add("Cycling");
        stats.suppressOnPlay = true;

        stats.hasOnPlaySpell = true;
        stats.spell = OnPlay;

        static IEnumerator OnPlay(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            if (friendlySlots[1].GetFriendly())
            {
                handManager.SetCanCycleCard(true);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator OnCycleOther(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            if (friendlySlots[1].GetFriendly())
            {
                handManager.SetCanCycleCard(true);
            }
            
            MinionManager targetMinion = null;
            int _index = 0;
            int chosenIndex = -1;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    if (targetMinion == null || minion.GetPower() < targetMinion.GetPower())
                    {
                        targetMinion = minion;
                        chosenIndex = _index;
                    }
                }
                _index += 1;
            }

            if (targetMinion != null)
            {
                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                spear.SetSlotToGo(enemySlots[chosenIndex]);
                if (enemySlots[chosenIndex].GetFriendly())
                {
                    spear.isEnemy = true;
                }

                while (!spear.reachDestination)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                spear.DestroySelf();

                MinionManager connectedMinion = enemySlots[chosenIndex].GetConnectedMinion();
                if (connectedMinion != null)
                {
                    connectedMinion.ReceiveDamage(damage);
                }
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.onCycleOtherEvent = OnCycleOther;

        stats.imagePath = "don_yumipon_final";
        stats.artistName = "korka123";
        return stats;
    }
}
