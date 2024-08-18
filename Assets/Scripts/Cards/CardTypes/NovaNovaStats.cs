using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaNovaStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        const int _healthLoss = 2;
        CardManager.CardStats stats = new CardManager.CardStats();
        stats.name = "Nova Nova";
        stats.description = "Destroy all non-Hatapon units. Deal " + _healthLoss.ToString() + " damage to your Hatapon for each unit destroyed this way.";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);
        //stats.legendary = true;

        stats.isSpell = true;
        static IEnumerator NovaNovaRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            //AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            //Vector3 position = new Vector3(0f, 0f, - 0.5f);
            //GameObject explosion = animationManager.CreateObject(AnimationManager.Animations.NovaNova, position);

            //yield return new WaitForSeconds(1.5f);
            int healthLoss = 0;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
                    healthLoss += _healthLoss;
                }
            }

            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                {
                    minion.DestroyMinion();
                    healthLoss += _healthLoss;
                }
            }
            
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardType() == CardTypes.Hatapon)
                {
                    minion.LoseLife(healthLoss);
                }
            }

            gameController.actionIsHappening = false;
            //yield return new WaitForSeconds(2f);
            //Destroy(explosion);
            yield return null;
            
        }

        stats.spell = NovaNovaRealization;
        stats.numberOfTargets = 0;
        stats.imagePath = "NovaNova";
        return stats;
    }
}
