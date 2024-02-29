using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConcedeButton : MonoBehaviour
{
    private GameController gameController;
    
    private bool mouseOver = false;

    private void Start() 
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }
    private void Update() 
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            gameController.Concede();
            StartCoroutine(Bounce());
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