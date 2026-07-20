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
    // How much smaller "Opponent's turn" renders relative to "End Turn"'s own authored size -
    // that string is noticeably longer, so it needs to shrink a bit to fit the same button.
    public float opponentsTurnFontScale = 0.65f;
    private GameController gameController;
    private SpriteRenderer background;
    private TextMeshPro label;
    private float normalFontSize;

    private bool mouseOver = false;

    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        background = GetComponent<SpriteRenderer>();
        label = gameObject.GetComponent<TextMeshPro>();
        normalFontSize = label.fontSize;
    }

    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonUp(0))
        {
            AudioController.PlaySound("click");
            gameController.EndTurnButton();
            mouseOver = false;
        }

        bool opponentsTurn = !GameController.playerTurn;
        label.text = opponentsTurn ? "Opponent's turn" : "End Turn";
        label.fontSize = opponentsTurn ? normalFontSize * opponentsTurnFontScale : normalFontSize;

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
