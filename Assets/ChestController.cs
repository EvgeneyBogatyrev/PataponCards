using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum ChestType
{
    Purple,
}

public class ChestController : MonoBehaviour
{
    private bool mouseOver = false;
    public float baseScale = 2f;
    private bool opened = false;
    public GameObject cardPrefab;
    private CardManager reward = null;
    public GameObject hat;
    private GameObject card;
    private bool UILocked = false;

    public bool mainChest = true;

    public float xShift = 10f;
    private float designatedX = 0f;

    private IEnumerator Bounce()
    {
        float startTime = Time.time;
        while (Time.time - startTime < 0.1f)
        {
            float scale = baseScale * (1f - Mathf.PingPong((Time.time - startTime) * 2f, 1.5f - 1f));
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return new WaitForSeconds(0.005f);
        }
        transform.localScale = new Vector3(baseScale, baseScale, 1f);
        yield return null;
    }
    


    // Update is called once per frame
    void Update()
    {
        if (((mouseOver && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Space) && transform.position.x - designatedX < 0.5f)) && !opened)
        {
            opened = true;
            StartCoroutine(Bounce());
            Vector3 cardPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 5f, this.gameObject.transform.position.z);
            GameObject cardObject = GameObject.Instantiate(cardPrefab, cardPosition, Quaternion.identity);
            CardManager card = cardObject.GetComponent<CardManager>();
            card.SetCardState(CardManager.CardState.openedFromPack);

            CardGenerator.CustomizeCard(card, FilterCardTypes.SelectCardFromList(FilterCardTypes.GetDefaultSet()));
            
            reward = card;
            UILocked = true;
        }

        if (opened && hat != null)
        {
            hat.transform.position = new Vector3(hat.transform.position.x - 5f * Time.deltaTime, hat.transform.position.y, hat.transform.position.z);
            float newAlpha = hat.GetComponent<SpriteRenderer>().material.color.a - 2f * Time.deltaTime;
            hat.GetComponent<SpriteRenderer>().material.color = new Color(hat.GetComponent<SpriteRenderer>().material.color.r, hat.GetComponent<SpriteRenderer>().material.color.g, hat.GetComponent<SpriteRenderer>().material.color.b, newAlpha);
            this.GetComponent<SpriteRenderer>().material.color = new Color(hat.GetComponent<SpriteRenderer>().material.color.r, hat.GetComponent<SpriteRenderer>().material.color.g, hat.GetComponent<SpriteRenderer>().material.color.b, newAlpha);
            if (newAlpha <= 0f)
            {
                newAlpha = 0f;
                GameObject.Destroy(hat.gameObject);
                this.GetComponent<SpriteRenderer>().material.color = new Color(0f, 0f, 0f, 0f);
            }
        }
        

        if (transform.position.x > designatedX)
        {
            transform.position = new Vector3(transform.position.x + (designatedX - transform.position.x) * Time.deltaTime * 10, transform.position.y, transform.position.z);
            if (card != null)
            {
                card.transform.position = new Vector3(transform.position.x + (designatedX - transform.position.x) * Time.deltaTime * 10f, transform.position.y, transform.position.z);
            }
        }

        if (UILocked)
        {
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
            {
                UILocked = false;
            }
        }

        if (opened && reward != null && !UILocked)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                CardTypes rewardType = reward.GetCardType();
                if (DeckManager.collection.ContainsKey(rewardType))
                {
                    DeckManager.collection[rewardType] += 1;
                }
                else
                {
                    DeckManager.collection.Add(reward.GetCardType(), 1);
                }
                SaveSystem.SaveCollection(DeckManager.collection);
                
                InfoSaver.chests -= 1;
                if (InfoSaver.chests <= 0)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    SceneManager.LoadScene("OpenChest");
                }
            }
        }
    }

    public void SetX(float xTo)
    {
        designatedX = xTo;
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
