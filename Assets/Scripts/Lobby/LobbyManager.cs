using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Networking;

public class LobbyManager : MonoBehaviour
{
    public GameObject inputField;
    public GameObject yourKey;
    public GameObject regenerateButton;
    public GameObject playButton;

    public IEnumerator Start()
    {
        // Re-pull from Firebase (the authoritative record) right before a match can start, so
        // local file tampering (editing collection.col/deck.dec directly to grant cards/decks
        // never actually earned) gets overwritten instead of silently trusted - this is the
        // last stop before a deck gets loaded and sent to an opponent.
        if (FirebaseConfig.HasAccount)
        {
            yield return CloudSave.DownloadCloudToLocal();
            DeckManager.collection = SaveSystem.LoadCollection();
        }

        DeckManager.deck = SaveSystem.LoadDeck(DeckLoadManager.deckIndex);
        DeckManager.runes = SaveSystem.LoadRunes(DeckLoadManager.deckIndex);

        // A friend challenge already negotiated myHash/opponentHash/onlineBattle over Firebase
        // before landing here (via DeckSelect) - skip generating a new random key and showing
        // the normal Play Online/manual-entry UI, and just go straight into the match once both
        // sides have actually finished picking a deck (see WaitForOpponentDeckSelect) - otherwise
        // whoever picks first would land in the Game scene alone while the other is still on
        // DeckSelect.
        if (InfoSaver.challengeAccepted)
        {
            InfoSaver.challengeAccepted = false;
            yield return WaitForOpponentDeckSelect();
            SceneManager.LoadScene("Game");
            yield break;
        }

        InfoSaver.myHash = UnityEngine.Random.Range(0, 9999);
        PrintKey();
    }

    private bool waitingForOpponentDeck = false;

    private IEnumerator WaitForOpponentDeckSelect()
    {
        waitingForOpponentDeck = true;
        yield return FirebaseChallenge.MarkDeckSelectReady();

        bool opponentReady = false;
        while (!opponentReady)
        {
            yield return FirebaseChallenge.PollOpponentDeckSelectReady(ready => opponentReady = ready);
            if (!opponentReady)
            {
                yield return new WaitForSeconds(1f);
            }
        }
        waitingForOpponentDeck = false;
    }

    // Same plain centered-message look as MainMenuController's DrawWaitingBanner, minus the
    // Cancel button - there's nothing to cancel back to here, both sides already committed to
    // this match by accepting the challenge.
    private void OnGUI()
    {
        if (!waitingForOpponentDeck)
        {
            return;
        }

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
        GUI.Label(messageRect, "Waiting for your opponent to select a deck...", style);
        GUI.color = previousColor;
    }

    public void PlayGameButton()
    {
        if (waitingForOpponentDeck)
        {
            return;
        }
        AudioController.PlaySound("click");
        if (!FirebaseConfig.HasAccount)
        {
            InfoSaver.sceneAfterLogin = "Lobby";
            SceneManager.LoadScene("Account");
            return;
        }

        int hash = Int32.Parse(inputField.GetComponent<TMP_InputField>().text);
        InfoSaver.opponentHash = hash;
        InfoSaver.onlineBattle = true;
        SceneManager.LoadScene("Game");
    }

    public void RegenerateButton()
    {
        if (waitingForOpponentDeck)
        {
            return;
        }
        AudioController.PlaySound("click");
        InfoSaver.myHash = UnityEngine.Random.Range(0, 99999);
        PrintKey();
    }

    public void PrintKey()
    {
        TextMeshProUGUI textField = yourKey.GetComponent<TextMeshProUGUI>();
        textField.text = "Key: " + InfoSaver.myHash.ToString();
    }

    public void PalyYourself()
    {
        if (waitingForOpponentDeck)
        {
            return;
        }
        AudioController.PlaySound("click");
        InfoSaver.opponentHash = InfoSaver.myHash;
        InfoSaver.onlineBattle = false;
        SceneManager.LoadScene("Game");
    }

    public void PlayOnline()
    {
        if (waitingForOpponentDeck)
        {
            return;
        }
        AudioController.PlaySound("click");
        if (!FirebaseConfig.HasAccount)
        {
            InfoSaver.sceneAfterLogin = "Lobby";
            SceneManager.LoadScene("Account");
            return;
        }

        InfoSaver.onlineBattle = true;
        SceneManager.LoadScene("FindGame");
    }

    public void Exit()
    {
        AudioController.PlaySound("click");
        SceneManager.LoadScene("MainMenu");
    }
}
