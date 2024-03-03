using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorRainStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        const int damage = 1;
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Meteor Rain";
        stats.description = "Deal " + damage.ToString() + " damage to all units.";
        
        //stats.legendary = true;

        stats.isSpell = true;
        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.ReceiveDamage(damage);
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.ReceiveDamage(damage);
                }
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = Realization;
        stats.numberOfTargets = 0;
        stats.imagePath = "meteor_rain";
        return stats;
    }
}
