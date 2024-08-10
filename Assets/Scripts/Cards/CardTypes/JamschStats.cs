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
        stats.description = "<b>On play: Poison</b> all enemy units (<b>Poisoned</b> units receive 1 damage at the start their controller's turn).";
        stats.name = "Jamsch";
        
        stats.additionalKeywords.Add("Poison");

        stats.onPlaySound = "jamsch_on_play";

        static IEnumerator Realization(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            int index;
            List<BoardManager.Slot> chooseSlots;
            List<BoardManager.Slot> oppSlots;
            if (target > 0)
            {
                chooseSlots = friendlySlots;
                oppSlots = enemySlots;
                index = target - 1;
            }
            else
            {
                chooseSlots = enemySlots;
                oppSlots = friendlySlots;
                index = -target - 1;
            }

            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            Vector3 position = new Vector3(chooseSlots[index].GetPosition().x,chooseSlots[index].GetPosition().y, chooseSlots[index].GetPosition().z - 0.5f);
            GameObject spores = animationManager.CreateObject(AnimationManager.Animations.Spores, position);
            if (target < 0)
            {
                spores.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }

            yield return new WaitForSeconds(1.5f);
            foreach (BoardManager.Slot slot in oppSlots)
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

        stats.hasAfterPlayEvent = true;
        stats.afterPlayEvent = Realization;
        //stats.numberOfTargets = 1;
        //stats.dummyTarget = true;

        stats.imagePath = "jamsch_final";
        stats.artistName = "korka123";
        return stats;
    }
}
