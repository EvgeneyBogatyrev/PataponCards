using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckSlotManager : MonoBehaviour
{
    public GameObject deckNameObject;
    public GameObject rune0;
    public GameObject rune1;
    public GameObject rune2;
    

    private int index;
    private bool mouseOver = false;

    public void Customize(List<Runes> runes, int index, string deckName="")
    {
        if (runes != null)
        {
            SetRune(runes[0], rune0);
            SetRune(runes[1], rune1);
            SetRune(runes[2], rune2);
        }
        else
        {
            rune0.SetActive(false);
            rune1.SetActive(false);
            rune2.SetActive(false);
        }

        deckNameObject.GetComponent<TextMeshPro>().text = deckName;

        this.index = index;
    }

    private void SetRune(Runes rune, GameObject runeObject)
    {
        if (rune == Runes.Spear)
        {
            runeObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/spear_rune");
        }
        else if (rune == Runes.Shield)
        {
            runeObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/shield_rune");
        }
        else if (rune == Runes.Bow)
        {
            runeObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/bow_rune");
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
