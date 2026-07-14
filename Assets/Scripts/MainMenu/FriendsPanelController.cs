using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Networking;

// Drives the friends side panel - not a scene of its own, added to MainMenu/Lobby per the plan.
// No background polling: the list is fetched on-demand (panel opened, or after an action
// completes), consistent with treating friend requests as durable state rather than presence.
// Online status is the one exception - it's checked once per accepted friend on each Refresh,
// not continuously polled.
public class FriendsPanelController : MonoBehaviour
{
    public GameObject panelRoot;
    public GameObject friendRowPrefab;

    public Transform acceptedContainer;
    public Transform incomingContainer;
    public Transform outgoingContainer;

    // Optional - a LayoutElement on each section's wrapper (e.g. the ScrollRect object). If
    // wired, the section is sized to fit its rows up to maxSectionHeight, beyond which it stays
    // capped (and scrolls, if that wrapper has a ScrollRect). Left null, sections just use
    // whatever their own layout settings already give them - no behavior change.
    public LayoutElement acceptedSectionLayout;
    public LayoutElement incomingSectionLayout;
    public LayoutElement outgoingSectionLayout;
    public float maxSectionHeight = 220f;
    // Floor so an empty section still leaves a little breathing room under its header instead of
    // collapsing to literally zero height.
    public float minSectionHeight = 24f;

    // Optional - the "Incoming requests:"/"Outgoing requests:" header labels. If wired, hidden
    // whenever that section has zero entries instead of always showing.
    public GameObject incomingHeader;
    public GameObject outgoingHeader;

    public GameObject addNicknameField;
    public GameObject statusText;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();
    private bool busy = false;

    // Forces the panel closed every time this controller loads, regardless of whatever active
    // state got saved in the scene/prefab - this script's own GameObject stays active the whole
    // time (it has to, to receive the Friends button's click), so relying on panelRoot's saved
    // Inspector checkbox alone is fragile: it's easy to leave it checked by accident while
    // editing, and any change made to it while in Play Mode never persists once you stop anyway.
    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
        {
            return;
        }
        bool nowOpen = !panelRoot.activeSelf;
        panelRoot.SetActive(nowOpen);
        if (nowOpen)
        {
            Refresh();
        }
    }

    public void SendRequestButton()
    {
        if (busy || addNicknameField == null)
        {
            return;
        }
        string nickname = addNicknameField.GetComponent<TMP_InputField>().text.Trim();
        SetBusy(true);
        StartCoroutine(FirebaseFriends.SendRequest(nickname, (success, error) =>
        {
            SetBusy(false);
            ShowStatus(success ? "Request sent." : error);
            if (success)
            {
                addNicknameField.GetComponent<TMP_InputField>().text = "";
                Refresh();
            }
        }));
    }

    public void Refresh()
    {
        if (!FirebaseConfig.HasAccount)
        {
            return;
        }
        StartCoroutine(FirebaseFriends.GetFriends((entries, error) =>
        {
            if (error != null)
            {
                ShowStatus(error);
                return;
            }
            Populate(entries);
        }));
    }

    private void Populate(List<FirebaseFriends.FriendEntry> entries)
    {
        foreach (GameObject row in spawnedRows)
        {
            Destroy(row);
        }
        spawnedRows.Clear();

        if (friendRowPrefab == null)
        {
            return;
        }

        int incomingCount = 0;
        int outgoingCount = 0;

        foreach (FirebaseFriends.FriendEntry entry in entries)
        {
            Transform container = entry.Status == "accepted" ? acceptedContainer
                : entry.Status == "pending_received" ? incomingContainer
                : outgoingContainer;
            if (container == null)
            {
                continue;
            }

            if (entry.Status == "pending_received")
            {
                incomingCount++;
            }
            else if (entry.Status != "accepted")
            {
                outgoingCount++;
            }

            GameObject rowObject = Instantiate(friendRowPrefab, container);
            spawnedRows.Add(rowObject);
            FriendRow row = rowObject.GetComponent<FriendRow>();
            string friendUid = entry.Uid;

            if (entry.Status == "accepted")
            {
                string friendNickname = entry.Nickname;
                row.Setup(entry.Nickname, "Remove", () => RemoveFriend(friendUid));
                StartCoroutine(FirebaseFriends.GetPresence(friendUid, online =>
                    row.SetOnline(online, () => ChallengeFriend(friendUid, friendNickname))));
            }
            else if (entry.Status == "pending_received")
            {
                row.Setup(entry.Nickname, "Accept", () => RespondToRequest(friendUid, true), "Decline", () => RespondToRequest(friendUid, false));
            }
            else
            {
                row.Setup(entry.Nickname, "Cancel", () => RemoveFriend(friendUid));
            }
        }

        if (incomingHeader != null)
        {
            incomingHeader.SetActive(incomingCount > 0);
        }
        if (outgoingHeader != null)
        {
            outgoingHeader.SetActive(outgoingCount > 0);
        }

        AutoSizeSection(acceptedContainer as RectTransform, acceptedSectionLayout);
        AutoSizeSection(incomingContainer as RectTransform, incomingSectionLayout);
        AutoSizeSection(outgoingContainer as RectTransform, outgoingSectionLayout);
    }

    // Sizes a section's wrapper to fit its current rows, capped at maxSectionHeight - a plain
    // ContentSizeFitter on the row container alone can't cap-then-scroll on its own, since that
    // component only ever grows to fit, with no maximum.
    private void AutoSizeSection(RectTransform content, LayoutElement wrapperLayout)
    {
        if (wrapperLayout == null || content == null)
        {
            return;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        wrapperLayout.preferredHeight = Mathf.Clamp(content.rect.height, minSectionHeight, maxSectionHeight);
    }

    private void RespondToRequest(string friendUid, bool accept)
    {
        if (busy)
        {
            return;
        }
        SetBusy(true);
        StartCoroutine(FirebaseFriends.RespondToRequest(friendUid, accept, (success, error) =>
        {
            SetBusy(false);
            if (!success)
            {
                ShowStatus(error);
            }
            Refresh();
        }));
    }

    // Just fires the challenge off and reports the immediate result (sent, or busy/error) here -
    // MainMenuController independently polls the same outgoing-challenge Firebase state and owns
    // the "Waiting for X to accept" / accepted / declined banner, so the two don't need to talk
    // to each other directly.
    private void ChallengeFriend(string friendUid, string friendNickname)
    {
        if (busy)
        {
            return;
        }
        SetBusy(true);
        StartCoroutine(FirebaseChallenge.SendChallenge(friendUid, friendNickname, (success, error) =>
        {
            SetBusy(false);
            ShowStatus(success ? "Challenge sent to " + friendNickname + "." : error);
        }));
    }

    private void RemoveFriend(string friendUid)
    {
        if (busy)
        {
            return;
        }
        SetBusy(true);
        StartCoroutine(FirebaseFriends.RemoveFriend(friendUid, (success, error) =>
        {
            SetBusy(false);
            if (!success)
            {
                ShowStatus(error);
            }
            Refresh();
        }));
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.GetComponent<TextMeshProUGUI>().text = message ?? "";
        }
    }

    private void SetBusy(bool value)
    {
        busy = value;
    }
}
