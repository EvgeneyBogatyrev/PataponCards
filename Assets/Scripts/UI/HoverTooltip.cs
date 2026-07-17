using UnityEngine;

// Generic "hover long enough to see a description" component - drop onto any world-space HUD
// icon (deck count, turn number, fatigue damage, etc.) and set tooltipText in the Inspector.
// Positions itself via OnGUI at the raw screen-space mouse position rather than a world-space
// object, so it can't run into the kind of camera/Z-depth raycast mismatches a 3D preview object
// can (see CardInfoManager's relevant-card hover fix) - it just always tracks the cursor.
// Requires a Collider on this GameObject for OnMouseOver/OnMouseExit to fire at all (same as
// every other hover-driven object in this project); adds a small default BoxCollider
// automatically if one isn't already present, so wiring this onto a bare text object works out
// of the box - resize/replace it in the Inspector if the default doesn't match the icon's shape.
public class HoverTooltip : MonoBehaviour
{
    [TextArea]
    public string tooltipText = "";
    public float hoverDelay = 0.6f;

    private bool mouseOver = false;
    private float hoverTimer = 0f;

    private void Awake()
    {
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 0.5f, 0.5f);
            boxCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (mouseOver)
        {
            hoverTimer += Time.deltaTime;
        }
    }

    private void OnGUI()
    {
        if (!mouseOver || hoverTimer < hoverDelay || string.IsNullOrEmpty(tooltipText))
        {
            return;
        }

        float scale = ConfirmationBanner.Scale();
        int fontSize = ConfirmationBanner.ScaledFontSize(14);
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        style.normal.textColor = Color.white;

        Vector2 textSize = style.CalcSize(new GUIContent(tooltipText));
        float width = Mathf.Clamp(textSize.x + 20f * scale, 120f * scale, 320f * scale);
        float height = style.CalcHeight(new GUIContent(tooltipText), width) + 14f * scale;

        // Input.mousePosition is measured from the bottom of the screen; OnGUI/IMGUI coordinates
        // are measured from the top - flip Y so the tooltip actually tracks the cursor.
        Vector2 mouseGuiPos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        Rect rect = new Rect(mouseGuiPos.x + 16f * scale, mouseGuiPos.y + 16f * scale, width, height);

        // Keep it fully on-screen instead of running off the right/bottom edge.
        if (rect.xMax > Screen.width) rect.x = Screen.width - rect.width - 4f;
        if (rect.yMax > Screen.height) rect.y = Screen.height - rect.height - 4f;

        Color previousColor = GUI.color;
        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(rect, tooltipText, style);
        GUI.color = previousColor;
    }

    private void OnMouseOver()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
        hoverTimer = 0f;
    }
}
