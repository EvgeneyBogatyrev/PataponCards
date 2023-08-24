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
    Moribu,
    Wondabarappa,
    Jamsch,
    DoomShroom,
    Venomist,
    Rantan,
    DeadlyShot,
    GongTheHawkeye,
    Moforumo,
    SparringPartner,
    Cannasault,
    TropicalTailwind,
    HuntingSpirit,
    Trent,
    PoisonArcher,
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
    Mushroom,
    TrentOnFire
};

//Rewrite this entire piece of sheesh
public static class CardGenerator
{

    public static CardManager.CardStats GetCardStats(CardTypes cardType)
    {
        static IEnumerator EmptyMethod(int index, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { yield return null; }
        static void EmptyMethod_(int index, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { }
        static IEnumerator EmptySpell(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { yield return null; }
        static bool EmptyCheckSpellTargets(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }
        static bool EmptyCheckSpellTarget(int target, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }

        CardManager.CardStats stats = new CardManager.CardStats();
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
                stats = AlldemoniumStats.GetStats();
                break;

            case CardTypes.CronoRiggers: 
                stats = CronoRiggersStats.GetStats();
                break;

            case CardTypes.TakeThatShield: 
                stats = TakeThatShieldStats.GetStats();
                break;

            case CardTypes.ProfessionalWIthStandards:
                stats = ProfessionalWithStandards.GetStats();
                break;

            case CardTypes.SpeedBoost: 
                stats = SpeedBoost.GetStats();
                break;

            case CardTypes.MyamsarHero:
                stats = MyamsarHeroStats.GetStats();        
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
                break;

            case CardTypes.Baloon:
                stats = BaloonStats.GetStats();
                break;

            case CardTypes.NovaNova:
                stats = NovaNovaStats.GetStats();
                break;

            case CardTypes.BanTatepon:
                stats = BanTateponStats.GetStats();
                break;

            case CardTypes.StoneFree:
                stats = StoneFreeStats.GetStats();
                break;

            case CardTypes.TurnToStone:
                stats = TurnToStoneStats.GetStats();
                break;

            case CardTypes.Moribu:
                stats = MoribuStats.GetStats();
                break;

            case CardTypes.Wondabarappa:
                stats = WondabarappaStats.GetStats();
                break;

            case CardTypes.Jamsch:
                stats = JamschStats.GetStats();
                break;

            case CardTypes.Mushroom:
                stats = MushroomStats.GetStats();
                break;

            case CardTypes.DoomShroom:
                stats = DoomShroomStats.GetStats();
                break;

            case CardTypes.Venomist:
                stats = VenomistStats.GetStats();
                break;

            case CardTypes.Rantan:
                stats = RantanStats.GetStats();
                break;

            case CardTypes.DeadlyShot:
                stats = DeadlyShotStats.GetStats();
                break;

            case CardTypes.GongTheHawkeye:
                stats = GongStats.GetStats();
                break;

            case CardTypes.Moforumo:
                stats = Moforumo.GetStats();
                break;

            case CardTypes.SparringPartner:
                stats = SparringPartnerStats.GetStats();
                break;

            case CardTypes.Cannasault:
                stats = CannasaultStats.GetStats();
                break;

            case CardTypes.TropicalTailwind:
                stats = TropicalTailwindStats.GetStats();
                break;

            case CardTypes.HuntingSpirit:
                stats = HuntingSpiritStats.GetStats();
                break;

            case CardTypes.Trent:
                stats = TrentStats.GetStats();
                break;

            case CardTypes.TrentOnFire:
                stats = TrentFireStats.GetStats();
                break;

            case CardTypes.PoisonArcher:
                stats = PoisonArcherStats.GetStats();
                break;

            default:
                break;
        }

        if (stats.onPlayEvent == null) stats.onPlayEvent = EmptyMethod_;
        //if (stats.endTurnEvent == null) stats.endTurnEvent = EmptyMethod;
        //if (stats.startTurnEvent == null) stats.startTurnEvent = EmptyMethod;
        if (stats.spell == null) stats.spell = EmptySpell;
        if (stats.checkSpellTarget == null) stats.checkSpellTarget = EmptyCheckSpellTarget;
        if (stats.checkSpellTargets == null) stats.checkSpellTargets = EmptyCheckSpellTargets;

        return stats;
    }

    public static void CustomizeCard(CardManager card, CardTypes cardType)
    {
        card.SetCardType(cardType);
        CardManager.CardStats stats = GetCardStats(cardType);
        //static bool EmptyCondition(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }

        //card.condition = EmptyCondition;
        //card.heroMode = EmptyMethod;

        
        card.SetCardStats(stats);
        card.SetPower(stats.power);
        card.SetDescription(stats.description);
        card.SetNameSize(stats.nameSize);
        card.SetDescriptionSize(stats.descriptionSize);
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
}

