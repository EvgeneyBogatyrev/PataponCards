using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionButton : MonoBehaviour
{
    private CollectionControl collection;
    
    private bool mouseOver = false;

    public int status = -1;

    private float holdTime = 0f;
    private float holdTimeMax = 1.5f;

    private float startScaleX;
    private float startScaleY;

    private void Start() 
    {
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
        startScaleX = transform.localScale.x;
        startScaleY = transform.localScale.y;
    }
    private void Update() 
    {
        if (mouseOver && Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
            float scale = 1f - holdTime / holdTimeMax * 0.5f;
            transform.localScale = new Vector3(startScaleX * scale, startScaleY * scale, 1f);
            switch (status)
            {
                case 0:
                    collection.LoadNextPage();
                    break;

                case 1:
                    collection.LoadPrevPage();
                    break;

                case 2:
                    collection.BackButton();
                    break;

                case 3:
                    if (holdTime > holdTimeMax) 
                    {
                        collection.DeleteButton();
                    }
                    break;
                
                default:
                    break;
            }
        }
        else
        {
            holdTime = 0f;
            transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
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