using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Sole concede entry point (round or match, GameController.RequestConcedeConfirmation picks
// which based on whose turn it is). A click just opens the Yes/Cancel confirmation banner - no
// hold-and-grow gesture, no instant concede.
public class ConcedeButton : MonoBehaviour
{
    public UITheme uiTheme;
    private GameController gameController;
    private SpriteRenderer background;

    private bool mouseOver = false;

    private void Start()
    {
        // A spectator can't act on anything, and this button's own click handling never checks
        // CursorController.cursorState (it just reads mouse-over/click state directly in
        // Update()), so it has to be disabled explicitly rather than relying on the cursor lock
        // that covers most other interactivity.
        if (InfoSaver.isSpectator)
        {
            gameObject.SetActive(false);
            return;
        }

        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        background = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            AudioController.PlaySound("click");
            gameController.RequestConcedeConfirmation();
        }

        WorldButtonSkin.Apply(background, uiTheme, danger: true, hovered: mouseOver, pressed: mouseOver && Input.GetMouseButton(0));
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
