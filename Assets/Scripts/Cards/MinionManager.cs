using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinionManager : MonoBehaviour
{
    public enum MinionState
    {
        Free,
        Selected,
        ChooseOption,
    }

    public GameObject imageObject;
    public GameObject powerObject;
    public GameObject powerSquare;
    public GameObject heartObject;
    public float normalScale;
    public float selectedScale;

    private bool attacking = false;
    private Vector3 attackPosition;

    private Vector3 desiredPosition;
    private float desiredScale;
    private int power;
    private bool friendly;
    private BoardManager.Slot connectedSlot;
    private CardTypes cardType;
    private CardManager.CardStats cardStats;
    private bool mouseOver;
    private MinionState state;
    private bool summoningSickness = true;
    private List<CardManager> connectedCardObjects;
    private Arrow arrow = null;
    public float fadeSpeed = 10f;
    public float shrinkSpeed = 3f;
    public bool fading = false;
    public bool dying = false;

    public void CustomizeMinion(CardManager playedCard, BoardManager.Slot slot)
    {
        SetCardType(playedCard.GetCardType());
        SetCardStats(playedCard.GetCardStats());
        SetPower(playedCard.GetPower());
        connectedSlot = slot;
        friendly = slot.GetFriendly();
        connectedSlot.SetFree(false);
        imageObject.GetComponent<SpriteRenderer>().sprite = playedCard.GetComponentInChildren<SpriteRenderer>().sprite;
        state = MinionState.Free;
        summoningSickness = true;
        if (cardStats.hasHaste || cardStats.isStatic)
        {
            summoningSickness = false;
        }
        slot.SetConnectedMinion(this);

        if (cardStats.isStatic || cardStats.canAttack == false)
        {
            powerSquare.SetActive(false);
        }
        else
        {
            heartObject.SetActive(false);
        }
    }

    private BoardManager boardManager;
    private GameController gameController;
    private HandManager handManager;

    private void Start()
    {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void SetCardType(CardTypes type)
    {
        cardType = type;
    }

    public CardTypes GetCardType()
    {
        return cardType;
    }

    public void SetCardStats(CardManager.CardStats stats)
    {
        cardStats = stats.CopyStats();
    }

    public CardManager.CardStats GetCardStats()
    {
        return cardStats;
    }

    public void SetPower(int p)
    {
        power = p;
        powerObject.GetComponent<TextMeshPro>().text = p.ToString();
    }

    public int GetPower()
    {
        return power;
    }

    public void TakePower(int damage)
    {
        SetPower(power - damage);
        CheckPower();
    }

    public void ReceiveDamage(int damage)
    {
        if (cardType == CardTypes.Hatapon)
        {
            if (IsHataponSafe(friendly))
            {
                return;
            }
        }
        int actualDamage = damage - cardStats.armor;
        if (actualDamage < 0) 
        {
            actualDamage = 0;
        }
        TakePower(actualDamage);
    }

    public void Heal(int amount)
    {
        SetPower(power + amount);
        CheckPower();
    }

    public bool IsHataponSafe(bool friendlyHatapon)
    {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        if (!friendlyHatapon)
        {
            foreach (BoardManager.Slot slot in boardManager.enemySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardStats().hasShield)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
            {
                MinionManager minion = slot.GetConnectedMinion();
                if (minion != null && minion.GetCardStats().hasShield)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Update()
    {
        
        switch (state)
        {
            case MinionState.Free:

                if (friendly)
                {
                    if (!mouseOver)
                    {
                        desiredPosition = new Vector3(connectedSlot.GetPosition().x, connectedSlot.GetPosition().y, connectedSlot.GetPosition().z - 0.1f);
                        //transform.localScale = new Vector3(normalScale, normalScale, 1f);
                        desiredScale = normalScale;
                    }
                    else if (CursorController.cursorState == CursorController.CursorStates.Free && !summoningSickness)
                    {
                        desiredPosition = new Vector3(connectedSlot.GetPosition().x, connectedSlot.GetPosition().y, connectedSlot.GetPosition().z - 0.2f);
                        desiredScale = selectedScale;

                        if (Input.GetMouseButtonDown(0) && !cardStats.isStatic)
                        {
                            state = MinionState.Selected;
                            CursorController.cursorState = CursorController.CursorStates.Hold;
                        }
                        else if (cardStats.isStatic)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                state = MinionState.ChooseOption;
                                CursorController.cursorState = CursorController.CursorStates.ChooseOption;
                                connectedCardObjects = new List<CardManager>();

                                CardManager option1 = handManager.GenerateCard(cardStats.connectedCards[0]).GetComponent<CardManager>();
                                CardManager option2 = handManager.GenerateCard(cardStats.connectedCards[1]).GetComponent<CardManager>();

                                option1.SetCardState(CardManager.CardState.asOption);
                                option2.SetCardState(CardManager.CardState.asOption);

                                option1.transform.position = this.transform.position + new Vector3(-3f, 1.75f, -2.75f);
                                option2.transform.position = this.transform.position + new Vector3(3f, 1.75f, -2.75f);

                                while (option1.transform.position.x < -8f)
                                {
                                    option1.transform.position = new Vector3(option1.transform.position.x + 0.1f, option1.transform.position.y, option1.transform.position.z);
                                    option2.transform.position = new Vector3(option2.transform.position.x + 0.1f, option2.transform.position.y, option2.transform.position.z);
                                }

                                while (option2.transform.position.x > 8f)
                                {
                                    option1.transform.position = new Vector3(option1.transform.position.x - 0.1f, option1.transform.position.y, option1.transform.position.z);
                                    option2.transform.position = new Vector3(option2.transform.position.x - 0.1f, option2.transform.position.y, option2.transform.position.z);
                                }

                                option1.hostMinion = this;
                                option2.hostMinion = this;

                                connectedCardObjects.Add(option1);
                                connectedCardObjects.Add(option2);

                                //powerObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        desiredPosition = new Vector3(connectedSlot.GetPosition().x, connectedSlot.GetPosition().y, connectedSlot.GetPosition().z - 0.1f);
                        desiredScale = normalScale;
                    }
                }
                else
                {
                    desiredPosition = new Vector3(connectedSlot.GetPosition().x, connectedSlot.GetPosition().y, connectedSlot.GetPosition().z - 0.1f);
                    desiredScale = normalScale;
                }

                break;

            case MinionState.Selected:
                if (arrow == null)
                {
                    arrow = new Arrow(transform.position);
                }
                arrow.UpdatePosition();
                desiredPosition = new Vector3(connectedSlot.GetPosition().x, connectedSlot.GetPosition().y, connectedSlot.GetPosition().z - 0.2f);
                desiredScale = selectedScale;
                if (Input.GetMouseButtonDown(1))
                {
                    state = MinionState.Free;
                    CursorController.cursorState = CursorController.CursorStates.Free;
                    arrow.DestroyArrow();
                    arrow = null;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.name == "Minion(Clone)" && !cardStats.isStatic) // Fix this shit!
                        {
                            MinionManager target = hit.collider.GetComponent<MinionManager>();

                            if (target != this && target.friendly && (Mathf.Abs(target.GetIndex() - GetIndex()) == 1) && !cardStats.limitedVision)
                            {
                                if (!target.cardStats.limitedVision && !target.summoningSickness && !target.cardStats.isStatic)
                                {
                                    Exchange(target.GetSlot(), record: true);
                                    state = MinionState.Free;
                                    CursorController.cursorState = CursorController.CursorStates.Free;
                                    arrow.DestroyArrow();
                                    arrow = null;
                                }
                            }
                            else
                            {

                                bool existsGreatshield = false;
                                foreach (BoardManager.Slot enemySlot in boardManager.enemySlots)
                                {
                                    if (enemySlot.GetConnectedMinion() != null && enemySlot.GetConnectedMinion().cardStats.hasGreatshield)
                                    {
                                        existsGreatshield = true;
                                        break;
                                    }
                                }

                                if (!cardStats.canAttack || target == this || target.friendly || target.cardStats.flying || (Mathf.Abs(target.GetIndex() - GetIndex()) > 1 && !cardStats.megaVision) || (!target.cardStats.hasGreatshield && existsGreatshield))
                                {
                                    state = MinionState.Free;
                                    CursorController.cursorState = CursorController.CursorStates.Free;
                                    arrow.DestroyArrow();
                                    arrow = null;
                                }
                                else
                                {
                                    Attack(target, record: true);
                                    summoningSickness = true;
                                    state = MinionState.Free;
                                    CursorController.cursorState = CursorController.CursorStates.Free;
                                    arrow.DestroyArrow();
                                    arrow = null;
                                }
                            }

                            
                        }
                        else if (hit.collider.name == "Slot(Clone)") // Fix this shit!
                        {
                            BoardManager.Slot slotToGo = null;
                            foreach (BoardManager.Slot slot in boardManager.friendlySlots)
                            { 
                                if (slot.GetSlotObject() == hit.collider.gameObject)
                                {
                                    slotToGo = slot;
                                    break;
                                }
                            }
                            if (slotToGo == null || !slotToGo.GetFree() || Mathf.Abs(slotToGo.GetIndex() - GetIndex()) > 1 || cardStats.limitedVision)
                            {
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            }
                            else
                            {
                                Move(slotToGo, record: true);
                                summoningSickness = true;
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            }
                        }
                    }
                }

                break;

            case MinionState.ChooseOption:

                if (Input.GetMouseButtonDown(1))
                {
                    ReturnToNormalAfterOptions();
                }

                break;

            default:
                break;
        }

        if (attacking)
        {
            transform.position += (attackPosition - transform.position) * Time.deltaTime * 6f;

            Vector3 diff = (transform.position - attackPosition);
            float length = diff.magnitude;

            if (length < 0.1)
            {
                attacking = false;
            }

        }
        else
        {
            transform.position += (desiredPosition - transform.position) * Time.deltaTime * 6f;
        }
        if (!dying)
        {
            transform.localScale = new Vector3(transform.localScale.x + (desiredScale - transform.localScale.x) * Time.deltaTime * 15f, transform.localScale.y + (desiredScale - transform.localScale.y) * Time.deltaTime * 15f, 1f);
        }
    }

    public void Move(BoardManager.Slot slotToMove, bool record=false)
    {
        if (record)
        {
            ServerDataProcesser.instance.Move(connectedSlot.GetIndex() + 1, slotToMove.GetIndex() + 1);
        }

        connectedSlot.SetFree(true);
        connectedSlot.SetConnectedMinion(null);
        slotToMove.SetConnectedMinion(this);
        slotToMove.SetFree(false);
        connectedSlot = slotToMove;
    }

    public void Exchange(BoardManager.Slot slotToMove, bool record = false)
    {
        if (record)
        {
            ServerDataProcesser.instance.Exchange(connectedSlot.GetIndex() + 1, slotToMove.GetIndex() + 1);
        }

        MinionManager other = slotToMove.GetConnectedMinion();

        connectedSlot.SetConnectedMinion(other);
        slotToMove.SetConnectedMinion(this);
        other.connectedSlot = connectedSlot;
        connectedSlot = slotToMove;

        this.summoningSickness = true;
        other.summoningSickness = true;
    }

    public void Attack(MinionManager enemy, bool record = false)
    {
        if (record)
        {
            int myIndex = GetIndex() + 1;
            int enemyIndex = -enemy.GetIndex() - 1;
            ServerDataProcesser.instance.Attack(myIndex, enemyIndex);
        }
        attacking = true;
        attackPosition = enemy.GetSlot().GetPosition();
        attackPosition = new Vector3(attackPosition.x, attackPosition.y, attackPosition.z - 1f);
        StartCoroutine(DoAttack(enemy));
    }

    private IEnumerator DoAttack(MinionManager enemy)
    {

        while (attacking)
        {
            /* epic attack sounds */
            yield return new WaitForSeconds(0.1f);
        }

        int enemyPower = enemy.GetPower();
        if (cardStats.canDealDamage && !cardStats.isStatic) 
        {
            if (enemy.GetCardType() == CardTypes.Hatapon)
            {
                if (!IsHataponSafe(!friendly))
                {
                    if (cardStats.fixedPower == -1)
                    {
                        enemy.ReceiveDamage(power);
                    }
                    else
                    {
                        enemy.ReceiveDamage(cardStats.fixedPower);
                    }
                }
            }
            else
            {
                if (cardStats.fixedPower == -1)
                {
                    enemy.ReceiveDamage(power);
                }
                else
                {
                    enemy.ReceiveDamage(cardStats.fixedPower);
                }
            }
        }

        if (enemy.GetCardStats().canDealDamage && !enemy.cardStats.isStatic)
        {
            if (GetCardType() == CardTypes.Hatapon)
            {
                if (!IsHataponSafe(friendly))
                {
                    if (enemy.cardStats.fixedPower == -1)
                    {
                        ReceiveDamage(enemyPower);
                    }
                    else
                    {
                        ReceiveDamage(enemy.cardStats.fixedPower);
                    }
                }
            }
            else
            {
                if (enemy.cardStats.fixedPower == -1)
                {
                    ReceiveDamage(enemyPower);
                }
                else
                {
                    ReceiveDamage(enemy.cardStats.fixedPower);
                }
            }
        }

        //enemy.CheckPower();
        //CheckPower();
    }

    public void CheckPower()
    {
        if (power <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        dying = true;
        StartCoroutine(FadeAway());
        while (fading)
        {
            yield return new WaitForSeconds(0.01f);
        }
        DestroySelf();
        if (GetCardType() == CardTypes.Hatapon)
        {
            gameController.EndRound(!friendly);
        }
        if (cardStats.hasDeathrattle)
        {
            if (friendly)
            {
                cardStats.onDeathEvent(connectedSlot.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots, cardStats);
            }
            else
            {
                cardStats.onDeathEvent(-connectedSlot.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots, cardStats);
            }
        }
        yield return null;
    }

    public IEnumerator FadeAway()
    {
        fading = true;

        float alpha = 1f;
        float scale = transform.localScale.x;
        float baseXPosition = transform.position.x;
        float baseYPosition = transform.position.y;
        while (alpha > 0.7f)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Image")
                {
                    Color spriteColor = child.GetComponent<SpriteRenderer>().color;
                    child.GetComponent<SpriteRenderer>().color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
                }
                
            } 
            float noise_x = Random.Range(-0.2f, 0.2f);
            float noise_y = Random.Range(-0.2f, 0.2f);
            transform.position = new Vector3(baseXPosition + noise_x, baseYPosition + noise_y, transform.position.z);
            transform.localScale = new Vector3(scale, scale, 1f);
            //Color objectColor = child.GetComponent<MeshRenderer>().material.color;
            //child.GetComponent<MeshRenderer>().material.color = new Color(objectColor.r, objectColor.g, objectColor.b, alpha);
                
            
            alpha -= fadeSpeed * Time.deltaTime;
            scale -= shrinkSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0.003f);
        }

        fading = false;
        yield return null;
    }

    public void DestroyMinion()
    {
        DestroySelf();
        if (GetCardType() == CardTypes.Hatapon)
        {
            gameController.EndRound(!friendly);
        }
        if (cardStats.hasDeathrattle)
        {
            if (friendly)
            {
                cardStats.onDeathEvent(connectedSlot.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots, cardStats);
            }
            else
            {
                cardStats.onDeathEvent(-connectedSlot.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots, cardStats);
            }
        }
    }

    public void DestroySelf()
    {
        connectedSlot.SetFree(true);
        connectedSlot.SetConnectedMinion(null);
        Destroy(gameObject);

        if (friendly)
        {
            boardManager.lastDeadYou = this.cardType;
        }
        else
        {
            boardManager.lastDeadOpponent = this.cardType;
        }
    }

    public void ReturnToNormalAfterOptions()
    {
        foreach (CardManager card in connectedCardObjects)
        {
            card.DestroyCard();
        }

        state = MinionState.Free;
        CursorController.cursorState = CursorController.CursorStates.Free;
        //powerObject.SetActive(true);
    }

    public void SetState(MinionState _st)
    {
        state = _st;
    }
    public MinionState GetState()
    {
        return state;
    }
    public bool GetCanAttack()
    {
        return !summoningSickness;
    }
    public void SetCanAttack(bool _can)
    {
        summoningSickness = !_can;
    }
    public int GetIndex()
    {
        return connectedSlot.GetIndex();
    }

    public bool GetFriendly()
    {
        return connectedSlot.GetFriendly();
    }

    public BoardManager.Slot GetSlot()
    {
        return connectedSlot;
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
