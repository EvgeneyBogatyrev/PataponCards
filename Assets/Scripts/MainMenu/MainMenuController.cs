using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayButton()
    {
        DeckManager.deck = SaveSystem.LoadDeck();
        DeckManager.runes = SaveSystem.LoadRunes();
        SceneManager.LoadScene("Lobby");
    }

    public void CollectionButton()
    {
        SceneManager.LoadScene("Collection");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
