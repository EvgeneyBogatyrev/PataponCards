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
                stats = TateponStats.GetStats();
                break;

            case CardTypes.Yaripon:
                stats = YariponStats.GetStats();
                break;

            case CardTypes.Yumipon:
                stats = YumiponStats.GetStats();
                break;

            case CardTypes.Kibapon:
                stats = KibaponStats.GetStats();
                break;

            case CardTypes.Dekapon:
                stats = DekaponStats.GetStats();
                break;


            case CardTypes.DivineProtection:
                stats = DivProtStats.GetStats();
                card.SetNameSize(4);
                break;

            case CardTypes.Fang: 
                stats = FangStats.GetStats();

                break;

            case CardTypes.Nutrition:
                stats = NutritionStats.GetStats();
                break;

            case CardTypes.DeadlyDispute:
                stats = DeadDispStats.GetStats();
                break;

            case CardTypes.Mahopon:
                stats = MahoponStats.GetStats();
                break;

            case CardTypes.Pyokorider:
                stats = PyokoriderStats.GetStats();

                break;

            case CardTypes.FuckingIdiot:
                stats = IdiotStats.GetStats();

                break;

            case CardTypes.Megapon:
                stats = MegaponStats.GetStats();

                break;

            case CardTypes.Buzzcrave:
                stats = BuzzcraveStats.GetStats();
                break;

            case CardTypes.Guardira:
                stats = GuardiraStats.GetStats();
                break;

            case CardTypes.GiveFang:
                stats = GiveFangStats.GetStats();
                break;

            case CardTypes.Kacheek:
                stats = KacheekStats.GetStats();
                break;

            case CardTypes.Myamsar:
                stats = MyamsarStats.GetStats();

                break;

            case CardTypes.Motiti:
                stats = MochichiStats.GetStats();
                break;

            case CardTypes.Motiti_option1:
                stats = MochiAccumStats.GetStats();
                break;

            case CardTypes.Motiti_option2:
                stats = MochiciCounterStats.GetStats();
                break;

            case CardTypes.MotitiAngry:
                stats = MochichiAngryStats.GetStats();
                break;

            case CardTypes.Robopon:
                stats = RoboponStats.GetStats();
                break;

            case CardTypes.Toripon:
                stats = ToriponStats.GetStats();
                break;

            case CardTypes.TargetDummy:
                stats = TargetDummyStats.GetStats();

                break;

            case CardTypes.Boulder:
                stats = BoulderStats.GetStats();
                break;

            case CardTypes.Bowmunk:
                stats = BowmunkStats.GetStats();
                break;

            case CardTypes.Grenburr:
                stats = GrenburrStats.GetStats();
                break;

            case CardTypes.Alldemonium: 
                const int alldemonuimGain = 4;
                const int alldemonuimDamage = 2;
                card.SetNameSize(4);
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

                break;

            case CardTypes.CronoRiggers: 
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

                break;

            case CardTypes.TakeThatShield: 
                const int takeThatShieldGain = 3;
                stats.description = "Target non-Hatapon creature under your control gains +" + takeThatShieldGain.ToString() + " power and Greatshield.";
                stats.name = "Take That Shield";
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
                stats.power = 2;
                stats.description = "When it dies, you draw a card.";
                stats.name = "Professional With Standards";
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
                stats.description = "Target non-Hatapon creature under your control gains can attack again this turn.";
                stats.name = "Speed Boost";
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
                stats.power = 3;
                stats.description = "When it enters the battlefield, enemy non-Hatapon minion in front of it loses all abilities, can't attack and move until this is alive.";
                stats.name = "Myamsar, hero";
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
                stats.power = 2;
                stats.description = "Choose a creature. If it's an artifact or it can't attack, destroy it. Otherwise, deal " + destroboDamage.ToString() +  " damage.";
                stats.name = "Destrobo";
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
                        selectedMinion.ReceiveDamage(destroboDamage);
                    }
                    
                }

                stats.spell = DestroboRealization;
                stats.numberOfTargets = 1;
                break;

            case CardTypes.Buruch:
                stats.power = 9;
                stats.description = "Destroy 2 creatures under your control.";
                stats.name = "Buruch";
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
                break;

            case CardTypes.TonKampon_option1:
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
                break;

            case CardTypes.TonKampon_option2:
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
                break;

            case CardTypes.Coppen:
                stats.power = 3;
                stats.description = "Summon an Ice Wall for your opponent.";
                stats.name = "Coppen";
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
                stats.power = 3;
                stats.description = "0: Deal 1 damage to this.";
                stats.name = "Ice Wall";

                stats.isStatic = true;
                stats.connectedCards = new List<CardTypes>();
                stats.connectedCards.Add(CardTypes.IceWall_option);
                stats.connectedCards.Add(CardTypes.IceWall_option);
                break;

            case CardTypes.IceWall_option:
                const int IceWallHealthCost = 0;
                const int IceWallSelfDamage = 1;
                
                stats.description = "Deal 1 damage to this.";
                stats.name = "Melt Down";

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

                    host.TakePower(IceWallHealthCost);
                    host.ReceiveDamage(IceWallSelfDamage);
                }
                stats.spell = IceWall_optionRealization;
                stats.numberOfTargets = 0;
                stats.damageToHost = IceWallHealthCost;
                break;

            case CardTypes.Concede: 
                stats.description = "You win this round";
                stats.name = "Opponent concedes";

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
                stats.power = 4;
                stats.description = "Deal " + alossonDamage.ToString() + " damage to the unit with highest power. If it survives, repeat the process (Up to " + alossonMax.ToString() + ")";
                stats.name = "Alosson";
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
                        strongestMinion.ReceiveDamage(alossonDamage);
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

                stats.power = 2;
                stats.description = "At the end of your turn deal " + pyokoDamage.ToString() + " damage to the most-right enemy.";
                stats.name = "Pyokorider, hero";
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
                       connectedMinion.ReceiveDamage(pyokoDamage);
                    }
                    yield return null;
                }
                stats.endTurnEvent = PyokoriderHeroEndTurn;
                break;

            case CardTypes.Baloon:
                const int baloonDamage = 1;
                stats.power = 2;
                stats.description = "Cannot be a target of an attack. At the end of your turn deal " + baloonDamage.ToString() + " damage to the enemy Hatapon.";
                stats.name = "Helicopter";
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
                       connectedMinion.ReceiveDamage(baloonDamage);
                    }
                    yield return null;
                }
                stats.endTurnEvent = BaloonEndTurn;

                break;

            case CardTypes.NovaNova:
            
                stats.name = "Nova Nova";
                stats.description = "Destroy all units.";
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
                stats.power = 5;
                stats.description = "On play: If you control a minion with power " + banTateponThreshold.ToString() + " or greater, fill your board with Tatepons with " + banTateponPower +  " power, Greatshield and Haste, and draw a card.";
                stats.name = "Ban Tatepon";
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
                stats.power = 1;
                stats.description = "Greatshield.\nCan't attack and move.";
                stats.name = "Petrified Patapon";
                stats.canAttack = false;
                stats.canDealDamage = false;
                stats.limitedVision = true;
                stats.hasGreatshield = true;
                break;

            case CardTypes.TurnToStone:
            
                stats.name = "Turn to stone";
                stats.description = "Summon the last friendly unit died this round. It has Greatshield, can't attack and deal damage.";
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

