using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Networking
{
    // Lightweight "is this player online" signal, since plain REST has no persistent connection
    // to hang a real presence system (onDisconnect, etc.) off of. Writes a timestamp every
    // HeartbeatIntervalSeconds while signed into a real account; FirebaseFriends.GetPresence
    // treats a friend as online if their last write is more recent than StaleAfterSeconds.
    // Also reports whether the player is currently sitting on MainMenu (see SetOnMainMenu),
    // which FirebaseChallenge uses to decide whether a friend can be challenged right now.
    public static class PresenceHeartbeat
    {
        public const float StaleAfterSeconds = 45f;
        private const float HeartbeatIntervalSeconds = 20f;

        private static bool started = false;
        private static bool onMainMenu = false;

        public static void Start()
        {
            if (started)
            {
                return;
            }
            started = true;
            CoroutineRunner.Run(HeartbeatLoop());
        }

        // Called from MainMenuController's OnEnable/OnDisable. Fires an immediate one-off write
        // on an actual change rather than waiting for the next ~20s heartbeat tick - otherwise a
        // player who just left MainMenu could still look "available" to challenge for up to that
        // long, which defeats the point of the busy check.
        public static void SetOnMainMenu(bool value)
        {
            if (onMainMenu == value)
            {
                return;
            }
            onMainMenu = value;
            if (FirebaseConfig.HasAccount)
            {
                JObject body = new JObject
                {
                    ["lastSeen"] = FirebaseConfig.NowUnixSeconds(),
                    ["availableForChallenge"] = onMainMenu
                };
                CoroutineRunner.Run(FirebaseDb.Put("users/" + FirebaseConfig.Uid + "/presence", body));
            }
        }

        private static IEnumerator HeartbeatLoop()
        {
            while (true)
            {
                if (FirebaseConfig.HasAccount)
                {
                    JObject body = new JObject
                    {
                        ["lastSeen"] = FirebaseConfig.NowUnixSeconds(),
                        ["availableForChallenge"] = onMainMenu
                    };
                    yield return FirebaseDb.Put("users/" + FirebaseConfig.Uid + "/presence", body);
                }
                yield return new UnityEngine.WaitForSeconds(HeartbeatIntervalSeconds);
            }
        }
    }
}
