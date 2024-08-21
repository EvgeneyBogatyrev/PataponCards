using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoomShroomStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int damage = 2;
        stats.description = "Deal " + damage.ToString() + " damage to a unit. If that unit dies, summon a Poisonous Mushroom on its place.\n<b>Cycling</b>.";
        stats.name = "Doom Shroom";

        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Bow);

        stats.cycling = true;

        stats.additionalKeywords.Add("Poison");

        stats.relevantCards.Add(CardTypes.Mushroom);

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
            targetMinion.ReceiveDamage(damage);
            if (targetMinion.GetPower() <= 0)
            {
                yield return new WaitForSeconds(1.2f);
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                CardManager card = handManager.GenerateCard(CardTypes.Mushroom, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
                
                boardManager.PlayCard(card, new Vector3(0f, 0f, 0f), targetSlot, destroy: false, record: false);
                card.DestroyCard();
     
            }
            gameController.actionIsHappening = false;
            yield return null;
        }


        stats.spell = realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "doom-shroom";
        stats.artistName = "Pavel Shpagin (Poki)";
        return stats;
    }
}
