using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Drives the two main-menu stat displays (winrate, collection completion). The physical
// GameObjects (with a TextMeshPro/TextMeshProUGUI child) are built in the Editor and assigned
// here - this script only computes the numbers, sets the text, and attaches a HoverTooltip with
// the detailed breakdown. Called explicitly by MainMenuController.Start() (not its own Start())
// so it always runs after DeckManager.collection has been loaded/cloud-synced/dev-granted, not
// racing it.
public class PlayerStatsController : MonoBehaviour
{
    public GameObject winrateObject;
    public GameObject collectionObject;

    public void RefreshStats()
    {
        RefreshWinrate();
        RefreshCollection();
    }

    // Only matches against real players count (SaveSystem.LoadMatchStats is only ever written to
    // by GameController.RecordMatchStats, which is itself gated on InfoSaver.onlineBattle) - bot
    // practice matches never reach this counter at all.
    private void RefreshWinrate()
    {
        (int wins, int losses) = SaveSystem.LoadMatchStats();
        int totalMatches = wins + losses;

        if (totalMatches == 0)
        {
            SetText(winrateObject, "No matches yet");
            SetTooltip(winrateObject, "Play a match against another player to start tracking your winrate.");
            return;
        }

        int winratePercent = Mathf.RoundToInt(100f * wins / totalMatches);
        SetText(winrateObject, winratePercent + "% Winrate");
        SetTooltip(winrateObject, wins + " wins, " + losses + " losses vs other players (" + totalMatches + " matches). Bot matches don't count.");
    }

    // A card counts as "fully collected" at DeckManager.maxCopy (3) - having more than 3 copies
    // doesn't push the percentage past 100%, it's just clamped down to the same 3.
    private void RefreshCollection()
    {
        Dictionary<CardTypes, int> collection = SaveSystem.LoadCollection();
        List<CardTypes> collectableCards = SaveSystem.GetCollectableCards();

        int totalCopiesNeeded = collectableCards.Count * DeckManager.maxCopy;
        int ownedCopies = 0;
        int fullyOwnedCards = 0;

        foreach (CardTypes type in collectableCards)
        {
            int owned = collection.TryGetValue(type, out int count) ? Mathf.Min(count, DeckManager.maxCopy) : 0;
            ownedCopies += owned;
            if (owned >= DeckManager.maxCopy)
            {
                fullyOwnedCards += 1;
            }
        }

        int collectionPercent = totalCopiesNeeded > 0 ? Mathf.RoundToInt(100f * ownedCopies / totalCopiesNeeded) : 0;
        SetText(collectionObject, collectionPercent + "% Collection");
        SetTooltip(collectionObject, fullyOwnedCards + "/" + collectableCards.Count + " cards fully collected (3 copies each)");
    }

    // Works whether the object carries a world-space TextMeshPro or a Canvas TextMeshProUGUI -
    // matches GameController.SetHudText's same dual-support fallback used elsewhere in the HUD.
    private static void SetText(GameObject obj, string text)
    {
        if (obj == null)
        {
            return;
        }
        TextMeshPro worldText = obj.GetComponent<TextMeshPro>();
        if (worldText != null)
        {
            worldText.text = text;
            return;
        }
        TextMeshProUGUI uiText = obj.GetComponent<TextMeshProUGUI>();
        if (uiText != null)
        {
            uiText.text = text;
        }
    }

    private static void SetTooltip(GameObject obj, string text)
    {
        if (obj == null)
        {
            return;
        }
        HoverTooltip tooltip = obj.GetComponent<HoverTooltip>();
        if (tooltip == null)
        {
            tooltip = obj.AddComponent<HoverTooltip>();
        }
        tooltip.tooltipText = text;
    }
}
