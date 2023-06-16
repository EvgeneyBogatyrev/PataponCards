using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuneDropdownManager : MonoBehaviour
{
    public Runes value;
    private CollectionControl collection;
    private bool noUpdate = false;
    void Start()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();

        dropdown.options.Clear();

        List<string> items = new List<string>();
        items.Add("Spear");
        items.Add("Shield");
        items.Add("Bow");

        foreach (var item in items)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = item });
        }
        //StartCoroutine(CallFunc(dropdown));

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }

    IEnumerator CallFunc(Dropdown dropdown)
    {
        yield return new WaitForSeconds(0.5f);
        DropdownItemSelected(dropdown);
    }

    void DropdownItemSelected(Dropdown dropdown)
    {
        int index = dropdown.value;

        if (index == 0)
        {
            value = Runes.Spear;
        }
        else if (index == 1)
        {
            value = Runes.Shield;
        }
        if (index == 2)
        {
            value = Runes.Bow;
        }
       
        if (!noUpdate)
        {
            collection.UpdateRunes();
        }
        noUpdate = false;
        
        
    } 

    public void SetRune(Runes rune)
    {
        noUpdate = true;
        value = rune;
        if (value == Runes.Spear || value == Runes.Neutral)
        {
            transform.GetComponent<Dropdown>().value = 0;
        }
        else if (value == Runes.Shield)
        {
            transform.GetComponent<Dropdown>().value = 1;
        }
        if (value == Runes.Bow)
        {
            transform.GetComponent<Dropdown>().value = 2;
        }
    }
    
}
