using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuneDropdownManager : MonoBehaviour
{
    public Runes value = Runes.Spear;
    private CollectionControl collection;
    
    private bool mouseOver = false;

    private void Start() {
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
    }

    public void SetRune(Runes rune)
    {
        value = rune;
        if (value == Runes.Spear)
        {
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/spear_rune");
        }
        else if (value == Runes.Shield)
        {
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/shield_rune");
        }
        else if (value == Runes.Bow)
        {
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/bow_rune");
        }
    }

    private void Update() 
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            if (value == Runes.Spear)
            {
                SetRune(Runes.Shield);
            }
            else if (value == Runes.Shield)
            {
                SetRune(Runes.Bow);
            }
            else if (value == Runes.Bow)
            {
                SetRune(Runes.Spear);
            }
            collection.UpdateRunes();
            StartCoroutine(Bounce());
            
        }
    }

    private IEnumerator Bounce()
    {
        float startTime = Time.time;
        for (int _ = 0; _ < 75; ++_)
        {
            float scale = 1f + Mathf.PingPong((Time.time - startTime) * 2.5f, 1.5f - 1f);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return new WaitForSeconds(0.005f);
        }
        transform.localScale = new Vector3(1f, 1f, 1f);
        yield return null;
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
