using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    public UITheme uiTheme;
    private SpriteRenderer background;

    private bool mouseOver = false;

    private void Start()
    {
        background = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("MainMenu");
        }

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