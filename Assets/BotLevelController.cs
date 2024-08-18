using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotLevelController : MonoBehaviour
{
    public GameObject dropdownObject;
    public void Start()
    {
        InfoSaver.botLevel = 0;
    }
    public void OnDropDownChanged(int value)
    {
        Debug.Log($"Changed to {dropdownObject.GetComponent<TMP_Dropdown>().value}");
        InfoSaver.botLevel = dropdownObject.GetComponent<TMP_Dropdown>().value - 1;
    }
}
