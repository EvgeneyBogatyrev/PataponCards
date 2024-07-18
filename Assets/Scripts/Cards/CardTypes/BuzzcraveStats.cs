using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuzzcraveStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Haste</b>.\nCan attack any enemy unit on the board.";
        stats.name = "Buzzcrave";
        stats.megaVision = true;
        stats.hasHaste = true;
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        
        stats.additionalRules.Add("<i>Buzzcrave</i> can attack any unit on the board unless it's not a legal target for attack. Damage prevention still applies.");

        stats.imagePath = "buzzcrave_updated";
        stats.onPlaySound = "buzzcrave_on_play";
        stats.onDeathSound = "dark_hero_death";
        stats.artistName = "Pavel Shpagin (Poki)";
        return stats;
    }
}
