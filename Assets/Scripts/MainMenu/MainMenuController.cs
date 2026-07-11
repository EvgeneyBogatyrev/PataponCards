using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Networking;

public class MainMenuController : MonoBehaviour
{
    public GameObject nicknameText;

    public IEnumerator Start()
    {
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
