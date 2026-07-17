using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardReprManager : MonoBehaviour
{
    public GameObject nameObject;
    public CardTypes type;
    public GameObject numberOfCopiesObject;
    public int numberOfCopies;
    public bool relevantCard = false;

    public CollectionControl collectionObject;
    public UITheme uiTheme;
    private SpriteRenderer background;
    private bool alternateRow;

    private bool mouseOver;
    public CardManager previewedCard = null;
    public GameObject cardPrefab;
    public int index = 0;

    private void Start()
    {
        background = GetComponent<SpriteRenderer>();
    }

    // Called by CollectionControl.ShowDeck() with the row/column position so adjacent rows read
    // as distinct bands instead of a flat wall of identical rows.
    public void SetRowIndex(int position)
    {
        alternateRow = position % 2 == 1;
    }

    public void SetName(string name)
    {
        nameObject.GetComponent<TextMeshPro>().text = name;
    }

    public void SetVisualNumber()
    {
        if (!relevantCard)
        {
            numberOfCopiesObject.GetComponent<TextMeshPro>().text = numberOfCopies.ToString();
        }
        else
        {
            numberOfCopiesObject.GetComponent<TextMeshPro>().text = "";
        }
    }

    public void Update()
    {
        if (!relevantCard && mouseOver && Input.GetMouseButtonUp(0))
        {
            numberOfCopies -= 1;
            SetVisualNumber();
            DeckManager.RemoveCard(type);
            collectionObject.ShowDeck();
        }

        if (background != null && uiTheme != null)
        {
            Color baseColor = alternateRow ? new Color(0.92f, 0.92f, 0.92f, 1f) : Color.white;
            background.color = mouseOver ? Color.Lerp(baseColor, uiTheme.primaryHover, 0.3f) : baseColor;
        }
    }

    public GameObject GenerateCard(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab);
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        newCard.GetComponent<CardManager>().numberOfCardsObject.SetActive(false);
        return newCard;
    }

    private void OnMouseOver()
    {
        if (CursorController.cursorState == CursorController.CursorStates.Free || relevantCard)
        {
            mouseOver = true;
            if (previewedCard == null)
            {
                CardPreview();
            }
        }
    }
    private void OnMouseExit()
    {
        mouseOver = false;
        if (previewedCard != null)
        {
            Destroy(previewedCard.gameObject);
            previewedCard = null;
        }
    }

    private void CardPreview()
    {
        if (previewedCard == null)
        {
            GameObject generated = GenerateCard(type);
            previewedCard = generated.GetComponent<CardManager>();
            previewedCard.SetCardState(CardManager.CardState.hilightOver);
            float yshift = 3.5f;
            if (index < 4)
            {
                yshift = 1.5f;
            }
            float xshift = -4.5f;
            if (relevantCard)
            {
                xshift = 3.5f;
            }
            // A small negative Z (just enough to draw in front of/behind neighboring popup
            // elements for sorting) - NOT a large offset. The previous -7f pushed this world
            // position 7 units further from whatever camera is active; that happened to still
            // land just in front of the Collection scene's camera (at Z=-10) but landed a full
            // unit BEHIND the Game scene's camera (at Z=-8), making the preview invisible
            // in-game despite being generated and positioned correctly every time.
            previewedCard.transform.position = this.transform.position + new Vector3(xshift, yshift, -1.5f);
        }
    }

}
