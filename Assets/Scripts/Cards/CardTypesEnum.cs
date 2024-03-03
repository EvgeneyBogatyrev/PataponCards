public enum CardTypes
{
    Yumipon,
    Megapon,
    Mahopon,
    Jamsch,
    DoomShroom,
    Rantan,
    Coppen,
    NovaNova,
    Alosson,
    PoisonArcher,
    DonTheYumipon,
    PanThePakapon,
    MedenKidnapped,
    TropicalTailwind,
    NaturalEnemy,
    HealingScepter,
    Armory,
    Scout,
    Desperado,
    BackToHideout,
    Tatepon,
    Dekapon,
    Robopon,
    Moforumo,
    Destrobo,
    Guardira,
    TargetDummy,
    DivineProtection,
    Buruch,
    Kuwagattan,
    Trent,
    Alldemonium,
    CronoRiggers,
    TonKampon,
    TakeThatShield,
    Bowmunk,
    BanTatepon,
    MyamsarHero,
    EarthShatteringBlow,
    SparringPartner,
    Yaripon,
    Kibapon,
    DeadlyDispute,
    Buzzcrave,
    Toripon,
    Myamsar,
    Baloon,
    Pyokorider,
    PyokoriderHero,
    BirdRider,
    Ponteo,
    HuntingSpirit,
    DeepImpact,
    GanTheYaripon,
    Wooyari,
    Wep,
    OfferingToKami,
    DarkOne,
    AvengingScout,
    Catapult,
    Fang,
    FuckingIdiot,
    Kacheek,
    Motiti,
    ProfessionalWIthStandards,
    YariponBushwacker,
    TraitorBoulder,
    ZigotonTroops,
    TurnToStone,
    Babatta,
    DeadlyShot,
    GongTheHawkeye,
    Cannasault,
    BringThePoison,
    Piekron,
    StormMiracle,
    BadaDrum,
    QueenKharma,
    //------------------------  
    SpeedBoost,
    Moribu,
    Grenburr,
    Wondabarappa,
    Venomist,
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
    TrentOnFire,
    Armory_option1,
    Armory_option2,
    TokenTatepon,
    Horserider,
    KibaForm,
    BirdForm,
    Catapult_option1,
    Catapult_option2,
    BabattaSwarm,
    LightningBolt,
    SleepingDust,
    MeteorRain
};


public static class CardTypeToStats
{
    public static CardManager.CardStats GetCardStats(CardTypes cardType)
    {
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

            case CardTypes.TraitorBoulder:
                stats = TraitorBoulderStats.GetStats();
                break;

            case CardTypes.BirdRider:
                stats = BirdRiderStats.GetStats();
                break;

            case CardTypes.Ponteo:
                stats = PonteoStats.GetStats();
                break;

            case CardTypes.DonTheYumipon:
                stats = DonTheYumiponStats.GetStats();
                break;

            case CardTypes.PanThePakapon:
                stats = PanThePakaponStats.GetStats();
                break;

            case CardTypes.MedenKidnapped:
                stats = MedenKidnappedStats.GetStats();
                break;

            case CardTypes.NaturalEnemy:
                stats = NaturalEnemyStats.GetStats();
                break;

            case CardTypes.HealingScepter:
                stats = HealingScepterStats.GetStats();
                break;

            case CardTypes.Armory:
                stats = ArmoryStats.GetStats();
                break;

            case CardTypes.Armory_option1:
                stats = Armory_option1Stats.GetStats();
                break;

            case CardTypes.Armory_option2:
                stats = Armory_option2Stats.GetStats();
                break;

            case CardTypes.Scout:
                stats = ScoutStats.GetStats();
                break;

            case CardTypes.Kuwagattan:
                stats = KuwagattanStats.GetStats();
                break;

            case CardTypes.Desperado:
                stats = DesperadoStats.GetStats();
                break;

            case CardTypes.BackToHideout:
                stats = BackToHideoutStats.GetStats();
                break;

            case CardTypes.EarthShatteringBlow:
                stats = EarthShatteringBlowStats.GetStats();
                break;

            case CardTypes.TokenTatepon:
                stats = TokenTateponStats.GetStats();
                break;

            case CardTypes.DeepImpact:
                stats = DeepImpactStats.GetStats();
                break;

            case CardTypes.Horserider:
                stats = HorseriderStats.GetStats();
                break;

            case CardTypes.GanTheYaripon:
                stats = GanTheYariponStats.GetStats();
                break;

            case CardTypes.YariponBushwacker:
                stats = YariponBushwackerStats.GetStats();
                break;

            case CardTypes.Wooyari:
                stats = WooyariStats.GetStats();
                break;
            
            case CardTypes.Wep:
                stats = WebStats.GetStats();
                break;

            case CardTypes.OfferingToKami:
                stats = OfferingToKamiStats.GetStats();
                break;

            case CardTypes.DarkOne:
                stats = DarkOneStats.GetStats();
                break;

            case CardTypes.KibaForm:
                stats = KibaFormStats.GetStats();
                break;

            case CardTypes.BirdForm:
                stats = BirdFormStats.GetStats();
                break;

            case CardTypes.AvengingScout:
                stats = AvengingScoutStats.GetStats();
                break;

            case CardTypes.Catapult:
                stats = CatapultStats.GetStats();
                break;

            case CardTypes.Catapult_option1:
                stats = Catapult_option1Stats.GetStats();
                break;

            case CardTypes.Catapult_option2:
                stats = Catapult_option2Stats.GetStats();
                break;

            case CardTypes.ZigotonTroops:
                stats = ZigotonStats.GetStats();
                break;

            case CardTypes.Babatta:
                stats = BabattaStats.GetStats();
                break;

            case CardTypes.BabattaSwarm:
                stats = BabattaSwarmStats.GetStats();
                break;

            case CardTypes.BringThePoison:
                stats = BringThePoisonStats.GetStats();
                break;

            case CardTypes.Piekron:
                stats = PiekronStats.GetStats();
                break;

            case CardTypes.StormMiracle:
                stats = StormMiracleStats.GetStats();
                break;

            case CardTypes.BadaDrum:
                stats = BadaDrumStats.GetStats();
                break;

            case CardTypes.QueenKharma:
                stats = QueenKharmaStats.GetStats();
                break;

            case CardTypes.LightningBolt:
                stats = LightningBoltStats.GetStats();
                break;

            case CardTypes.MeteorRain:
                stats = MeteorRainStats.GetStats();
                break;

            case CardTypes.SleepingDust:
                stats = SleepingDustStats.GetStats();
                break;

            default:
                break;
        }
        return stats;
    }
}