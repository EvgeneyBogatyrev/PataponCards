using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class YariponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int yariponDamage = 2;

        stats.power = 3;
        stats.description = "<b>End of turn</b>: Deal " + yariponDamage.ToString() + " damage to an enemy unit next to it.";
        stats.name = "Yaripon";
        stats.imagePath = "yaripon";
        
        stats.runes.Add(Runes.Spear);

        static IEnumerator YariponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
            gameController.actionIsHappening = true;


            // Throw spear
            MinionManager connectedMinion = enemySlots[index].GetConnectedMinion();
            if (connectedMinion != null)
            {
                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                spear.SetSlotToGo(enemySlots[index]);
                if (enemySlots[index].GetFriendly())
                {
                    spear.isEnemy = true;
                }

                while (!spear.reachDestination)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                spear.DestroySelf();

                
                connectedMinion.ReceiveDamage(yariponDamage);
            }
            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = YariponEndTurn;

        return stats;
    }
}
