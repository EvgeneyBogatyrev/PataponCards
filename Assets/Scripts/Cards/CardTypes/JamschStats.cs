using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JamschStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.power = 1;
        stats.description = "On play: Poison all enemy units (Poisoned units receive 1 damage at the end thier controllers' turn).";
        stats.name = "Jamsch";
        

        static IEnumerator Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            Vector3 position = new Vector3(friendlySlots[targets[0]].GetPosition().x, friendlySlots[targets[0]].GetPosition().y, friendlySlots[targets[0]].GetPosition().z - 0.5f);
            GameObject spores = animationManager.CreateObject(AnimationManager.Animations.Spores, position);
            if (enemySlots[0].GetFriendly())
            {
                spores.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }

            yield return new WaitForSeconds(1.5f);
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.PoisonMinion();
                }
            }
            yield return new WaitForSeconds(3f);
            Destroy(spores);
            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.hasOnPlaySpell = true;
        stats.spell = Realization;
        stats.numberOfTargets = 1;
        stats.dummyTarget = true;

        stats.imagePath = "jamsch";
        return stats;
    }
}
