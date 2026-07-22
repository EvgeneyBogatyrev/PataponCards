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
        // Non-null while a real online match is in progress - myHash/opponentHash of THAT match,
        // not this player's identity. Lets a friend's FriendsPanelController offer a Spectate
        // button. Both fields carried forward on every write (not just the one from SetInMatch),
        // or the periodic HeartbeatLoop's own PUT (which replaces the whole node) would silently
        // wipe them back to absent on the very next ~20s tick.
        private static int? inMatchHash = null;
        private static int? inMatchOpponentHash = null;

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
            WriteNow();
        }

        // Called by GameController.StartGame() when a real online match begins (pass both
        // hashes), and cleared (both null) by CleanUpOnlineMatch() once the match ends/is
        // cleaned up. Writes immediately rather than waiting for the next heartbeat tick, same
        // reasoning as SetOnMainMenu - a friend's Spectate button should appear/disappear
        // promptly, not up to 20s late.
        public static void SetInMatch(int? myHash, int? opponentHash)
        {
            inMatchHash = myHash;
            inMatchOpponentHash = opponentHash;
            WriteNow();
        }

        private static void WriteNow()
        {
            if (!FirebaseConfig.HasAccount)
            {
                return;
            }
            CoroutineRunner.Run(FirebaseDb.Put("users/" + FirebaseConfig.Uid + "/presence", BuildBody()));
        }

        private static JObject BuildBody()
        {
            return new JObject
            {
                ["lastSeen"] = FirebaseConfig.NowUnixSeconds(),
                ["availableForChallenge"] = onMainMenu,
                ["inMatchHash"] = inMatchHash,
                ["inMatchOpponentHash"] = inMatchOpponentHash
            };
        }

        private static IEnumerator HeartbeatLoop()
        {
            while (true)
            {
                if (FirebaseConfig.HasAccount)
                {
                    yield return FirebaseDb.Put("users/" + FirebaseConfig.Uid + "/presence", BuildBody());
                }
                yield return new UnityEngine.WaitForSeconds(HeartbeatIntervalSeconds);
            }
        }
    }
}
