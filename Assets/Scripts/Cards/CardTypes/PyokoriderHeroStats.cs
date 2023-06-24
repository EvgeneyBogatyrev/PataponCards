using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PyokoriderHeroStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int pyokoDamage = 3;

        stats.power = 3;
        stats.description = "At the end of your turn deal " + pyokoDamage.ToString() + " damage to the right-most enemy.";
        stats.name = "Pyokorider, hero";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.nameSize = 4;


        static IEnumerator PyokoriderHeroEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager connectedMinion = null;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot.GetConnectedMinion() != null)
                {
                    connectedMinion = slot.GetConnectedMinion();
                }
            }
            
            if (connectedMinion != null)
            {
                connectedMinion.ReceiveDamage(pyokoDamage);
            }
            yield return null;
        }
        stats.endTurnEvent = PyokoriderHeroEndTurn;

        stats.imagePath = "pyokorider_hero";

        return stats;
    }
}
