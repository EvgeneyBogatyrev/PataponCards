using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AitonStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.power = 1;
        stats.description = "<b>On play</b>: Switch places with a friendly unit.";
        stats.name = "Aiton";
        stats.runes.Add(Runes.Spear);
        stats.nameSize = 4;
        stats.hasOnPlaySpell = true;

        // On-play spells resolve BEFORE the card is actually placed on the board (BoardManager.
        // PlayCard runs afterward, once this coroutine finishes) - see BowmunkStats for the same
        // pattern. targets[0] is auto-filled by dummyTarget with Aiton's own intended slot as a
        // raw board index (not the +-1 interactive-click convention). targets[1] is the friendly
        // unit the player clicked to swap with, using that +-1 convention like any other click.
        static IEnumerator AitonRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            BoardManager.Slot aitonSlot = friendlySlots[targets[0]];
            BoardManager.Slot swapSlot = friendlySlots[targets[1] - 1];
            MinionManager swapMinion = swapSlot.GetConnectedMinion();

            if (swapMinion != null && CardManager.CurrentlyResolvingOnPlayCard != null)
            {
                // Move the target unit into Aiton's original destination, then redirect Aiton's
                // own destination to the slot that unit just vacated - the two end up having
                // genuinely swapped places once BoardManager.PlayCard runs.
                swapMinion.SetSlot(aitonSlot);
                CardManager.CurrentlyResolvingOnPlayCard.SetSlotToPlay(swapSlot);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool AitonCheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            // Friendly only (positive) - the interactive-click raycast only ever hits existing
            // minion colliders, so an occupied-slot check isn't needed here.
            return target > 0;
        }

        static bool AitonCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            return true;
        }

        stats.spell = AitonRealization;
        stats.checkSpellTarget = AitonCheckTarget;
        stats.checkSpellTargets = AitonCheckTargets;
        stats.numberOfTargets = 2;
        stats.dummyTarget = true;

        stats.imagePath = "aiton";
        stats.artistName = "Official render";        
        return stats;
    }
}
