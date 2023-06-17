using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TonKamponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 6;
        stats.description = "-1: Add Crono Riggers to your hand.\n-2: Add Alldemonium to your hand.";
        stats.name = "Ton Kampon";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);


        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.TonKampon_option1);
        stats.connectedCards.Add(CardTypes.TonKampon_option2);

        return stats;
    }
}

public static class TonKampon_option2Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int TonKamponAlldemoniumHealthCost = 2;
                
        stats.description = "Add Alldemonium to your hand.";
        stats.name = "Demon Weapon";

        stats.isSpell = true;
        static void TonKampon_option2Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.TakePower(TonKamponAlldemoniumHealthCost);
            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.AddCardToHand(CardTypes.Alldemonium);
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
            }
        }
        stats.spell = TonKampon_option2Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = TonKamponAlldemoniumHealthCost;

        stats.imagePath = "alldemonium";

        return stats;
    }
}

public static class TonKampon_option1Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int TonKamponCronoRiggersHealthCost = 1;
                
        stats.description = "Add Crono Riggers to your hand.";
        stats.name = "Divine Weapon";

        stats.isSpell = true;
        static void TonKampon_option1Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.TakePower(TonKamponCronoRiggersHealthCost);
            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.AddCardToHand(CardTypes.CronoRiggers);
            }
            else
            {
                //Debug.Log("Here");
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                //Debug.Log(handManager.GetNumberOfOpponentsCards());
                handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
            }
        }
        stats.spell = TonKampon_option1Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = TonKamponCronoRiggersHealthCost;
        stats.imagePath = "crono_riggers";

        return stats;
    }
}


public static class CronoRiggersStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int cronoRiggersDamageReduction = 1;
        stats.description = "Target creature under your controll gains: \"Receive " + cronoRiggersDamageReduction.ToString() + " less damage from any source\".";
        stats.name = "Crono Riggers";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.isSpell = true;

        static bool CronoRiggersCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            return true;
        }

        static void CronoRiggersRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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

            CardManager.CardStats stats = targetMinion.GetCardStats();
            stats.armor += cronoRiggersDamageReduction;
            targetMinion.SetCardStats(stats);

        }

        stats.spell = CronoRiggersRealization;
        stats.checkSpellTarget = CronoRiggersCheckTarget;
        stats.numberOfTargets = 1;
        
        stats.imagePath = "crono_riggers";

        return stats;
    }
}


public static class AlldemoniumStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int alldemonuimGain = 4;
        const int alldemonuimDamage = 2;
        stats.description = "Target creature under your controll gains " + alldemonuimGain.ToString() + " power, but recieves " + alldemonuimDamage.ToString() + " at the end of your turn.";
        stats.name = "Alldemonium Shield";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.isSpell = true;

        static IEnumerator AlldemoniumEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            MinionManager connectedMinion = friendlySlots[index].GetConnectedMinion();
            if (connectedMinion != null)
            {
                connectedMinion.ReceiveDamage(alldemonuimDamage);
            }
            yield return null;
        }

        static void AlldemoniumRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
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
            targetMinion.Heal(alldemonuimGain);

            CardManager.CardStats stats = targetMinion.GetCardStats();
            stats.additionalEndTurnEvents.Add(AlldemoniumEndTurn);
            //targetMinion.SetCardStats(stats);

        }

        static bool AlldemoniumCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            if (_target < 0)
            {
                return false;
            }
            BoardManager.Slot _targetSlot;

            if (_target > 0)
            {
                _targetSlot = friendlySlots[_target - 1];
            }
            else
            {
                _targetSlot = enemySlots[-_target - 1];
            }
            MinionManager _targetMinion = _targetSlot.GetConnectedMinion();
            if (_targetMinion.GetCardType() == CardTypes.Hatapon)
            {
                return false;
            }
            return true;
        }

        stats.spell = AlldemoniumRealization;
        stats.checkSpellTarget = AlldemoniumCheckTarget;
        stats.numberOfTargets = 1;

        stats.imagePath = "alldemonium";

        return stats;
    }
}