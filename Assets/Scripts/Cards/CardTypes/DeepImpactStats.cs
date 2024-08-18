using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class DeepImpactStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int horsePower = 3;
        stats.description = "Transform target non-Hatapon unit into the Horserider with " + horsePower.ToString() + " and <b>Haste</b>.\n Draw a card.";
        stats.name = "Bullgam the Bully";
        stats.nameSize = 4;
    
        stats.runes.Add(Runes.Spear);
        //stats.runes.Add(Runes.Spear);

        stats.relevantCards.Add(CardTypes.Horserider);

        stats.isSpell = true;
        static IEnumerator realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
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
            
            CardTypes cardType = targetMinion.GetCardType();
            CardManager cardIntoDeck = handManager.GenerateCard(cardType).GetComponent<CardManager>();
            cardIntoDeck.SetCardState(CardManager.CardState.ShuffleIntoDeck);

            bool opp;
            if (enemySlots[1].GetFriendly())
            {
                opp = true;
            }
            else
            {
                opp = false;
            }

            CardManager newCard = handManager.GenerateCard(CardTypes.Horserider, new Vector3(-10f, -10f, 1f)).GetComponent<CardManager>();
            newCard.SetPower(horsePower);
            BoardManager.Slot slot = targetSlot;
            //host.TakePower(host.GetPower());
            targetSlot.GetConnectedMinion().DestroySelf(unattach:true);
            boardManager.PlayCard(newCard, new Vector3(0f, 0f, 0f), slot, destroy: true, record: false);

            if (friendlySlots[1].GetFriendly())
            {
                //HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCard();
            }
            else
            {
                //HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent();
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
            return true;
        }
        
        stats.checkSpellTarget = CheckTarget;

        stats.spell = realization;
        stats.numberOfTargets = 1;

        stats.imagePath = "Bullgam_the_Bully";
        stats.artistName = "Pavel Shpagin (Poki)";
        return stats;
    }
}


public static class HorseriderStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 3;
        stats.description = "<b>Haste</b>.";
        stats.name = "Horserider";
        stats.runes.Add(Runes.Spear);

        stats.hasHaste = true;

        stats.imagePath = "kiba_art";
        stats.artistName = "Pavel Shpagin (Poki)";
        return stats;
    }
}
