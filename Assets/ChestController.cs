using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    private bool mouseOver = false;
    public float baseScale = 2f;
    private bool opened = false;
    public GameObject cardPrefab;
    private CardManager reward = null;
    private IEnumerator Bounce()
    {
        float startTime = Time.time;
        while (Time.time - startTime < 0.5f)
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
        if (mouseOver && Input.GetMouseButtonDown(0) && !opened)
        {
            opened = true;
            StartCoroutine(Bounce());
            Vector3 cardPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 5f, this.gameObject.transform.position.z);
            GameObject cardObject = GameObject.Instantiate(cardPrefab, cardPosition, Quaternion.identity);
            CardManager card = cardObject.GetComponent<CardManager>();
            card.SetCardState(CardManager.CardState.openedFromPack);

            CardGenerator.CustomizeCard(card, FilterCardTypes.SelectCardFromList(FilterCardTypes.GetShitSet()));
            reward = card;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (reward != null)
            {
                GameObject.Destroy(reward.gameObject);
                opened = false;
            }
        }
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
