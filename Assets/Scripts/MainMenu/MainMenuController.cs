using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Start()
    {
        if (DeckManager.collection == null)
        {
            DeckManager.collection = SaveSystem.LoadCollection();
            List<bool> botStats = SaveSystem.LoadBotStats();
            for (int i = 0; i < botStats.Count; ++i)
            {
                InfoSaver.botDefeated[i] = botStats[i];
            }
        }
    }
    public void PlayButton()
    {
        if (SaveSystem.LoadRunes(0).Count > 0)
        {
            DeckLoadManager.roomToGo = "Lobby";
            SceneManager.LoadScene("DeckSelect");
        }
    }

    public void CollectionButton()
    {
        //SceneManager.LoadScene("Collection");
        DeckLoadManager.roomToGo = "Collection";
        SceneManager.LoadScene("DeckSelect");
        
    }

    public void Patahell()
    {
        Application.OpenURL("https://discord.gg/4p9RJuhcjx");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
