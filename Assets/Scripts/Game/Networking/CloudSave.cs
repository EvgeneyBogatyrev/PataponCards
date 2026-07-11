using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Networking
{
    // Cloud mirror of SaveSystem's local files, keyed under /users/{uid}/private. SaveSystem
    // calls the Mirror* methods as a fire-and-forget side effect after each local write;
    // FirebaseAuth uses the upload/download methods directly (awaited) on sign-up/sign-in.
    public static class CloudSave
    {
        private const int MaxDeckSlots = 6;

        private static string PrivatePath => "users/" + FirebaseConfig.Uid + "/private";

        public static void MirrorDeck(int index, List<CardTypes> deck, List<Runes> runes)
        {
            if (!FirebaseConfig.HasAccount)
            {
                return;
            }
            JObject body = new JObject
            {
                ["cards"] = new JArray(deck.ConvertAll(c => (int)c)),
                ["runes"] = new JArray(runes.ConvertAll(r => (int)r))
            };
            CoroutineRunner.Run(FirebaseDb.Put(PrivatePath + "/decks/" + index, body));
        }

        public static void MirrorCollection(Dictionary<CardTypes, int> collection)
        {
            if (!FirebaseConfig.HasAccount)
            {
                return;
            }
            JObject body = new JObject();
            foreach (KeyValuePair<CardTypes, int> entry in collection)
            {
                body[((int)entry.Key).ToString()] = entry.Value;
            }
            CoroutineRunner.Run(FirebaseDb.Put(PrivatePath + "/collection", body));
        }

        public static void MirrorBotStats(List<bool> stats)
        {
            if (!FirebaseConfig.HasAccount)
            {
                return;
            }
            CoroutineRunner.Run(FirebaseDb.Put(PrivatePath + "/botDefeated", new JArray(stats)));
        }

        // A brand-new account must start from the same clean slate a fresh install would have -
        // NOT whatever happens to be sitting in local files, which could easily belong to a
        // previously-signed-in user on this same device (downloaded there during their own
        // sign-in, or left over from their own editing). Used once, right after SignUp.
        public static IEnumerator SeedFreshAccount()
        {
            SaveSystem.SaveCollection(SaveSystem.GetStarterCollection());

            // Only slot 0 starts as a real deck - the rest start genuinely empty (an empty
            // runes list is what the rest of the game already treats as "unused slot", e.g.
            // DeleteDeck's own shift-and-clear logic).
            SaveSystem.SaveDeckAndRunes(
                SaveSystem.GetDefaultDeck(),
                new List<Runes> { Runes.Spear, Runes.Shield, Runes.Bow },
                0);
            for (int i = 1; i < MaxDeckSlots; ++i)
            {
                SaveSystem.SaveDeckAndRunes(new List<CardTypes>(), new List<Runes>(), i);
            }

            SaveSystem.SaveBotStats(new List<bool> { false, false, false, false });
            yield return null;
        }

        // Pulls /users/{uid}/private down and overwrites local SaveSystem files - used on
        // SignIn so an account's data follows the player to this device. Callers are
        // responsible for refreshing any already-loaded DeckManager fields afterward.
        public static IEnumerator DownloadCloudToLocal()
        {
            JToken snapshot = null;
            yield return FirebaseDb.Get(PrivatePath, token => snapshot = token);

            if (!(snapshot is JObject data))
            {
                yield break;
            }

            if (data["collection"] != null)
            {
                Dictionary<CardTypes, int> collection = new Dictionary<CardTypes, int>();
                foreach (KeyValuePair<int, JToken> entry in IndexedEntries(data["collection"]))
                {
                    collection[(CardTypes)entry.Key] = entry.Value.Value<int>();
                }
                SaveSystem.SaveCollection(collection);
            }

            if (data["decks"] != null)
            {
                foreach (KeyValuePair<int, JToken> entry in IndexedEntries(data["decks"]))
                {
                    JToken deckEntry = entry.Value;

                    List<CardTypes> deck = new List<CardTypes>();
                    foreach (JToken cardToken in deckEntry["cards"] as JArray ?? new JArray())
                    {
                        deck.Add((CardTypes)cardToken.Value<int>());
                    }

                    List<Runes> runes = new List<Runes>();
                    foreach (JToken runeToken in deckEntry["runes"] as JArray ?? new JArray())
                    {
                        runes.Add((Runes)runeToken.Value<int>());
                    }

                    // No need to mirror back to the cloud - this data just came from there.
                    SaveSystem.SaveDeck(deck, entry.Key, mirrorToCloud:false);
                    SaveSystem.SaveRunes(runes, entry.Key, mirrorToCloud:false);
                }
            }

            if (data["botDefeated"] is JArray botArray)
            {
                List<bool> stats = new List<bool>();
                foreach (JToken b in botArray)
                {
                    stats.Add(b.Value<bool>());
                }
                SaveSystem.SaveBotStats(stats);
            }
        }

        // Firebase's REST API silently returns a node as a JSON array (instead of an object)
        // whenever its children happen to be small sequential integer keys ("0","1","2",...) -
        // exactly what /decks and /collection look like. Normalizing both possible shapes here
        // means the parsing above never has to care which one Firebase decided to send back.
        private static IEnumerable<KeyValuePair<int, JToken>> IndexedEntries(JToken node)
        {
            if (node is JArray array)
            {
                for (int i = 0; i < array.Count; ++i)
                {
                    if (array[i] != null && array[i].Type != JTokenType.Null)
                    {
                        yield return new KeyValuePair<int, JToken>(i, array[i]);
                    }
                }
            }
            else if (node is JObject obj)
            {
                foreach (JProperty prop in obj.Properties())
                {
                    yield return new KeyValuePair<int, JToken>(int.Parse(prop.Name), prop.Value);
                }
            }
        }
    }

    // Lets static code (SaveSystem, CloudSave) fire-and-forget a coroutine without needing its
    // own MonoBehaviour. Created lazily and kept alive across scene loads.
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        private static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject host = new GameObject("CloudSaveRunner");
                    Object.DontDestroyOnLoad(host);
                    instance = host.AddComponent<CoroutineRunner>();
                }
                return instance;
            }
        }

        public static void Run(IEnumerator routine)
        {
            Instance.StartCoroutine(routine);
        }
    }
}
