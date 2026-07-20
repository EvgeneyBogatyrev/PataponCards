using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Networking
{
    // Redeemable chest codes - a single global (not per-user) Firebase node any signed-in player
    // can read and consume, rather than something scoped under a user's own /private data.
    // Codes are created and deleted entirely outside the game now, via a separate Python admin
    // script (Tools/redeem_codes.py) that uses the Firebase Admin SDK with a service-account key
    // and bypasses the database's security rules altogether - the app itself only ever reads and
    // deletes a code (on redemption), never writes a new one, so the security rules can safely
    // disallow client writes entirely instead of trusting a dev-only in-game gate.
    public static class FirebaseCodes
    {
        private static string CodePath(string code) => "redeemCodes/" + code;
        private const string LogPath = "redeemLog";

        public static IEnumerator RedeemCode(string code, Action<bool, string> onResult)
        {
            string trimmed = (code ?? "").Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                onResult?.Invoke(false, "Enter a code.");
                yield break;
            }

            JToken existing = null;
            yield return FirebaseDb.Get(CodePath(trimmed), token => existing = token);
            int chests = existing?["chests"]?.Value<int>() ?? 0;
            if (existing == null || existing.Type == JTokenType.Null || chests <= 0)
            {
                onResult?.Invoke(false, "That code isn't valid.");
                yield break;
            }

            // Delete before granting anything, not after - shrinks (doesn't eliminate, see the
            // class-level note above) the window where two players redeeming the same code at
            // nearly the same instant could both succeed, since a concurrent redeemer's own GET
            // has a better chance of finding it already gone.
            yield return FirebaseDb.Delete(CodePath(trimmed));

            yield return FirebaseDb.Post(LogPath, new JObject
            {
                ["uid"] = FirebaseConfig.Uid,
                ["nickname"] = PlayerProfile.Nickname,
                ["code"] = trimmed,
                ["chests"] = chests,
                ["timestamp"] = FirebaseConfig.NowUnixSeconds()
            });

            InfoSaver.chests = chests;
            onResult?.Invoke(true, "Redeemed! You got " + chests + " chest" + (chests == 1 ? "" : "s") + ".");
        }
    }
}
