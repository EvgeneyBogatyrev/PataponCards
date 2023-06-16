using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardReprManager : MonoBehaviour
{
    public GameObject nameObject;
    public CardTypes type;
    public GameObject qtyObject;
    public int qty;

    public CollectionControl collectionObject;

    private bool mouseOver;

    public void SetName(string name)
    {
        nameObject.GetComponent<TextMeshPro>().text = name;
    }

    public void SetQty()
    {
        qtyObject.GetComponent<TextMeshPro>().text = qty.ToString();
    }

    public void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            qty -= 1;
            SetQty();
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
