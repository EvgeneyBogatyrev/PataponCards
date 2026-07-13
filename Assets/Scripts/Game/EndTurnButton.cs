using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Ends your turn on click - that's it. Conceding (round or match, with confirmation) lives
// entirely on ConcedeButton.cs now, not here - a single click can't mean two different things,
// and the old press-and-hold-to-concede gesture is gone in favor of click + confirmation dialog.
public class EndTurnButton : MonoBehaviour
{
    public GameObject gameObject;
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
        if (mouseOver && Input.GetMouseButtonUp(0))
        {
            gameController.EndTurnButton();
            mouseOver = false;
        }

        gameObject.GetComponent<TextMeshPro>().text = GameController.playerTurn ? "End Turn" : "Opponent's turn";

        WorldButtonSkin.Apply(background, uiTheme, danger: false, hovered: mouseOver, pressed: mouseOver && Input.GetMouseButton(0));
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
