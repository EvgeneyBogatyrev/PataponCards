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
        InfoSaver.myHash = UnityEngine.Random.Range(0, 9999);
        PrintKey();
    }

    public void PlayGameButton()
    {
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
        InfoSaver.opponentHash = InfoSaver.myHash;
        InfoSaver.onlineBattle = false;
        SceneManager.LoadScene("Game");
    }

    public void PlayOnline()
    {
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
        SceneManager.LoadScene("MainMenu");
    }
}
