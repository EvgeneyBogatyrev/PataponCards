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
    private static string pathDeckName => Application.persistentDataPath + "/deckname_" + AccountFileSuffix + ".dnm";
    private static string pathCollection => Application.persistentDataPath + "/collection_" + AccountFileSuffix + ".col";
    private static string pathBotStats => Application.persistentDataPath + "/bot_" + AccountFileSuffix + ".bot";
    private static string pathMatchStats => Application.persistentDataPath + "/matchstats_" + AccountFileSuffix + ".mst";
    public static bool loading = false;

    // Shown for a deck slot that's genuinely empty/untouched (no name ever saved) - see
    // LoadDeckName/SaveDeckName's fallback and DecksController's empty-slot placeholder. Distinct
    // from DefaultDeckName below, which is only ever used for the one deck actually seeded for a
    // new account (see CloudSave.SeedFreshAccount) - once a player renames either, this name is
    // gone for good, exactly like any other deck name.
    public const string NewDeckName = "New deck";
    public const string DefaultDeckName = "Default deck";
    public const int MaxDeckNameLength = 20;

    // Both of these are a matched pair - GetStarterCollection's first 12 entries are exactly
    // GetDefaultDeck's card types at the same quantities, so a new player can open the deck
    // builder and see their starter deck already fully legal, no packs needed. Spear/Spear/Shield
    // devotion (see the matching rune default in CloudSave.SeedFreshAccount and
    // CollectionControl.Start). Extras below give a taste of the Bow side and a few other
    // mechanics without being deck-legal in this specific rune combo.
    public static Dictionary<CardTypes, int> GetStarterCollection()
    {
        Dictionary<CardTypes, int> tmpCollection = new Dictionary<CardTypes, int>();
        tmpCollection.Add(CardTypes.Tatepon, 3);
        tmpCollection.Add(CardTypes.TargetDummy, 3);
        tmpCollection.Add(CardTypes.Myamsar, 2);
        tmpCollection.Add(CardTypes.Yaripon, 2);
        tmpCollection.Add(CardTypes.Kibapon, 2);
        tmpCollection.Add(CardTypes.Piekron, 1);
        tmpCollection.Add(CardTypes.Alldemonium, 2);
        tmpCollection.Add(CardTypes.Fang, 2);
        tmpCollection.Add(CardTypes.FuckingIdiot, 2);
        tmpCollection.Add(CardTypes.Kacheek, 1);
        tmpCollection.Add(CardTypes.YariponBushwacker, 2);
        tmpCollection.Add(CardTypes.ZigotonTroops, 2);
        // ------------------------------------------
        // Not deck-legal in this Spear/Spear/Shield build (or simply not included) - single/double
        // copies so a new player can see a taste of other mechanics/runes to collect toward.
        tmpCollection.Add(CardTypes.QueenKharma, 1);
        tmpCollection.Add(CardTypes.Motiti, 2);
        tmpCollection.Add(CardTypes.Yumipon, 2);
        tmpCollection.Add(CardTypes.DeadlyShot, 1);
        tmpCollection.Add(CardTypes.HealingScepter, 1);
        tmpCollection.Add(CardTypes.TropicalTailwind, 1);

        return tmpCollection;
    }

    public static List<CardTypes> GetDefaultDeck()
    {
        return new List<CardTypes>() {
            CardTypes.Tatepon,
            CardTypes.Tatepon,
            CardTypes.Tatepon,
            CardTypes.TargetDummy,
            CardTypes.TargetDummy,
            CardTypes.TargetDummy,
            CardTypes.Myamsar,
            CardTypes.Myamsar,
            CardTypes.Yaripon,
            CardTypes.Yaripon,
            CardTypes.Kibapon,
            CardTypes.Kibapon,
            CardTypes.Piekron,
            CardTypes.Alldemonium,
            CardTypes.Alldemonium,
            CardTypes.Fang,
            CardTypes.Fang,
            CardTypes.FuckingIdiot,
            CardTypes.FuckingIdiot,
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
            CloudSave.MirrorDeck(index, deck, LoadRunes(index), LoadDeckName(index));
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
        CloudSave.MirrorDeck(index, deck, runes, LoadDeckName(index));
    }

    public static void SaveDeckName(string name, int index=0, bool mirrorToCloud=true)
    {
        string trimmed = (name ?? "").Trim();

        // Safety net, not the primary gate - DeckNameField already blocks non-Latin keystrokes
        // as the player types, but this strips anything that slips through some other caller
        // (e.g. a name pulled down from the cloud) rather than throwing the whole name away.
        System.Text.StringBuilder latinOnly = new System.Text.StringBuilder();
        foreach (char c in trimmed)
        {
            if (TextValidation.IsLatinChar(c))
            {
                latinOnly.Append(c);
            }
        }
        trimmed = latinOnly.ToString();

        if (trimmed.Length > MaxDeckNameLength)
        {
            trimmed = trimmed.Substring(0, MaxDeckNameLength);
        }
        if (string.IsNullOrEmpty(trimmed))
        {
            trimmed = NewDeckName;
        }

        string newPathDeckName = pathDeckName;
        if (index != 0) newPathDeckName += index.ToString();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(newPathDeckName, FileMode.Create);

        formatter.Serialize(stream, trimmed);
        stream.Close();

        if (mirrorToCloud)
        {
            CloudSave.MirrorDeck(index, LoadDeck(index), LoadRunes(index), trimmed);
        }
    }

    public static string LoadDeckName(int index=0)
    {
        string newPathDeckName = pathDeckName;
        if (index != 0) newPathDeckName += index.ToString();
        if (File.Exists(newPathDeckName))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(newPathDeckName, FileMode.Open);

                string name = formatter.Deserialize(stream) as string;
                stream.Close();
                return string.IsNullOrEmpty(name) ? NewDeckName : name;
            }
            catch
            {
                return NewDeckName;
            }
        }
        return NewDeckName;
    }

    public static void DeleteDeck(int index, int maxdecks=6)
    {
        for (int i = index; i < maxdecks; ++i)
        {
            if (i == 5)
            {
                SaveDeckName(NewDeckName, i, mirrorToCloud:false);
                SaveDeckAndRunes(new List<CardTypes>(), new List<Runes>(), i);
                break;
            }

            List<Runes> runes = SaveSystem.LoadRunes(i + 1);
            if (runes.Count == 0)
            {
                SaveDeckName(NewDeckName, i, mirrorToCloud:false);
                SaveDeckAndRunes(new List<CardTypes>(), new List<Runes>(), i);
                break;
            }

            SaveDeckName(LoadDeckName(i + 1), i, mirrorToCloud:false);
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
            CloudSave.MirrorDeck(index, LoadDeck(index), runes, LoadDeckName(index));
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

    // Used only for developer/test accounts (see DeveloperAccounts.cs) - sets every collectable
    // card to 3 copies, mutating the given dictionary in place. Normal accounts get their real
    // saved collection from LoadCollection() instead, with no such override.
    public static void GrantAllCollectableCards(Dictionary<CardTypes, int> collection)
    {
        foreach (CardTypes cardType in GetCollectableCards())
        {
            collection[cardType] = 3;
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
                return new List<bool>() {false, false, false, false, false};
            }
        }
        else
        {
            loading = false;
            return new List<bool>() {false, false, false, false};
        }
    }

    // Wins/losses tracked against human opponents only (see GameController.RecordMatchStats,
    // which gates this on InfoSaver.onlineBattle) - bot matches never call this.
    public static void SaveMatchStats(int wins, int losses, bool mirrorToCloud=true)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(pathMatchStats, FileMode.Create);

        formatter.Serialize(stream, new List<int>() { wins, losses });
        stream.Close();

        if (mirrorToCloud)
        {
            CloudSave.MirrorMatchStats(wins, losses);
        }
    }

    public static (int wins, int losses) LoadMatchStats()
    {
        if (File.Exists(pathMatchStats))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(pathMatchStats, FileMode.Open);

                List<int> stats = formatter.Deserialize(stream) as List<int>;
                stream.Close();
                return (stats[0], stats[1]);
            }
            catch
            {
                return (0, 0);
            }
        }
        return (0, 0);
    }

    public static List<CardTypes> GetCollectableCards()
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
            CardTypes.MotitiCalm,
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
            //CardTypes.Moribu,
            //CardTypes.Grenburr,
            CardTypes.Wondabarappa,
            //CardTypes.Venomist,
            CardTypes.KibaForm,
            CardTypes.BirdForm,
            CardTypes.Catapult_option1,
            CardTypes.Catapult_option2,
            CardTypes.BabattaSwarm,
            CardTypes.LightningBolt,
            CardTypes.MeteorRain,
            CardTypes.SleepingDust,
            CardTypes.Megapon,
            //CardTypes.SparringPartner,
            CardTypes.AvengingScout,
            //CardTypes.NaturalEnemy,
            CardTypes.Fatique,
            CardTypes.Mahopon
        };

        return reservedList;
    }
}
