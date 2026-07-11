using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System;
using Networking;

public static class SaveSystem
{

    // Scoped by the signed-in account's uid (not just the device) - otherwise two accounts on
    // the same device/machine (e.g. two ParrelSync clones, which share the same
    // Application.persistentDataPath since they share company/product name) would read and
    // write the exact same physical files and appear to "share" a collection.
    private static string AccountFileSuffix => string.IsNullOrEmpty(FirebaseConfig.Uid) ? "guest" : FirebaseConfig.Uid;
    private static string pathDeck => Application.persistentDataPath + "/deck_" + AccountFileSuffix + ".dec";
    private static string pathRunes => Application.persistentDataPath + "/runes_" + AccountFileSuffix + ".run";
    private static string pathCollection => Application.persistentDataPath + "/collection_" + AccountFileSuffix + ".col";
    private static string pathBotStats => Application.persistentDataPath + "/bot_" + AccountFileSuffix + ".bot";
    public static bool loading = false;

    public static Dictionary<CardTypes, int> GetStarterCollection()
    {
        Dictionary<CardTypes, int> tmpCollection = new Dictionary<CardTypes, int>();
        tmpCollection.Add(CardTypes.Yumipon, 2);
        tmpCollection.Add(CardTypes.Scout, 2);
        tmpCollection.Add(CardTypes.Tatepon, 2);
        tmpCollection.Add(CardTypes.Dekapon, 2);
        tmpCollection.Add(CardTypes.Kibapon, 2);
        tmpCollection.Add(CardTypes.Fang, 2);
        tmpCollection.Add(CardTypes.YariponBushwacker, 2);
        tmpCollection.Add(CardTypes.ZigotonTroops, 2);
        tmpCollection.Add(CardTypes.Kacheek, 2);
        tmpCollection.Add(CardTypes.Yaripon, 2);
        tmpCollection.Add(CardTypes.Alldemonium, 2);
        tmpCollection.Add(CardTypes.HuntingSpirit, 2);
        tmpCollection.Add(CardTypes.FuckingIdiot, 2);
        // ------------------------------------------
        
        tmpCollection.Add(CardTypes.Motiti, 2);
        tmpCollection.Add(CardTypes.DeepImpact, 2);
        tmpCollection.Add(CardTypes.MyamsarHero, 2);
        tmpCollection.Add(CardTypes.Rantan, 2);

        return tmpCollection;
    }

    public static List<CardTypes> GetDefaultDeck()
    {
        return new List<CardTypes>() {
            CardTypes.Yumipon,
            CardTypes.Yumipon,
            CardTypes.Scout,
            CardTypes.Scout,
            CardTypes.Tatepon,
            CardTypes.Tatepon,
            CardTypes.Dekapon,
            CardTypes.Dekapon,
            CardTypes.Yaripon,
            CardTypes.Yaripon,
            CardTypes.Alldemonium,
            CardTypes.Alldemonium,
            CardTypes.Kibapon,
            CardTypes.Kibapon,
            CardTypes.HuntingSpirit,
            CardTypes.HuntingSpirit,
            CardTypes.Fang,
            CardTypes.Fang,
            CardTypes.Kacheek,
            CardTypes.Kacheek,
            CardTypes.YariponBushwacker,
            CardTypes.YariponBushwacker,
            CardTypes.ZigotonTroops,
            CardTypes.ZigotonTroops
        };
    }

    public static void SaveDeck(List<CardTypes> deck, int index=0, bool mirrorToCloud=true)
    {
        string newPathDeck = pathDeck;
        if (index != 0) newPathDeck += index.ToString();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(newPathDeck, FileMode.Create);

        formatter.Serialize(stream, deck);
        stream.Close();

        if (mirrorToCloud)
        {
            CloudSave.MirrorDeck(index, deck, LoadRunes(index));
        }
    }

