using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TonKamponStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        stats.power = 9;
        stats.description = "<b>Pacifism. Abilities</b>:\n-4: Add a random weapon to your hand.\n-5: Double the power of another weakest non-Hatapon unit you controll.";
        stats.name = "Ton Kampon";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);

        stats.descriptionSize = 3;

        stats.relevantCards.Add(CardTypes.Alldemonium);
        stats.relevantCards.Add(CardTypes.CronoRiggers);
        stats.relevantCards.Add(CardTypes.TakeThatShield);


        stats.isStatic = true;
        stats.connectedCards = new List<CardTypes>();
        stats.connectedCards.Add(CardTypes.TonKampon_option1);
        stats.connectedCards.Add(CardTypes.TonKampon_option2);

        stats.pacifism = true;

        stats.imagePath = "ton_kampon";

        return stats;
    }
}

public static class TonKampon_option2Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int TonKamponAlldemoniumHealthCost = 5;
                
        stats.description = "-5: Double the power of another weakest non-Hatapon unit you controll.";
        stats.name = "Craft an Alloy";
        stats.nameSize = 4;

        stats.isSpell = true;
        static IEnumerator TonKampon_option2Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.LoseLife(TonKamponAlldemoniumHealthCost);
            

            MinionManager weakMinion = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion != host)
                {
                    if ((weakMinion == null || minion.GetPower() < weakMinion.GetPower()) && minion.GetCardType() != CardTypes.Hatapon)
                    {
                        weakMinion = minion;
                    }
                }
            }
            weakMinion.Heal(weakMinion.GetPower());

            gameController.actionIsHappening = false;
            yield return null;
        }
        stats.spell = TonKampon_option2Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = TonKamponAlldemoniumHealthCost;

        stats.imagePath = "ton_kampon";

        return stats;
    }
}

public static class TonKampon_option1Stats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int TonKamponCronoRiggersHealthCost = 4;
                
        stats.description = "-4: Add random weapon to your hand.";
        stats.name = "Craft a Weapon";
        stats.nameSize = 4;

        stats.isSpell = true;
        static IEnumerator TonKampon_option1Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            List<CardTypes> drop = new List<CardTypes>(){CardTypes.CronoRiggers, CardTypes.Alldemonium, CardTypes.TakeThatShield};

            MinionManager host;
            if (targets[0] < 0)
            {
                host = enemySlots[targets[0] * (-1) - 1].GetConnectedMinion();
            }
            else
            {
                host = friendlySlots[targets[0] - 1].GetConnectedMinion();
            }

            host.LoseLife(TonKamponCronoRiggersHealthCost);
            if (!enemySlots[0].GetFriendly())
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.AddCardToHand(drop[UnityEngine.Random.Range(0, drop.Count)]);
            }
            else
            {
                HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                handManager.DrawCardOpponent(fromDeck:false);
                //handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
            }
            yield return null;
        }
        stats.spell = TonKampon_option1Realization;
        stats.numberOfTargets = 0;
        stats.damageToHost = TonKamponCronoRiggersHealthCost;
        stats.imagePath = "ton_kampon";

        return stats;
    }
}


public static class CronoRiggersStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int cronoRiggersDamageReduction = 2;
        const int threshold = 8;
        stats.description = "All units under your controll gain +" + cronoRiggersDamageReduction.ToString() + " power. Then, your strongest non-Hatapon unit attacks the strongest enemy unit.";
        stats.name = "Oharan";
        stats.runes.Add(Runes.Shield);
        stats.runes.Add(Runes.Shield);
        
        stats.descriptionSize = 3;

        stats.isSpell = true;

        stats.additionalRules.Add("If the attacking unit has an <b>On attack</b> effect, it will trigger.");

        static IEnumerator CronoRiggersRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;


            MinionManager attackMinion = null;
            foreach (BoardManager.Slot slot in friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    minion.Heal(cronoRiggersDamageReduction);
                    if ((attackMinion == null || minion.GetPower() > attackMinion.GetPower()) && minion.GetCardType() != CardTypes.Hatapon && minion.GetCardStats().canDealDamage)
                    {
                        attackMinion = minion;
                    }
                }
            }

            MinionManager attackEnemyMinion = null;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null)
                {
                    //minion.Heal(cronoRiggersDamageReduction);
                    if (attackEnemyMinion == null || minion.GetPower() >= attackEnemyMinion.GetPower())
                    {
                        attackEnemyMinion = minion;
                    }
                }
            }

            if (attackEnemyMinion != null && attackMinion != null)
            {
                attackMinion.Attack(attackEnemyMinion);
            }

            gameController.actionIsHappening = false;
            yield return null;
        }

        stats.spell = CronoRiggersRealization;
        
        stats.imagePath = "oharan";

        return stats;
    }
}


public static class AlldemoniumStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int alldemonuimGain = 5;
        const int alldemonuimDamage = 1;
        stats.description = "Target non-Hatapon character under your controll gains +" + alldemonuimGain.ToString() + " power, but recieves " + alldemonuimDamage.ToString() + " damage at the end of your turn.";
        stats.name = "Alldemonium Shield";
        stats.nameSize = 3;
        stats.runes.Add(Runes.Shield);

        stats.isSpell = true;

        static IEnumerator AlldemoniumEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager connectedMinion = friendlySlots[index].GetConnectedMinion();
            if (connectedMinion != null)
            {
                connectedMinion.ReceiveDamage(alldemonuimDamage);
            }
            //GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = false;
            yield return null;
        }

        static IEnumerator AlldemoniumRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
            yield return null;
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
        stats.artistName = "Pavel Shpagin (Poki)";

        return stats;
    }
}