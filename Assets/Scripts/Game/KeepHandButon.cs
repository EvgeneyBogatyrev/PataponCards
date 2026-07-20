using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeepHandButton : MonoBehaviour
{
    public UITheme uiTheme;
    private HandManager handManager;
    private SpriteRenderer background;

    private bool mouseOver = false;

    private void Start()
    {
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        background = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            AudioController.PlaySound("click");
            handManager.KeepHandButton();
            //StartCoroutine(Bounce());
        }

        WorldButtonSkin.Apply(background, uiTheme, danger: false, hovered: mouseOver, pressed: mouseOver && Input.GetMouseButton(0));
    }

    private IEnumerator Bounce()
    {
        //float startTime = Time.time;
        //while (Time.time - startTime < 1.5f)
        //{
        //    float scale = 1f + Mathf.PingPong((Time.time - startTime) * 250f * Time.deltaTime, 1.5f - 1f);
        //    transform.localScale = new Vector3(scale, scale, 1f);
        //    yield return new WaitForSeconds(0.005f);
        //}
        //transform.localScale = new Vector3(1f, 1f, 1f);
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