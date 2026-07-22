using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One row in the friends panel - accepted/incoming/outgoing sections all reuse this same
// prefab, configured differently by FriendsPanelController.Populate() per section (which
// button(s) show, what they do).
public class FriendRow : MonoBehaviour
{
    public GameObject nicknameText;
    public GameObject primaryButtonObject;
    public GameObject primaryButtonLabel;
    public GameObject secondaryButtonObject;
    public GameObject secondaryButtonLabel;

    // Optional - a small dot/Image GameObject. Left unwired, SetOnline is a no-op, so existing
    // rows built before this was added keep working with no editor changes required.
    public GameObject onlineIndicatorObject;
    private static readonly Color OnlineColor = new Color(0.3f, 0.8f, 0.35f);
    private static readonly Color OfflineColor = new Color(0.5f, 0.5f, 0.5f);

    // Optional - a third button slot, shown only for accepted rows while the friend is online
    // (see SetOnline). Left unwired, no Challenge button ever appears - no editor change forced
    // on existing FriendRow setups.
    public GameObject challengeButtonObject;
    public GameObject challengeButtonLabel;

    public void Setup(string nickname, string primaryLabel, Action onPrimary, string secondaryLabel = null, Action onSecondary = null)
    {
        nicknameText.GetComponent<TextMeshProUGUI>().text = nickname;

        // Deactivate every optional slot up front, then only reactivate what's actually needed.
        // challengeButtonObject specifically matters here: SetOnline only runs once an async
        // presence check resolves (a Firebase round-trip later), so without this it would sit
        // in whatever state the prefab happened to be authored with - visibly "flickering" in
        // (and shoving Accept/Remove sideways as the Horizontal Layout Group reflows around it)
        // once SetOnline finally ran a moment later.
        SetActiveIfPresent(challengeButtonObject, false);
        SetActiveIfPresent(onlineIndicatorObject, false);

        ConfigureButton(primaryButtonObject, primaryButtonLabel, primaryLabel, onPrimary);
        ConfigureButton(secondaryButtonObject, secondaryButtonLabel, secondaryLabel, onSecondary);
    }

    // Called separately/later than Setup, since presence is fetched with its own async request
    // per accepted friend rather than being part of the main friends-list read. onChallenge is
    // only meaningful for accepted rows - passing null (the default) keeps the Challenge button
    // hidden regardless of online state, same as leaving challengeButtonObject unwired.
    // inMatch/onSpectate swap this same slot to "Spectate" instead of "Challenge" while the
    // friend is in a real online match - the two are mutually exclusive (a friend who's in a
    // match isn't challengeable anyway), so there's only ever one button here at a time.
    public void SetOnline(bool online, Action onChallenge = null, bool inMatch = false, Action onSpectate = null)
    {
        if (onlineIndicatorObject != null)
        {
            onlineIndicatorObject.SetActive(true);
            Image image = onlineIndicatorObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = online ? OnlineColor : OfflineColor;
            }
        }

        SetActiveIfPresent(challengeButtonObject, false);
        if (online && inMatch && onSpectate != null)
        {
            ConfigureButton(challengeButtonObject, challengeButtonLabel, "Spectate", onSpectate);
        }
        else
        {
            ConfigureButton(challengeButtonObject, challengeButtonLabel, (online && onChallenge != null) ? "Challenge" : null, onChallenge);
        }
    }

    private static void SetActiveIfPresent(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    private static void ConfigureButton(GameObject buttonObject, GameObject labelObject, string label, Action onClick)
    {
        if (buttonObject == null)
        {
            return;
        }
        bool show = label != null;
        buttonObject.SetActive(show);
        if (!show)
        {
            return;
        }
        if (labelObject != null)
        {
            labelObject.GetComponent<TextMeshProUGUI>().text = label;
        }
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            AudioController.PlaySound("click");
            onClick?.Invoke();
        });
    }
}
