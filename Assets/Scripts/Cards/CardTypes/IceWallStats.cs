using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IceWallStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 3;
        stats.description = "Pacifism. Abilities:\n-0: Deal 1 damage to this.";
        stats.name = "Ice Wall";

        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.IceWall_option);
        stats.connectedCards.Add(CardTypes.IceWall_option);

        stats.imagePath = "ice_wall";

        return stats;
    }
}
