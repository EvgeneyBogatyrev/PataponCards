using UnityEngine;

// Shared skinning helper for the world-space "fake button" scripts (EndTurnButton, ConcedeButton,
// MulliganButton, KeepHandButton, BackToMenuButton, CollectionButton) - they're SpriteRenderer +
// OnMouseOver/Input.GetMouseButton driven, not uGUI, so they can't use UIThemeApplier's
// Button/Image pass. This keeps the state->sprite mapping in one place instead of duplicated
// across all 8 scripts. Safe to call every frame; no-ops if background or theme is unassigned.
public static class WorldButtonSkin
{
    public static void Apply(SpriteRenderer background, UITheme theme, bool danger, bool hovered, bool pressed)
    {
        if (background == null || theme == null)
        {
            return;
        }

        background.drawMode = SpriteDrawMode.Sliced;
        background.sprite = danger
            ? theme.buttonDanger
            : (pressed ? theme.buttonPressed : (hovered ? theme.buttonHover : theme.buttonNormal));
        background.color = Color.white;
    }
}
