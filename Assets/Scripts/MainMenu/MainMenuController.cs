using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Networking;

public class MainMenuController : MonoBehaviour
{
    public GameObject nicknameText;
    public PlayerStatsController playerStatsController;

    // Only one of these is ever meaningfully non-null/non-"pending" at a time in practice (you
    // can't have an unanswered incoming challenge and an outgoing one simultaneously in this
    // simple model), but both are tracked independently since they come from separate polls.
    private FirebaseChallenge.IncomingChallenge pendingIncomingChallenge;
    private string outgoingChallengeStatus; // null | "pending" | "declined"
    private string outgoingChallengeNickname;
    private bool respondingToChallenge = false;

    public IEnumerator Start()
    {
        if (InfoSaver.justLoggedIn)
        {
            InfoSaver.justLoggedIn = false;
            AudioController.PlaySound("game_starts");
        }

        if (nicknameText != null)
        {
            nicknameText.GetComponent<TextMeshProUGUI>().text = PlayerProfile.Nickname;
        }

        if (DeckManager.collection == null)
        {
            if (FirebaseConfig.HasAccount)
            {
                // Pull this account's cloud copy down first, so a different device (or a
                // reinstall) sees its latest collection/decks instead of stale local defaults.
                yield return CloudSave.DownloadCloudToLocal();
            }

            DeckManager.collection = SaveSystem.LoadCollection();
            List<bool> botStats = SaveSystem.LoadBotStats();
            for (int i = 0; i < botStats.Count; ++i)
            {
                InfoSaver.botDefeated[i] = botStats[i];
            }
        }

        // Deliberately NOT inside the block above - DeckManager.collection is a static field that
        // can outlive a single Play session (or just be stale from before a new card was added),
        // so this re-grants and re-saves every time MainMenu loads rather than only once, keeping
        // developer accounts current without needing a full domain reload to pick up new cards.
        if (DeveloperAccounts.IsDeveloper(FirebaseConfig.AccountEmail))
        {
            SaveSystem.GrantAllCollectableCards(DeckManager.collection);
            SaveSystem.SaveCollection(DeckManager.collection);
        }

        if (playerStatsController != null)
        {
            playerStatsController.RefreshStats();
        }

        StartCoroutine(PollChallenges());
    }

    // Marks this account as challengeable while actually sitting on this screen - fires an
    // immediate presence write on the transition rather than waiting for the next ~20s
    // heartbeat tick (see PresenceHeartbeat.SetOnMainMenu).
    private void OnEnable()
    {
        PresenceHeartbeat.SetOnMainMenu(true);
    }

    private void OnDisable()
    {
        PresenceHeartbeat.SetOnMainMenu(false);
    }

