using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyShotStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int threshold = 5;

        stats.description = "Destroy target non-Hatapon unit with power " + threshold.ToString() + " or less.";
        stats.name = "Deadly Shot";

        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Bow);

        stats.isSpell = true;
        static IEnumerator realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            BoardManager.Slot targetSlot;

            int target = targets[0];
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            if (targetMinion != null)
            {

                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                    
                // Main projectile
                SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[3].GetPosition()).GetComponent<SpearManager>();
                spear.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/cannonball_aligned_medium");
                spear.SetSlotToGo(targetMinion.GetSlot());
                spear.speed = 20f;
                spear.rotate = false;
                spear.constantSpeed = true;

                List <SpearManager> ghostly = new();
                for (int i = 0; i < 3; ++i)
                {
                    yield return new WaitForSeconds(0.03f);
                    SpearManager ghost = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[3].GetPosition()).GetComponent<SpearManager>();
                    ghost.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/cannonball_aligned_medium");  
                    var tmp = ghost.gameObject.transform.GetComponent<SpriteRenderer>().color;
                    tmp.a = 0.7f - i * 0.2f;
                    ghost.gameObject.transform.GetComponent<SpriteRenderer>().color = tmp;
                    ghost.SetSlotToGo(targetMinion.GetSlot());
                    ghost.speed = 20f;
                    ghost.rotate = false;
                    ghost.constantSpeed = true;
                    ghostly.Add(ghost);
                }

                while (!spear.reachDestination)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                targetMinion.DestroyMinion();
                spear.DestroySelf(); 
                foreach (SpearManager ghost in ghostly)
                {
                    ghost.DestroySelf();
                }                   
                
            }
            gameController.actionIsHappening = false;
            yield return null;
        }

        static bool CheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            BoardManager.Slot targetSlot;
            if (target > 0)
            {
                targetSlot = friendlySlots[target - 1];
            }
            else
            {
                targetSlot = enemySlots[-target - 1];
            }

            MinionManager targetMinion = targetSlot.GetConnectedMinion();
            if (targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            if (targetMinion.GetPower() > threshold)
            {
                return false;
            }
            return true;
        }
        
        stats.checkSpellTarget = CheckTarget;

        stats.spell = realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "deadly_shot";
        return stats;
    }
}
