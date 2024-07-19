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
    public GameObject outlineBackAbilitiesObject;
    public GameObject poisonObject;
    public GameObject shieldObject;
    public GameObject lifelinkObject;
    public GameObject lifelinkObjectHatapon;
    public GameObject hexproofObject;
    public GameObject endTurnObject;
    public GameObject tailwindObject;
    public GameObject circleMyamsarObject;
    public GameObject onAttackObject;
    public float normalScale;
    public float selectedScale;
    public Renderer myRenderer;

    private bool attacking = false;
    public bool onAttackActionProgress = false;
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
    public bool attacked = false;
    public bool moved = false;
    public ShakeScreen _shakeScreen;
    public bool alreadyShake = false;

    public float tailwindTimer = 0f;

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
        SetAbilityToAttack(false);
        
        if (cardStats.hasHaste || cardStats.isStatic)
        {
            SetAbilityToAttack(true);
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
        _shakeScreen = GameObject.Find("Main Camera").GetComponent<ShakeScreen>();

        damageObject.SetActive(false);
        //outlineBackAbilitiesObject.SetActive(false);
        //outlineBackObject.SetActive(false);
        //normalBackObject.SetActive(false);
        poisonObject.SetActive(false);
        shieldObject.SetActive(false);
        lifelinkObject.SetActive(false);
        hexproofObject.SetActive(false);
        endTurnObject.SetActive(false);
        tailwindObject.SetActive(false);
        circleMyamsarObject.SetActive(false);

        startTime = Time.time;
    }

    public void SetAbilityToAttack(bool can=true)
    {
        if (GameController.playerTurn && GetFriendly() && !(!cardStats.canAttack && cardStats.limitedVision && !cardStats.isStatic))
        {
            summoningSickness = !can;
            if (cardStats.isStatic)
            {
                outlineBackAbilitiesObject.SetActive(can);
                outlineBackObject.SetActive(false);
                normalBackObject.SetActive(can);
            }
            else
            {
                outlineBackObject.SetActive(can);
                outlineBackAbilitiesObject.SetActive(false);
                normalBackObject.SetActive(false);
            }
            //normalBackObject.SetActive(!can);
        }
        else
        {
            outlineBackObject.SetActive(false);
            outlineBackAbilitiesObject.SetActive(false);
            normalBackObject.SetActive(false);
            //normalBackObject.SetActive(false);
        }
    }

    public void GiveAdditionalAttack()
    {
        if (GameController.playerTurn && GetFriendly() && !(!cardStats.canAttack && cardStats.limitedVision && !cardStats.isStatic))
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
        poisonObject.SetActive(poison);
        cardStats.poisoned = poison;
    }

    public bool LifelinkRedirectDamage(int damage)
    {
        bool found = false;
        foreach (MinionManager minion in cardStats.lifelinkedTo)
        {
            if (minion != null && minion.GetFriendly() == GetFriendly())
            {
                //Debug.Log(minion.GetSlot().GetIndex());
                minion.ReceiveDamage(damage);
                found = true;
            }
        }
        return found;
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

    public bool DealDamageToThis(int damage)
    {
        // Just to make it easier to understand
        return LoseLife(damage);
    }

    public bool LoseLife(int damage)
    {
        if (damage <= 0)
        {
            return false;
        }
        if (LifelinkRedirectDamage(damage))
        {
            return false;
        }
        damageObject.SetActive(true);
        damageObject.GetComponent<TextMeshPro>().text = "-" + damage.ToString();
        //damageObject.GetComponent<TextMeshPro>().overrideColorTags = true;
        damageObject.GetComponent<TextMeshPro>().color = new Color(255f, 0f, 0f, 1f);
        StartCoroutine(HideDamage(damage:true));
        SetPower(power - damage);
        if (CheckPower())
        {
            if (cardStats.onDamageEvent != null)
            {
                int idx = connectedSlot.GetIndex();
                QueueData newEvent = new();
                newEvent.actionType = QueueData.ActionType.OnDamage;
                newEvent.hostUnit = this;
                newEvent.index = this.GetIndex();

                if (GetFriendly())
                {
                    newEvent.friendlySlots = boardManager.friendlySlots;
                    newEvent.enemySlots = boardManager.enemySlots;
                }
                else
                {
                    newEvent.friendlySlots = boardManager.enemySlots;
                    newEvent.enemySlots = boardManager.friendlySlots;
                }
                GameController.eventQueue.Insert(0, newEvent);
                // On damage
                //if (GetFriendly())
                //{
                //    StartCoroutine(cardStats.onDamageEvent(idx, boardManager.enemySlots, boardManager.friendlySlots));
                //}
                //else
                //{
                //    StartCoroutine(cardStats.onDamageEvent(idx, boardManager.friendlySlots, boardManager.enemySlots));
                //}
            }
            return false;
        }
        else
        {
            return true;
        }


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

    public bool ReceiveDamage(int damage)
    {
        if (cardType == CardTypes.Hatapon)
        {
            if (IsHataponSafe(GetFriendly()))
            {
                return false;
            }
        }
        if (LifelinkRedirectDamage(damage))
        {
            return false;
        }
        int actualDamage = damage - cardStats.armor;
        if (actualDamage < 0) 
        {
            actualDamage = 0;
        }
        return LoseLife(actualDamage);
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

                if (GetFriendly() && !dying)
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

                        if (Input.GetMouseButtonDown(0) && !cardStats.isStatic && GameController.CanPerformActions())
                        {
                            state = MinionState.Selected;
                            CursorController.cursorState = CursorController.CursorStates.Hold;
                        }
                        else if (cardStats.isStatic)
                        {
                            if (Input.GetMouseButtonDown(0) && GameController.CanPerformActions())
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

                if (Input.GetMouseButtonDown(0)  && GameController.CanPerformActions())
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
                                SetAbilityToAttack(false);
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            } else if (!summoningSickness && target != this && target.GetFriendly() && (Mathf.Abs(target.GetIndex() - GetIndex()) == 1) && !cardStats.limitedVision)
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

                                if (!cardStats.canAttack || target == this || target.GetFriendly() || target.cardStats.flying || (Mathf.Abs(target.GetIndex() - GetIndex()) > 1 && !cardStats.megaVision) || (!target.cardStats.hasGreatshield && existsGreatshield))
                                {
                                    state = MinionState.Free;
                                    CursorController.cursorState = CursorController.CursorStates.Free;
                                    arrow.DestroyArrow();
                                    arrow = null;
                                }
                                else
                                {
                                    Attack(target, record: true);
                                    SetAbilityToAttack(false);
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
                                SetAbilityToAttack(false);
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
                                SetAbilityToAttack(false);
                                state = MinionState.Free;
                                CursorController.cursorState = CursorController.CursorStates.Free;
                                arrow.DestroyArrow();
                                arrow = null;
                            }
                        }
                    }
                }

                if (!GameController.playerTurn)
                {
                    state = MinionState.Free;
                    CursorController.cursorState = CursorController.CursorStates.Free;
                    arrow.DestroyArrow();
                    arrow = null;
                }

                break;

            case MinionState.ChooseOption:

                if (Input.GetMouseButtonDown(1) || !GameController.playerTurn)
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

            if (length < 0.2 && !alreadyShake)
            {
                alreadyShake = true;
                // Shake screen
                float shakeStrength = 0f;
                if (this.GetPower() <= 2)
                {
                    shakeStrength = 0f;
                }
                else if (this.GetPower() <= 6)
                {
                    shakeStrength = 1f;
                }
                else if (this.GetPower() <= 9)
                {
                    shakeStrength = 2f;
                }
                else
                {
                    shakeStrength = 3f;
                }
                _shakeScreen.shakeTheScreen(shakeStrength);
            }

        }
        else if (!onAttackActionProgress)
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

        if (cardStats.hasShield != shieldObject.activeSelf)
        {
            shieldObject.SetActive(cardStats.hasShield);
        }

        if (cardStats.hexproof != hexproofObject.activeSelf)
        {
            hexproofObject.SetActive(cardStats.hexproof);
        }

        if (cardStats.endTurnEvent != null && !endTurnObject.activeSelf)
        {
            endTurnObject.SetActive(true);
        }
        if (cardStats.endTurnEvent == null && endTurnObject.activeSelf)
        {
            endTurnObject.SetActive(false);
        }

        if (cardStats.onAttackEvent != null && !onAttackObject.activeSelf)
        {
            onAttackObject.SetActive(true);
        }
        if (cardStats.onAttackEvent == null && onAttackObject.activeSelf)
        {
            onAttackObject.SetActive(false);
        }

        bool hataponLifelinkActive = false;
        if (cardType == CardTypes.Hatapon)
        {
            List<BoardManager.Slot> slotList;
            if (GetSlot().GetFriendly())
            {
                slotList = boardManager.friendlySlots;
            }
            else
            {
                slotList = boardManager.enemySlots;
            }
            foreach (BoardManager.Slot _slot in slotList)
            {
                MinionManager minion = _slot.GetConnectedMinion();
                if (minion != null && minion.GetCardStats().hasShield)
                {
                    hataponLifelinkActive = true;
                }
            }
        }

        if (hataponLifelinkActive != lifelinkObjectHatapon.activeSelf)
        {
            lifelinkObjectHatapon.SetActive(hataponLifelinkActive);
        }

        //tailwind

        if (tailwindTimer > 0f)
        {
            tailwindTimer -= Time.deltaTime;
            tailwindObject.SetActive(true);
            tailwindObject.transform.eulerAngles = new Vector3(0f, 0f, tailwindObject.transform.eulerAngles.z + 5f);
            Debug.Log(tailwindTimer);
        }
        else
        {
            tailwindObject.SetActive(false);
        }


        /*
        bool lifelinkActive = false;
        foreach (MinionManager potentialLifelinker in cardStats.lifelinkedTo)
        {
            if (potentialLifelinker != null && potentialLifelinker.GetFriendly() == friendly)
            {
                lifelinkActive = true;

                float yPositionDiff = 0.3f * Mathf.Sin(1.5f * (startTime - Time.time));

                Vector3 from = new Vector3(this.transform.position.x, this.transform.position.y + yPositionDiff, this.transform.position.z);
                Vector3 to = new Vector3(potentialLifelinker.transform.position.x, potentialLifelinker.transform.position.y + yPositionDiff, potentialLifelinker.transform.position.z);

                lifelinkObject.GetComponent<LineRenderer>().SetPosition(0, from);
                lifelinkObject.GetComponent<LineRenderer>().SetPosition(1, to);
            }
        }

        if (lifelinkActive != shieldObject.activeSelf)
        {
            shieldObject.SetActive(lifelinkActive);
            lifelinkObject.SetActive(lifelinkActive);
        }
        */
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
        moved = true;
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

        this.SetAbilityToAttack(false);
        other.SetAbilityToAttack(false);
    }

    public void Attack(MinionManager enemy, bool record = false)
    {
        if (record)
        {
            int myIndex = GetIndex() + 1;
            int enemyIndex = -enemy.GetIndex() - 1;
            ServerDataProcesser.instance.Attack(myIndex, enemyIndex);
        }
        attacked = true;
        attackPosition = enemy.GetSlot().GetPosition();
        attackPosition = new Vector3(attackPosition.x, attackPosition.y, attackPosition.z - 1f);
        StartCoroutine(DoAttack(enemy));
    }

    private IEnumerator DoAttack(MinionManager enemy)
    {
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

            QueueData newEvent = new();
            newEvent.actionType = QueueData.ActionType.OnAttack;
            newEvent.thisStats = cardStats;
            newEvent.hostUnit = this;
            newEvent.targets = targets;
            newEvent.friendlySlots = friends;
            newEvent.enemySlots = enemies;
           
            GameController.eventQueue.Insert(0, newEvent);
            onAttackActionProgress = true;

            // On attack
            //StartCoroutine(GetCardStats().onAttackEvent(targets, enemies, friends));
        }

        while (onAttackActionProgress)
        {
            yield return new WaitForSeconds(0.1f);
        }

        attacking = true;


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
                if (!IsHataponSafe(!GetFriendly()))
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
                if (!IsHataponSafe(GetFriendly()))
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

    public bool CheckPower()
    {
        if (power <= 0)
        {
            StartCoroutine(Die());
            return false;
        }
        return true;
    }

    private IEnumerator Die()
    {
        if (dying)
        {
            yield return null;
        }
        dying = true;
        connectedSlot.SetFree(true);
        connectedSlot.SetConnectedMinion(null);

        Debug.Log("On death");
        Debug.Log(cardStats.onDeathSound);
        AudioController.PlaySound(GetCardStats().onDeathSound);

        if (cardStats.hasOnDeath)
        {
            QueueData newEvent = new();
            newEvent.actionType = QueueData.ActionType.OnDeath;
            newEvent.thisStats = cardStats;
            newEvent.hostUnit = this;
            newEvent.index = this.GetIndex();

            if (GetFriendly())
            {
                newEvent.friendlySlots = boardManager.friendlySlots;
                newEvent.enemySlots = boardManager.enemySlots;
            }
            else
            {
                newEvent.friendlySlots = boardManager.enemySlots;
                newEvent.enemySlots = boardManager.friendlySlots;
            }
            GameController.eventQueue.Insert(0, newEvent);
            // On death
            //if (GetFriendly())
            //{
            //   StartCoroutine(cardStats.onDeathEvent(connectedSlot.GetIndex(), boardManager.enemySlots, boardManager.friendlySlots, cardStats));
            //}
            //else
            //{
            //    StartCoroutine(cardStats.onDeathEvent(-connectedSlot.GetIndex(), boardManager.friendlySlots, boardManager.enemySlots, cardStats));
            //}
        }

        while (!GameController.CanPerformActions())
        {
            yield return new WaitForSeconds(0.01f);
        }

        dying = true;
        StartCoroutine(FadeAway());
        while (fading)
        {
            yield return new WaitForSeconds(0.01f);
        }
        DestroySelf();
        if (GetCardType() == CardTypes.Hatapon)
        {
            gameController.EndRound(!GetFriendly());
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
        
        /*
        if (friendly)
        {
            boardManager.lastDeadYou = this.cardType;
        }
        else
        {
            boardManager.lastDeadOpponent = this.cardType;
        }
        */


        Destroy(gameObject);
        if (previewedCard != null)
        {
            previewedCard.DestroyCard();
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
        attacked = false;
        moved = false;
        SetAbilityToAttack(_can);
        if (_can)
        {
            alreadyShake = false;
        }
    }
    public int GetIndex()
    {
        return connectedSlot.GetIndex();
    }

    public bool GetFriendly()
    {
        return connectedSlot.GetFriendly();
    }

    public void SetFriendly(bool friendly)
    {
        this.friendly = friendly;
    }

    public BoardManager.Slot GetSlot()
    {
        return connectedSlot;
    }

    public void SetSlot(BoardManager.Slot _slot)
    {
        connectedSlot.SetConnectedMinion(null);
        connectedSlot.SetFree(true);
        connectedSlot = _slot;
        connectedSlot.SetConnectedMinion(this);
    }

    public int GetDevotion(Runes kind)
    {
        int count = 0;
        foreach (Runes rune in cardStats.runes)
        {
            if (rune == kind)
            {
                count += 1;
            }
        }
        return count;
    }

    private IEnumerator CardPreview()
    {
        yield return new WaitForSeconds(0.5f);

        if (mouseOver && previewedCard == null)
        {
            previewedCard = handManager.GenerateCard(cardType).GetComponent<CardManager>();
            previewedCard.SetCardState(CardManager.CardState.hilightOver);
            
            if (GetFriendly() && connectedSlot.GetIndex() > 1)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(-4f, 3.5f, -6f);
            }
            else if (GetFriendly() && connectedSlot.GetIndex() <= 1)
            {
                previewedCard.transform.position = this.transform.position + new Vector3(3.5f, 3.5f, -6f);
            }
            else if (!GetFriendly() && connectedSlot.GetIndex() > 1)
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
