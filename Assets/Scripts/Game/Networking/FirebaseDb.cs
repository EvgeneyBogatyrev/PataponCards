using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    // Thin coroutine wrapper around the Firebase Realtime Database REST API.
    // Every call ensures a valid anonymous-auth ID token is attached first, refreshing it if needed.
    public static class FirebaseDb
    {
        private const string SignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
        private const string RefreshUrl = "https://securetoken.googleapis.com/v1/token?key=";

        private const float InitialBackoffSeconds = 0.5f;
        private const float MaxBackoffSeconds = 8f;
        private const int FailuresBeforeDisconnected = 3;

        // Single source of truth other code polls to tell "opponent is idle" apart from
        // "we personally can't reach Firebase right now".
        public static bool IsConnected { get; private set; } = true;
        private static int consecutiveFailures = 0;

        private static void MarkSuccess()
        {
            consecutiveFailures = 0;
            IsConnected = true;
        }

        private static void MarkFailure()
        {
            consecutiveFailures++;
            if (consecutiveFailures >= FailuresBeforeDisconnected)
            {
                IsConnected = false;
            }
        }

        public static IEnumerator EnsureSignedIn()
        {
            if (FirebaseConfig.IsSignedIn && !FirebaseConfig.TokenNeedsRefresh())
            {
                yield break;
            }

            if (string.IsNullOrEmpty(FirebaseConfig.RefreshToken))
            {
                // First call this session (fresh Editor Play, or a real app launch) - reuse a
                // previously saved identity instead of minting a brand-new anonymous user.
                FirebaseConfig.LoadPersistedAuth();
            }

            if (!string.IsNullOrEmpty(FirebaseConfig.RefreshToken))
            {
                yield return Refresh();
                if (FirebaseConfig.IsSignedIn)
                {
                    FirebaseConfig.PersistAuth();
                    yield break;
                }
                // Refresh failed (e.g. revoked token) - fall through to a fresh anonymous sign-in.
            }

            string body = "{\"returnSecureToken\":true}";
            using (UnityWebRequest www = new UnityWebRequest(SignUpUrl + FirebaseConfig.ApiKey, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Firebase anonymous sign-in failed: " + www.error + " " + www.downloadHandler.text);
                    yield break;
                }

                JObject response = JObject.Parse(www.downloadHandler.text);
                FirebaseConfig.IdToken = response["idToken"]?.ToString();
                FirebaseConfig.RefreshToken = response["refreshToken"]?.ToString();
                FirebaseConfig.Uid = response["localId"]?.ToString();
                double expiresIn = double.Parse(response["expiresIn"]?.ToString() ?? "3600");
                FirebaseConfig.TokenExpiresAtUnixSeconds = FirebaseConfig.NowUnixSeconds() + expiresIn;
                FirebaseConfig.PersistAuth();
            }
        }

        private static IEnumerator Refresh()
        {
            string form = "grant_type=refresh_token&refresh_token=" + Uri.EscapeDataString(FirebaseConfig.RefreshToken);
            using (UnityWebRequest www = new UnityWebRequest(RefreshUrl + FirebaseConfig.ApiKey, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(form));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Firebase token refresh failed, discarding cached identity and re-authenticating: " + www.error);
                    // Clear (and re-persist) the whole cached identity, not just the ID token -
                    // otherwise every retry iteration would keep retrying this same dead refresh
                    // token forever instead of moving on to a clean sign-up.
                    FirebaseConfig.IdToken = null;
                    FirebaseConfig.RefreshToken = null;
                    FirebaseConfig.Uid = null;
                    FirebaseConfig.AccountEmail = null;
                    FirebaseConfig.PersistAuth();
                    yield break;
                }

                JObject response = JObject.Parse(www.downloadHandler.text);
                FirebaseConfig.IdToken = response["id_token"]?.ToString();
                FirebaseConfig.RefreshToken = response["refresh_token"]?.ToString();
                FirebaseConfig.Uid = response["user_id"]?.ToString();
                double expiresIn = double.Parse(response["expires_in"]?.ToString() ?? "3600");
                FirebaseConfig.TokenExpiresAtUnixSeconds = FirebaseConfig.NowUnixSeconds() + expiresIn;
            }
        }

        private static string UrlFor(string path)
        {
            return FirebaseConfig.DatabaseUrl + "/" + path + ".json?auth=" + FirebaseConfig.IdToken;
        }

        // Reads whatever JSON lives at `path`. Returns null (via callback) if there's nothing there.
        // Single-attempt: the caller (ObtainData's own loop) already re-polls every cycle.
        public static IEnumerator Get(string path, Action<JToken> callback)
        {
            yield return EnsureSignedIn();
            using (UnityWebRequest www = UnityWebRequest.Get(UrlFor(path)))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Firebase GET " + path + " failed: " + www.error + " | " + www.downloadHandler.text);
                    MarkFailure();
                    callback?.Invoke(null);
                    yield break;
                }

                MarkSuccess();
                string text = www.downloadHandler.text;
                callback?.Invoke(string.IsNullOrEmpty(text) || text == "null" ? null : JToken.Parse(text));
            }
        }

        // Writes below retry with capped exponential backoff until they succeed, instead of
        // silently dropping the write on a transient failure - a lost game action is worse
        // than a delayed one.
        public static IEnumerator Put(string path, JToken body, Action success = null)
        {
            yield return SendWithRetry(path, "PUT", body, success);
        }

        // PATCH updates/deletes only the given children of `path` in one atomic write -
        // useful for e.g. writing a match while clearing both players' queue entries at once.
        public static IEnumerator Patch(string path, JToken body, Action success = null)
        {
            yield return SendWithRetry(path, "PATCH", body, success);
        }

        public static IEnumerator Delete(string path, Action success = null)
        {
            float backoff = InitialBackoffSeconds;
            while (true)
            {
                yield return EnsureSignedIn();
                bool ok;
                using (UnityWebRequest www = UnityWebRequest.Delete(UrlFor(path)))
                {
                    www.downloadHandler = new DownloadHandlerBuffer();
                    yield return www.SendWebRequest();
                    ok = www.result == UnityWebRequest.Result.Success;
                    if (ok)
                    {
                        MarkSuccess();
                    }
                    else
                    {
                        Debug.LogWarning("Firebase DELETE " + path + " failed, retrying: " + www.error + " | " + www.downloadHandler.text);
                        MarkFailure();
                    }
                }

                if (ok)
                {
                    success?.Invoke();
                    yield break;
                }

                yield return new WaitForSeconds(backoff);
                backoff = Mathf.Min(backoff * 2f, MaxBackoffSeconds);
            }
        }

        // Appends `body` under a new auto-ordered push key (chronologically sortable) and
        // hands the generated key back - this replaces the hand-rolled messageId counter.
        public static IEnumerator Post(string path, JToken body, Action<string> pushIdCallback = null)
        {
            float backoff = InitialBackoffSeconds;
            while (true)
            {
                yield return EnsureSignedIn();
                bool ok = false;
                string pushId = null;
                using (UnityWebRequest www = new UnityWebRequest(UrlFor(path), "POST"))
                {
                    www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        ok = true;
                        MarkSuccess();
                        JObject response = JObject.Parse(www.downloadHandler.text);
                        pushId = response["name"]?.ToString();
                    }
                    else
                    {
                        Debug.LogWarning("Firebase POST " + path + " failed, retrying: " + www.error + " | body=" + body.ToString(Formatting.None) + " | " + www.downloadHandler.text);
                        MarkFailure();
                    }
                }

                if (ok)
                {
                    pushIdCallback?.Invoke(pushId);
                    yield break;
                }

                yield return new WaitForSeconds(backoff);
                backoff = Mathf.Min(backoff * 2f, MaxBackoffSeconds);
            }
        }

        private static IEnumerator SendWithRetry(string path, string method, JToken body, Action success)
        {
            float backoff = InitialBackoffSeconds;
            while (true)
            {
                yield return EnsureSignedIn();
                bool ok;
                using (UnityWebRequest www = new UnityWebRequest(UrlFor(path), method))
                {
                    www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    yield return www.SendWebRequest();

                    ok = www.result == UnityWebRequest.Result.Success;
                    if (ok)
                    {
                        MarkSuccess();
                    }
                    else
                    {
                        Debug.LogWarning("Firebase " + method + " " + path + " failed, retrying: " + www.error + " | body=" + body.ToString(Formatting.None) + " | " + www.downloadHandler.text);
                        MarkFailure();
                    }
                }

                if (ok)
                {
                    success?.Invoke();
                    yield break;
                }

                yield return new WaitForSeconds(backoff);
                backoff = Mathf.Min(backoff * 2f, MaxBackoffSeconds);
            }
        }
    }
}
