using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndTurnButton : MonoBehaviour
{
    public GameObject gameObject;
    private GameController gameController;
    
    private bool mouseOver = false;
    private float concedeTimer = 0f;
    private float concedeTimerMax = 3f;
    private bool concedeMode = false;
    private float startScaleX;
    private float startScaleY;

    private void Start() 
    {
        startScaleX = transform.localScale.x;
        startScaleY = transform.localScale.y;
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }
    private void Update() 
    {
        if (mouseOver && Input.GetMouseButtonUp(0) && !concedeMode)
        {
            gameController.EndTurnButton();
            mouseOver = false;
            //StartCoroutine(Bounce());
        }

        if (mouseOver && Input.GetMouseButton(0))
        {
            concedeTimer += Time.deltaTime;
            if (concedeTimer >= concedeTimerMax)
            {
                gameController.Concede();
                concedeTimer = 0f;
            }
            if (concedeTimer > 0.5f)
            {
                concedeMode = true;
            }
        }
        else
        {
            concedeTimer = 0f;
            concedeMode = false;
            transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
        }

        if (concedeMode)
        {
            gameObject.GetComponent<TextMeshPro>().text = "CONCEDE";
            float scale = 1f + concedeTimer / concedeTimerMax * 0.5f;
            transform.localScale = new Vector3(startScaleX * scale, startScaleY * scale, 1f);
        }
        else
        {
            gameObject.GetComponent<TextMeshPro>().text = "End Turn";
        }
        
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