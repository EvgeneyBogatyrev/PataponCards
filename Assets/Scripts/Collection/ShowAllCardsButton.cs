using UnityEngine;
using TMPro;

// World-space toggle for CollectionControl's Show All Cards state - same SpriteRenderer +
// OnMouseOver/Input-driven click pattern as CollectionButton/ConcedeButton/etc. (not a uGUI
// Toggle), since this needs to sit as a physical button in the Collection scene like everything
// else there. Unlike a momentary button, its skin reflects the sticky on/off state rather than
// just the instantaneous mouse-down.
public class ShowAllCardsButton : MonoBehaviour
{
    public UITheme uiTheme;

    // Optional - a child/sibling TextMeshPro label. Left unwired, the button still works, just
    // with no on/off text of its own (only the pressed-look sprite state to go by).
    public GameObject label;
    public string offLabel = "Show All Cards";
    public string onLabel = "Show All Cards: On";

    private CollectionControl collection;
    private SpriteRenderer background;

    private bool mouseOver = false;
    private bool isOn = false;

    private void Start()
    {
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
        background = GetComponent<SpriteRenderer>();
        UpdateLabel();
    }

    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            AudioController.PlaySound("click");
            isOn = !isOn;
            collection.SetShowAllCards(isOn);
            UpdateLabel();
        }

        // "pressed" reused here as the toggle's sticky on-state (not just the instantaneous
        // mouse-down every other world button uses it for) - WorldButtonSkin has no separate
        // toggled-on sprite, and buttonPressed already reads as "this is active" at a glance.
        WorldButtonSkin.Apply(background, uiTheme, danger: false, hovered: mouseOver, pressed: isOn || (mouseOver && Input.GetMouseButton(0)));
    }

    private void UpdateLabel()
    {
        if (label != null)
        {
            label.GetComponent<TextMeshPro>().text = isOn ? onLabel : offLabel;
        }
    }

    private void OnMouseOver()
    {
        mouseOver = true;
    }
    private void OnMouseExit()
    {
        mouseOver = false;
    }
}
