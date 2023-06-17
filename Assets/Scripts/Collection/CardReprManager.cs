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

    public CollectionControl collectionObject;

    private bool mouseOver;

    public void SetName(string name)
    {
        nameObject.GetComponent<TextMeshPro>().text = name;
    }

    public void SetVisualNumber()
    {
        numberOfCopiesObject.GetComponent<TextMeshPro>().text = numberOfCopies.ToString();
    }

    public void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            numberOfCopies -= 1;
            SetVisualNumber();
            DeckManager.RemoveCard(type);
            collectionObject.ShowDeck();
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
