using System;
using UnityEngine;

// Shared plain-IMGUI Yes/Cancel confirmation prompt - same look used for GameController's round
// and match concede confirmations and CollectionControl's delete-deck confirmation. No Canvas/TMP
// wiring needed, and keeps every "are you sure" dialog in the game visually consistent.
public static class ConfirmationBanner
{
    public static void Draw(Color color, string message, string yesLabel, string noLabel, Action onYes, Action onCancel,
        string extraLabel = null, Action onExtra = null)
    {
        float width = Mathf.Min(500f, Screen.width - 40f);
        Rect messageRect = new Rect((Screen.width - width) / 2f, 10f, width, 50f);

        // Drawing a plain white texture (rather than GUI.Box, whose default skin texture is
        // itself semi-transparent) means the alpha we pass is the actual final alpha, not
        // compounded with the skin's own transparency.
        Color opaqueBacking = color;
        opaqueBacking.a = Mathf.Max(color.a, 0.95f);
        Color previousColor = GUI.color;
        GUI.color = opaqueBacking;
        GUI.DrawTexture(messageRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(messageRect, message, style);
        GUI.color = previousColor;

        float buttonWidth = 220f;
        float spacing = 10f;
        int buttonCount = extraLabel != null ? 3 : 2;
        float totalWidth = buttonWidth * buttonCount + spacing * (buttonCount - 1);
        float startX = (Screen.width - totalWidth) / 2f;

        Rect yesRect = new Rect(startX, 70f, buttonWidth, 40f);
        if (GUI.Button(yesRect, yesLabel))
        {
            onYes?.Invoke();
        }

        Rect noRect;
        if (extraLabel != null)
        {
            Rect extraRect = new Rect(startX + buttonWidth + spacing, 70f, buttonWidth, 40f);
            if (GUI.Button(extraRect, extraLabel))
            {
                onExtra?.Invoke();
            }
            noRect = new Rect(startX + (buttonWidth + spacing) * 2f, 70f, buttonWidth, 40f);
        }
        else
        {
            noRect = new Rect(startX + buttonWidth + spacing, 70f, buttonWidth, 40f);
        }

        if (GUI.Button(noRect, noLabel))
        {
            onCancel?.Invoke();
        }
    }
}
