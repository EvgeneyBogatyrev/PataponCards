using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CardTypes
{
    Tatepon,
    Yaripon,
    Yumipon,
    Kibapon,
    Dekapon,
    DivineProtection,
    Fang,
    DeadlyDispute,
    Mahopon,
    Pyokorider,
    Megapon,
    FuckingIdiot,
    Buzzcrave,
    Guardira,
    Kacheek,
    Motiti,
    Robopon,
    Myamsar,
    Toripon,
    TargetDummy,
    Bowmunk,
    Grenburr,
    Alldemonium,
    CronoRiggers,
    TakeThatShield,
    ProfessionalWIthStandards,
    SpeedBoost,
    MyamsarHero,
    Destrobo,
    Buruch,
    TonKampon,
    Coppen,
    Alosson,
    PyokoriderHero,
    Baloon,
    NovaNova,
    BanTatepon,
    TurnToStone,
    //------------------------
    Motiti_option1,
    Motiti_option2,
    MotitiAngry,
    Nutrition,
    GiveFang,
    Hatapon,
    Boulder,
    TonKampon_option1,
    TonKampon_option2,
    IceWall,
    IceWall_option,
    Concede,
    StoneFree,
};

//Rewrite this entire piece of sheesh
public static class CardGenerator
{
    public static void CustomizeCard(CardManager card, CardTypes cardType)
    {
        card.SetCardType(cardType);
        CardManager.CardStats stats = new CardManager.CardStats();

        static IEnumerator EmptyMethod(int index, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { yield return null; }
        static void EmptyMethod_(int index, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { }
        static void EmptySpell(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { }
        static bool EmptyCheckSpellTargets(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }
        static bool EmptyCheckSpellTarget(int target, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }
        //static bool EmptyCondition(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }

        stats.onPlayEvent = EmptyMethod_;
        stats.endTurnEvent = EmptyMethod;
        stats.startTurnEvent = EmptyMethod;
        //card.endOpponentsTurnEvent = EmptyMethod;
        stats.spell = EmptySpell;
        stats.checkSpellTarget = EmptyCheckSpellTarget;
        stats.checkSpellTargets = EmptyCheckSpellTargets;
        //card.condition = EmptyCondition;
        //card.heroMode = EmptyMethod;

        switch (cardType)
        {
            case CardTypes.Hatapon:
                stats = HataponStats.GetStats();
                break;

            case CardTypes.Tatepon:
                stats.power = 4;
                stats.description = "Your Hatapon is immune.";
                stats.name = "Tatepon";
                stats.hasShield = true;
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.Yaripon:
                stats = YariponStats.GetStats();
                break;

            case CardTypes.Yumipon:
                const int yumiponDamage = 1;

                stats.power = 2;
                stats.description = "At the end of your turn deal " + yumiponDamage.ToString() + " damage to all enemys.";
                stats.name = "Yumipon";
                stats.runes.Add(Runes.Bow);

                static IEnumerator YumiponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
                    gameController.actionIsHappening = true;
                    
                    AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                    List<SpearManager> spearArray = new List<SpearManager>();
                    
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (!slot.GetFree())
                        {
                            //slot.GetConnectedMinion().GetDamage(yumiponDamage);
                            SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                            spear.SetSlotToGo(slot);
                            spearArray.Add(spear);
                            if (enemySlots[index].GetFriendly())
                            {
                                spear.isEnemy = true;
                            }
                        }
                    }

                    bool arrowsExists = true;

                    while (arrowsExists)
                    {
                        arrowsExists = false;

                        foreach (SpearManager spear in spearArray)
                        {
                            if (!spear.reachDestination)
                            {
                                arrowsExists = true;
                            }
                            else if (!spear.exhausted)
                            {
                                spear.DestroySelf();
                                spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(yumiponDamage);
                                spear.exhausted = true;
                            }
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                        

                    gameController.actionIsHappening = false;
                    yield return null;
                }
                stats.endTurnEvent = YumiponEndTurn;
                break;

            case CardTypes.Kibapon:
                stats.power = 4;
                stats.description = "Haste.";
                stats.name = "Kibapon";
                stats.hasHaste = true;
                //stats.hasShield = true;
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Dekapon:
                stats.power = 7;
                stats.description = "Can not move.";
                stats.name = "Dekapon";
                stats.limitedVision = true;
                stats.runes.Add(Runes.Shield);
                break;


            case CardTypes.DivineProtection:
                const int divineProtectionTateponCount = 3;
                const int divineProtectionTateponPower = 2;

                stats.description = "Summon " + divineProtectionTateponCount.ToString() + " Tatepons with " + divineProtectionTateponPower.ToString() + " power.";
                stats.name = "Divine Protection";
                card.SetNameSize(4);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                stats.isSpell = true;
                static void DivineProtectionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                    CardManager tateponCard = handManager.GenerateCard(CardTypes.Tatepon).GetComponent<CardManager>();
                    tateponCard.SetPower(divineProtectionTateponPower);
                    
                    int count = 0;
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        if (count >= divineProtectionTateponCount)
                        {
                            break;
                        }
                        if (slot.GetFree())
                        {
                            boardManager.PlayCard(tateponCard, slot, destroy:false, record:false);
                            count += 1;
                        }
                    }

                    tateponCard.DestroyCard();
                }

                stats.spell = DivineProtectionRealization;
                stats.numberOfTargets = 0;

                break;

            case CardTypes.Fang: 
                const int fangDamage = 3;
                stats.description = "Deal " + fangDamage.ToString() + " damage to an enemy.";
                stats.name = "Fang";

                stats.isSpell = true;
                static void FangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
                    targetMinion.ReceiveDamage(fangDamage);

                }

                static bool FangCheckTarget(int target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (target > 0)
                    {
                        return false;
                    }
                    return true;
                }

                stats.spell = FangRealization;
                stats.checkSpellTarget = FangCheckTarget;
                stats.numberOfTargets = 1;

                break;

            case CardTypes.Nutrition:
                const int nutritionHeal = 1;
                const int nutritionHealthCost = 1;
                
                stats.description = "Restore " + nutritionHeal.ToString() + " power to all allies.";
                stats.name = "Nutrition";

                stats.isSpell = true;
                
                static void NutritionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    host.TakePower(nutritionHealthCost);

                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null && minion != host)
                        {
                            minion.Heal(nutritionHeal);
                        }
                    }
                }
                stats.spell = NutritionRealization;
                stats.numberOfTargets = 0;
                stats.damageToHost = nutritionHealthCost;
                break;

            case CardTypes.DeadlyDispute:
                stats.description = "Choose 2 creatures. They fight each other.";
                stats.name = "Deadly Dispute";
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                stats.isSpell = true;
                static void DeadlyDisputeRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    List<MinionManager> minions = new List<MinionManager>();

                    for (int i = 0; i < 2; ++i)
                    {
                        int target = targets[i];
                        if (target < 0)
                        {
                            target *= -1;
                            target -= 1;
                            minions.Add(enemySlots[target].GetConnectedMinion());
                        }
                        else
                        {
                            target -= 1;
                            minions.Add(friendlySlots[target].GetConnectedMinion());
                        }
                    }

                    minions[0].Attack(minions[1]);
                }

                static bool DeadlyDisputeCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (targets[0] == targets[1])
                    {
                        return false;
                    }
                    return true;
                }

                stats.spell = DeadlyDisputeRealization;
                stats.checkSpellTargets = DeadlyDisputeCheckTargets;
                stats.numberOfTargets = 2;
                break;

            case CardTypes.Mahopon:
                const int mahoponTargetDamage = 3;
                const int mahoponAoEDamage = 2;
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);

                stats.power = 2;
                stats.description = "Deal " + mahoponTargetDamage.ToString() + " damage to target creature and " + mahoponAoEDamage.ToString() + " damage to all other creatures.";
                stats.name = "Mahopon";

                stats.hasOnPlay = true;

                static void MahoponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    BoardManager.Slot selectedSlot;
                    int target = targets[0];
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

                    selectedSlot.GetConnectedMinion().ReceiveDamage(mahoponTargetDamage);
                    
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (slot != selectedSlot && !slot.GetFree())
                        {
                            slot.GetConnectedMinion().ReceiveDamage(mahoponAoEDamage);
                        }
                    }

                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        if (slot != selectedSlot && !slot.GetFree())
                        {
                            slot.GetConnectedMinion().ReceiveDamage(mahoponAoEDamage);
                        }
                    }
                }

                stats.spell = MahoponRealization;
                stats.numberOfTargets = 1;

                break;

            case CardTypes.Pyokorider:
                const int pyokoriderStartTurnPower = 2;

                stats.power = 5;
                stats.description = "Haste.\nAt the start of your turn set this creature's power to " + pyokoriderStartTurnPower.ToString();
                stats.name = "Pyokorider";
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                static IEnumerator PyokoriderStartTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    friendlySlots[index].GetConnectedMinion().SetPower(pyokoriderStartTurnPower);
                    yield return null;
                }

                stats.startTurnEvent = PyokoriderStartTurn;
                stats.hasHaste = true;

                break;

            case CardTypes.FuckingIdiot:
                const int burusSelfDamage = 1;
                stats.power = 2;
                stats.description = "At the end of your turn draw a card and deal " + burusSelfDamage.ToString() + " damage to itself.";
                stats.name = "Fucking Idiot";

                static IEnumerator FuckingIdiotEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (!enemySlots[0].GetFriendly())
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.DrawCard();
                    }
                    else
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                    }
                    friendlySlots[index].GetConnectedMinion().ReceiveDamage(burusSelfDamage);
                    yield return null;
                }

                stats.endTurnEvent = FuckingIdiotEndTurn;

                break;

            case CardTypes.Megapon:
                stats.power = 2;
                stats.description = "Deal two damage split between 1 or 2 creatures.\nDraw a card.";
                stats.name = "Megapon";
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);


                stats.hasBattlecry = true;
                stats.hasOnPlay = true;

                static void MegaponBattlecry(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (index > 0)
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.DrawCard();
                    }
                }

                static void MegaponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (targets[0] == targets[1])
                    {
                        BoardManager.Slot selectedSlot;
                        if (targets[0] < 0)
                        {
                            selectedSlot = enemySlots[targets[0] * (-1) - 1];
                        }
                        else
                        {
                            selectedSlot = friendlySlots[targets[0] - 1];
                        }

                        selectedSlot.GetConnectedMinion().ReceiveDamage(2);
                    }
                    else
                    {
                        foreach (int target in targets)
                        {
                            BoardManager.Slot selectedSlot;
                            if (target < 0)
                            {
                                selectedSlot = enemySlots[target * (-1) - 1];
                            }
                            else
                            {
                                selectedSlot = friendlySlots[target - 1];
                            }

                            selectedSlot.GetConnectedMinion().ReceiveDamage(1);
                        }
                    }

                    if (enemySlots[1].GetFriendly())
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                    }
                }

                stats.spell = MegaponRealization;
                stats.numberOfTargets = 2;

                stats.onPlayEvent = MegaponBattlecry;

                break;

            case CardTypes.Buzzcrave:
                stats.power = 4;
                stats.description = "Haste.\nCan attack any enemy unit on the board.";
                stats.name = "Buzzcrave";
                stats.megaVision = true;
                stats.hasHaste = true;
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Guardira:
                const int guardiraPower = 1;
                
                stats.power = 9;
                stats.description = "Greatshield.\nAlways deals " + guardiraPower.ToString() + " damage regardless of its power.";
                stats.name = "Guardira";
                stats.hasGreatshield = true;
                stats.fixedPower = guardiraPower;
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.GiveFang:
                const int giveFangHealthCost = 1;
                
                stats.description = "Add Fang to your hand.";
                stats.name = "Bone Weapon";

                stats.isSpell = true;
                static void GiveFangRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    host.TakePower(giveFangHealthCost);
                    if (!enemySlots[0].GetFriendly())
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.AddCardToHand(CardTypes.Fang);
                    }
                    else
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                    }
                    
                }
                stats.spell = GiveFangRealization;
                stats.numberOfTargets = 0;
                stats.damageToHost = giveFangHealthCost;
                break;

            case CardTypes.Kacheek:
                stats.power = 5;
                stats.description = "-1: Add Fang to your hand.\n-1: Give your creatures +1 power.";
                stats.name = "Kacheek";

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.GiveFang);
                stats.connectedCards.Add(CardTypes.Nutrition);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Myamsar:
                stats.power = 2;
                stats.description = "At the end of your turn summon a copy of this minion with 1 less power.";
                stats.name = "Myamsar";
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                static IEnumerator MyamsarEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    MinionManager thisOne = friendlySlots[index].GetConnectedMinion();

                    BoardManager.Slot targetSlot = null;
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        if (slot.GetFree())
                        {
                            targetSlot = slot;
                            break;
                        }
                    }

                    if (targetSlot == null)
                    {
                        yield return null;
                    }

                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                    int newPower = thisOne.GetPower() - 1;
                    if (newPower > 0)
                    {
                        CardManager newCard = handManager.GenerateCard(CardTypes.Myamsar).GetComponent<CardManager>();
                        newCard.SetPower(newPower);
                        boardManager.PlayCard(newCard, targetSlot, destroy: true, record: false);
                    }
                    yield return null;
                }
                stats.endTurnEvent = MyamsarEndTurn;

                break;

            case CardTypes.Motiti:
                stats.power = 3;
                stats.description = "+0: Add +2 power.\n+0: Transform into Angry Motiti with Haste.";
                stats.name = "Motiti";

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.Motiti_option1);
                stats.connectedCards.Add(CardTypes.Motiti_option2);
                break;

            case CardTypes.Motiti_option1:
                const int motiti1HealthCost = 0;
                const int motiti1Heal = 2;

                stats.description = "Give Motiti +" + motiti1Heal.ToString() + " power.";
                stats.name = "Accumulate power";

                stats.isSpell = true;
                static void MotitiOpt1Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    host.TakePower(motiti1HealthCost);
                    host.Heal(motiti1Heal);
                }
                stats.spell = MotitiOpt1Realization;
                stats.numberOfTargets = 0;
                stats.damageToHost = motiti1HealthCost;
                break;

            case CardTypes.Motiti_option2:
                const int motiti2HealthCost = 0;

                stats.description = "Transform Motiti into Angry Motiti with Haste.";
                stats.name = "Motiti Counteratack";

                stats.isSpell = true;
                static void MotitiOpt2Realization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                    host.TakePower(motiti2HealthCost);
                    CardManager newCard = handManager.GenerateCard(CardTypes.MotitiAngry).GetComponent<CardManager>();
                    newCard.SetPower(host.GetPower());
                    BoardManager.Slot slot = host.GetSlot();
                    host.TakePower(host.GetPower());
                    boardManager.PlayCard(newCard, slot, destroy: true, record: false);

                }
                stats.spell = MotitiOpt2Realization;
                stats.numberOfTargets = 0;
                stats.damageToHost = motiti2HealthCost;
                break;

            case CardTypes.MotitiAngry:
                stats.power = 3;
                stats.description = "Haste.";
                stats.name = "Angry Motiti";

                stats.hasHaste = true;
                break;

            case CardTypes.Robopon:
                stats.power = 3;
                stats.description = "At the end and the start of your turn gain +1 power.";
                stats.name = "Robopon";
                stats.runes.Add(Runes.Shield);


                static IEnumerator RoboponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    friendlySlots[index].GetConnectedMinion().Heal(1);
                    yield return null;
                }

                stats.endTurnEvent = RoboponEndTurn;
                stats.startTurnEvent = RoboponEndTurn;
                break;

            case CardTypes.Toripon:
                stats.power = 3;
                stats.description = "Cannot be a target of an attack.";
                stats.name = "Toripon";
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                stats.flying = true;
                break;

            case CardTypes.TargetDummy:
                stats.power = 3;
                stats.description = "Greatshield.";
                stats.name = "Target dummy";
                stats.hasGreatshield = true;
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                break;

            case CardTypes.Boulder:
                stats.power = 3;
                stats.description = "Greatshield.\nCan't attack, move and deal damage.";
                stats.name = "The rock";
                stats.canAttack = false;
                stats.canDealDamage = false;
                stats.limitedVision = true;
                stats.hasGreatshield = true;
                break;

            case CardTypes.Bowmunk:
                const int bowmunkHealing = 2;
                stats.power = 2;
                stats.description = "Summon the rock with Greatshield. At the end of your turn heal your Hatapon by " + bowmunkHealing.ToString() + ".";
                stats.name = "Bowmunk";
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);


                static IEnumerator BowmunkEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        MinionManager connectedMinion = slot.GetConnectedMinion();
                        if (connectedMinion != null)
                        {
                            if (connectedMinion.GetCardType() == CardTypes.Hatapon)
                            {
                                connectedMinion.Heal(bowmunkHealing);
                            }
                        }
                    }
                    yield return null;
                }
                stats.endTurnEvent = BowmunkEndTurn;


                stats.hasOnPlay = true;

                static void BowmunkRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    BoardManager.Slot thisOne = friendlySlots[targets[0]];

                    BoardManager.Slot targetSlot = null;
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        if (slot.GetFree() && slot != thisOne)
                        {
                            targetSlot = slot;
                            break;
                        }
                    }

                    if (targetSlot != null)
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                        BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                        CardManager boulderCard = handManager.GenerateCard(CardTypes.Boulder).GetComponent<CardManager>();
                        boardManager.PlayCard(boulderCard, targetSlot, destroy:false, record:false);
                        
                        boulderCard.DestroyCard();
                    }
                }

                stats.spell = BowmunkRealization;
                stats.numberOfTargets = 1;
                stats.dummyTarget = true;
                break;

            case CardTypes.Grenburr:
                const int grenburrPower = 6;
                
                stats.power = 3;
                stats.description = "Always deals " + grenburrPower.ToString() + " damage regardless of its power.";
                stats.name = "Grenburr";
                stats.fixedPower = grenburrPower;
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.Alldemonium: 
                stats = AlldemoniumStats.GetStats();
                card.SetNameSize(4);
                break;

            case CardTypes.CronoRiggers: 
                stats = CronoRiggersStats.GetStats();
                break;

            case CardTypes.TakeThatShield: 
                stats = TakeThatShieldStats.GetStats();
                card.SetNameSize(4);
                break;

            case CardTypes.ProfessionalWIthStandards:
                stats = ProfessionalWithStandards.GetStats();
                card.SetNameSize(3);
                break;

            case CardTypes.SpeedBoost: 
                stats = SpeedBoost.GetStats();
                break;

            case CardTypes.MyamsarHero:
                stats = MyamsarHeroStats.GetStats();
                card.SetDescriptionSize(3);            
                break;

            case CardTypes.Destrobo:
                stats = DestroboStats.GetStats();
                break;

            case CardTypes.Buruch:
                stats = BuruchStats.GetStats();
                break;

            case CardTypes.TonKampon:
                stats = TonKamponStats.GetStats();
                break;

            case CardTypes.TonKampon_option1:
                stats = TonKampon_option1Stats.GetStats();
                break;

            case CardTypes.TonKampon_option2:
                stats = TonKampon_option2Stats.GetStats();
                break;

            case CardTypes.Coppen:
                stats = CoppenStats.GetStats();
                break;

            case CardTypes.IceWall:
                stats = IceWallStats.GetStats();
                break;

            case CardTypes.IceWall_option:
                stats = IceWall_optionStats.GetStats();
                break;

            case CardTypes.Concede: 
                stats = ConcedeStats.GetStats();
                break;

            case CardTypes.Alosson:
                stats = AlossonStats.GetStats();
                break;

            case CardTypes.PyokoriderHero:
                stats = PyokoriderHeroStats.GetStats();
                card.SetNameSize(4);
                break;

            case CardTypes.Baloon:
                stats = BaloonStats.GetStats();
                break;

            case CardTypes.NovaNova:
                stats = NovaNovaStats.GetStats();
                break;

            case CardTypes.BanTatepon:
                stats = BanTateponStats.GetStats();
                card.SetDescriptionSize(3);
                break;

            case CardTypes.StoneFree:
                stats = StoneFreeStats.GetStats();
                break;

            case CardTypes.TurnToStone:
                stats = TurnToStoneStats.GetStats();
                break;

            default:
                break;
        }

        card.SetCardStats(stats);
        card.SetPower(stats.power);
        card.SetDescription(stats.description);
        card.SetName(stats.name);
        card.imageObject.GetComponent<SpriteRenderer>().sprite = stats.GetSprite();

        if (stats.isSpell)
        {
            card.powerObject.SetActive(false);
            card.powerSquare.SetActive(false);
            card.heartObject.SetActive(false);
        }        

        int spearCount = 0;
        int shieldCount = 0;
        int bowCount = 0;

        foreach (Runes rune in stats.runes)
        {
            if (rune == Runes.Spear)
            {
                spearCount += 1;
            }
            else if (rune == Runes.Shield)
            {
                shieldCount += 1;
            }
            else if (rune == Runes.Bow)
            {
                bowCount += 1;
            }
        }
    
        MeshRenderer meshRenderer = card.nameOutline.GetComponent<MeshRenderer>();
        var materialsCopy = meshRenderer.materials;

        if (spearCount == 0 && shieldCount == 0 && bowCount == 0)
        {
            materialsCopy[0] = card.neutralMaterial;
        }
        else if (spearCount > 0 && shieldCount == 0 && bowCount == 0)
        {
            materialsCopy[0] = card.spearMaterial;
        }
        else if (spearCount == 0 && shieldCount > 0 && bowCount == 0)
        {
            materialsCopy[0] = card.shieldMaterial;
        } 
        else if (spearCount == 0 && shieldCount == 0 && bowCount > 0)
        {
            materialsCopy[0] = card.bowMaterial;
        }
        else
        {
            materialsCopy[0] = card.multiclassMaterial;
        }

        meshRenderer.materials = materialsCopy;

        if (stats.isStatic || stats.canAttack == false)
        {
            card.powerSquare.SetActive(false);
        }
        else
        {
            card.heartObject.SetActive(false);
        }

        
        while (stats.runes.Count < 3)
        {
            stats.runes.Add(Runes.Neutral);
        }

        MeshRenderer _meshRenderer;
        foreach (var (key, value) in Enumerable.Zip(stats.runes, card.runeObjects, (key, value) => (key, value)))
        {
            switch (key)
            {
                case Runes.Neutral:
                    value.SetActive(false);
                    break;

                case Runes.Spear:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var _materialsCopy = _meshRenderer.materials;
                    _materialsCopy[0] = card.spearMaterial;
                    _meshRenderer.materials = _materialsCopy;
                    break;

                case Runes.Shield:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var __materialsCopy = _meshRenderer.materials;
                    __materialsCopy[0] = card.shieldMaterial;
                    _meshRenderer.materials = __materialsCopy;
                    break;

                case Runes.Bow:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var ___materialsCopy = _meshRenderer.materials;
                    ___materialsCopy[0] = card.bowMaterial;
                    _meshRenderer.materials = ___materialsCopy;
                    break;
            }    
        }

    }

    public static Sprite GetSpriteFromType(CardTypes _type)
    {
        switch (_type)
        {
            case CardTypes.Tatepon:
                return Resources.Load<Sprite>("Images/tatepon");

            case CardTypes.Yaripon:
                return Resources.Load<Sprite>("Images/yaripon");

            case CardTypes.Yumipon:
                return Resources.Load<Sprite>("Images/yumipon");

            case CardTypes.Kibapon:
                return Resources.Load<Sprite>("Images/kibapon");

            case CardTypes.Dekapon:
                return Resources.Load<Sprite>("Images/Dekapon");

            case CardTypes.DivineProtection:
                return Resources.Load<Sprite>("Images/DivineProtection");

            case CardTypes.Fang:
                return Resources.Load<Sprite>("Images/Fang");

            case CardTypes.GiveFang:
                return Resources.Load<Sprite>("Images/Fang");

            case CardTypes.Mahopon:
                return Resources.Load<Sprite>("Images/mahopon");

            case CardTypes.Nutrition:
                return Resources.Load<Sprite>("Images/Meat");

            case CardTypes.DeadlyDispute:
                return Resources.Load<Sprite>("Images/pon_chaka_song");

            case CardTypes.Pyokorider:
                return Resources.Load<Sprite>("Images/pyokorider");
            
            case CardTypes.Megapon:
                return Resources.Load<Sprite>("Images/Megapon");

            case CardTypes.Kacheek:
                return Resources.Load<Sprite>("Images/Kacheek");

            case CardTypes.Motiti:
                return Resources.Load<Sprite>("Images/Motiti");

            case CardTypes.Motiti_option1:
                return Resources.Load<Sprite>("Images/Motiti");

            case CardTypes.Motiti_option2:
                return Resources.Load<Sprite>("Images/MotitiAngry");

            case CardTypes.MotitiAngry:
                return Resources.Load<Sprite>("Images/MotitiAngry");

            case CardTypes.Hatapon:
                return Resources.Load<Sprite>("Images/Hatapon");

            case CardTypes.Concede:
                return Resources.Load<Sprite>("Images/Hatapon");

            case CardTypes.Buzzcrave:
                return Resources.Load<Sprite>("Images/Buzzcrave");

            case CardTypes.Guardira:
                return Resources.Load<Sprite>("Images/guardira");
            
            case CardTypes.FuckingIdiot:
                return Resources.Load<Sprite>("Images/Buruch");

             case CardTypes.Buruch:
                return Resources.Load<Sprite>("Images/burus");

            case CardTypes.Myamsar:
                return Resources.Load<Sprite>("Images/myamsar");

            case CardTypes.MyamsarHero:
                return Resources.Load<Sprite>("Images/myamsar");

            case CardTypes.Robopon:
                return Resources.Load<Sprite>("Images/robopon");

            case CardTypes.Toripon:
                return Resources.Load<Sprite>("Images/Toripon");

            case CardTypes.TargetDummy:
                return Resources.Load<Sprite>("Images/target_dummy");

            case CardTypes.Boulder:
                return Resources.Load<Sprite>("Images/boulder");

            case CardTypes.Bowmunk:
                return Resources.Load<Sprite>("Images/bowmunk"); 

            case CardTypes.Grenburr:
                return Resources.Load<Sprite>("Images/grenburr"); 

            case CardTypes.Alldemonium:
                return Resources.Load<Sprite>("Images/alldemonium"); 

            case CardTypes.CronoRiggers:
                return Resources.Load<Sprite>("Images/crono_riggers"); 

            case CardTypes.TonKampon_option2:
                return Resources.Load<Sprite>("Images/alldemonium"); 

            case CardTypes.TonKampon_option1:
                return Resources.Load<Sprite>("Images/crono_riggers"); 

            case CardTypes.TakeThatShield:
                return Resources.Load<Sprite>("Images/500x500");
             
            case CardTypes.ProfessionalWIthStandards:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.SpeedBoost:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.Destrobo:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.TonKampon:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.Coppen:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.IceWall:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.Alosson:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.PyokoriderHero:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.Baloon:
                return Resources.Load<Sprite>("Images/500x500");

            case CardTypes.NovaNova:
                return Resources.Load<Sprite>("Images/NovaNova");

            case CardTypes.BanTatepon:
                return Resources.Load<Sprite>("Images/banTatepon");

            case CardTypes.TurnToStone:
                return Resources.Load<Sprite>("Images/turnToStone");

            case CardTypes.StoneFree:
                return Resources.Load<Sprite>("Images/turnToStone");

            default:
                Debug.Log($"Unknown card type: {_type}. Can't find sprite.");
                return null;
        }
    }

    public static CardTypes GenerateRandomCard()
    {
        var namesCount = Enum.GetNames(typeof(CardTypes)).Length;
        return (CardTypes) UnityEngine.Random.Range(1, namesCount);
    }
}

