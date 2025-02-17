using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public GameObject inputField;
    public GameObject yourKey;
    public GameObject regenerateButton;
    public GameObject playButton;

    public void Start()
    {
        DeckManager.deck = SaveSystem.LoadDeck(DeckLoadManager.deckIndex);
        DeckManager.runes = SaveSystem.LoadRunes(DeckLoadManager.deckIndex);
        InfoSaver.myHash = UnityEngine.Random.Range(0, 9999);
        PrintKey();
    }

    public void PlayGameButton()
    {
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
        InfoSaver.onlineBattle = true;
        SceneManager.LoadScene("FindGame");
    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
