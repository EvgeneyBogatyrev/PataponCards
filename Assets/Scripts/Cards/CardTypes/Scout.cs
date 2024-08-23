using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScoutStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 2;
        stats.description = "<b>End of turn</b>: Draw a card. Your Hatapon loses 1 life.";
        stats.name = "Megapon";
        stats.imagePath = "Megapon";
        
        stats.runes.Add(Runes.Bow);
        //stats.runes.Add(Runes.Bow);

        stats.additionalRules.Add("Damage happens even if you didn't draw a card for any reason.");


        static IEnumerator EndTurn(int thisIndex, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;
            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();

            int cardsDrawn = 0;

            BoardManager.Slot thisSlot;
            thisSlot = friendlySlots[thisIndex];
           
            MinionManager minion = thisSlot.GetConnectedMinion();
            if (minion != null)
            { 
               minion.GetCardStats().cardsDrawnByThis = minion.GetCardStats().cardsDrawnByThis + 1;
               Debug.Log(minion.GetCardStats().cardsDrawnByThis);
               cardsDrawn = minion.GetCardStats().cardsDrawnByThis;
            }

            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion_ = slot.GetConnectedMinion();
                if (minion_ != null && minion_.GetCardType() == CardTypes.Hatapon)
                {
                    SpearManager soundMain = animationManager.CreateObject(AnimationManager.Animations.Spear, thisSlot.GetPosition()).GetComponent<SpearManager>();
                    string imagePath = "Images/megapon_large";
                    soundMain.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(imagePath);
                    soundMain.SetSlotToGo(slot);
                    soundMain.rotate = false;
                    soundMain.speed = 20f;
                    soundMain.bounce = true;
                    if (enemySlots[0].GetFriendly())
                    {
                        soundMain.isEnemy = true;
                    }

                    while (!soundMain.reachDestination)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    soundMain.DestroySelf();
                    minion_.DealDamageToThis(1);
                    break;
                }
            }
            
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = EndTurn;
        stats.artistName = "Official render";

        return stats;
    }
}