    // Polls for both directions of a challenge every ~2s while this screen is up - the one place
    // in this feature background polling is justified, since a challenge needs to actually pop
    // up promptly, unlike friend requests (checked on-demand when the panel opens).
    private IEnumerator PollChallenges()
    {
        while (true)
        {
            if (FirebaseConfig.HasAccount && !respondingToChallenge)
            {
                // Once a banner is showing, stop re-polling incoming so it doesn't get silently
                // replaced/cleared out from under the player while they're deciding.
                if (pendingIncomingChallenge == null)
                {
                    yield return FirebaseChallenge.PollIncomingChallenge(challenge => pendingIncomingChallenge = challenge);
                }

                yield return FirebaseChallenge.PollOutgoingChallenge((status, toNickname, toHash) =>
                {
                    outgoingChallengeStatus = status;
                    outgoingChallengeNickname = toNickname;
                    if (status == "accepted")
                    {
                        InfoSaver.opponentHash = toHash;
                        InfoSaver.onlineBattle = true;
                        InfoSaver.challengeAccepted = true;
                        DeckLoadManager.roomToGo = "Lobby";
                        outgoingChallengeStatus = null;
                        // RespondToChallenge only patches status on our node, it never deletes it -
                        // consume it here now that we've acted on it, or the next time this poll
                        // runs (e.g. back on MainMenu after the match) it'd see "accepted" again
                        // and immediately send us right back into another match.
                        CoroutineRunner.Run(FirebaseChallenge.CancelOutgoingChallenge());
                        SceneManager.LoadScene("DeckSelect");
                    }
                });
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private bool showVersionMismatch = false;

    private void OnGUI()
    {
        if (showVersionMismatch)
        {
            DrawVersionMismatchBanner();
            return;
        }

        if (pendingIncomingChallenge != null)
        {
            ConfirmationBanner.Draw(new Color(0.2f, 0.4f, 0.2f, 0.95f),
                "Accept a challenge from " + pendingIncomingChallenge.FromNickname + "?",
                "Yes", "No", AcceptChallenge, DeclineChallenge);
            return;
        }

        // "Busy" is reported synchronously by FriendsPanelController right when Challenge is
        // clicked (SendChallenge never writes a Firebase record in that case, so it never shows
        // up here) - only "pending"/"declined" are ever actually polled from outgoingChallenge.
        if (outgoingChallengeStatus == "pending")
        {
            DrawWaitingBanner("Waiting for " + outgoingChallengeNickname + " to accept...");
        }
        else if (outgoingChallengeStatus == "declined")
        {
            DrawWaitingBanner(outgoingChallengeNickname + " declined your challenge.");
        }
    }

    // Same visual technique as ConfirmationBanner.Draw/GameController's info banners (opaque
    // white texture tinted by GUI.color, centered bold label) but with a single Cancel/dismiss
    // button instead of a Yes/No pair.
    private void DrawWaitingBanner(string message)
    {
        float scale = ConfirmationBanner.Scale();
        int fontSize = ConfirmationBanner.ScaledFontSize(16);
        float width = Mathf.Min(500f * scale, Screen.width - 40f);
        Rect messageRect = new Rect((Screen.width - width) / 2f, 10f * scale, width, 50f * scale);

        Color previousColor = GUI.color;
        GUI.color = new Color(0.2f, 0.2f, 0.6f, 0.98f);
        GUI.DrawTexture(messageRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(messageRect, message, style);
        GUI.color = previousColor;

        float buttonWidth = 220f * scale;
        float buttonHeight = 40f * scale;
        Rect dismissRect = new Rect((Screen.width - buttonWidth) / 2f, messageRect.yMax + 10f * scale, buttonWidth, buttonHeight);
        if (ConfirmationBanner.DrawOpaqueButton(dismissRect, "Dismiss", fontSize, dismissRect.Contains(Event.current.mousePosition)))
        {
            CancelOutgoingChallenge();
        }
    }

    // Same visual technique as DrawWaitingBanner, tinted red for an error/blocked state.
    private void DrawVersionMismatchBanner()
    {
        float scale = ConfirmationBanner.Scale();
        int fontSize = ConfirmationBanner.ScaledFontSize(16);
        float width = Mathf.Min(500f * scale, Screen.width - 40f);
        Rect messageRect = new Rect((Screen.width - width) / 2f, 10f * scale, width, 70f * scale);

        Color previousColor = GUI.color;
        GUI.color = new Color(0.6f, 0.2f, 0.2f, 0.98f);
        GUI.DrawTexture(messageRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        style.normal.textColor = Color.white;
        GUI.Label(messageRect, "Your game version is out of date. Please update via the itch.io app, then relaunch.", style);
        GUI.color = previousColor;

        float buttonWidth = 220f * scale;
        float buttonHeight = 40f * scale;
        Rect dismissRect = new Rect((Screen.width - buttonWidth) / 2f, messageRect.yMax + 10f * scale, buttonWidth, buttonHeight);
        if (ConfirmationBanner.DrawOpaqueButton(dismissRect, "Dismiss", fontSize, dismissRect.Contains(Event.current.mousePosition)))
        {
            showVersionMismatch = false;
        }
    }

    private void AcceptChallenge()
    {
        if (respondingToChallenge)
        {
            return;
        }
        StartCoroutine(CheckVersionThenAcceptChallenge());
    }

    // Re-checks against Firebase right before actually accepting - a player who launched
    // successfully and then left the app open never re-checks on their own, so a stale client
    // could otherwise still accept a challenge hours after a new required version was published.
    private IEnumerator CheckVersionThenAcceptChallenge()
    {
        bool isCurrent = false;
        yield return VersionGate.IsCurrentVersion(result => isCurrent = result);
        if (!isCurrent)
        {
            showVersionMismatch = true;
            yield break;
        }

        respondingToChallenge = true;
        yield return FirebaseChallenge.RespondToChallenge(true, (success, error) =>
        {
            respondingToChallenge = false;
            pendingIncomingChallenge = null;
            if (success)
            {
                DeckLoadManager.roomToGo = "Lobby";
                SceneManager.LoadScene("DeckSelect");
            }
        });
    }

    private void DeclineChallenge()
    {
        if (respondingToChallenge)
        {
            return;
        }
        respondingToChallenge = true;
        StartCoroutine(FirebaseChallenge.RespondToChallenge(false, (success, error) =>
        {
            respondingToChallenge = false;
            pendingIncomingChallenge = null;
        }));
    }

    private void CancelOutgoingChallenge()
    {
        outgoingChallengeStatus = null;
        CoroutineRunner.Run(FirebaseChallenge.CancelOutgoingChallenge());
    }

    public void PlayButton()
    {
        AudioController.PlaySound("click");
        if (SaveSystem.LoadRunes(0).Count > 0)
        {
            CancelOutgoingChallenge();
            DeckLoadManager.roomToGo = "Lobby";
            SceneManager.LoadScene("DeckSelect");
        }
    }

    public void CollectionButton()
    {
        AudioController.PlaySound("click");
        CancelOutgoingChallenge();
        DeckLoadManager.roomToGo = "Collection";
        SceneManager.LoadScene("DeckSelect");
    }

    public void EnterCodeButton()
    {
        AudioController.PlaySound("click");
        CancelOutgoingChallenge();
        SceneManager.LoadScene("RedeemCode");
    }

    public void Patahell()
    {
        AudioController.PlaySound("click");
        Application.OpenURL("https://discord.gg/4p9RJuhcjx");
    }

    public void Exit()
    {
        AudioController.PlaySound("click");
        CancelOutgoingChallenge();
        Application.Quit();
    }
}