    // Saves both halves of a deck slot and mirrors to the cloud exactly once, with the fully
    // up-to-date deck+runes together - calling SaveDeck/SaveRunes separately would each fire
    // their own independent cloud write, and since those race over the network with no
    // ordering guarantee, a briefly-stale one (paired with the other half's old value) can land
    // after the fresher one and corrupt the cloud copy.
    public static void SaveDeckAndRunes(List<CardTypes> deck, List<Runes> runes, int index=0)
    {
        SaveDeck(deck, index, mirrorToCloud:false);
        SaveRunes(runes, index, mirrorToCloud:false);
        CloudSave.MirrorDeck(index, deck, runes);
    }

    public static void DeleteDeck(int index, int maxdecks=6)
    {
        for (int i = index; i < maxdecks; ++i)
        {
            if (i == 5)
            {
                SaveDeckAndRunes(new List<CardTypes>(), new List<Runes>(), i);
                break;
            }

            List<Runes> runes = SaveSystem.LoadRunes(i + 1);
            if (runes.Count == 0)
            {
                SaveDeckAndRunes(new List<CardTypes>(), new List<Runes>(), i);
                break;
            }

            SaveDeckAndRunes(LoadDeck(i + 1), LoadRunes(i + 1), i);
        }
    }

    public static List<CardTypes> LoadDeck(int index=0)
    {
        string newPathDeck = pathDeck;
        if (index != 0) newPathDeck += index.ToString();
        if (File.Exists(newPathDeck))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(newPathDeck, FileMode.Open);

                List<CardTypes> deck = formatter.Deserialize(stream) as List<CardTypes>;
                stream.Close();

                Dictionary<CardTypes, int> deckStats = new Dictionary<CardTypes, int>();
                foreach (CardTypes deckItem in deck)
                {
                    if (deckStats.ContainsKey(deckItem))
                    {
                        deckStats[deckItem] += 1;
                    }
                    else
                    {
                        deckStats[deckItem] = 1;
                    }
                }


                // Build a new list rather than removing from "deck" while a foreach is iterating
                // it - List<T>'s enumerator throws InvalidOperationException the moment a
                // Remove() happens mid-iteration, and since that gets silently swallowed by the
                // catch below, ANY deck that ever needed trimming (e.g. the player's collection
                // legitimately has fewer copies of a card than the deck uses) would silently
                // revert to the default starter deck instead of just dropping that one card.
                List<CardTypes> validatedDeck = new List<CardTypes>();
                foreach (CardTypes deckItem in deck)
                {
                    if (DeckManager.collection[deckItem] >= deckStats[deckItem])
                    {
                        validatedDeck.Add(deckItem);
                    }
                }
                deck = validatedDeck;

                if (deck.Count == 0)
                {
                    return GetDefaultDeck();
                }

                return deck;
            }
            catch
            {
                return GetDefaultDeck();
            }
        }
        else
        {
            return GetDefaultDeck();
        }
    }

    public static void SaveRunes(List<Runes> runes, int index=0, bool mirrorToCloud=true)
    {
        string newPathRunes = pathRunes;
        if (index != 0) newPathRunes += index.ToString();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(newPathRunes, FileMode.Create);

        formatter.Serialize(stream, runes);
        stream.Close();

        if (mirrorToCloud)
        {
            CloudSave.MirrorDeck(index, LoadDeck(index), runes);
        }
    }

    public static List<Runes> LoadRunes(int index=0)
    {
        loading = true;
        string newPathRunes = pathRunes;
        if (index != 0) newPathRunes += index.ToString();
        if (File.Exists(newPathRunes))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(newPathRunes, FileMode.Open);

                List<Runes> runes = formatter.Deserialize(stream) as List<Runes>;
                stream.Close();
                loading = false;
                if (LoadDeck(index).Count == 0)
                {
                    return new List<Runes>() {Runes.Spear, Runes.Shield, Runes.Bow};
                }
                return runes;
            }
            catch
            {
                loading = false;
                return new List<Runes>() {Runes.Spear, Runes.Shield, Runes.Bow};
            }
        }
        else
        {
            loading = false;
            return new List<Runes>() {Runes.Spear, Runes.Shield, Runes.Bow};
        }
    }

    public static void SaveCollection(Dictionary<CardTypes, int> collection)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(pathCollection, FileMode.Create);

        formatter.Serialize(stream, collection);
        stream.Close();

        CloudSave.MirrorCollection(collection);
    }

    public static Dictionary<CardTypes, int> LoadCollection()
    {
        if (File.Exists(pathCollection))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(pathCollection, FileMode.Open);

                Dictionary<CardTypes, int> collection = formatter.Deserialize(stream) as Dictionary<CardTypes, int>;
                stream.Close();

                
                foreach (CardTypes cardType in GetCollectableCards())
                {
                    //collection[cardType] = 3;
                    //collection.Remove(cardType);
                }
                foreach (CardTypes cardType in GetForbiddenCards())
                {
                    collection.Remove(cardType);
                }
                
            

                return collection;
            }
            catch
            {
                return GetStarterCollection();
            }
        }
        else
        { 
            return GetStarterCollection();
        }
    }

    public static void SaveBotStats(List<bool> stats)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(pathBotStats, FileMode.Create);

        formatter.Serialize(stream, stats);
        stream.Close();

        CloudSave.MirrorBotStats(stats);
    }

    public static List<bool> LoadBotStats()
    {
        loading = true;
        if (File.Exists(pathBotStats))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(pathBotStats, FileMode.Open);

                List<bool> stats = formatter.Deserialize(stream) as List<bool>;
                stream.Close();
                return stats;
            }
            catch
            {
                loading = false;
                return new List<bool>() {false, false, false, false};
            }
        }
        else
        {
            loading = false;
            return new List<bool>() {false, false, false, false};
        }
    }

    private static List<CardTypes> GetCollectableCards()
    {
        string[] allCards = Enum.GetNames(typeof(CardTypes));
        List<CardTypes> relevantCards = new();
        List<CardTypes> reservedList = GetForbiddenCards();
        foreach (string stringType in allCards)
        {
            CardTypes type = (CardTypes)Enum.Parse(typeof(CardTypes), stringType);
            if (!reservedList.Contains(type))
            {
                relevantCards.Add(type);
            }
        }
        return relevantCards;
    }

    public static List<CardTypes> GetForbiddenCards()
    {
        // Cards that are not collectable and should not be displayed
        List<CardTypes> reservedList = new()
        {
            CardTypes.Hatapon,
            CardTypes.Nutrition,
            CardTypes.GiveFang,
            CardTypes.Motiti_option1,
            CardTypes.Motiti_option2,
            CardTypes.MotitiAngry,
            CardTypes.Boulder,
            CardTypes.TonKampon_option1,
            CardTypes.TonKampon_option2,
            CardTypes.IceWall,
            CardTypes.IceWall_option,
            CardTypes.Concede,
            CardTypes.StoneFree,
            CardTypes.Mushroom,
            CardTypes.TrentOnFire,
            CardTypes.Armory_option1,
            CardTypes.Armory_option2,
            CardTypes.Horserider,
            CardTypes.TokenTatepon,
            CardTypes.SpeedBoost,
            CardTypes.Moribu,
            CardTypes.Grenburr,
            CardTypes.Wondabarappa,
            CardTypes.Venomist,
            CardTypes.KibaForm,
            CardTypes.BirdForm,
            CardTypes.Catapult_option1,
            CardTypes.Catapult_option2,
            CardTypes.BabattaSwarm,
            CardTypes.LightningBolt,
            CardTypes.MeteorRain,
            CardTypes.SleepingDust,
            CardTypes.Megapon,
            CardTypes.SparringPartner,
            CardTypes.AvengingScout,
            CardTypes.NaturalEnemy,
            CardTypes.Fatique,
        };

        return reservedList;
    }
}
