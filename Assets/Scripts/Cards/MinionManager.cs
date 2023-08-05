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
    public GameObject damageObject;
    public GameObject powerSquare;
    public GameObject heartObject;
    public GameObject normalBackObject;
    public GameObject outlineBackObject;
    public GameObject poisonObject;
    public float normalScale;
    public float selectedScale;
    public Renderer myRenderer;

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
    private bool additionalAttack = false;
    private List<CardManager> connectedCardObjects;
    private Arrow arrow = null;
    public float fadeSpeed = 10f;
    public float shrinkSpeed = 3f;
    public bool fading = false;
    public bool dying = false;
    public float startTime = 0f;
    public CardManager previewedCard = null;

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
        OnCanAttack(false);
        
        if (cardStats.hasHaste || cardStats.isStatic)
        {
            OnCanAttack(true);
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

        damageObject.SetActive(false);
        //outlineBackObject.SetActive(false);
        normalBackObject.SetActive(false);
        poisonObject.SetActive(false);

        startTime = Time.time;
    }

    public void OnCanAttack(bool can=true)
    {
        if (GameController.playerTurn && friendly && !(!cardStats.canAttack && cardStats.limitedVision && !cardStats.isStatic))
        {
            summoningSickness = !can;
            outlineBackObject.SetActive(can);
            //normalBackObject.SetActive(!can);
        }
        else
        {
            outlineBackObject.SetActive(false);
            //normalBackObject.SetActive(false);
        }
    }

    public void GiveAdditionalAttack()
    {
        if (GameController.playerTurn && friendly && !(!cardStats.canAttack && cardStats.limitedVision && !cardStats.isStatic))
        {
            additionalAttack = true;
            outlineBackObject.SetActive(true);
            //normalBackObject.SetActive(!can);
        }
        else
        {
            outlineBackObject.SetActive(false);
            //normalBackObject.SetActive(false);
        }
    }

    public void PoisonMinion(bool poison=true)
    {
        if (cardStats.isStatic || cardStats.canDealDamage == false)
        {
            return;
        }
        poisonObject.SetActive(poison);
        cardStats.poisoned = poison;
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
        if (damage <= 0)
        {
            return;
        }
        damageObject.SetActive(true);
        damageObject.GetComponent<TextMeshPro>().text = "-" + damage.ToString();
        //damageObject.GetComponent<TextMeshPro>().overrideColorTags = true;
        damageObject.GetComponent<TextMeshPro>().color = new Color(255f, 0f, 0f, 1f);
        StartCoroutine(HideDamage(damage:true));
        SetPower(power - damage);
        CheckPower();
    }

    public IEnumerator HideDamage(bool damage=true)
    {
        StartCoroutine(ScaleDamage(random:damage));
        yield return new WaitForSeconds(1.5f);
        damageObject.SetActive(false);
        yield return null;
    }

    public IEnumerator ScaleDamage(bool random=true)
    {   
        float basePosition = damageObject.transform.localPosition.y;
        float module = 0.35f;
        float startTime = Time.time;
        while (damageObject.activeSelf)
        {
            float noise;
            if (random)
            {
                noise = Random.Range(-module, module);
            }
            else
            {
                noise = module * Mathf.Sin((Time.time - startTime) * 5f);
            }
            module -= 2f * Time.deltaTime;
            module = Mathf.Max(0, module);
            damageObject.transform.localPosition = new Vector3(damageObject.transform.localPosition.x, basePosition + noise, damageObject.transform.localPosition.z);
            yield return new WaitForSeconds(0.01f);
        }
        damageObject.transform.localPosition = new Vector3(damageObject.transform.localPosition.x, basePosition, damageObject.transform.localPosition.z);
        yield return null;
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
        if (amount <= 0)
        {
            return;
        }
        damageObject.SetActive(true);
        damageObject.GetComponent<TextMeshPro>().text = "+" + amount.ToString();
        //damageObject.GetComponent<TextMeshPro>().overrideColorTags = true;
        damageObject.GetComponent<TextMeshPro>().color = Color.green;
        StartCoroutine(HideDamage(damage:false));
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
                    else if (CursorController.cursorState == CursorController.CursorStates.Free && (!summoningSickness || additionalAttack))
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

                                CardManager option1 = handManager.GenerateCard(cardStats.connectedCards[0], transform.position).GetComponent<CardManager>();
                                CardManager option2 = handManager.GenerateCard(cardStats.connectedCards[1], transform.position).GetComponent<CardManager>();

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

                            if (!summoningSickness && GetCardType() == CardTypes.Moribu && !target.GetFriendly() && Mathf.Abs(target.GetIndex() - GetIndex()) == 2)
                            {
                                BoardManager.Slot nextSlot = target.GetSlot();
                                target.DestroyMinion();
                                Move(nextSlot, record: true);
                                OnCanAttack(false);
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            } else if (!summoningSickness && target != this && target.friendly && (Mathf.Abs(target.GetIndex() - GetIndex()) == 1) && !cardStats.limitedVision)
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
                                    OnCanAttack(false);
                                    state = MinionState.Free;
                                    CursorController.cursorState = CursorController.CursorStates.Free;
                                    arrow.DestroyArrow();
                                    arrow = null;
                                    additionalAttack = false;
                                }
                            }

                            
                        }
                        else if (hit.collider.name == "Slot(Clone)" && !summoningSickness) // Fix this shit!
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

                            if (slotToGo == null)
                            {
                                foreach (BoardManager.Slot slot in boardManager.enemySlots)
                                { 
                                    if (slot.GetSlotObject() == hit.collider.gameObject)
                                    {
                                        slotToGo = slot;
                                        break;
                                    }
                                }
                            }

                            if (GetCardType() == CardTypes.Moribu && !slotToGo.GetFriendly() && Mathf.Abs(slotToGo.GetIndex() - GetIndex()) == 2)
                            {
                                MinionManager connectedMinion = slotToGo.GetConnectedMinion();
                                if (connectedMinion != null)
                                {
                                    connectedMinion.DestroyMinion();
                                }
                                Move(slotToGo, record: true);
                                OnCanAttack(false);
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            }
                            else if (slotToGo == null || !slotToGo.GetFree() || Mathf.Abs(slotToGo.GetIndex() - GetIndex()) > 1 || cardStats.limitedVision)
                            {
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            }
                            else if (slotToGo.GetFriendly())
                            {
                                Move(slotToGo, record: true);
                                OnCanAttack(false);
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
            if (GetCardStats().flying)
            {
                //transform.localScale = new Vector3()
                transform.position = new Vector3(transform.position.x + 0.04f * Mathf.Sin(1.5f * (startTime - Time.time)), transform.position.y + 0.025f * Mathf.Cos(1.5f * (startTime - Time.time)), transform.position.z);
                //transform.rotation = Quaternion.Euler(10f * Mathf.Sin(Time.time), 10f * Mathf.Cos(Time.time), 0f);
            }
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
            int sign = 1;
            if (!slotToMove.GetFriendly())
            {
                sign = -1;
            }
            ServerDataProcesser.instance.Move(connectedSlot.GetIndex() + 1, sign * (slotToMove.GetIndex() + 1));
        }

        connectedSlot.SetFree(true);
        connectedSlot.SetConnectedMinion(null);
        slotToMove.SetConnectedMinion(this);
        slotToMove.SetFree(false);
        friendly = slotToMove.GetFriendly();        
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

        this.OnCanAttack(false);
        other.OnCanAttack(false);
    }

    public void Attack(MinionManager enemy, bool record = false)
    {
        if (record)
        {
            int myIndex = GetIndex() + 1;
            int enemyIndex = -enemy.GetIndex() - 1;
            ServerDataProcesser.instance.Attack(myIndex, enemyIndex);
        }
        if (GetCardStats().onAttackEvent != null)
        {
            int myIndex, enemyIndex;
            List<BoardManager.Slot> friends, enemies; 
            if (GetFriendly())
            {
                myIndex = GetIndex() + 1;
                enemyIndex = -enemy.GetIndex() - 1;
                friends = boardManager.friendlySlots;
                enemies = boardManager.enemySlots;
            }
            else
            {
                myIndex = -GetIndex() - 1;
                enemyIndex = enemy.GetIndex() + 1;
                enemies = boardManager.friendlySlots;
                friends = boardManager.enemySlots;
            }
            List<int> targets = new()
            {
                myIndex,
                enemyIndex
            };
            StartCoroutine(GetCardStats().onAttackEvent(targets, enemies, friends));
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
        connectedSlot.SetFree(true);
        connectedSlot.SetConnectedMinion(null);
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
        StartCoroutine(Die());
    }

    public void DestroySelf(bool unattach=false)
    {
        if (unattach)
        {
            connectedSlot.SetFree(true);
            connectedSlot.SetConnectedMinion(null);
        }
        Destroy(gameObject);
        if (previewedCard != null)
        {
            previewedCard.DestroyCard();
        }

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
        return (!summoningSickness || additionalAttack);
    }
    public void SetCanAttack(bool _can)
    {
        OnCanAttack(_can);
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

    private IEnumerator CardPreview()
    {
        yield return new WaitForSeconds(0.5f);

        if (mouseOver && previewedCard == null)
        {
            previewedCard = handManager.GenerateCard(cardType).GetComponent<CardManager>();
            previewedCard.SetCardState(CardManager.CardState.hilightOver);
            
            if (friendly && connectedSlot.GetIndex() > 1)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(-4f, 3.5f, -6f);
            }
            else if (friendly && connectedSlot.GetIndex() <= 1)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(3.5f, 3.5f, -6f);
            }
            else if (!friendly && connectedSlot.GetIndex() > 1)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(-4f, -2.5f, -6f);
            } 
            else
            {
                previewedCard.transform.position = this.transform.position + new Vector3(3.5f, -2.5f, -6f);
            }
        }
        
        yield return null;
    }

    private void OnMouseOver()
    {
        mouseOver = true;
        if (previewedCard == null)
        {
            StartCoroutine(CardPreview());
        }
    }
    private void OnMouseExit()
    {
        mouseOver = false;
        if (previewedCard != null)
        {
            previewedCard.DestroyCard();
        }
    }
}
