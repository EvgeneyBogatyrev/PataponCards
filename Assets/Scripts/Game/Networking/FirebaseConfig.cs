using UnityEngine;

namespace Networking
{
    // Fill these in after creating the Firebase project (see FIREBASE_SETUP instructions).
    public static class FirebaseConfig
    {
        // e.g. "https://patapon-cards-default-rtdb.europe-west1.firebasedatabase.app"
        public const string DatabaseUrl = "https://pataponcardgame-default-rtdb.firebaseio.com";

        // Web API key from Project settings -> General -> Web API Key.
        public const string ApiKey = "AIzaSyAaGrJlgpKVrXMObBJQUIJYsXA-I8rjLLY";

        // Populated by FirebaseDb.EnsureSignedIn(); persists across scene loads like InfoSaver.
        public static string IdToken;
        public static string RefreshToken;
        public static string Uid;
        public static double TokenExpiresAtUnixSeconds;

        public static bool IsSignedIn => !string.IsNullOrEmpty(IdToken);

        public static bool TokenNeedsRefresh()
        {
            if (!IsSignedIn)
            {
                return false;
            }
            // Refresh a little early to avoid a request landing right on expiry.
            return NowUnixSeconds() > TokenExpiresAtUnixSeconds - 60;
        }

        public static double NowUnixSeconds()
        {
            return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
        }

        private const string RefreshTokenPrefsKey = "Firebase_RefreshToken";
        private const string UidPrefsKey = "Firebase_Uid";

        // Firebase refresh tokens don't expire on their own - reusing one across app/Editor
        // restarts avoids minting a brand-new anonymous user every launch, which is both the
        // correct behavior for a returning player and avoids tripping Google's anonymous
        // sign-up rate limit during repeated local testing.
        public static void LoadPersistedAuth()
        {
            if (!string.IsNullOrEmpty(RefreshToken))
            {
                return;
            }
            if (PlayerPrefs.HasKey(RefreshTokenPrefsKey))
            {
                RefreshToken = PlayerPrefs.GetString(RefreshTokenPrefsKey);
                Uid = PlayerPrefs.GetString(UidPrefsKey, "");
            }
        }

        public static void PersistAuth()
        {
            PlayerPrefs.SetString(RefreshTokenPrefsKey, RefreshToken ?? "");
            PlayerPrefs.SetString(UidPrefsKey, Uid ?? "");
            PlayerPrefs.Save();
        }
    }
}
