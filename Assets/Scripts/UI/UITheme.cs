using UnityEngine;
using TMPro;

// Single source of truth for the game's visual palette, used by both the Editor-time sprite
// generator/theme applier (Assets/Editor/UISpriteGenerator.cs, UIThemeApplier.cs) and runtime
// scripts that need theme colors (the world-space "fake button" scripts in Assets/Scripts/Game
// and Assets/Scripts/Collection). Create one instance via Assets > Create > PataponCards > UI Theme
// and reference it from those scripts/tools - there should only ever be one in the project.
[CreateAssetMenu(fileName = "UITheme", menuName = "PataponCards/UI Theme")]
public class UITheme : ScriptableObject
{
    [Header("Base palette")]
    public Color background = new Color(0.10f, 0.11f, 0.15f);
    public Color surface = new Color(0.16f, 0.18f, 0.24f);
    public Color primary = new Color(0.86f, 0.71f, 0.30f);
    public Color primaryHover = new Color(0.93f, 0.79f, 0.40f);
    public Color primaryPressed = new Color(0.74f, 0.59f, 0.20f);
    public Color danger = new Color(0.75f, 0.20f, 0.20f);
    public Color dangerHover = new Color(0.85f, 0.28f, 0.28f);
    public Color textPrimary = new Color(0.96f, 0.96f, 0.96f);
    public Color textMuted = new Color(0.70f, 0.70f, 0.74f);

    [Header("Input fields (light, not dark like the rest of the theme)")]
    public Color inputFieldBackground = new Color(0.97f, 0.95f, 0.90f);
    public Color textOnLight = new Color(0.16f, 0.13f, 0.10f);
    public Color textOnLightMuted = new Color(0.50f, 0.46f, 0.40f);

    [Header("Rune identity (matches CardGenerator's spear/shield/bow materials)")]
    public Color spear = new Color(0.72f, 0.20f, 0.20f);
    public Color shield = new Color(0.22f, 0.45f, 0.75f);
    public Color bow = new Color(0.25f, 0.65f, 0.35f);
    public Color neutral = new Color(0.55f, 0.55f, 0.55f);

    [Header("Card placeability (Collection/deck-builder)")]
    // Color CardManager.SetPlaceabilityIndicators retints backObject to (the semi-transparent
    // gray backing every card already has) when a card doesn't fit the deck's current runes. The
    // copy/deck-limit case uses the existing lock icon only, no tint. Deliberately much darker/
    // more opaque than backObject's baked default ({0.22,0.22,0.22,0.66}) - that default is too
    // close to a "no tint at all" gray to read as a state change on its own.
    public Color cardBlockedByRunes = new Color(0.04f, 0.04f, 0.04f, 0.92f);

    [Header("Shape")]
    [Range(0, 64)] public int cornerRadiusPx = 18;
    [Range(64, 512)] public int spriteSizePx = 128;
    [Range(1, 64)] public int spriteBorderPx = 24;

    [Header("Typography")]
    public TMP_FontAsset font;

    [Header("Generated sprites (filled in by Tools > UI > Generate Theme Sprites)")]
    public Sprite buttonNormal;
    public Sprite buttonHover;
    public Sprite buttonPressed;
    public Sprite buttonDanger;
    public Sprite panel;
    public Sprite inputField;
    public Sprite cardSlot;
}
