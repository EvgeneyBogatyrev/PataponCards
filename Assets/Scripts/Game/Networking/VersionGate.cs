using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Networking
{
    // Shared version-match check against config/requiredVersion in Firebase - originally only
    // run once at launch (CheckVersionModule, TitleScreen), which isn't enough on its own: a
    // player who launched successfully and then just leaves the app open never re-checks, so a
    // stale client can still start an online match hours after a new required version was
    // published. Anything that starts a real match against another player (matchmaking, sending
    // or accepting a friend challenge, joining via a manual key) should re-run this check
    // immediately beforehand, not just trust the one-time gate from launch.
    public static class VersionGate
    {
        private const string RequiredVersionPath = "config/requiredVersion";

        // Fails OPEN (isCurrent = true) on a network hiccup or an unset value - same policy as
        // the launch-time gate, so a transient read failure doesn't lock a player out entirely.
        // Fails CLOSED only on a confirmed mismatch.
        public static IEnumerator IsCurrentVersion(Action<bool> onResult)
        {
            string requiredVersion = null;
            bool readFailed = false;
            yield return FirebaseDb.Get(RequiredVersionPath, token =>
            {
                if (token == null)
                {
                    readFailed = true;
                    return;
                }
                requiredVersion = token.Value<string>();
            });

            bool isCurrent = readFailed || string.IsNullOrEmpty(requiredVersion) || requiredVersion == Application.version;
            onResult?.Invoke(isCurrent);
        }
    }
}
