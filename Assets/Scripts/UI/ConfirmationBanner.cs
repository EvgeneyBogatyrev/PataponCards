using System;
using UnityEngine;

// Shared plain-IMGUI Yes/Cancel confirmation prompt - same look used for GameController's round
// and match concede confirmations and CollectionControl's delete-deck confirmation. No Canvas/TMP
// wiring needed, and keeps every "are you sure" dialog in the game visually consistent.
public static class ConfirmationBanner
{
    // IMGUI has no reference-resolution/Canvas Scaler equivalent, so every hardcoded pixel size
    // below was authored against a 1080p-tall window - on a 4K monitor that same pixel count reads
    // as tiny. Scale (used for font size and box/button dimensions alike) grows proportionally
    // with screen height above that reference, but never shrinks a smaller window's text below the
    // originally-authored size.
    private const float ReferenceHeight = 1080f;

    public static float Scale()
    {
        return Mathf.Max(1f, Screen.height / ReferenceHeight);
    }

    public static int ScaledFontSize(int baseFontSize)
    {
        return Mathf.RoundToInt(baseFontSize * Scale());
    }

    private static Texture2D _solidWhite;
    private static Texture2D SolidWhite()
    {
        if (_solidWhite == null)
        {
            _solidWhite = new Texture2D(1, 1);
            _solidWhite.SetPixel(0, 0, Color.white);
            _solidWhite.Apply();
        }
        return _solidWhite;
    }

    // The default IMGUI skin's button background is a translucent gray that reads as "see-through"
    // against the game view behind it - draw an explicit opaque backing rect first, then an
    // otherwise-transparent GUI.Button on top just for the label/click handling, so button opacity
    // isn't at the mercy of the built-in skin. Public so every other OnGUI dialog in the game
    // (GameController's connection banners, MainMenuController's waiting banner) can match.
    public static bool DrawOpaqueButton(Rect rect, string label, int fontSize, bool hovered)
    {
        Color previousColor = GUI.color;
        GUI.color = hovered ? new Color(0.95f, 0.95f, 0.95f, 1f) : new Color(0.82f, 0.82f, 0.82f, 1f);
        GUI.DrawTexture(rect, SolidWhite());
        GUI.color = previousColor;

        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            normal = { background = null },
            hover = { background = null },
            active = { background = null }
        };
        style.normal.textColor = Color.black;
        style.hover.textColor = Color.black;
        style.active.textColor = Color.black;

        return GUI.Button(rect, label, style);
    }

    public static void Draw(Color color, string message, string yesLabel, string noLabel, Action onYes, Action onCancel,
        string extraLabel = null, Action onExtra = null)
    {
        float scale = Scale();
        int fontSize = ScaledFontSize(16);

        float width = Mathf.Min(500f * scale, Screen.width - 40f);
        float messageHeight = 50f * scale;
        Rect messageRect = new Rect((Screen.width - width) / 2f, 10f * scale, width, messageHeight);

        // Drawing a plain white texture (rather than GUI.Box, whose default skin texture is
        // itself semi-transparent) means the alpha we pass is the actual final alpha, not
        // compounded with the skin's own transparency.
        Color opaqueBacking = color;
        opaqueBacking.a = Mathf.Max(color.a, 0.98f);
        Color previousColor = GUI.color;
        GUI.color = opaqueBacking;
        GUI.DrawTexture(messageRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(messageRect, message, style);
        GUI.color = previousColor;

        float buttonWidth = 220f * scale;
        float buttonHeight = 40f * scale;
        float spacing = 10f * scale;
        int buttonCount = extraLabel != null ? 3 : 2;
        float totalWidth = buttonWidth * buttonCount + spacing * (buttonCount - 1);
        float startX = (Screen.width - totalWidth) / 2f;
        float buttonY = messageRect.yMax + 10f * scale;

        Rect yesRect = new Rect(startX, buttonY, buttonWidth, buttonHeight);
        if (DrawOpaqueButton(yesRect, yesLabel, fontSize, yesRect.Contains(Event.current.mousePosition)))
        {
            onYes?.Invoke();
        }

        Rect noRect;
        if (extraLabel != null)
        {
            Rect extraRect = new Rect(startX + buttonWidth + spacing, buttonY, buttonWidth, buttonHeight);
            if (DrawOpaqueButton(extraRect, extraLabel, fontSize, extraRect.Contains(Event.current.mousePosition)))
            {
                onExtra?.Invoke();
            }
            noRect = new Rect(startX + (buttonWidth + spacing) * 2f, buttonY, buttonWidth, buttonHeight);
        }
        else
        {
            noRect = new Rect(startX + buttonWidth + spacing, buttonY, buttonWidth, buttonHeight);
        }

        if (DrawOpaqueButton(noRect, noLabel, fontSize, noRect.Contains(Event.current.mousePosition)))
        {
            onCancel?.Invoke();
        }
    }
}
