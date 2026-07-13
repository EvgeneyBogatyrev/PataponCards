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
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        background = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
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
