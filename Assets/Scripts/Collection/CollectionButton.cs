using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionButton : MonoBehaviour
{
    private CollectionControl collection;
    
    private bool mouseOver = false;

    public int status = -1;

    private void Start() 
    {
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
    }
    private void Update() 
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
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
                
                default:
                    break;
            }
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