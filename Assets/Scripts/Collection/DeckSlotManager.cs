using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DeckSlotManager : MonoBehaviour
{
    public GameObject deckNameObject;
    public GameObject rune0;
    public GameObject rune1;
    public GameObject rune2;
    public GameObject deckCountObject;
    

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
        List<CardTypes> thisDeck = SaveSystem.LoadDeck(this.index);
        int count = thisDeck.Count;
        deckCountObject.GetComponent<TextMeshPro>().text = count.ToString() + "/24";
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
        if (Input.GetMouseButtonUp(0))
        {
            List<CardTypes> thisDeck = SaveSystem.LoadDeck(this.index);
            if (thisDeck.Count == 24 || DeckLoadManager.roomToGo == "Collection")
            {
                DeckLoadManager.deckIndex = this.index;
                SceneManager.LoadScene(DeckLoadManager.roomToGo);
            }
        }
    }
    private void OnMouseExit()
    {
        mouseOver = false;
    }

}
