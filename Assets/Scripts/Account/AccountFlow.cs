using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Networking;

// Shared post-login step for both the Account (sign-in) and CreateAccount (sign-up) scenes.
public static class AccountFlow
{
    public static void OnLoginSuccess()
    {
        PresenceHeartbeat.Start();

        // Load everything MainMenuController would normally load on first boot, since this now
        // runs before MainMenu ever does and its own guard would otherwise be skipped.
        DeckManager.collection = SaveSystem.LoadCollection();
        DeckManager.deck = SaveSystem.LoadDeck(DeckLoadManager.deckIndex);
        DeckManager.runes = SaveSystem.LoadRunes(DeckLoadManager.deckIndex);
        List<bool> botStats = SaveSystem.LoadBotStats();
        for (int i = 0; i < botStats.Count; ++i)
        {
            InfoSaver.botDefeated[i] = botStats[i];
        }

        if (InfoSaver.sceneAfterLogin == "MainMenu")
        {
            InfoSaver.justLoggedIn = true;
        }

        SceneManager.LoadScene(InfoSaver.sceneAfterLogin);
    }
}
