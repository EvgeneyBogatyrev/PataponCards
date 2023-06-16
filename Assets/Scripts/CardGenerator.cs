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
public static class CardGenerator
{
    public static void CustomizeCard(CardManager card, CardTypes cardType)
    {
        card.SetCardType(cardType);
        CardManager.CardStats stats = new CardManager.CardStats();
        card.imageObject.GetComponent<SpriteRenderer>().sprite = GetSpriteFromType(cardType);

       // RectTransform rt = card.imageObject.GetComponent<RectTransform>();
        //rt.sizeDelta = new Vector2(500, 500);

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
                card.SetPower(20);
                card.SetDescription("Protect him at all cost!");
                card.SetName("Hatapon");
                stats.canAttack = false;
                stats.canDealDamage = false;
                stats.hasHaste = true;
                break;

            case CardTypes.Tatepon:
                card.SetPower(4);
                card.SetDescription("Your Hatapon is immune.");
                card.SetName("Tatepon");
                stats.hasShield = true;
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.Yaripon:
                const int yariponDamage = 3;

                card.SetPower(2);
                card.SetDescription("At the end of your turn deal " + yariponDamage.ToString() + " damage to an enemy next to it.");
                card.SetName("Yaripon");
                stats.runes.Add(Runes.Spear);

                static IEnumerator YariponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    GameController gameController = GameObject.Find("GameController").GetComponent<GameController>(); 
                    gameController.actionIsHappening = true;


                    // Throw spear
                    MinionManager connectedMinion = enemySlots[index].GetConnectedMinion();
                    if (connectedMinion != null)
                    {
                        AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                        SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                        spear.SetSlotToGo(enemySlots[index]);
                        if (enemySlots[index].GetFriendly())
                        {
                            spear.isEnemy = true;
                        }

                        while (!spear.reachDestination)
                        {
                            yield return new WaitForSeconds(0.1f);
                        }

                        spear.DestroySelf();

                        
                        connectedMinion.GetDamage(yariponDamage);
                    }
                    gameController.actionIsHappening = false;
                    yield return null;
                }
                stats.endTurnEvent = YariponEndTurn;
                break;

            case CardTypes.Yumipon:
                const int yumiponDamage = 1;

                card.SetPower(2);
                card.SetDescription("At the end of your turn deal " + yumiponDamage.ToString() + " damage to all enemys.");
                card.SetName("Yumipon");
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
                                spear.GetSlotToGo().GetConnectedMinion().GetDamage(yumiponDamage);
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
                card.SetPower(4);
                card.SetDescription("Haste.");
                card.SetName("Kibapon");
                stats.hasHaste = true;
                //stats.hasShield = true;
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Dekapon:
                card.SetPower(7);
                card.SetDescription("Can not move.");
                card.SetName("Dekapon");
                stats.limitedVision = true;
                stats.runes.Add(Runes.Shield);
                break;


            case CardTypes.DivineProtection:
                const int divineProtectionTateponCount = 3;
                const int divineProtectionTateponPower = 2;

                card.SetDescription("Summon " + divineProtectionTateponCount.ToString() + " Tatepons with " + divineProtectionTateponPower.ToString() + " power.");
                card.SetName("Divine Protection");
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
                card.SetDescription("Deal " + fangDamage.ToString() + " damage to an enemy.");
                card.SetName("Fang");

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
                    targetMinion.GetDamage(fangDamage);

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
                
                card.SetDescription("Restore " + nutritionHeal.ToString() + " power to all allies.");
                card.SetName("Nutrition");

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

                    host.ReceiveDamage(nutritionHealthCost);

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
                card.SetDescription("Choose 2 creatures. They fight each other.");
                card.SetName("Deadly Dispute");
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

                card.SetPower(2);
                card.SetDescription("Deal " + mahoponTargetDamage.ToString() + " damage to target creature and " + mahoponAoEDamage.ToString() + " damage to all other creatures.");
                card.SetName("Mahopon");

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

