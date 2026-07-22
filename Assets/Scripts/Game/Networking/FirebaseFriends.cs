using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Networking
{
    // Friend requests/list, mirrored under /users/{uid}/friends/{friendUid} on both sides of
    // every relationship. Resolution from a typed nickname goes through /nicknameIndex (see
    // FirebaseAuth.SignUp, which reserves it) - existing accounts from before that reservation
    // existed may not be uniquely resolvable, a known limitation at this project's scale.
    public static class FirebaseFriends
    {
        public class FriendEntry
        {
            public string Uid;
            public string Nickname;
            public string Status; // "pending_sent" | "pending_received" | "accepted"
        }

        private static string FriendsPath => "users/" + FirebaseConfig.Uid + "/friends";

        public static IEnumerator SendRequest(string nickname, Action<bool, string> onResult)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                onResult?.Invoke(false, "Enter a nickname first.");
                yield break;
            }

            string nicknameKey = FirebaseAuth.NicknameKey(nickname);
            JToken targetUidToken = null;
            yield return FirebaseDb.Get("nicknameIndex/" + nicknameKey, token => targetUidToken = token);
            if (targetUidToken == null || targetUidToken.Type == JTokenType.Null)
            {
                onResult?.Invoke(false, "No player found with that nickname.");
                yield break;
            }
            string targetUid = targetUidToken.ToString();

            if (targetUid == FirebaseConfig.Uid)
            {
                onResult?.Invoke(false, "You can't friend yourself.");
                yield break;
            }

            JToken existing = null;
            yield return FirebaseDb.Get(FriendsPath + "/" + targetUid, token => existing = token);
            if (existing is JObject existingObj)
            {
                string existingStatus = existingObj["status"]?.ToString();
                if (existingStatus == "accepted")
                {
                    onResult?.Invoke(false, "You're already friends.");
                    yield break;
                }
                if (existingStatus == "pending_sent")
                {
                    onResult?.Invoke(false, "You've already sent a request.");
                    yield break;
                }
                if (existingStatus == "pending_received")
                {
                    onResult?.Invoke(false, "They've already sent you a request - check your incoming requests.");
                    yield break;
                }
            }

            // Fetch the canonical nickname rather than trusting whatever casing was typed, so the
            // sender's own friend-list entry displays it correctly.
            JToken targetNicknameToken = null;
            yield return FirebaseDb.Get("users/" + targetUid + "/profile/nickname", token => targetNicknameToken = token);
            string targetNickname = targetNicknameToken != null && targetNicknameToken.Type != JTokenType.Null
                ? targetNicknameToken.ToString()
                : nickname;

            JObject sentEntry = new JObject { ["status"] = "pending_sent", ["nickname"] = targetNickname };
            JObject receivedEntry = new JObject { ["status"] = "pending_received", ["nickname"] = PlayerProfile.Nickname };

            bool selfOk = false, targetOk = false;
            yield return FirebaseDb.Put(FriendsPath + "/" + targetUid, sentEntry, () => selfOk = true);
            yield return FirebaseDb.Put("users/" + targetUid + "/friends/" + FirebaseConfig.Uid, receivedEntry, () => targetOk = true);

            bool ok = selfOk && targetOk;
            onResult?.Invoke(ok, ok ? null : "Something went wrong sending the request.");
        }

        public static IEnumerator RespondToRequest(string friendUid, bool accept, Action<bool, string> onResult)
        {
            if (!accept)
            {
                yield return RemoveFriend(friendUid, onResult);
                yield break;
            }

            JObject statusUpdate = new JObject { ["status"] = "accepted" };
            bool selfOk = false, otherOk = false;
            yield return FirebaseDb.Patch(FriendsPath + "/" + friendUid, statusUpdate, () => selfOk = true);
            yield return FirebaseDb.Patch("users/" + friendUid + "/friends/" + FirebaseConfig.Uid, statusUpdate, () => otherOk = true);

            bool ok = selfOk && otherOk;
            onResult?.Invoke(ok, ok ? null : "Something went wrong accepting the request.");
        }

        // Also covers cancelling an outgoing request and unfriending an accepted friend - same
        // action either way, delete both sides' mirrored entries.
        public static IEnumerator RemoveFriend(string friendUid, Action<bool, string> onResult)
        {
            bool selfOk = false, otherOk = false;
            yield return FirebaseDb.Delete(FriendsPath + "/" + friendUid, () => selfOk = true);
            yield return FirebaseDb.Delete("users/" + friendUid + "/friends/" + FirebaseConfig.Uid, () => otherOk = true);

            bool ok = selfOk && otherOk;
            onResult?.Invoke(ok, ok ? null : "Something went wrong removing the friend.");
        }

        public static IEnumerator GetFriends(Action<List<FriendEntry>, string> onResult)
        {
            JToken snapshot = null;
            yield return FirebaseDb.Get(FriendsPath, token => snapshot = token);

            List<FriendEntry> entries = new List<FriendEntry>();
            if (snapshot is JObject obj)
            {
                foreach (JProperty prop in obj.Properties())
                {
                    if (prop.Value is JObject entryObj)
                    {
                        entries.Add(new FriendEntry
                        {
                            Uid = prop.Name,
                            Nickname = entryObj["nickname"]?.ToString() ?? "?",
                            Status = entryObj["status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            onResult?.Invoke(entries, null);
        }

        // "Online" is inferred from a recent heartbeat write (see PresenceHeartbeat), not a real
        // persistent-connection signal - REST has no onDisconnect to hang true presence off of.
        public static IEnumerator GetPresence(string uid, Action<bool> onResult)
        {
            JToken lastSeenToken = null;
            yield return FirebaseDb.Get("users/" + uid + "/presence/lastSeen", token => lastSeenToken = token);

            if (lastSeenToken == null || lastSeenToken.Type == JTokenType.Null)
            {
                onResult?.Invoke(false);
                yield break;
            }

            double lastSeen = lastSeenToken.Value<double>();
            bool online = FirebaseConfig.NowUnixSeconds() - lastSeen <= PresenceHeartbeat.StaleAfterSeconds;
            onResult?.Invoke(online);
        }

        // Reads whether a friend is currently in a real online match (see
        // PresenceHeartbeat.SetInMatch) - both hashes null if they aren't, or if their presence
        // is stale (same staleness window as GetPresence, so a crashed/quit client whose
        // inMatchHash never got cleared doesn't show a phantom Spectate button forever).
        public static IEnumerator GetMatchInfo(string uid, Action<int?, int?> onResult)
        {
            JToken presence = null;
            yield return FirebaseDb.Get("users/" + uid + "/presence", token => presence = token);

            if (!(presence is JObject presenceObj))
            {
                onResult?.Invoke(null, null);
                yield break;
            }

            double lastSeen = presenceObj["lastSeen"]?.Value<double>() ?? 0;
            bool fresh = FirebaseConfig.NowUnixSeconds() - lastSeen <= PresenceHeartbeat.StaleAfterSeconds;

            JToken hashToken = presenceObj["inMatchHash"];
            JToken opponentHashToken = presenceObj["inMatchOpponentHash"];
            if (!fresh || hashToken == null || hashToken.Type == JTokenType.Null)
            {
                onResult?.Invoke(null, null);
                yield break;
            }

            onResult?.Invoke(hashToken.Value<int>(), opponentHashToken?.Value<int>());
        }
    }
}
