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

    private bool mouseOver;
    public CardManager previewedCard = null;
    public GameObject cardPrefab;
    public int index = 0;

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
    }

    public GameObject GenerateCard(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab);
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        return newCard;
    }

    private void OnMouseOver()
    {
        mouseOver = true;
        if (previewedCard == null)
        {
            CardPreview();
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
            previewedCard = GenerateCard(type).GetComponent<CardManager>();
            previewedCard.SetCardState(CardManager.CardState.hilightOver);
            float yshift = 3.5f;
            if (index < 4)
            {
                yshift = -0.5f;
            }
            float xshift = -4.5f;
            if (relevantCard)
            {
                xshift = 3.5f;
            }
            previewedCard.transform.position = this.transform.position + new Vector3(xshift, yshift, -7f);
        }
    }

}