                    selectedSlot.GetConnectedMinion().GetDamage(mahoponTargetDamage);
                    
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (slot != selectedSlot && !slot.GetFree())
                        {
                            slot.GetConnectedMinion().GetDamage(mahoponAoEDamage);
                        }
                    }

                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        if (slot != selectedSlot && !slot.GetFree())
                        {
                            slot.GetConnectedMinion().GetDamage(mahoponAoEDamage);
                        }
                    }
                }

                stats.spell = MahoponRealization;
                stats.numberOfTargets = 1;

                break;

            case CardTypes.Pyokorider:
                const int pyokoriderStartTurnPower = 2;

                card.SetPower(5);
                card.SetDescription("Haste.\nAt the start of your turn set this creature's power to " + pyokoriderStartTurnPower.ToString());
                card.SetName("Pyokorider");
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
                card.SetPower(2);
                card.SetDescription("At the end of your turn draw a card and deal " + burusSelfDamage.ToString() + " damage to itself.");
                card.SetName("Fucking Idiot");

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
                    friendlySlots[index].GetConnectedMinion().GetDamage(burusSelfDamage);
                    yield return null;
                }

                stats.endTurnEvent = FuckingIdiotEndTurn;

                break;

            case CardTypes.Megapon:
                card.SetPower(2);
                card.SetDescription("Deal two damage split between 1 or 2 creatures.\nDraw a card.");
                card.SetName("Megapon");
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

                        selectedSlot.GetConnectedMinion().GetDamage(2);
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

                            selectedSlot.GetConnectedMinion().GetDamage(1);
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
                card.SetPower(4);
                card.SetDescription("Haste.\nCan attack any enemy unit on the board.");
                card.SetName("Buzzcrave");
                stats.megaVision = true;
                stats.hasHaste = true;
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Guardira:
                const int guardiraPower = 1;
                
                card.SetPower(9);
                card.SetDescription("Greatshield.\nAlways deals " + guardiraPower.ToString() + " damage regardless of its power.");
                card.SetName("Guardira");
                stats.hasGreatshield = true;
                stats.fixedPower = guardiraPower;
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.GiveFang:
                const int giveFangHealthCost = 1;
                
                card.SetDescription("Add Fang to your hand.");
                card.SetName("Bone Weapon");

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

                    host.ReceiveDamage(giveFangHealthCost);
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
                card.SetPower(5);
                card.SetDescription("-1: Add Fang to your hand.\n-1: Give your creatures +1 power.");
                card.SetName("Kacheek");

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.GiveFang);
                stats.connectedCards.Add(CardTypes.Nutrition);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                break;

            case CardTypes.Myamsar:
                card.SetPower(2);
                card.SetDescription("At the end of your turn summon a copy of this minion with 1 less power.");
                card.SetName("Myamsar");
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
                card.SetPower(3);
                card.SetDescription("+0: Add +2 power.\n+0: Transform into Angry Motiti with Haste.");
                card.SetName("Motiti");

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.Motiti_option1);
                stats.connectedCards.Add(CardTypes.Motiti_option2);
                break;

            case CardTypes.Motiti_option1:
                const int motiti1HealthCost = 0;
                const int motiti1Heal = 2;

                card.SetDescription("Give Motiti +" + motiti1Heal.ToString() + " power.");
                card.SetName("Accumulate power");

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

                    host.ReceiveDamage(motiti1HealthCost);
                    host.Heal(motiti1Heal);
                }
                stats.spell = MotitiOpt1Realization;
                stats.numberOfTargets = 0;
                stats.damageToHost = motiti1HealthCost;
                break;

            case CardTypes.Motiti_option2:
                const int motiti2HealthCost = 0;

                card.SetDescription("Transform Motiti into Angry Motiti with Haste.");
                card.SetName("Motiti Counteratack");

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

                    host.ReceiveDamage(motiti2HealthCost);
                    CardManager newCard = handManager.GenerateCard(CardTypes.MotitiAngry).GetComponent<CardManager>();
                    newCard.SetPower(host.GetPower());
                    BoardManager.Slot slot = host.GetSlot();
                    host.ReceiveDamage(host.GetPower());
                    boardManager.PlayCard(newCard, slot, destroy: true, record: false);

                }
                stats.spell = MotitiOpt2Realization;
                stats.numberOfTargets = 0;
                stats.damageToHost = motiti2HealthCost;
                break;

            case CardTypes.MotitiAngry:
                card.SetPower(3);
                card.SetDescription("Haste.");
                card.SetName("Angry Motiti");

                stats.hasHaste = true;
                break;

            case CardTypes.Robopon:
                card.SetPower(3);
                card.SetDescription("At the end and the start of your turn gain +1 power.");
                card.SetName("Robopon");
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
                card.SetPower(3);
                card.SetDescription("Cannot be a target of an attack.");
                card.SetName("Toripon");
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                stats.flying = true;
                break;

            case CardTypes.TargetDummy:
                card.SetPower(3);
                card.SetDescription("Greatshield.");
                card.SetName("Target dummy");
                stats.hasGreatshield = true;
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                break;

            case CardTypes.Boulder:
                card.SetPower(3);
                card.SetDescription("Greatshield.\nCan't attack, move and deal damage.");
                card.SetName("The rock");
                stats.canAttack = false;
                stats.canDealDamage = false;
                stats.limitedVision = true;
                stats.hasGreatshield = true;
                break;

            case CardTypes.Bowmunk:
                const int bowmunkHealing = 2;
                card.SetPower(2);
                card.SetDescription("Summon the rock with Greatshield. At the end of your turn heal your Hatapon by " + bowmunkHealing.ToString() + ".");
                card.SetName("Bowmunk");
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
                
                card.SetPower(3);
                card.SetDescription("Always deals " + grenburrPower.ToString() + " damage regardless of its power.");
                card.SetName("Grenburr");
                stats.fixedPower = grenburrPower;
                stats.runes.Add(Runes.Shield);
                break;

            case CardTypes.Alldemonium: 
                const int alldemonuimGain = 4;
                const int alldemonuimDamage = 2;
                card.SetNameSize(4);
                card.SetDescription("Target creature under your controll gains " + alldemonuimGain.ToString() + " power, but recieves " + alldemonuimDamage.ToString() + " at the end of your turn.");
                card.SetName("Alldemonium Shield");
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                stats.isSpell = true;

                static IEnumerator AlldemoniumEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    MinionManager connectedMinion = friendlySlots[index].GetConnectedMinion();
                    if (connectedMinion != null)
                    {
                       connectedMinion.GetDamage(alldemonuimDamage);
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

                break;

            case CardTypes.CronoRiggers: 
                const int cronoRiggersDamageReduction = 1;
                card.SetDescription("Target creature under your controll gains: \"Receive " + cronoRiggersDamageReduction.ToString() + " less damage from any source\".");
                card.SetName("Crono Riggers");
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

                break;

            case CardTypes.TakeThatShield: 
                const int takeThatShieldGain = 3;
                card.SetDescription("Target non-Hatapon creature under your control gains +" + takeThatShieldGain.ToString() + " power and Greatshield.");
                card.SetName("Take That Shield");
                card.SetNameSize(4);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                stats.isSpell = true;

                static bool TakeThatShieldCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                static void TakeThatShieldRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
                    stats.hasGreatshield = true;
                    targetMinion.SetCardStats(stats);

                    targetMinion.Heal(takeThatShieldGain);

                }

                stats.spell = TakeThatShieldRealization;
                stats.checkSpellTarget = TakeThatShieldCheckTarget;
                stats.numberOfTargets = 1;

                break;

            case CardTypes.ProfessionalWIthStandards:
                card.SetPower(2);
                card.SetDescription("When it dies, you draw a card.");
                card.SetName("Professional With Standards");
                card.SetNameSize(3);

                static void ProfessionalWIthStandardsDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
                {
                    if (index > 0)
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.DrawCard();
                    }
                    else
                    {
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                        handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                    }
                }

                stats.hasDeathrattle = true;
                stats.onDeathEvent = ProfessionalWIthStandardsDeathrattle;

                break;

            case CardTypes.SpeedBoost: 
                card.SetDescription("Target non-Hatapon creature under your control gains can attack again this turn.");
                card.SetName("Speed Boost");
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                stats.isSpell = true;

                static bool SpeedBoostCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                static void SpeedBoostRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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
    
                    targetMinion.SetCanAttack(true);

                }

                stats.spell = SpeedBoostRealization;
                stats.checkSpellTarget = SpeedBoostCheckTarget;
                stats.numberOfTargets = 1;

                break;

            case CardTypes.MyamsarHero:
                card.SetPower(3);
                card.SetDescription("When it enters the battlefield, enemy non-Hatapon minion in front of it loses all abilities, can't attack and move until this is alive.");
                card.SetName("Myamsar, hero");
                card.SetDescriptionSize(3);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);

                stats.hasBattlecry = true;
                stats.hasDeathrattle = true;

                static void MyamsarBattlecry(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    int targetMinionIndex = -1 * index;

                    CardManager.CardStats newStats = new CardManager.CardStats();
                    newStats.canAttack = false;
                    newStats.limitedVision = true;

                    MinionManager targetMinion;
                    if (targetMinionIndex > 0) 
                    {
                        targetMinion = friendlySlots[targetMinionIndex - 1].GetConnectedMinion();
                    }
                    else
                    {
                        targetMinion = enemySlots[-1 * targetMinionIndex - 1].GetConnectedMinion();
                    }
                    if (targetMinion == null)
                    {
                        return;
                    }
                    if (targetMinion.GetCardType() == CardTypes.Hatapon)
                    {
                        return;
                    }
                    newStats.savedStats = targetMinion.GetCardStats();

                    targetMinion.SetCardStats(newStats);
                    CardManager.CardStats thisStats;
                    if (index > 0)
                    {
                        thisStats = friendlySlots[index - 1].GetConnectedMinion().GetCardStats();
                    }
                    else
                    {
                        thisStats = enemySlots[-index - 1].GetConnectedMinion().GetCardStats();
                    }

                    thisStats.connectedMinions.Add(targetMinion);

                    if (index > 0)
                    {
                        friendlySlots[index - 1].GetConnectedMinion().SetCardStats(thisStats);
                    }
                    else
                    {
                        enemySlots[-index - 1].GetConnectedMinion().SetCardStats(thisStats);
                    }

                }


                static void MyamsarDeathrattle(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots, CardManager.CardStats thisStats)
                {
                    foreach (MinionManager connectedMinion in thisStats.connectedMinions)
                    {
                        foreach (BoardManager.Slot slot in enemySlots)
                        {
                            MinionManager slotMinion = slot.GetConnectedMinion();
                            if (slotMinion != null && slotMinion == connectedMinion)
                            {
                                 connectedMinion.SetCardStats(connectedMinion.GetCardStats().savedStats);
                            }
                        }
                    }
                }


                stats.onPlayEvent = MyamsarBattlecry;
                stats.onDeathEvent = MyamsarDeathrattle;                
                
                break;

            case CardTypes.Destrobo:
                const int destroboDamage = 1;
                card.SetPower(2);
                card.SetDescription("Choose a creature. If it's an artifact or it can't attack, destroy it. Otherwise, deal " + destroboDamage.ToString() +  " damage.");
                card.SetName("Destrobo");
                stats.runes.Add(Runes.Shield);

                stats.hasOnPlay = true;

                static void DestroboRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    MinionManager selectedMinion = selectedSlot.GetConnectedMinion();
                    
                    if (selectedMinion.GetCardStats().isStatic || (!selectedMinion.GetCardStats().canAttack && selectedMinion.GetCardType() != CardTypes.Hatapon))
                    {
                        selectedMinion.DestroyMinion();
                    }
                    else
                    {
                        selectedMinion.GetDamage(destroboDamage);
                    }
                    
                }

                stats.spell = DestroboRealization;
                stats.numberOfTargets = 1;
                break;

            case CardTypes.Buruch:
                card.SetPower(9);
                card.SetDescription("Destroy 2 creatures under your control.");
                card.SetName("Buruch");
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);


                stats.hasOnPlay = true;

                static void BuruchRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {

                    BoardManager.Slot selectedSlot;
                    MinionManager selectedMinion;
                    foreach (int curTarget in targets)
                    {
                        int target = curTarget;
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

                        selectedMinion = selectedSlot.GetConnectedMinion();
                        
                        selectedMinion.DestroyMinion();   
                    }                 
                }

                static bool BuruchCheckTarget(int _target, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (_target < 0)
                    {
                        return false;
                    }
                    return true;
                }

                static bool BuruchCheckTargets(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    if (targets[0] == targets[1])
                    {
                        return false;
                    }
                    return true;
                }

                stats.spell = BuruchRealization;
                stats.checkSpellTarget = BuruchCheckTarget;
                stats.checkSpellTargets = BuruchCheckTargets;
                stats.numberOfTargets = 2;
                break;

            case CardTypes.TonKampon:
                card.SetPower(6);
                card.SetDescription("-1: Add Crono Riggers to your hand.\n-2: Add Alldemonium to your hand.");
                card.SetName("Ton Kampon");
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);


                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.TonKampon_option1);
                stats.connectedCards.Add(CardTypes.TonKampon_option2);
                break;

            case CardTypes.TonKampon_option1:
                const int TonKamponCronoRiggersHealthCost = 1;
                
                card.SetDescription("Add Crono Riggers to your hand.");
                card.SetName("Divine Weapon");

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

                    host.ReceiveDamage(TonKamponCronoRiggersHealthCost);
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
                break;

            case CardTypes.TonKampon_option2:
                const int TonKamponAlldemoniumHealthCost = 2;
                
                card.SetDescription("Add Alldemonium to your hand.");
                card.SetName("Demon Weapon");

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

                    host.ReceiveDamage(TonKamponAlldemoniumHealthCost);
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
                break;

            case CardTypes.Coppen:
                card.SetPower(3);
                card.SetDescription("Summon an Ice Wall for your opponent.");
                card.SetName("Coppen");
                stats.runes.Add(Runes.Bow);


                stats.hasOnPlay = true;

                static void CoppenRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (slot.GetFree())
                        {
                            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                            CardManager iceWallCard = handManager.GenerateCard(CardTypes.IceWall).GetComponent<CardManager>();
                            boardManager.PlayCard(iceWallCard, slot, destroy:false, record:false);
                            
                            iceWallCard.DestroyCard();
                            break;
                        }
                    }                                   
                }

                stats.spell = CoppenRealization;
                stats.numberOfTargets = 0;
                break;

            case CardTypes.IceWall:
                card.SetPower(3);
                card.SetDescription("0: Deal 1 damage to this.");
                card.SetName("Ice Wall");

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.IceWall_option);
                stats.connectedCards.Add(CardTypes.IceWall_option);
                break;

            case CardTypes.IceWall_option:
                const int IceWallHealthCost = 0;
                const int IceWallSelfDamage = 1;
                
                card.SetDescription("Deal 1 damage to this.");
                card.SetName("Melt Down");

                stats.isSpell = true;
                static void IceWall_optionRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
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

                    host.ReceiveDamage(IceWallHealthCost);
                    host.GetDamage(IceWallSelfDamage);
                }
                stats.spell = IceWall_optionRealization;
                stats.numberOfTargets = 0;
                stats.damageToHost = IceWallHealthCost;
                break;

            case CardTypes.Concede: 
                card.SetDescription("You win this round");
                card.SetName("Opponent concedes");

                stats.isSpell = true;
                static void ConcedeRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null)
                        {
                            if (minion.GetCardType() == CardTypes.Hatapon)
                            {
                                minion.DestroyMinion();
                                break;
                            }
                        }
                    }
                }

                stats.spell = ConcedeRealization;
                stats.numberOfTargets = 0;

                break;

            case CardTypes.Alosson:
                const int alossonDamage = 2;
                const int alossonMax = 5;
                card.SetPower(4);
                card.SetDescription("Deal " + alossonDamage.ToString() + " damage to the unit with highest power. If it survives, repeat the process (Up to " + alossonMax.ToString() + ")");
                card.SetName("Alosson");
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);
                

                stats.hasOnPlay = true;

                static void AlossonRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {

                    for (int limit = 0; limit < alossonMax; ++limit)
                    {
                        MinionManager strongestMinion = null;
                        foreach (BoardManager.Slot slot in enemySlots)
                        {
                            MinionManager minion = slot.GetConnectedMinion();
                            if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                            {
                                if (strongestMinion == null || strongestMinion.GetPower() < minion.GetPower())
                                {
                                    strongestMinion = minion;
                                }
                            }
                        }
                        foreach (BoardManager.Slot slot in friendlySlots)
                        {
                            MinionManager minion = slot.GetConnectedMinion();
                            if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                            {
                                if (strongestMinion == null || strongestMinion.GetPower() < minion.GetPower())
                                {
                                    strongestMinion = minion;
                                }
                            }
                        }

                        if (strongestMinion == null)
                        {
                            break;
                        }
                        strongestMinion.GetDamage(alossonDamage);
                        if (strongestMinion.GetPower() <= 0)
                        {
                            break;
                        }
                    }
                }

                stats.spell = AlossonRealization;
                stats.numberOfTargets = 0;
                break;

            case CardTypes.PyokoriderHero:
                const int pyokoDamage = 5;

                card.SetPower(2);
                card.SetDescription("At the end of your turn deal " + pyokoDamage.ToString() + " damage to the most-right enemy.");
                card.SetName("Pyokorider, hero");
                card.SetNameSize(4);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);


                static IEnumerator PyokoriderHeroEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    MinionManager connectedMinion = null;
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (slot.GetConnectedMinion() != null)
                        {
                            connectedMinion = slot.GetConnectedMinion();
                        }
                    }
                    
                    if (connectedMinion != null)
                    {
                       connectedMinion.GetDamage(pyokoDamage);
                    }
                    yield return null;
                }
                stats.endTurnEvent = PyokoriderHeroEndTurn;
                break;

            case CardTypes.Baloon:
                const int baloonDamage = 1;
                card.SetPower(2);
                card.SetDescription("Cannot be a target of an attack. At the end of your turn deal " + baloonDamage.ToString() + " damage to the enemy Hatapon.");
                card.SetName("Helicopter");
                stats.flying = true;
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);
                stats.runes.Add(Runes.Spear);

                static IEnumerator BaloonEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    MinionManager connectedMinion = null;
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        if (slot.GetConnectedMinion() != null && slot.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
                        {
                            connectedMinion = slot.GetConnectedMinion();
                        }
                    }
                    
                    if (connectedMinion != null)
                    {
                       connectedMinion.GetDamage(baloonDamage);
                    }
                    yield return null;
                }
                stats.endTurnEvent = BaloonEndTurn;

                break;

            case CardTypes.NovaNova:
            
                card.SetName("Nova Nova");
                card.SetDescription("Destroy all units.");
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);
                stats.runes.Add(Runes.Bow);
                //card.SetNameSize(4);

                stats.isSpell = true;
                static void NovaNovaRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    foreach (BoardManager.Slot slot in enemySlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                        {
                            minion.DestroyMinion();
                        }
                    }

                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                        {
                            minion.DestroyMinion();
                        }
                    }
                }

                stats.spell = NovaNovaRealization;
                stats.numberOfTargets = 0;

                break;

            case CardTypes.BanTatepon:
                const int banTateponThreshold = 8;
                const int banTateponPower = 2;
                card.SetPower(5);
                card.SetDescription("On play: If you control a minion with power " + banTateponThreshold.ToString() + " or greater, fill your board with Tatepons with " + banTateponPower +  " power, Greatshield and Haste, and draw a card.");
                card.SetName("Ban Tatepon");
                card.SetDescriptionSize(3);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                
                stats.hasOnPlay = true;

                static void BanTateponRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    bool completed = false;
                    foreach (BoardManager.Slot slot in friendlySlots)
                    {
                        MinionManager minion = slot.GetConnectedMinion();
                        if (minion != null && minion.GetCardType() != CardTypes.Hatapon)
                        {
                            if (minion.GetPower() >= banTateponThreshold)
                            {
                                completed = true;
                                break;
                            }
                        }
                    }

                    if (completed)
                    {
                        HandManager _handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                        BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();

                        CardManager tateponCard = _handManager.GenerateCard(CardTypes.Tatepon).GetComponent<CardManager>();
                        tateponCard.SetPower(banTateponPower);
                        tateponCard.GetCardStats().hasGreatshield = true;
                        tateponCard.GetCardStats().hasHaste = true;

                        foreach (BoardManager.Slot slot in friendlySlots)
                        {
                            if (slot.GetFree() && slot != friendlySlots[targets[0]])
                            {
                                boardManager.PlayCard(tateponCard, slot, destroy:false, record:false);
                            }
                        }

                        tateponCard.DestroyCard();

                        if (friendlySlots[1].GetFriendly())
                        {
                            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                            handManager.DrawCard();
                        }
                        else
                        {
                            HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
                            handManager.SetNumberOfOpponentsCards(handManager.GetNumberOfOpponentsCards() + 1);
                        }

                    }

                }

                stats.spell = BanTateponRealization;
                stats.numberOfTargets = 1;
                stats.dummyTarget = true;
                break;

            case CardTypes.StoneFree:
                card.SetPower(1);
                card.SetDescription("Greatshield.\nCan't attack and move.");
                card.SetName("Petrified Patapon");
                stats.canAttack = false;
                stats.canDealDamage = false;
                stats.limitedVision = true;
                stats.hasGreatshield = true;
                break;

            case CardTypes.TurnToStone:
            
                card.SetName("Turn to stone");
                card.SetDescription("Summon the last friendly unit died this round. It has Greatshield, can't attack and deal damage.");
                stats.runes.Add(Runes.Shield);
                stats.runes.Add(Runes.Shield);
                //stats.runes.Add(Runes.Bow);
                //card.SetNameSize(4);

                stats.isSpell = true;
                static void TurnToStoneRealization(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
                {
                    int powerToSet = 0;
                    BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
                    if (enemySlots[1].GetFriendly())
                    {
                        if (boardManager.lastDeadOpponent == CardTypes.Hatapon)
                        {
                            return;
                        }
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                        CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadOpponent).GetComponent<CardManager>();
                        powerToSet = minionCard.GetPower();
                        minionCard.DestroyCard();

                        foreach (BoardManager.Slot slot in friendlySlots)
                        {
                            if (slot.GetFree())
                            {
                                CardManager boulderCard = handManager.GenerateCard(CardTypes.StoneFree).GetComponent<CardManager>();
                                boulderCard.SetPower(powerToSet);
                                boardManager.PlayCard(boulderCard, slot, destroy:false, record:false);
                                boulderCard.DestroyCard();
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (boardManager.lastDeadYou == CardTypes.Hatapon)
                        {
                            return;
                        }
                        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>(); 
                        CardManager minionCard = handManager.GenerateCard(boardManager.lastDeadYou).GetComponent<CardManager>();
                        powerToSet = minionCard.GetPower();
                        minionCard.DestroyCard();
                        foreach (BoardManager.Slot slot in friendlySlots)
                        {
                            if (slot.GetFree())
                            {
                                CardManager boulderCard = handManager.GenerateCard(CardTypes.StoneFree).GetComponent<CardManager>();
                                boulderCard.SetPower(powerToSet);
                                boardManager.PlayCard(boulderCard, slot, destroy:false, record:false);
                                boulderCard.DestroyCard();
                                break;
                            }
                        }
                    }
                }

                stats.spell = TurnToStoneRealization;
                stats.numberOfTargets = 0;

                break;


            default:
                break;
        }

        card.SetCardStats(stats);
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

