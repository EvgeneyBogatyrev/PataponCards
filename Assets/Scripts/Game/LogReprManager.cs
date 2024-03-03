using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogReprManager : MonoBehaviour
{
    public CardTypes type;
    public int power;
    public GameObject powerObject;
    public GameObject heartObject;
    public GameObject textObject;
    public GameObject damageObject;
    public GameObject imageObject;
    public GameObject cycleObject;
    public GameObject attackObject;
    public GameObject outline;
    public GameObject outlineOpp;
    public HandManager handManager;
    public Arrow arrow = null;
    public int index = 0;


    private bool mouseOver;
    public CardManager previewedCard = null;
    public List<CardManager> targetCards = new();


    public List<CardTypes> targets;
    public bool friendly;
    public bool isCycled = false;
    public bool isAttacking = false;
    
    public void Start()
    {
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
    }

    public void Update()
    {
        if (arrow != null && previewedCard != null)
        {
            arrow.UpdatePosition(previewedCard.transform.position);
        }
    }

    public void Customize(CardTypes cardType, bool friendly, int power=-1)
    {
        type = cardType;
        
        CardManager.CardStats cardStats = CardTypeToStats.GetCardStats(type);
        if (power != -1)
        {
            cardStats.power = power;
        }
        imageObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/" + cardStats.imagePath);
        
        heartObject.SetActive(false);
        powerObject.SetActive(false);
        damageObject.SetActive(false);
        textObject.SetActive(false);
        cycleObject.SetActive(false);
        attackObject.SetActive(false);

        if (friendly)
        {
            outlineOpp.SetActive(false);
        }
        else
        {
            outline.SetActive(false);
        }

        if (isCycled)
        {
            cycleObject.SetActive(true);
        }
        
        if (isAttacking)
        {
            attackObject.SetActive(true);
        }
        
    }


    private void OnMouseOver()
    {
        mouseOver = true;
        if (previewedCard == null)
        {
            CardPreview();
        }
    }
    private void OnMouseExit()
    {
        mouseOver = false;
        if (previewedCard != null)
        {
            previewedCard.DestroyCard();
        }
        foreach (CardManager card in targetCards)
        {
            card.DestroyCard();
        }
        targetCards = new();
        if (arrow != null)
        {
            arrow.DestroyArrow();
            arrow = null;
        }
    }

    private void CardPreview()
    {
        if (previewedCard == null)
        {
            previewedCard = handManager.GenerateCard(type).GetComponent<CardManager>();
            previewedCard.SetCardState(CardManager.CardState.hilightOver);

            if (isCycled)
            {
                previewedCard.SetName("Cycling: " + previewedCard.GetName());
                previewedCard.SetNameSize(3);
            }
            
            if (index < 4)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(3.5f, 3.5f, -7f);
            }
            else
            {
                previewedCard.transform.position = this.transform.position + new Vector3(3.5f, -2.5f, -7f);
            }
        }

        targetCards = new();
        int i = 0;
        foreach (CardTypes type in targets)
        {
            CardManager _card = handManager.GenerateCard(type).GetComponent<CardManager>();
            _card.SetCardState(CardManager.CardState.hilightOver);
            
            if (index < 4)
            {
                _card.transform.position = this.transform.position + new Vector3(3.5f + 4.5f * (1 + i), 3.5f, -7f);
            }
            else
            {
                _card.transform.position = this.transform.position + new Vector3(3.5f + 4.5f * (1 + i), -2.5f, -7f);
            }

            if (i == 0)
            {
                arrow = new Arrow(previewedCard.transform.position, _card.transform.position);
            }

            i += 1;
            targetCards.Add(_card);
        }    
    }

}
