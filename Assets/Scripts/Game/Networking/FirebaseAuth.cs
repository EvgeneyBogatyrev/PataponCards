using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Networking
{
    // Email/password account creation and sign-in, built on top of the anonymous session
    // FirebaseDb.EnsureSignedIn() already maintains.
    public static class FirebaseAuth
    {
        private const string SignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
        private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
        private const string SendOobCodeUrl = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";
        public const int MaxNicknameLength = 10;

        // Creates a brand-new email/password account (a fresh uid, distinct from whatever
        // anonymous session was active - some Firebase projects reject the alternative
        // "link to current session" approach with OPERATION_NOT_ALLOWED / "verify the new
        // email before changing email"). Starts from a clean slate rather than uploading
        // whatever's on local disk, since that could belong to a different, previously
        // signed-in user on this same device (see CloudSave.SeedFreshAccount).
        public static IEnumerator SignUp(string email, string password, string nickname, Action<bool, string> onResult)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                onResult?.Invoke(false, "Please enter a nickname.");
                yield break;
            }
            if (nickname.Trim().Length > MaxNicknameLength)
            {
                onResult?.Invoke(false, "Nickname must be " + MaxNicknameLength + " characters or fewer.");
                yield break;
            }

            // Checked before accounts:signUp runs, so a taken nickname never creates an orphaned
            // account the player has to abandon and can't easily delete themselves.
            string nicknameKey = NicknameKey(nickname);
            JToken existingOwner = null;
            yield return FirebaseDb.Get("nicknameIndex/" + nicknameKey, token => existingOwner = token);
            if (existingOwner != null && existingOwner.Type != JTokenType.Null)
            {
                onResult?.Invoke(false, "That nickname is taken - try another.");
                yield break;
            }

            JObject body = new JObject
            {
                ["email"] = email,
                ["password"] = password,
                ["returnSecureToken"] = true
            };

            bool ok = false;
            yield return Send(SignUpUrl + FirebaseConfig.ApiKey, body, (success, response, error) =>
            {
                ok = success;
                if (!success)
                {
                    onResult?.Invoke(false, error);
                    return;
                }
                ApplyAuthResponse(response, email);
            });

            if (!ok)
            {
                yield break;
            }

            yield return FirebaseDb.Put("users/" + FirebaseConfig.Uid + "/profile/nickname", nickname);
            // Reserved once, never edited afterward (no rename feature exists) - written after
            // the account exists so FirebaseConfig.Uid is populated, matching the profile write above.
            yield return FirebaseDb.Put("nicknameIndex/" + nicknameKey, FirebaseConfig.Uid);
            yield return CloudSave.SeedFreshAccount();
            onResult?.Invoke(true, null);
        }

        // Firebase RTDB keys can't contain '.', '#', '$', '[', ']', or '/' - strip anything not
        // safe, alongside lowercasing so lookups are case-insensitive. Public - FirebaseFriends
        // uses the same key when resolving a typed nickname to a uid.
        public static string NicknameKey(string nickname)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in nickname.Trim().ToLowerInvariant())
            {
                if (c != '.' && c != '#' && c != '$' && c != '[' && c != ']' && c != '/')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static IEnumerator SignIn(string email, string password, Action<bool, string> onResult)
        {
            JObject body = new JObject
            {
                ["email"] = email,
                ["password"] = password,
                ["returnSecureToken"] = true
            };

            bool ok = false;
            yield return Send(SignInUrl + FirebaseConfig.ApiKey, body, (success, response, error) =>
            {
                ok = success;
                if (!success)
                {
                    onResult?.Invoke(false, error);
                    return;
                }
                ApplyAuthResponse(response, email);
            });

            if (!ok)
            {
                yield break;
            }

            yield return CloudSave.DownloadCloudToLocal();

            // Load this account's nickname for display (main menu, during a match) - SignUp
            // already knows it locally and sets PlayerProfile.Nickname directly instead.
            JToken nicknameToken = null;
            yield return FirebaseDb.Get("users/" + FirebaseConfig.Uid + "/profile/nickname", token => nicknameToken = token);
            if (nicknameToken != null && nicknameToken.Type != JTokenType.Null)
            {
                PlayerProfile.Nickname = nicknameToken.ToString();
            }

            onResult?.Invoke(true, null);
        }

        // Doesn't need a signed-in session - Firebase emails a reset link straight to the
        // account's address, no idToken/uid touched here at all.
        public static IEnumerator SendPasswordResetEmail(string email, Action<bool, string> onResult)
        {
            JObject body = new JObject
            {
                ["requestType"] = "PASSWORD_RESET",
                ["email"] = email
            };

            yield return Send(SendOobCodeUrl + FirebaseConfig.ApiKey, body, (success, response, error) =>
            {
                onResult?.Invoke(success, error);
            });
        }

        private static void ApplyAuthResponse(JObject response, string email)
        {
            FirebaseConfig.IdToken = response["idToken"]?.ToString();
            FirebaseConfig.RefreshToken = response["refreshToken"]?.ToString();
            FirebaseConfig.Uid = response["localId"]?.ToString();
            FirebaseConfig.AccountEmail = email;
            double expiresIn = double.Parse(response["expiresIn"]?.ToString() ?? "3600");
            FirebaseConfig.TokenExpiresAtUnixSeconds = FirebaseConfig.NowUnixSeconds() + expiresIn;
            FirebaseConfig.PersistAuth();
        }

        private static IEnumerator Send(string url, JObject body, Action<bool, JObject, string> onDone)
        {
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                JObject response = null;
                try
                {
                    response = JObject.Parse(www.downloadHandler.text);
                }
                catch
                {
                    // Leave response null - handled via www.error below.
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onDone(true, response, null);
                }
                else
                {
                    string rawMessage = response?["error"]?["message"]?.ToString() ?? www.error;
                    onDone(false, null, FriendlyAuthError(rawMessage));
                }
            }
        }

        private static string FriendlyAuthError(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage))
            {
                return "Something went wrong. Please try again.";
            }
            if (rawMessage.StartsWith("EMAIL_EXISTS"))
            {
                return "An account with this email already exists. Try signing in instead.";
            }
            if (rawMessage.StartsWith("EMAIL_NOT_FOUND"))
            {
                return "No account found with this email.";
            }
            if (rawMessage.StartsWith("INVALID_PASSWORD") || rawMessage.StartsWith("INVALID_LOGIN_CREDENTIALS"))
            {
                return "Incorrect email or password.";
            }
            if (rawMessage.StartsWith("WEAK_PASSWORD"))
            {
                return "Password must be at least 6 characters.";
            }
            if (rawMessage.StartsWith("INVALID_EMAIL"))
            {
                return "That doesn't look like a valid email address.";
            }
            return rawMessage;
        }
    }
}
