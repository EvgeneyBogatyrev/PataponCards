using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Networking
{
    // Direct friend-to-friend match challenges. Reuses the exact same hash-pairing mechanism
    // Queue.cs/LobbyManager already use to connect two clients into an online match - a
    // challenge just negotiates myHash/opponentHash over Firebase instead of a queue or manual
    // key entry, then both sides flow through the existing DeckSelect -> Lobby -> Game path
    // (see InfoSaver.challengeAccepted / LobbyManager.Start()).
    public static class FirebaseChallenge
    {
        public class IncomingChallenge
        {
            public string FromUid;
            public string FromNickname;
            public int FromHash;
        }

        private static string IncomingPath => "users/" + FirebaseConfig.Uid + "/incomingChallenge";
        private static string OutgoingPath => "users/" + FirebaseConfig.Uid + "/outgoingChallenge";

        // Checks the target's presence (fresh heartbeat + sitting on MainMenu) before writing
        // anything - a busy/offline friend never gets a challenge written to their node at all,
        // so there's nothing to clean up on their side afterward.
        public static IEnumerator SendChallenge(string targetUid, string targetNickname, Action<bool, string> onResult)
        {
            bool available = false;
            yield return GetAvailability(targetUid, ok => available = ok);
            if (!available)
            {
                onResult?.Invoke(false, "Player is busy.");
                yield break;
            }

            // Only one outgoing challenge at a time - starting a new one cancels whatever was
            // already pending.
            yield return CancelOutgoingChallenge();

            InfoSaver.myHash = UnityEngine.Random.Range(0, 9999);

            JObject incoming = new JObject
            {
                ["fromUid"] = FirebaseConfig.Uid,
                ["fromNickname"] = PlayerProfile.Nickname,
                ["fromHash"] = InfoSaver.myHash,
                ["ts"] = FirebaseConfig.NowUnixSeconds()
            };
            JObject outgoing = new JObject
            {
                ["toUid"] = targetUid,
                ["toNickname"] = targetNickname,
                ["status"] = "pending"
            };

            bool sentOk = false, ownOk = false;
            yield return FirebaseDb.Put("users/" + targetUid + "/incomingChallenge", incoming, () => sentOk = true);
            yield return FirebaseDb.Put(OutgoingPath, outgoing, () => ownOk = true);

            bool ok = sentOk && ownOk;
            onResult?.Invoke(ok, ok ? null : "Something went wrong sending the challenge.");
        }

        // Best-effort - deletes our own outgoing record and, if we can tell who it was addressed
        // to, their mirrored incoming copy too, so an unanswered challenge doesn't linger on
        // their side once we've moved on (cancelled, challenged someone else, left MainMenu).
        public static IEnumerator CancelOutgoingChallenge(Action onDone = null)
        {
            JToken existing = null;
            yield return FirebaseDb.Get(OutgoingPath, token => existing = token);

            if (existing is JObject existingObj)
            {
                string targetUid = existingObj["toUid"]?.ToString();
                if (!string.IsNullOrEmpty(targetUid))
                {
                    yield return FirebaseDb.Delete("users/" + targetUid + "/incomingChallenge");
                }
            }
            yield return FirebaseDb.Delete(OutgoingPath);
            onDone?.Invoke();
        }

        public static IEnumerator RespondToChallenge(bool accept, Action<bool, string> onResult)
        {
            JToken existingToken = null;
            yield return FirebaseDb.Get(IncomingPath, token => existingToken = token);

            if (!(existingToken is JObject existing) || string.IsNullOrEmpty(existing["fromUid"]?.ToString()))
            {
                onResult?.Invoke(false, "That challenge is no longer available.");
                yield break;
            }
            string fromUid = existing["fromUid"].ToString();

            if (accept)
            {
                InfoSaver.myHash = UnityEngine.Random.Range(0, 9999);
                int fromHash = existing["fromHash"]?.Value<int>() ?? 0;

                JObject statusUpdate = new JObject
                {
                    ["status"] = "accepted",
                    ["toHash"] = InfoSaver.myHash
                };
                yield return FirebaseDb.Patch("users/" + fromUid + "/outgoingChallenge", statusUpdate);

                InfoSaver.opponentHash = fromHash;
                InfoSaver.onlineBattle = true;
                InfoSaver.challengeAccepted = true;
            }
            else
            {
                JObject statusUpdate = new JObject { ["status"] = "declined" };
                yield return FirebaseDb.Patch("users/" + fromUid + "/outgoingChallenge", statusUpdate);
            }

            yield return FirebaseDb.Delete(IncomingPath);
            onResult?.Invoke(true, null);
        }

        public static IEnumerator PollIncomingChallenge(Action<IncomingChallenge> onFound)
        {
            JToken token = null;
            yield return FirebaseDb.Get(IncomingPath, t => token = t);
            if (token is JObject obj)
            {
                onFound?.Invoke(new IncomingChallenge
                {
                    FromUid = obj["fromUid"]?.ToString(),
                    FromNickname = obj["fromNickname"]?.ToString() ?? "?",
                    FromHash = obj["fromHash"]?.Value<int>() ?? 0
                });
            }
            else
            {
                onFound?.Invoke(null);
            }
        }

        // status: null (no outgoing challenge) | "pending" | "accepted" | "declined". toHash is
        // only meaningful once status == "accepted".
        public static IEnumerator PollOutgoingChallenge(Action<string, string, int> onUpdate)
        {
            JToken token = null;
            yield return FirebaseDb.Get(OutgoingPath, t => token = t);
            if (token is JObject obj)
            {
                string status = obj["status"]?.ToString();
                string toNickname = obj["toNickname"]?.ToString() ?? "?";
                int toHash = obj["toHash"]?.Value<int>() ?? 0;
                onUpdate?.Invoke(status, toNickname, toHash);
            }
            else
            {
                onUpdate?.Invoke(null, null, 0);
            }
        }

        // Lets both sides of an accepted challenge wait for each other at deck selection instead
        // of whoever picks first racing straight into the Game scene alone (see LobbyManager.
        // Start()'s challengeAccepted branch). Keyed under the same matches/{matchId} node
        // ServerDataProcesser already uses, so it's cleaned up for free by
        // GameController.CleanUpOnlineMatch()'s existing whole-match delete - no separate
        // cleanup needed here.
        private static string DeckSelectReadyPath(int hash) => "matches/" + ServerDataProcesser.MatchId() + "/deckSelectReady/" + hash;

        public static IEnumerator MarkDeckSelectReady()
        {
            yield return FirebaseDb.Put(DeckSelectReadyPath(InfoSaver.myHash), true);
        }

        public static IEnumerator PollOpponentDeckSelectReady(Action<bool> onResult)
        {
            JToken token = null;
            yield return FirebaseDb.Get(DeckSelectReadyPath(InfoSaver.opponentHash), t => token = t);
            onResult?.Invoke(token != null && token.Type != JTokenType.Null);
        }

        public static IEnumerator GetAvailability(string uid, Action<bool> onResult)
        {
            JToken token = null;
            yield return FirebaseDb.Get("users/" + uid + "/presence", t => token = t);

            if (!(token is JObject presence))
            {
                onResult?.Invoke(false);
                yield break;
            }

            double lastSeen = presence["lastSeen"]?.Value<double>() ?? 0;
            bool availableFlag = presence["availableForChallenge"]?.Value<bool>() ?? false;
            bool fresh = FirebaseConfig.NowUnixSeconds() - lastSeen <= PresenceHeartbeat.StaleAfterSeconds;
            onResult?.Invoke(fresh && availableFlag);
        }
    }
}
