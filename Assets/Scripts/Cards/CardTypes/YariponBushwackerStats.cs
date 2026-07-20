using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class YariponBushwackerStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int destroboDamage = 1;
        stats.power = 2;
        stats.description = "<b>Haste</b>.\n<b>On play</b>: Deal " + destroboDamage.ToString() +  " damage.";
        stats.name = "Fah Zakpon";
        stats.hasHaste = true;
        stats.runes.Add(Runes.Spear);
        stats.nameSize = 4;
        stats.hasOnPlaySpell = true;

        stats.onPlaySound = "fah_z";

        // targets[0] is auto-filled by dummyTarget with Fah Zakpon's own intended slot as a raw
        // board index (not the +-1 interactive-click convention) - see Aiton.cs for the same
        // pattern - used so the spear throw starts from his own position instead of a fixed slot.
        // targets[1] is the actual damage target the player clicked, using the +-1 convention.
        static IEnumerator DestroboRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot selectedSlot;
            int target = targets[1];
            if (target < 0)
            {
                target *= -1;
                target -= 1;
                selectedSlot = enemySlots[target];
            }
            else
            {
                target -= 1;
                selectedSlot = friendlySlots[target];
            }

            MinionManager selectedMinion = selectedSlot.GetConnectedMinion();
            BoardManager.Slot ownSlot = friendlySlots[targets[0]];

            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, ownSlot.GetPosition()).GetComponent<SpearManager>();
            spear.SetSlotToGo(selectedSlot);
            if (selectedSlot.GetFriendly())
            {
                spear.isEnemy = true;
            }

            while (!spear.reachDestination)
            {
                yield return new WaitForSeconds(0.1f);
            }

            AudioController.PlaySound("spear");
            spear.DestroySelf();

            selectedMinion.ReceiveDamage(destroboDamage);
            
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool DestroboCheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            // No restriction - any minion, friend or foe (the raycast + hexproof check at the
            // click site already filters out invalid clicks before this ever runs).
            return true;
        }

        static bool DestroboCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            return true;
        }

        stats.spell = DestroboRealization;
        stats.checkSpellTarget = DestroboCheckTarget;
        stats.checkSpellTargets = DestroboCheckTargets;
        // 2, not 1 - dummyTarget auto-fills targets[0] with Fah Zakpon's own slot the instant he's
        // dropped, so numberOfTargets has to also account for the one real damage target the
        // player still needs to click (see Aiton.cs, which uses the same dummyTarget + 2 pairing).
        stats.numberOfTargets = 2;
        stats.dummyTarget = true;

        stats.imagePath = "farmer";
        stats.artistName = "Evgeney Bogatyrev";        
        return stats;
    }
}
