using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Newtonsoft.Json.Linq;
using Networking;

// Distribution goes through the itch.io app now, but its own "update available" nag is only a
// suggestion - it never stops a player from launching a stale install (or one downloaded outside
// the app entirely). That's fine for a single-player game, but this one's online multiplayer is
// deterministic lockstep: both clients independently replay the same action stream with no
// server-side authority to correct divergence (see GameController's desync-halt safeguard, which
// only detects a mismatch after it's already happened mid-match). A client running different
// card logic than its opponent can silently corrupt a match instead of just missing a feature.
//
// So this hard-gates login on an EXACT match against config/requiredVersion in Firebase - not
// just "new enough", since even a newer untested build could disagree with everyone else still on
// the current release. Update that Firebase value by hand (Realtime Database console) to the
// Player Settings > Version string every time you push a build via butler that everyone must be
// on. Firebase rule needed: "config": { "requiredVersion": { ".read": "auth != null" } }.
public class CheckVersionModule : MonoBehaviour
{
    private const string RequiredVersionPath = "config/requiredVersion";

    public GameObject textbox;
    public GameObject button;
    public GameObject buttonExit;
    public string itchPageUrl = "https://evgeney-bogatyrev.itch.io/patapon-card-game";

    private void Start()
    {
        if (button != null) button.SetActive(false);
        if (buttonExit != null) buttonExit.SetActive(false);
        SetText("Checking for updates...");
        StartCoroutine(CheckVersionAndProceed());
    }

    private IEnumerator CheckVersionAndProceed()
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

        // Fail OPEN on a network hiccup or an unset value (e.g. this is a fresh Firebase project
        // that hasn't had requiredVersion configured yet) rather than locking everyone out over a
        // transient read failure - but fail CLOSED the moment we get a real answer that differs.
        if (readFailed || string.IsNullOrEmpty(requiredVersion) || requiredVersion == Application.version)
        {
            StartCoroutine(GoToAccount());
            yield break;
        }

        ShowUpdateRequired();
    }

    private IEnumerator GoToAccount()
    {
        SetText("Loading...");
        yield return new WaitForSeconds(1f);
        // Login is mandatory - Account always runs first and hands off to MainMenu once
        // signed in (see AccountController.OnAuthResult).
        InfoSaver.sceneAfterLogin = "MainMenu";
        SceneManager.LoadScene("Account");
    }

    private void ShowUpdateRequired()
    {
        SetText("A new version is required to play. Please update via the itch.io app, then relaunch.");
        if (button != null)
        {
            button.SetActive(true);
            TextMeshProUGUI buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonLabel != null)
            {
                buttonLabel.text = "Open itch.io Page";
            }
        }
        if (buttonExit != null) buttonExit.SetActive(true);
    }

    private void SetText(string text)
    {
        if (textbox != null) textbox.GetComponent<TextMeshProUGUI>().text = text;
    }

    // Named to match the scene's existing Button.OnClick() persistent binding on "button" (left
    // over from the old Google-Drive flow, where this opened the download page the same way) -
    // Unity silently drops a persistent call whose target method no longer exists, so renaming
    // this would silently break the click with no visible error, in both Editor and builds.
    public void Download()
    {
        AudioController.PlaySound("click");
        Application.OpenURL(itchPageUrl);
    }

    public void ExitButton()
    {
        AudioController.PlaySound("click");
        Application.Quit();
    }
}
